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
public class StudentsController(ApplicationDbContext db, IUserService userService) : ControllerBase
{
	[HttpGet]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> GetAll(int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));

		// Check if teacher has access to this class
		var hasAccess = await db.ClassTeachers
			.AnyAsync(t => t.Teacher.Id == userId && t.Class.Id == classId);

		if (!hasAccess)
		{
			return Forbid("You don't have access to this class");
		}

		var students = await GetStudents(classId);

		return students;
	}

	[HttpPost("whitelist")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> PostWhitelist(int classId, [FromForm] IFormFile file)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		// Check if teacher has access to this class
		var hasAccess = await db.ClassTeachers
			.AnyAsync(t => t.Teacher.Id == userId && t.Class.Id == classId);
			
		if (!hasAccess)
		{
			return Forbid("You don't have access to this class");
		}

		if (file == null || file.Length == 0)
		{
			return BadRequest("No file uploaded.");
		}

		using var reader = new StreamReader(file.OpenReadStream());
		await userService.CreateStudentAllowlist(classId, reader);

		var students = await GetStudents(classId);

		return students;
	}

	[HttpPost("add")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> AddStudent(int classId, string email)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		// Check if teacher has access to this class
		var hasAccess = await db.ClassTeachers
			.AnyAsync(t => t.Teacher.Id == userId && t.Class.Id == classId);
			
		if (!hasAccess)
		{
			return Forbid("You don't have access to this class");
		}

		if (string.IsNullOrWhiteSpace(email))
		{
			return BadRequest("Email is required.");
		}

		await userService.AddStudentToClass(classId, email);

		var students = await GetStudents(classId);

		return students;
	}

	async Task<List<StudentInfoAndSubmission>> GetStudents(int classId)
	{
		return await db.Students
			.Include(s => s.Files)
			.Include(s => s.User)
			.Include(s => s.Preferences)
				.ThenInclude(p => p.Project)
			.Include(s => s.Class)
			.Where(s => s.Class.Id == classId)
			.Select(s => s.ToDto())
			.ToListAsync();
	}

	[HttpGet("me")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<StudentSubmissionDto>> Get(int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var student = await db.Students
			.Include(s => s.User)
			.Include(s => s.Files)
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
	public async Task<ActionResult> Post([FromBody] StudentSubmissionDto preferences)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var user = await db.Users.FindAsync(userId);
		if (user == null)
		{
			return BadRequest("User doesn't exist");
		}

		var @class = await db.Classes.FindAsync(preferences.ClassId);
		if (@class == null)
		{
			return BadRequest("Class not found");
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

		return Ok();
	}

	[HttpGet("populate/{classId:int}")]
	public async Task<ActionResult> PopulateRandomPreferences(int classId)
	{
		var projects = await db.Projects.Where(p => p.Class.Id == classId).ToListAsync();
		var students = await db.Students.Where(s => s.Class.Id == classId).ToListAsync();

		foreach (var student in students)
		{
			var preferences = new List<PreferenceModel>();
			for (int i = 0; i < 10; i++)
			{
				var preference = new PreferenceModel
				{
					Project = projects[i],
					Student = student,
					Ordinal = i
				};

				db.Add(preference);
				preferences.Add(preference);
			}

			db.Students.Update(student);
		}

		await db.SaveChangesAsync();

		return Ok("Successfully populated preferences");
	}

	[HttpPost("file")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<FileDetailsDto>> PostFile(int classId, [FromForm] IFormFile file)
	{
		switch (file.Length)
		{
			case 0:
				return BadRequest("No file uploaded.");
			case > 10 * 1024 * 1024:
				return BadRequest("File is too large (>10MB)");
		}

		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		var user = await db.Users.FindAsync(userId);
		if (user == null)
		{
			return Unauthorized("Not logged in.");
		}

		var fileBytes = new byte[file.Length];
		await using (var stream = file.OpenReadStream())
		{
			await stream.ReadExactlyAsync(fileBytes);
		}

		var student = await db.Students.FirstOrDefaultAsync(s => s.Id == userId && s.Class.Id == classId);
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

	[HttpGet("files")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<FileDetailsDto>>> GetFiles(int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		// Check if teacher has access to this class
		var hasAccess = await db.ClassTeachers
			.AnyAsync(t => t.Teacher.Id == userId && t.Class.Id == classId);
			
		if (!hasAccess)
		{
			return Forbid("You don't have access to this class");
		}

		var files = await db.Files
			.Include(f => f.Student)
			.Where(f => f.Student.Class.Id == classId)
			.ToListAsync();
			
		return files.Select(f => new FileDetailsDto
		{
			Id = f.Id,
			Name = f.Name,
			UserId = f.Student.Id,
		}).ToList();
	}

	[HttpGet("file/{id:int}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult> GetFile(int id, int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		// Check if teacher has access to this class
		var hasAccess = await db.ClassTeachers
			.AnyAsync(t => t.Teacher.Id == userId && t.Class.Id == classId);
			
		if (!hasAccess)
		{
			return Forbid("You don't have access to this class");
		}
		
		var file = await db.Files
			.Include(f => f.Student)
			.FirstOrDefaultAsync(f => f.Id == id && f.Student.Class.Id == classId);
			
		if (file == null)
		{
			return NotFound("File not found");
		}

		return File(file.Blob, "application/octet-stream", file.Name);
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> DeleteStudent(int id, int classId)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		// Check if teacher has access to this class
		var hasAccess = await db.ClassTeachers
			.AnyAsync(t => t.Teacher.Id == userId && t.Class.Id == classId);
			
		if (!hasAccess)
		{
			return Forbid("You don't have access to this class");
		}

		await db.Students.Where(s => s.Id == id && s.Class.Id == classId).ExecuteDeleteAsync();
		await db.SaveChangesAsync();
		return await GetAll(classId);
	}
}
