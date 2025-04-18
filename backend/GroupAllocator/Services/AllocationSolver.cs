using Google.OrTools.LinearSolver;
using GroupAllocator.Database.Model;

namespace GroupAllocator.Services;

/*
 * General info on using OR-Tools in C#: https://developers.google.com/optimization/introduction/dotnet
 * Solving an assignment problem with OR-Tools (very similar to what we are doing): https://developers.google.com/optimization/assignment/assignment_example#c_3
 * I think using mixed integer optimization is most suitable for our problem
 */

public interface IAllocationSolver
{
	IEnumerable<StudentAssignmentModel> AssignStudentsToGroups(SolveRunModel solveRun, IEnumerable<StudentModel> students, IEnumerable<ProjectModel> projects, IEnumerable<ClientModel> clients, IEnumerable<PreferenceModel> preferences);
}

public class AllocationSolver : IAllocationSolver
{
	public IEnumerable<StudentAssignmentModel> AssignStudentsToGroups(SolveRunModel solveRun, IEnumerable<StudentModel> students, IEnumerable<ProjectModel> projects, IEnumerable<ClientModel> clients, IEnumerable<PreferenceModel> preferences)
	{
		// Create "SCIP" Solver
		Solver solver = Solver.CreateSolver("SCIP");
		if (solver == null)
		{
			throw new Exception("Could not create solver");
		}

		var studentList = students.ToList();
		var projectList = projects.ToList();
		var variables = new Dictionary<(int studentId, int projectId), Variable>();
		var projectActivityMap = new Dictionary<int, Variable>();



		// Create a 2d array of boolean (0 or 1) 'Variable's
		// each row represents one student and each column is one project
		// and each variable is if that student is assigned to that project
		foreach (var student in studentList)
		{
			foreach (var project in projectList)
			{

				//don't aissign variable if student won't sign contract and project needs one
				if (!student.WillSignContract && project.RequiresNda)
				{
					continue;
				}

				//makes variable
				variables[(student.Id, project.Id)] = solver.MakeIntVar(0, 1, $"x_{student.Id}_{project.Id}");

			}
		}



		// Create constraint for each student is only assigned to one project
		foreach (var student in studentList)
		{

			//creates a list of variables based on the projects for specific student
			var terms = projectList
				.Where(p => variables.ContainsKey((student.Id, p.Id)))
				.Select(p => variables[(student.Id, p.Id)]);

			if (terms.Any())
			{
				//sum of variables must be equal to 1 (student can only do 1 project)
				Constraint constraint = solver.MakeConstraint(1, 1, $"student_{student.Id}_assignment");

				foreach (var variable in terms)
				{
					constraint.SetCoefficient(variable, 1);
				}
			}

		}


		// Project constraint for the amount of students allowed in groups
		foreach (var project in projectList)
		{

			//list of student variables for specific project
			var assignedVars = studentList
				.Where(s => variables.ContainsKey((s.Id, project.Id)))
				.Select(s => variables[(s.Id, project.Id)])
				.ToList();

			//variable that will represent number of students assigned to projects
			//on creation the max amount is all students but this will be narrowed down later
			var countVar = solver.MakeIntVar(0, studentList.Count, $"count_proj_{project.Id}");

			LinearExpr sumExpr = assignedVars.First();

			//this creates an equation of the variables added togther for a specific project
			foreach (var v in assignedVars.Skip(1))
			{
				sumExpr += v;
			}

			//this countVar will track the amount students are assigned to a project based on how many variables in the sumExpr are 1
			solver.Add(countVar == sumExpr);


			//variable for whether or not project is running
			var isActive = solver.MakeIntVar(0, 1, $"isActive_{project.Id}");

			//if the project isActive then the countVar must be between the min and max (inclusive)
			//if isActive is 0 then the countVar will be 0 as this project isn't running so it has no students
			solver.Add(isActive * project.MinStudents <= countVar);
			solver.Add(countVar <= isActive * project.MaxStudents);


			//this will track what projects are active
			projectActivityMap[project.Id] = isActive;

		}

            //had to add this because for some reason real data exanple has clients that don't have projects 
            //this probably will be deleted because i think if a client exists they should be able to run a project
            if (clientProjects.Count() == 0)
            {
                continue;
            }


		//Client project limits
		foreach (var client in clients)
		{
			//list of projects for specific client
			var clientProjects = projectList
				.Where(p => p.Client == client)
				.Select(p => projectActivityMap[p.Id])
				.ToList();


			LinearExpr projectCountExpr = clientProjects.First();

			//this makes an equation which is the isActive variables added together
			foreach (var v in clientProjects.Skip(1))
			{
				projectCountExpr += v;
			}

			//the sets the bounds for the amount of isActive variables in the projectCountExpr equation that can be 1
			solver.Add(projectCountExpr >= client.MinProjects);
			solver.Add(projectCountExpr <= client.MaxProjects);

		}

		//Preference-based objective function
		Objective objective = solver.Objective();

		foreach (var pref in preferences)
		{
			var key = (pref.Student.Id, pref.Project.Id);

			if (variables.ContainsKey(key))
			{
				//this sets the coefficients for the variables based on strengths
				objective.SetCoefficient(variables[key], pref.Strength);
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
		foreach (var ((studentId, projectId), variable) in variables)
		{

			//if the variable has assigned student to project
			if (variable.SolutionValue() > 0.5)
			{
				//assign student to first student in student list where the id matches with the studentId we are iterated to
				var student = studentList.First(s => s.Id == studentId);
				var project = projectList.First(p => p.Id == projectId);

				assignments.Add(new StudentAssignmentModel
				{
					Student = student,
					Project = project,
					Run = solveRun
				});

			}

		}

		return assignments;
	}
}
