using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController(ApplicationDbContext db) : ControllerBase
{
	[HttpGet]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> GetAll()
	{
		// move to a service if it pleases you
		return Ok(await db.Student.Select(s => s.ToDto()).ToListAsync());
	}

	[HttpGet("mine")]
	[Authorize]
	public async Task<IActionResult> Get()
	{
		var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
		if (userEmail == null)
		{
			return BadRequest("Not logged in");
		}

		var student = await db.Student.Where(s => s.User.Email == userEmail).FirstOrDefaultAsync();
		if (student == null)
		{
			return NotFound();
		}

		return Ok(student.ToDto());
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete(int id)
	{
		await db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
		return await GetAll();
	}
}
