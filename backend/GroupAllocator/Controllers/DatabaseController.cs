using GroupAllocator.Database;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;


[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
#if DEBUG
	[HttpGet("reset")]
	public async Task<ActionResult<string>> ResetDatabase([FromServices] ApplicationDbContext db)
	{
		await db.Database.EnsureDeletedAsync();
		await db.Database.EnsureCreatedAsync();
		return "Database reset successfully.";
	}
#endif
}
