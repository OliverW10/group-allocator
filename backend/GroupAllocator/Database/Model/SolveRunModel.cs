namespace GroupAllocator.Database.Model;

public class SolveRunModel
{
	public int Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required double PreferenceExponent { get; set; }

	public ICollection<ProjectPreAssignmentModel> ProjectPreAssignments { get; set; } = new List<ProjectPreAssignmentModel>();
	public ICollection<StudentAssignmentModel> StudentAssignments { get; set; } = new List<StudentAssignmentModel>();
}
