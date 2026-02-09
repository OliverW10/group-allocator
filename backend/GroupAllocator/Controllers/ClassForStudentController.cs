using System.IdentityModel.Tokens.Jwt;
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("class")]
public class ClassForStudentController(ApplicationDbContext db) : ControllerBase
{
	[HttpGet("list")]
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
				Payed = c.Payments.Any(p => p.Status == PaymentStatus.Valid)
			})
			.ToListAsync();

		return classes;
	}

	[HttpGet("join-code/{code}")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<int>> JoinClassFromCode(string code)
	{
		var classId = (await db.Classes.FirstAsync(x => x.Code == code)).Id;
		return await JoinClass(classId);
	}

	[HttpGet("join/{id}")]
	[Authorize(Policy = "StudentOnly")]
	public async Task<ActionResult<int>> JoinClass(int id)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));

		var @class = await db.Classes.FindAsync(id) ?? throw new InvalidOperationException("Class not found");
		var user = await db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found"); ;
		if (db.Students.Any(x => x.Class == @class && x.User == user))
		{
			return Ok(@class.Id);
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
