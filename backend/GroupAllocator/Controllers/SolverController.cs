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
public class SolverController(IAllocationSolver solver, ApplicationDbContext db, PaymentService paymentService, IUserService userService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<SolveRunDto?>> GetLatest(int classId)
	{
		var runs = await db.SolveRuns
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Student)
					.ThenInclude(s => s.User)
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Student)
					.ThenInclude(s => s.Preferences)
			.Include(r => r.StudentAssignments)
				.ThenInclude(sa => sa.Project)
			.Include(r => r.Class)
			.Include(r => r.ClientLimits)
				.ThenInclude(cl => cl.Client)
			.Where(r => r.Class.Id == classId)
			.ToListAsync();
		var allProjects = await db.Projects.Include(p => p.Client).Include(p => p.Class).Where(p => p.Class.Id == classId).ToListAsync();

		var lastRun = runs.OrderByDescending(x => x.Timestamp).FirstOrDefault();
		if (lastRun == null)
		{
			return Ok(null);
		}

		var result = new SolveRunDto
		{
			Id = lastRun.Id,
			RanAt = lastRun.Timestamp,
			Projects = allProjects.SelectMany(ProjectGroupsForProject),
			Histogram = CalculateHistogram(lastRun.StudentAssignments),
			PreferenceExponent = lastRun.PreferenceExponent,
			ClientLimits = lastRun.ClientLimits.Select(cl => new ClientLimitsDto {
				ClientId = cl.Client.Id,
				MinProjects = cl.MinProjects,
				MaxProjects = cl.MaxProjects,
			}).ToList(),
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

		IEnumerable<int> CalculateHistogram(IEnumerable<StudentAssignmentModel> assignments)
		{
			var histogram = new int[11]; // Assuming max 10 preferences
			
			foreach (var assignment in assignments)
			{
				var student = assignment.Student;
				var project = assignment.Project;
				
				// Find the student's preference rank for this project
				var preference = student.Preferences.FirstOrDefault(p => p.Project.Id == project.Id);
				var hadTop10Preferences = false;
				if (preference != null)
				{
					// Preference rank is 0-based, use directly as index
					var rank = preference.Ordinal;
					if (rank >= 0 && rank < histogram.Length)
					{
						histogram[rank]++;
						hadTop10Preferences = true;
					}
				}
				// Note: If no preference is found, the student was assigned to a project
				// not in their top 10 preferences
				if (!hadTop10Preferences && student.Preferences.Count > 0)
				{
					histogram[10]++;
				}
			}
			
			return histogram;
		}
	}

	[HttpPost]
	public async Task<ActionResult<SolveRunDto?>> Solve(SolveRequestDto solveConfig)
	{
		if (!await userService.IsCurrentTeacherPartOfClass(solveConfig.ClassId, User))
		{
			return Forbid("Teacher is not in this class");
		}

		var @class = await db.Classes
			.Include(c => c.Students)
			.Include(c => c.Projects)
			.Include(c => c.Clients)
			.Include(c => c.Payments)
			.FirstOrDefaultAsync(c => c.Id == solveConfig.ClassId);
		if (@class == null)
		{
			return BadRequest("Class not found");
		}

		var plan = paymentService.GetPaymentPlanForClass(@class);
		var paymentEnabled = false;
		if (paymentEnabled && plan == PaymentPlan.None && @class.Students.Count > 20)
		{
			return StatusCode(402, "Upgrade required: Free plan classes are limited to 20 students for solver runs.");
		}

		var solveRun = new SolveRunModel
		{
			Timestamp = DateTime.UtcNow,
			PreferenceExponent = solveConfig.PreferenceExponent,
			Class = @class,
		};

		var assignments = solver.AssignStudentsToGroups(solveRun,
			@class.Students.ToList(),
			@class.Projects.ToList(),
			@class.Clients.ToList(),
			db.Preferences.Include(p => p.Student).Include(p => p.Project).Where(p => p.Project.Class.Id == solveConfig.ClassId).ToList(),
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
		db.ClientLimits.AddRange(solveConfig.ClientLimits.Select(cl => new ClientLimitModel {
			Client = db.Clients.First(c => c.Id == cl.ClientId),
			MinProjects = cl.MinProjects,
			MaxProjects = cl.MaxProjects,
			SolveRun = solveRun,
		}));
		await db.SaveChangesAsync();
		return await GetLatest(solveConfig.ClassId);
	}
}
