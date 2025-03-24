namespace GroupAllocator.Database.Model;

public class ProjectModel
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required bool RequiresContract { get; set; }
	public required ClientModel Client { get; set; }
}