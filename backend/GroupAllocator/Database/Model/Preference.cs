namespace GroupAllocator.Database.Model;

public class Preference
{
	public required int Id { get; set; }
	public required double Strength { get; set; }
	public required Student Student { get; set; }
	public required Project Project { get; set; }
}