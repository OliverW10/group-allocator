namespace GroupAllocator.Database.Model;

public class Client
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required int MinProjects { get; set; }
	public required int MaxProjects { get; set; }
}