using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
	[HttpGet("status")]
	public IActionResult Status()
	{
		return new JsonResult(Request.Cookies);
	}
}
