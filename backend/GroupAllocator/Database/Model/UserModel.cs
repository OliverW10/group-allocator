namespace GroupAllocator.Database.Model;

public class UserModel
{
	public int Id { get; set; }
	public required bool IsAdmin { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}
