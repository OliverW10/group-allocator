using System.IdentityModel.Tokens.Jwt;
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ClassController(ApplicationDbContext db, PaymentService paymentService) : ControllerBase
{
	[HttpGet("list-teacher")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<ClassResponseDto>>> GetClassesForTeacher()
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var classes = await db.Classes
			.Include(c => c.Teachers)
			.Include(c => c.Students)
			.Include(c => c.Payments)
			.AsSplitQuery()
			.Where(c => c.Teachers.Any(t => t.Teacher.Id == userId))
			.Select(c => new ClassResponseDto
			{
				Id = c.Id,
				Code = c.Code,
				Name = c.Name,
				StudentCount = c.Students.Count,
				CreatedAt = c.CreatedAt,
				TeacherRole = c.Teachers.First(t => t.Teacher.Id == userId).Role,
				Payed = paymentService.GetPaymentPlanForClass(c) != PaymentPlan.None
			})
			.ToListAsync();

		return classes;
	}

	[HttpGet("list-student")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<List<ClassResponseDto>>> GetClassesForStudent()
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));

		var classes = await db.Classes
			.Include(c => c.Students)
			.Include(c => c.Payments)
			.Where(c => c.Students.Any(s => s.User.Id == userId))
			.Select(c => new ClassResponseDto
			{
				Id = c.Id,
				Code = c.Code,
				Name = c.Name,
				StudentCount = c.Students.Count,
				CreatedAt = c.CreatedAt,
				TeacherRole = null,
				Payed = paymentService.GetPaymentPlanForClass(c) != PaymentPlan.None
			})
			.ToListAsync();

		return classes;
	}

	[HttpGet("code/{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ClassInfoDto>> GetClassInfo(int id)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
			.Include(c => c.Students)
				.ThenInclude(s => s.Preferences)
			.FirstOrDefaultAsync(c => c.Id == id);

		if (@class == null)
		{
			return NotFound("Class not found");
		}

		// Check if the teacher is in the class
		var teacherInClass = @class.Teachers.Any(t => t.Teacher.Id == userId);
		if (!teacherInClass)
		{
			return Forbid("Teacher is not in this class");
		}

		var studentsWithPreferences = @class.Students.Count(s => s.Preferences.Any());

		return new ClassInfoDto
		{
			Code = @class.Code,
			StudentCount = @class.Students.Count,
			StudentsWithPreferencesCount = studentsWithPreferences
		};
	}

	async Task<string> GenerateUniqueCode()
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		var random = new Random();
		string code;
		
		do
		{
			code = new string(Enumerable.Repeat(chars, 5)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		} while (await db.Classes.AnyAsync(c => c.Code == code));
		
		return code;
	}

	[HttpPost("")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ClassResponseDto>> CreateClass(ClassDto classDto)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var teacher = await db.Users.FirstOrDefaultAsync(t => t.Id == userId) ?? throw new InvalidOperationException("Teacher not found");

		var newClass = new ClassModel
		{
			Code = await GenerateUniqueCode(),
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

		return new ClassResponseDto
		{
			Id = newClass.Id,
			Code = newClass.Code,
			Name = newClass.Name,
			CreatedAt = newClass.CreatedAt,
			TeacherRole = ClassTeacherRole.Owner,
			Payed = false,
			StudentCount = 0
		};
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ClassResponseDto>> UpdateClass(int id, ClassDto classDto)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
			.Include(c => c.Students)
			.Include(c => c.Payments)
			.FirstOrDefaultAsync(c => c.Id == id);

		if (@class == null)
		{
			return NotFound("Class not found");
		}

		// Check if the teacher is the owner of the class
		var teacherRole = @class.Teachers
			.FirstOrDefault(t => t.Teacher.Id == userId)?.Role;

		if (teacherRole != ClassTeacherRole.Owner)
		{
			return Forbid("Only the class owner can update the class");
		}

		@class.Name = classDto.Name;

		await db.SaveChangesAsync();

		return new ClassResponseDto
		{
			Id = @class.Id,
			Code = @class.Code,
			Name = @class.Name,
			CreatedAt = @class.CreatedAt,
			TeacherRole = ClassTeacherRole.Owner,
			Payed = paymentService.GetPaymentPlanForClass(@class) != PaymentPlan.None,
			StudentCount = @class.Students.Count
		};
	}

	[HttpGet("join-code/{code}")]
	public async Task<ActionResult<int>> JoinClassFromCode(string code)
	{
		var classId = db.Classes.First(x => x.Code == code).Id;
		return await JoinClass(classId);
	}

	[HttpGet("join/{id}")]
	public async Task<ActionResult<int>> JoinClass(int id)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));

		var @class = await db.Classes.FindAsync(id) ?? throw new InvalidOperationException("Class not found");
		var user = await db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found"); ;
		if (db.Students.Any(x => x.Class == @class && x.User == user))
		{
			return Ok();
		}

		db.Add(new StudentModel
		{
			Class = @class,
			User = user,
		});
		await db.SaveChangesAsync();

		return Ok(@class.Id);
	}
}
