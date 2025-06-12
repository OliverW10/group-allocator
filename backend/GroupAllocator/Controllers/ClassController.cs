using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ClassController : ControllerBase
{
	private readonly ApplicationDbContext _dbContext;

	public ClassController(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	[HttpGet("list")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<IActionResult> GetClassesForTeacher()
	{
		var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		
		var classes = await _dbContext.Classes
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
		var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
		
		var classes = await _dbContext.Classes
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
}
