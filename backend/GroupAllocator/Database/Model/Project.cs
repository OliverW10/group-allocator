namespace GroupAllocator.Database.Model;

public class Project
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required bool RequiresContract { get; set; }
	public required Client Client { get; set; }
}