namespace GroupAllocator.Database.Model;

public class SolveRunModel
{
	public int Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required double PreferenceExponent { get; set; }

	public ICollection<StudentAssignmentModel> StudentAssignments { get; set; } = new List<StudentAssignmentModel>();
}
