using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
	[HttpGet]
	public IActionResult GetProjects()
	{
		return Ok();
	}
}