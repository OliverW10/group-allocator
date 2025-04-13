namespace GroupAllocator.Database.Model;

public class StudentAssignmentModel
{
	public int Id { get; set; }
	public required StudentModel Student { get; set; }
	public required ProjectModel Project { get; set; }
	public required SolveRunModel Run { get; set; }
}