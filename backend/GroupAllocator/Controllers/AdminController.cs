using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
[Authenticate]
public class AdminController : ControllerBase
{
	[HttpGet("status")]
	public IActionResult Status()
	{
		return Content("yep");
	}
}
