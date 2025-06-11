namespace GroupAllocator.Database.Model;

public class ClientModel
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required ClassModel Class { get; set; }
}
