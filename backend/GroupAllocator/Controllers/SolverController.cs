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
[Authorize(Policy = "TeacherOnly")]
public class SolverController(IAllocationSolver solver, ApplicationDbContext db) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<SolveRunDto>> GetLatest(int classId)
	{
		var runs = await db.SolveRuns
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Student)
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Project)
			.Include(r => r.Class)
			.Where(r => r.Class.Id == classId)
			.ToListAsync();
		var allProjects = await db.Projects.Include(p => p.Client).Include(p => p.Class).Where(p => p.Class.Id == classId).ToListAsync();

		var lastRun = runs.OrderByDescending(x => x.Timestamp).FirstOrDefault();
		if (lastRun == null)
		{
			return NotFound();
		}

		var result = new SolveRunDto
		{
			Id = lastRun.Id,
			RanAt = lastRun.Timestamp,
			Projects = allProjects.SelectMany(ProjectGroupsForProject),
		};
		return result;

		IEnumerable<AllocationDto> ProjectGroupsForProject(ProjectModel p)
		{
			var assignmentsToThisProject = lastRun.StudentAssignments.Where(a => a.Project.Id == p.Id);
			var groupNumbers = assignmentsToThisProject.Select(a => a.GroupInstanceId).Distinct();
			return groupNumbers.Select(groupId =>
				new AllocationDto()
				{
					Project = p.ToDto(),
					Students = assignmentsToThisProject
						.Where(a => a.GroupInstanceId == groupId)
						.Select(a => a.Student.ToInfoDto()),
					InstanceId = groupId,
				}
			);
		};
	}

	[HttpPost]
	public async Task<ActionResult<SolveRunDto>> Solve(SolveRequestDto solveConfig)
	{
		var @class = await db.Classes.FindAsync(solveConfig.ClassId);
		if (@class == null)
		{
			return BadRequest("Class not found");
		}

		var solveRun = new SolveRunModel
		{
			Timestamp = DateTime.UtcNow,
			PreferenceExponent = solveConfig.PreferenceExponent,
			Class = @class
		};

		var assignments = solver.AssignStudentsToGroups(solveRun,
			db.Users.ToList(),
			db.Projects.ToList(),
			db.Clients.ToList(),
			db.Preferences.Include(p => p.Student).Include(p => p.Project).ToList(),
			solveConfig.PreAllocations.ToList(),
			solveConfig.ClientLimits.ToList(),
			solveConfig.PreferenceExponent
		).ToList();

		if (!assignments.Any())
		{
			return BadRequest("Solver failed to find a solution");
		}

		solveRun.StudentAssignments = assignments;
		db.SolveRuns.Add(solveRun);
		db.StudentAssignments.AddRange(assignments);
		await db.SaveChangesAsync();
		return await GetLatest(solveConfig.ClassId);
	}
}
