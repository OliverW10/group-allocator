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
    public IActionResult GetPreferences()
    {
        return Ok(new StudentPreferencesDto);
    }

    [HttpPost]
    public IActionResult PostPreferences(StudentPreferencesDto data)
    {
        return Ok();
    }
}
