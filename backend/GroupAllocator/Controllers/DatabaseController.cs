using GroupAllocator.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;


[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
#if !DEBUG
	[Authorize(Policy = "AdminOnly")]
#endif
	[HttpGet("reset")]
	public async Task<ActionResult<string>> ResetDatabase([FromServices] ApplicationDbContext db)
	{
		await db.Database.EnsureDeletedAsync();
		await db.Database.EnsureCreatedAsync();
		return "Database reset successfully.";
	}
}
