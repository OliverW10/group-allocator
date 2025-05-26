using GroupAllocator.Database;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;


[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
#if DEBUG
	[HttpGet("reset")]
	public async Task<IActionResult> ResetDatabase([FromServices] ApplicationDbContext db)
	{
		await db.Database.EnsureDeletedAsync();
		await db.Database.EnsureCreatedAsync();
		return Ok("Database reset successfully.");
	}
#endif
}
