using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers
{
	[Controller]
	[Route("/api/[controller]")]
	public class StudentController : Controller
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
}
