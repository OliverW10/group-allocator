using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers
{
	[Controller()]
	[Route("/api/[controller]")]
	public class AdminController
	{
		[HttpGet]
		public IActionResult Status()
		{
			return "yep";
		}
	}
}
