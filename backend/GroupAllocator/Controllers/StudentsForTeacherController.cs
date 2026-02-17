using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("students")]
public class StudentsForTeacherController(ApplicationDbContext db, IUserService userService, StudentsService studentsService) : ControllerBase
{
    [HttpGet]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> GetAllStudentsInClass(int classId)
	{
		if (!await userService.IsCurrentTeacherPartOfClass(classId, User))
		{
			return Forbid("Teacher is not in this class");
		}

		return await studentsService.GetStudents(classId);
	}

	[HttpPost("whitelist")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> PostWhitelist(int classId, [FromForm] IFormFile file)
	{
		if (!await userService.IsCurrentTeacherPartOfClass(classId, User))
		{
			return Forbid("Teacher is not in this class");
		}

		if (file == null || file.Length == 0)
		{
			return BadRequest("No file uploaded.");
		}

		using var reader = new StreamReader(file.OpenReadStream());
		await userService.CreateStudentAllowlist(classId, reader);

		return await studentsService.GetStudents(classId);
	}

	[HttpPost("add")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<StudentInfoAndSubmission>>> AddStudent(int classId, string email)
	{
		if (!await userService.IsCurrentTeacherPartOfClass(classId, User))
		{
			return Forbid("Teacher is not in this class");
		}

		if (string.IsNullOrWhiteSpace(email))
		{
			return BadRequest("Email is required.");
		}

		await userService.AddStudentToClass(classId, email);

		return await studentsService.GetStudents(classId);
	}

    [HttpGet("populate/{classId:int}")]
    [Authorize(Policy = "TeacherOnly")]
    public async Task<ActionResult> PopulateRandomPreferences(int classId)
    {
        if (!await userService.IsCurrentTeacherPartOfClass(classId, User))
        {
            return Forbid("Teacher is not in this class");
        }

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
		if (!await userService.IsCurrentTeacherPartOfClass(classId, User))
		{
			return Forbid("Teacher is not in this class");
		}

		await db.Students.Where(s => s.Id == id && s.Class.Id == classId).ExecuteDeleteAsync();
		await db.SaveChangesAsync();
		return await GetAllStudentsInClass(classId);
	}
}
