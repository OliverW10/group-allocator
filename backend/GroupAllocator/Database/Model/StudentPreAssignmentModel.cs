namespace GroupAllocator.Database.Model;

public class StudentPreAssignmentModel
{
	public int Id { get; set; }
	public required StudentModel Student { get; set; }
	public required ProjectPreAssignmentModel Project { get; set; }
}
