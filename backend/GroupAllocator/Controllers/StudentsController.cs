using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "AdminOnly")]
// admin controller to view all students, not controller for students to use
public class StudentsController(ApplicationDbContext db) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		// move to a service if it pleases you
		return Ok(await db.Student.Select(s => s.ToDto()).ToListAsync());
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		await db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
		return await GetAll();
	}
}
