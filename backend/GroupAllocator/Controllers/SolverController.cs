using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
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
	public async Task<IActionResult> GetLatest()
	{
		// this logic should really be in a service
		var runs = await db.SolveRuns
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Student)
					.ThenInclude(s => s.User)
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Project)
			.ToListAsync();
		var allProjects = db.Projects.Include(p => p.Client).ToList();

		var lastRun = runs.OrderBy(x => x.Timestamp).FirstOrDefault();
		if (lastRun == null)
		{
			return NotFound();
		}

		var result = new SolveRunDto
		{
			Id = lastRun.Id,
			RanAt = lastRun.Timestamp,
			Projects = allProjects.Select(p => new AllocationDto()
			{
				Project = p.ToDto(),
				Students = lastRun.StudentAssignments
					.Where(a => a.Project == p)
					.Select(a => a.Student.ToInfoDto())
			})
		};
		return Ok(result);
	}

	[HttpPost]
	public async Task<IActionResult> Solve(SolveRequestDto solveConfig)
	{
		var solveRun = new SolveRunModel
		{
			Timestamp = DateTime.UtcNow,
			PreferenceExponent = solveConfig.PreferenceExponent,
		};
		var assignments = solver.AssignStudentsToGroups(solveRun, db.Student.ToList(), db.Projects.ToList(), db.Clients.ToList(), db.Preferences.ToList());

		db.SolveRuns.Add(solveRun);
		db.StudentAssignments.AddRange(assignments);
		await db.SaveChangesAsync();
		return await GetLatest();
	}
}
