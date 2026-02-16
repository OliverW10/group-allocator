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
[Route("class")]
public class ClassForTeacherController(ApplicationDbContext db, PaymentService paymentService, IUserService userService,
	ProjectsController projectsController, StudentsForTeacherController studentsForTeacherController) : ControllerBase
{
	[HttpGet("list")]
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

	[HttpGet("code/{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ClassInfoDto>> GetClassInfo(int id)
	{
		if (!await userService.IsCurrentTeacherPartOfClass(id, User))
		{
			return Forbid("Teacher is not in this class");
		}
		
		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
			.Include(c => c.Students)
				.ThenInclude(s => s.Preferences)
			.FirstAsync(c => c.Id == id);

		var studentsWithPreferences = @class.Students.Count(s => s.Preferences.Any());

		return new ClassInfoDto
		{
			Name = @class.Name,
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

	[HttpPost()]
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
		if (!await userService.IsCurrentTeacherPartOfClass(id, User))
		{
			return Forbid("Teacher is not in this class");
		}

		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
			.Include(c => c.Students)
			.Include(c => c.Payments)
			.FirstAsync(c => c.Id == id);

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

	[HttpPost("{id}/add-teacher/{teacherEmail}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult> AddTeacherToClass(int id, string teacherEmail)
	{
		// Check if current teacher is part of the class
		if (!await userService.IsCurrentTeacherPartOfClass(id, User))
		{
			return Forbid("Teacher is not in this class");
		}

		var @class = await db.Classes.FindAsync(id) ?? throw new InvalidOperationException("Class not found");
		var teacherToAdd = await db.Users.FirstOrDefaultAsync(u => u.Email == teacherEmail) ?? await userService.GetOrCreateUserAsync(teacherEmail.Split("@").First(), teacherEmail);

		// Check if teacher is already in the class
		if (await db.ClassTeachers.AnyAsync(ct => ct.Class.Id == id && ct.Teacher.Id == teacherToAdd.Id))
		{
			return BadRequest("Teacher is already in this class");
		}

		// Add teacher to class as NonOwner
		db.ClassTeachers.Add(new ClassTeacherModel
		{
			Teacher = teacherToAdd,
			Class = @class,
			Role = ClassTeacherRole.NonOwner
		});
		await db.SaveChangesAsync();

		return Ok();
	}

	[HttpDelete("{id}/remove-teacher/{teacherEmail}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult> RemoveTeacherFromClass(int id, string teacherEmail)
	{
		// Check if current teacher is the owner of the class
		if (!await userService.IsCurrentTeacherPartOfClass(id, User, isOwner: true))
		{
			return Forbid("Teacher is not in this class");
		}

		// Check if trying to remove the owner
		var teacherToRemove = await db.ClassTeachers
			.Include(ct => ct.Teacher)
			.FirstOrDefaultAsync(ct => ct.Class.Id == id && ct.Teacher.Email == teacherEmail);

		if (teacherToRemove == null)
		{
			return BadRequest("Teacher is not in this class");
		}

		if (teacherToRemove.Role == ClassTeacherRole.Owner)
		{
			return BadRequest("Cannot remove the owner from the class");
		}

		// Remove teacher from class
		db.ClassTeachers.Remove(teacherToRemove);
		await db.SaveChangesAsync();

		return Ok();
	}

	[HttpGet("{id}/teachers")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<TeacherDto[]>> GetTeachersForClass(int id)
	{
		// Check if current teacher is part of the class
		if (!await userService.IsCurrentTeacherPartOfClass(id, User))
		{
			return Forbid("Teacher is not in this class");
		}

		var teachers = await db.ClassTeachers
			.Include(ct => ct.Teacher)
			.Where(ct => ct.Class.Id == id)
			.Select(ct => new TeacherDto
			{
				Email = ct.Teacher.Email,
				IsOwner = ct.Role == ClassTeacherRole.Owner
			})
			.ToListAsync();

		return Ok(teachers);
	}

	[HttpGet("{id}/download")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ExportDto> GetClassBackup(int id)
	{
		return new ExportDto()
		{
			ClassInfo = (await GetClassInfo(id)).Value ?? throw new InvalidOperationException("Failed to get class info"),
			Projects = (await projectsController.GetProjects(id)).Value ?? throw new InvalidOperationException("Failed to get projects"),
			Students = await studentsForTeacherController.GetStudents(id)
		}
	}
}
