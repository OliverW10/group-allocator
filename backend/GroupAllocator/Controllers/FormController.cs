using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("/submit")]
[Authorize]
public class StudentFormSubmitController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStudents()
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult PostPreferences(StudentPreferencesDto data)
    {
        return Ok();
    }
}
