using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController(ApplicationDbContext db, IStudentService studentService, IUserService userService) : ControllerBase
{
	[HttpGet]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> GetAll()
	{
		return Ok(await studentService.GetStudents().Select(s => s.ToDto()).ToListAsync());
	}

	[HttpPost("whitelist")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> PostWhitelist([FromForm] IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest("No file uploaded.");
		}

		using var reader = new StreamReader(file.OpenReadStream());
		await userService.CreateStudentAllowlist(reader);

		return Ok(await studentService.GetStudents().Select(s => s.ToDto()).ToListAsync());
	}

	[HttpGet("me")]
	[Authorize]
	public async Task<IActionResult> Get()
	{
		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return BadRequest("Not logged in");
		}

		var student = await studentService.GetStudents().Where(s => s.User.Email == userEmail).FirstOrDefaultAsync();
		if (student == null)
		{
			return NoContent();
		}

		return Ok(student.ToDto());
	}

	[HttpPost("me")]
	[Authorize]
	public async Task<IActionResult> Post([FromBody] StudentSubmissionDto preferences)
	{
		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return BadRequest("Not logged in");
		}

		var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

		if (user == null)
		{
			// database resets often in development
			return BadRequest("User doesn't exist");
		}

		await using var transaction = await db.Database.BeginTransactionAsync();
		await db.Student.Where(s => s.User.Email == userEmail).ExecuteDeleteAsync();
		var student = new StudentModel()
		{
			User = user,
			WillSignContract = preferences.WillSignContract,
		};
		var preferenceModels = new List<PreferenceModel>();
		var allProjects = await db.Projects.ToListAsync();

		double strength = 1.0;
		foreach (var preference in preferences.OrderedPreferences.Take(10))
		{
			var proj = allProjects.FirstOrDefault(x => x.Id == preference);

			if (proj == null) throw new InvalidOperationException("Project doesn't exist");

			if (preferenceModels.Any(p => p.Project == proj)) throw new InvalidOperationException("Duplicate preferences");

			var newPreference = new PreferenceModel
			{
				Project = proj,
				Student = student,
				Strength = strength,
			};

			db.Add(newPreference);
			preferenceModels.Add(newPreference);
			strength -= 0.1;
		}

		// TODO: handle files

		db.Student.Add(student);

		await db.SaveChangesAsync();
		await transaction.CommitAsync();

		return Ok();
	}

	[HttpPost("file")]
	[Authorize]
	public async Task<IActionResult> PostFile([FromForm] IFormFile file)
	{
		switch (file.Length)
		{
			case 0:
				return BadRequest("No file uploaded.");
			case > 10 * 1024 * 1024:
				return BadRequest("File is too large (>10MB)");
		}

		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return Unauthorized("Not logged in.");
		}
		var student = await db.Student.FirstOrDefaultAsync(s => s.User.Email == userEmail) ?? throw new BadHttpRequestException("Student not found");

		var fileBytes = new byte[file.Length];
		await using (var stream = file.OpenReadStream())
		{
			await stream.ReadExactlyAsync(fileBytes);
		}

		var fileModel = new FileModel
		{
			Name = file.FileName,
			Student = student,
			Blob = fileBytes
		};

		await db.Files.AddAsync(fileModel);
		await db.SaveChangesAsync();
		return Ok(new FileDetailsDto
		{
			Id = fileModel.Id,
			Name = fileModel.Name,
			StudentId = fileModel.Student.Id,
		});
	}

	[HttpDelete("file/{id:int}")]
	[Authorize]
	public async Task<IActionResult> DeleteFile(int id)
	{
		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return Unauthorized("Not logged in.");
		}

		var student = await db.Student.FirstOrDefaultAsync(s => s.User.Email == userEmail);
		var file = await db.Files.FirstOrDefaultAsync(f => f.Id == id);

		if (file == null)
		{
			return NotFound("File not found");
		}

		if (User.HasClaim("admin", "true") && (student == null || file.Student.Id != student.Id))
		{
			return BadRequest("You are not the owner of this file.");
		}
		
		await db.Files.Where(f => f.Id == id).ExecuteDeleteAsync();
		await db.SaveChangesAsync();
		return Ok();
	}

	[HttpGet("files")]
	[Authorize]
	public async Task<IActionResult> GetFiles()
	{
		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return Unauthorized("Not logged in.");
		}
		var student = await db.Student.FirstOrDefaultAsync(s => s.User.Email == userEmail) ?? throw new BadHttpRequestException("Student not found");
		var files = await db.Files.Where(f => f.Student.Id == student.Id).ToListAsync();
		return Ok(files.Select(f => new FileDetailsDto
		{
			Id = f.Id,
			Name = f.Name,
			StudentId = f.Student.Id,
		}));
	}

	[HttpGet("file/{id:int}")]
	[Authorize]
	public async Task<IActionResult> GetFile(int id)
	{
		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return Unauthorized("Not logged in.");
		}
		
		var file = await db.Files.Include(f => f.Student.User).FirstOrDefaultAsync(f => f.Id == id);
		if (file == null)
		{
			return NotFound("File not found");
		}

		if (User.HasClaim("admin", "true") && file.Student.User.Email != userEmail)
		{
			return BadRequest("You do not have permissions to view this file.");
		}

		return File(file.Blob, "application/octet-stream", file.Name);
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete(int id)
	{
		await db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
		await db.SaveChangesAsync();
		return await GetAll();
	}
}
