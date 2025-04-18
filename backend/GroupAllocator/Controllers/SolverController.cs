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
	public async Task<IActionResult> GetAll()
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

		var result = runs.Select(r =>
			new SolveRunDto
			{
				Id = r.Id,
				Evaluation = r.Evaluation,
				RanAt = r.Timestamp,
				Projects = allProjects.Select(p => new AllocationDto()
				{
					Project = p.ToDto(),
					Students = r.StudentAssignments
						.Where(a => a.Project == p)
						.Select(a => a.Student.ToInfoDto()) // fuck you
				})
			}
		);
		return Ok(result);
	}

	[HttpGet("/export/{runId}")]
	public Task<IActionResult> Export(int runId)
	{
		return Task.FromResult<IActionResult>(Ok("TODO: work out export format"));
	}

	[HttpPost]
	public async Task<IActionResult> Solve()
	{
		var solveRun = new SolveRunModel
		{
			Evaluation = -1, // todo: get some metric for how good the result is
			Timestamp = DateTime.UtcNow,
		};
		var assignments = solver.AssignStudentsToGroups(solveRun, db.Student.ToList(), db.Projects.ToList(), db.Clients.ToList(), db.Preferences.ToList());

		db.SolveRuns.Add(solveRun);
		db.StudentAssignments.AddRange(assignments);
		await db.SaveChangesAsync();
		return await GetAll();
	}
}
