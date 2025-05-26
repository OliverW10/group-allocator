using Google.OrTools.LinearSolver;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace GroupAllocator.Services;

/*
 * General info on using OR-Tools in C#: https://developers.google.com/optimization/introduction/dotnet
 * Solving an assignment problem with OR-Tools (very similar to what we are doing): https://developers.google.com/optimization/assignment/assignment_example#c_3
 * I think using mixed integer optimization is most suitable for our problem
 */

public interface IAllocationSolver
{
	IEnumerable<StudentAssignmentModel> AssignStudentsToGroups(SolveRunModel solveRun,
		IEnumerable<UserModel> users,
		IEnumerable<ProjectModel> projects,
		IEnumerable<ClientModel> clients,
		IEnumerable<PreferenceModel> preferences,
		IReadOnlyCollection<AllocationDto> manualAllocations,
		IReadOnlyCollection<ClientLimitsDto> clientLimits,
		double preferenceExponent);
}

record ProjectInstance(ProjectModel Project, int GroupInstanceId);

public class AllocationSolver : IAllocationSolver
{
	public IEnumerable<StudentAssignmentModel> AssignStudentsToGroups(SolveRunModel solveRun,
		IEnumerable<UserModel> users,
		IEnumerable<ProjectModel> projects,
		IEnumerable<ClientModel> clients,
		IEnumerable<PreferenceModel> preferences,
		IReadOnlyCollection<AllocationDto> manualAllocations,
		IReadOnlyCollection<ClientLimitsDto> clientLimits,
		double preferenceExponent)
	{
		// Create "SCIP" Solver
		Solver solver = Solver.CreateSolver("SCIP");
		if (solver == null)
		{
			throw new Exception("Could not create solver");
		}

		var studentList = users.Where(u => !u.IsAdmin).ToList();
		var projectList = projects.Select(x => new ProjectInstance(x, 0)).ToList();
		foreach (var proj in projects)
		{
			if (proj.MaxInstances > 1)
			{
				for (int i = 1; i < proj.MaxInstances; i++)
				{
					projectList.Add(new ProjectInstance(proj, i));
				}
			}
		}
		var variables = new Dictionary<(int studentId, int projectId, int instanceId), Variable>();
		var projectActivityMap = new Dictionary<(int projectId, int instanceId), Variable>();


		// Create a 2d array of boolean (0 or 1) 'Variable's
		// each row represents one student and each column is one project
		// and each variable is if that student is assigned to that project
		foreach (var student in studentList)
		{
			foreach (var project in projectList)
			{
				//makes variable
				variables[(student.Id, project.Project.Id, project.GroupInstanceId)] = solver.MakeIntVar(0, 1, $"x_{student.Id}_{project.Project.Id}");
			}
		}

		// Create constraint for each student is only assigned to one project
		foreach (var student in studentList)
		{

			// variable for each project for this specific student
			var terms = projectList
				.Select(p => variables[(student.Id, p.Project.Id, p.GroupInstanceId)]);

			var manuallyAssignedAllocation = manualAllocations.FirstOrDefault(a => a.Students.Any(s => s.StudentId == student.Id));
			// create constraints that the student does the assigned project
			if (manuallyAssignedAllocation?.Project is not null)
			{
				var manualAssignmentConstraint = solver.MakeConstraint(1, 1, $"student_{student.Id}_manual_assignment");
				manualAssignmentConstraint.SetCoefficient(variables[(student.Id, manuallyAssignedAllocation.Project.Id, manuallyAssignedAllocation.InstanceId)], 1);
			}

			//sum of variables must be equal to 1 (student can only do 1 project)
			Constraint constraint = solver.MakeConstraint(1, 1, $"student_{student.Id}_single_assignment");
			foreach (var variable in terms)
			{
				constraint.SetCoefficient(variable, 1);
			}
		}

		// Constraint for students who are manually assigned together without a project
		foreach (var allocation in manualAllocations)
		{
			if (allocation.Project is null && allocation.Students.Count() > 0)
			{
				var firstStudentId = allocation.Students.First().StudentId;
				// having students[0] == 1 implies that students[1..] should also be 1 for all projects
				foreach (var project in projectList)
				{
					// count of the students allocated together that are on this project
					LinearExpr sumExpr = new LinearExpr();
					// the variables for all students in this project
					var projectStudentVariables = variables
						.Where(v => v.Key.projectId == project.Project.Id && v.Key.instanceId == project.GroupInstanceId)
						.Select(v => v.Value);
					foreach (var v in projectStudentVariables)
					{
						sumExpr += v;
					}
					var countVar = solver.MakeIntVar(0, studentList.Count, $"count_manual_{firstStudentId}_proj_{project.Project.Id}");
					// TODO: do we even need this countVar?
					solver.Add(countVar == sumExpr);

					var firstStudentVariable = variables[(firstStudentId, project.Project.Id, project.GroupInstanceId)];
					solver.Add(countVar == firstStudentVariable * allocation.Students.Count());
				}
			}
		}

		// Project constraint for the amount of students allowed in groups
		foreach (var project in projectList)
		{
			//list of student variables for specific project
			var assignedVars = studentList
				// .Where(s => variables.ContainsKey((s.Id, project.Project.Id, project.GroupInstanceId)))
				.Select(s => variables[(s.Id, project.Project.Id, project.GroupInstanceId)])
				.ToList();

			//variable that will represent number of students assigned to projects
			//on creation the max amount is all students but this will be narrowed down later
			var countVar = solver.MakeIntVar(0, studentList.Count, $"count_proj_{project.Project.Id}");

			LinearExpr sumExpr = new LinearExpr();

			//this creates an equation of the variables added togther for a specific project
			foreach (var v in assignedVars)
			{
				sumExpr += v;
			}

			//this countVar will track the amount students are assigned to a project based on how many variables in the sumExpr are 1
			solver.Add(countVar == sumExpr);


			//variable for whether or not project is running
			var isActive = solver.MakeIntVar(0, 1, $"isActive_{project.Project.Id}");

			var isManuallyAssigned = manualAllocations.Any(a => a?.Project?.Id == project.Project.Id);
			if (isManuallyAssigned)
			{
				solver.Add(isActive == 1);
			}

			//if the project isActive then the countVar must be between the min and max (inclusive)
			//if isActive is 0 then the countVar will be 0 as this project isn't running so it has no students
			solver.Add(countVar >= isActive * project.Project.MinStudents);
			solver.Add(countVar <= isActive * project.Project.MaxStudents);


			//this will track what projects are active
			projectActivityMap[(project.Project.Id, project.GroupInstanceId)] = isActive;
		}

		// Project min instance number constraint
		foreach (var project in projects)
		{
			// constrain count of project instances with isActive to be >= minInstaces
			var instanceCountExr = new LinearExpr();
			foreach (var projInstance in projectList.Where(p => p.Project.Id == project.Id))
			{
				var isActiveVariable = projectActivityMap.GetValueOrDefault((projInstance.Project.Id, projInstance.GroupInstanceId));
			}
		}


		// Client project limits
		foreach (var client in clients)
		{
			var clientLimit = clientLimits.FirstOrDefault(cl => cl.ClientId == client.Id);
			if (clientLimit == null)
			{
				continue;
			}

			//list of projects for specific client
			var clientProjects = projectList
				.Where(p => p.Project.Client == client)
				.Select(p => projectActivityMap[(p.Project.Id, p.GroupInstanceId)])
				.ToList();

			//had to add this because for some reason real data exanple has clients that don't have projects 
			//this probably will be deleted because i think if a client exists they should be able to run a project
			if (clientProjects.Count() == 0)
			{
				continue;
			}


			LinearExpr projectCountExpr = clientProjects.First();

			//this makes an equation which is the isActive variables added together
			foreach (var v in clientProjects.Skip(1))
			{
				projectCountExpr += v;
			}

			//the sets the bounds for the amount of isActive variables in the projectCountExpr equation that can be 1
			solver.Add(projectCountExpr >= clientLimit.MinProjects);
			solver.Add(projectCountExpr <= clientLimit.MaxProjects);
		}

		// Project instance number constraint
		foreach (var project in projects)
		{
			var projectInstancesExpr = new LinearExpr();
			foreach (var projInstance in projectList.Where(p => p.Project.Id == project.Id))
			{
				var isActiveVariable = projectActivityMap.GetValueOrDefault((projInstance.Project.Id, projInstance.GroupInstanceId));
				if (isActiveVariable != null)
				{
					projectInstancesExpr += isActiveVariable;
				}
			}

			solver.Add(projectInstancesExpr >= project.MinInstances);
			solver.Add(projectInstancesExpr <= project.MaxInstances);
		}

		//Preference-based objective function
		Objective objective = solver.Objective();

		foreach (var pref in preferences)
		{
			foreach (var projectInstance in projectList.Where(x => x.Project.Id == pref.Project.Id))
			{
				var key = (pref.Student.Id, pref.Project.Id, projectInstance.GroupInstanceId);

				if (variables.TryGetValue(key, out var variable))
				{
					//this sets the coefficients for the variables based on strengths
					objective.SetCoefficient(variable, pref.Strength);
				}
			}

		}

		//Invoke solver for maximation
		objective.SetMaximization();

		//Solve the model
		var resultStatus = solver.Solve();

		//if no solution return empty list
		if (resultStatus != Solver.ResultStatus.OPTIMAL && resultStatus != Solver.ResultStatus.FEASIBLE)
		{
			Console.WriteLine("solver failed: " + resultStatus);
			; return new List<StudentAssignmentModel>();
		}

		//if there is a solution

		var assignments = new List<StudentAssignmentModel>();

		//loops through dictionary of variables
		foreach (var ((studentId, projectId, instanceNum), variable) in variables)
		{

			//if the variable has assigned student to project
			if (variable.SolutionValue() > 0.5)
			{
				//assign student to first student in student list where the id matches with the studentId we are iterated to
				var student = studentList.First(s => s.Id == studentId);
				var project = projectList.First(p => p.Project.Id == projectId);

				assignments.Add(new StudentAssignmentModel
				{
					Student = student,
					Project = project.Project,
					Run = solveRun,
					GroupInstanceId = instanceNum,
				});

			}

		}

		return assignments;
	}
}
