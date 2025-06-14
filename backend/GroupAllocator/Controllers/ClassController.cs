using System.IdentityModel.Tokens.Jwt;
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ClassController(ApplicationDbContext db) : ControllerBase
{
	[HttpGet("list")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<IActionResult> GetClassesForTeacher()
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var classes = await db.Classes
			.Include(c => c.Teachers)
			.Where(c => c.Teachers.Any(t => t.Teacher.User.Id == userId))
			.Select(c => new
			{
				c.Id,
				c.Code,
				TeacherRole = c.Teachers.First(t => t.Teacher.User.Id == userId).Role
			})
			.ToListAsync();

		return Ok(classes);
	}

	[HttpGet("")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<IActionResult> GetClassesForStudent()
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));

		var classes = await db.Classes
			.Include(c => c.Students)
			.Where(c => c.Students.Any(s => s.User.Id == userId))
			.Select(c => new
			{
				c.Id,
				c.Code
			})
			.ToListAsync();

		return Ok(classes);
	}

	[HttpPost("")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<IActionResult> CreateClass([FromBody] ClassDto classDto)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		// Check if code already exists
		if (await db.Classes.AnyAsync(c => c.Code == classDto.Code))
		{
			return BadRequest("A class with this code already exists");
		}

		var teacher = await db.Teachers
			.Include(t => t.User)
			.FirstOrDefaultAsync(t => t.User.Id == userId) ?? throw new InvalidOperationException("Teacher not found");

		var newClass = new ClassModel
		{
			Code = classDto.Code,
			Name = classDto.Name,
			CreatedAt = DateTimeOffset.UtcNow
		};

		db.Classes.Add(newClass);
		await db.SaveChangesAsync();

		// Add the teacher as the owner of the class
		db.ClassTeachers.Add(new ClassTeacherModel
		{
			Teacher = teacher,
			Class = newClass,
			Role = ClassTeacherRole.Owner
		});
		await db.SaveChangesAsync();

		return Ok(new ClassResponseDto
		{
			Id = newClass.Id,
			Code = newClass.Code,
			Name = newClass.Name,
			CreatedAt = newClass.CreatedAt,
			TeacherRole = ClassTeacherRole.Owner
		});
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassDto classDto)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
					.ThenInclude(t => t.User)
			.FirstOrDefaultAsync(c => c.Id == id);

		if (@class == null)
		{
			return NotFound("Class not found");
		}

		// Check if the teacher is the owner of the class
		var teacherRole = @class.Teachers
			.FirstOrDefault(t => t.Teacher.User.Id == userId)?.Role;

		if (teacherRole != ClassTeacherRole.Owner)
		{
			return Forbid("Only the class owner can update the class");
		}

		// Check if the new code conflicts with an existing class
		if (classDto.Code != @class.Code && await db.Classes.AnyAsync(c => c.Code == classDto.Code))
		{
			return BadRequest("A class with this code already exists");
		}

		@class.Code = classDto.Code;
		@class.Name = classDto.Name;

		await db.SaveChangesAsync();

		return Ok(new ClassResponseDto
		{
			Id = @class.Id,
			Code = @class.Code,
			Name = @class.Name,
			CreatedAt = @class.CreatedAt,
			TeacherRole = ClassTeacherRole.Owner
		});
	}

	[HttpGet("join-code/{code}")]
	public async Task<IActionResult> JoinClassFromCode(string code)
	{
		var classId = db.Classes.FirstAsync(x => x.Code == code).Id;
		return await JoinClass(classId);
	}

	[HttpGet("join/{id}")]
	public async Task<IActionResult> JoinClass(int id)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));

		var @class = await db.Classes.FindAsync(id) ?? throw new InvalidOperationException("Class not found");
		var user = await db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found"); ;

		db.Add(new StudentModel
		{
			Class = @class,
			User = user,
		});
		await db.SaveChangesAsync();

		return Ok();
	}
}
