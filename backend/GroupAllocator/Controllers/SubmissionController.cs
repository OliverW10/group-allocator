using System.Security.Claims;
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("students")]
public class SubmissionController(ApplicationDbContext db) : ControllerBase
{
	[HttpGet("me")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<StudentSubmissionDto>> Get([BindRequired] int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		Console.WriteLine($"userId: {userId}, classId: {classId}");
		var student = await db.Students
			.Include(s => s.User)
			.Include(s => s.Files)
			.Include(s => s.Class)
			.Include(s => s!.Preferences)
				.ThenInclude(p => p.Project)
			.Where(s => s.User.Id == userId && s.Class.Id == classId)
			.FirstOrDefaultAsync();
			
		if (student == null)
		{
			return NoContent();
		}

		return student.ToSubmissionDto();
	}

	[HttpPost("me")]
	[Authorize(Policy = "StudentOnly")]
	public Task<ActionResult> Post([FromBody, BindRequired] StudentSubmissionDto submission) => SubmitPreferences(submission, db, User);

	public static async Task<ActionResult> SubmitPreferences([FromBody] StudentSubmissionDto preferences, ApplicationDbContext db, ClaimsPrincipal User)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var user = await db.Users.FindAsync(userId);
		if (user == null)
		{
			return new BadRequestObjectResult("User doesn't exist");
		}

		var @class = await db.Classes.FindAsync(preferences.ClassId);
		if (@class == null)
		{
			return new BadRequestObjectResult("Class not found");
		}

		await db.Students.Where(s => s.User.Email == user.Email && s.Class.Id == preferences.ClassId).ExecuteDeleteAsync();
		await db.SaveChangesAsync();
		
		var student = new StudentModel()
		{
			User = user,
			Class = @class,
			WillSignContract = preferences.WillSignContract,
			IsVerified = false,
			Notes = preferences.Notes
		};
		var preferenceModels = new List<PreferenceModel>();
		var allProjects = await db.Projects.Where(p => p.Class.Id == preferences.ClassId).ToListAsync();

		int ordinal = 0;
		foreach (var preference in preferences.OrderedPreferences.Take(10))
		{
			var proj = allProjects.FirstOrDefault(x => x.Id == preference);

			if (proj == null) throw new InvalidOperationException("Project doesn't exist");

			if (preferenceModels.Any(p => p.Project == proj)) throw new InvalidOperationException("Duplicate preferences");

			var newPreference = new PreferenceModel
			{
				Project = proj,
				Student = student,
				Ordinal = ordinal++
			};

			db.Add(newPreference);
			preferenceModels.Add(newPreference);
		}

		db.Students.Add(student);

		await db.SaveChangesAsync();

		return new OkResult();
	}

	[HttpPost("file")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<FileDetailsDto>> PostFile([BindRequired] int classId, [FromForm] IFormFile file)
	{
		switch (file.Length)
		{
			case 0:
				return BadRequest("No file uploaded.");
			case > 10 * 1024 * 1024:
				return BadRequest("File is too large (>10MB)");
		}

		var fileBytes = new byte[file.Length];
		await using (var stream = file.OpenReadStream())
		{
			await stream.ReadExactlyAsync(fileBytes);
		}

		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		var student = await db.Students.FirstOrDefaultAsync(s => s.User.Id == userId && s.Class.Id == classId);
		if (student == null)
		{
			return BadRequest("Student not found");
		}

		var fileModel = new FileModel
		{
			Name = file.FileName,
			Student = student,
			Blob = fileBytes
		};

		await db.Files.AddAsync(fileModel);
		await db.SaveChangesAsync();
		return new FileDetailsDto
		{
			Id = fileModel.Id,
			Name = fileModel.Name,
			UserId = fileModel.Student.Id,
		};
	}

	[HttpDelete("file/{id:int}")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult> DeleteFile(int id, int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		var user = await db.Users.FindAsync(userId);
		if (user == null)
		{
			return Unauthorized("Not logged in.");
		}

		var file = await db.Files
			.Include(f => f.Student)
			.FirstOrDefaultAsync(f => f.Id == id && f.Student.Class.Id == classId);

		if (file == null)
		{
			return NotFound("File not found");
		}

		if (!User.HasClaim("admin", "true") && file.Student.Id != userId)
		{
			return BadRequest("You are not the owner of this file.");
		}
		
		await db.Files.Where(f => f.Id == id).ExecuteDeleteAsync();
		await db.SaveChangesAsync();
		return Ok();
	}
}
