namespace GroupAllocator.Database.Model;

public class ProjectPreAssignmentModel
{
	public int Id { get; set; }
	public required ProjectModel? Project { get; set; }
	public required SolveRunModel Run { get; set; }

	public ICollection<StudentAssignmentModel> Students { get; set; } = new List<StudentAssignmentModel>();
}
