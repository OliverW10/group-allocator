using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
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
