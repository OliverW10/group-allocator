namespace GroupAllocator.Database.Model;

public class StudentAssignment
{
	public required int Id { get; set; }
	public required Student Student { get; set; }
	public required Project Project { get; set; }
	public required SolveRun Run { get; set; }
}