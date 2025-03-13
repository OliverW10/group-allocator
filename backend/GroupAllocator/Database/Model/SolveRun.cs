namespace GroupAllocator.Database.Model;

public class SolveRun
{
	public required int Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required double Evaluation { get; set; }
}