namespace GroupAllocator.Database.Model;

public class SolveRunModel
{
	public required int Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required double Evaluation { get; set; }

	public ICollection<StudentAssignmentModel> StudentAssignments { get; set; }	= new List<StudentAssignmentModel>();
}