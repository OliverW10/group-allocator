using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "AdminOnly")]
public class SolverController(IAllocationSolver solver) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Solve()
    {
        solver.AssignStudentsToGroups([], [], [], []);
        return await GetAll();
    }
}
