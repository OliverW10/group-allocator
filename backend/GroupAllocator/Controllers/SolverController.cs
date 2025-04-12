using GroupAllocator.Database;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "AdminOnly")]
public class SolverController(IAllocationSolver solver, ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var runs = db.SolveRuns;
        var runsIncludeAssignments = runs.Include(r => r.StudentAssignments);
        runsIncludeAssignments.ThenInclude(sa => sa.Student);
        runsIncludeAssignments.ThenInclude(sa => sa.Project);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Solve()
    {
        solver.AssignStudentsToGroups([], [], [], []);
        return await GetAll();
    }
}
