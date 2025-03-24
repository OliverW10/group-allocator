using Google.OrTools.LinearSolver;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;

namespace GroupAllocator.Services;

/*
 * General info on using OR-Tools in C#: https://developers.google.com/optimization/introduction/dotnet
 * Solving an assignment problem with OR-Tools (very similar to what we are doing): https://developers.google.com/optimization/assignment/assignment_example#c_3
 * I think using mixed integer optimization is most suitable for our problem
 */

public interface IAllocationSolver
{
    IEnumerable<StudentAssignmentModel> AssignStudentsToGroups(IEnumerable<StudentModel> students, IEnumerable<ProjectModel> projects, IEnumerable<ClientModel> clients, IEnumerable<PreferenceModel> preferences);
}

public class AllocationSolver : IAllocationSolver
{
    public IEnumerable<StudentAssignmentModel> AssignStudentsToGroups(IEnumerable<StudentModel> students, IEnumerable<ProjectModel> projects, IEnumerable<ClientModel> clients, IEnumerable<PreferenceModel> preferences)
    {
        // Create "SCIP" Solver

        // Create a 2d array of boolean (0 or 1) 'Variable's
        // each row represents one student and each column is one project
        // and each variable is if that student is assigned to that project

        // Create constraint for each student is only assigned to one project

        // Create constraint for each project has 3, 4 or 0 students (todo: how to forulate constraint with gap?)

        // Create constraint that no student that has said no to a contract is assigned to a project requiring one

        // Create constraint for each client that the number of their projects that have any students is within their bound (todo: how to formulate 'any' in constraint)

        // Create objective function with a coefficient for each preference with a value of the .Strength of the preference
        // might also need to set all other coefficients to zero (idk)

        // Invoke solver for minimization

        // for each variable that has .SolutionValue of 1(ish) create a StudentAssignmentModel

        return new List<StudentAssignmentModel>();
    }
}
