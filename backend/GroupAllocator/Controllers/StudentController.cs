using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
	[HttpGet]
	public IActionResult GetPreferences()
	{
		return Ok();
	}

	[HttpPost]
	public IActionResult PostPreferences(StudentPreferencesDto data)
	{
		return Ok();
	}

	[HttpGet]
	public IActionResult GetProjects()
	{
		return Ok();
	}
}
