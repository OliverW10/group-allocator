namespace GroupAllocator.Database.Model;

public class PreferenceModel
{
	public int Id { get; set; }
	public required double Strength { get; set; }
	public required StudentModel Student { get; set; }
	public required ProjectModel Project { get; set; }
}