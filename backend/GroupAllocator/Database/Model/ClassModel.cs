namespace GroupAllocator.Database.Model;

public class ClassModel
{
	public int Id { get; set; }
	public required string Code { get; set; }

	public ICollection<ClassTeacherModel> Teachers { get; set; }
	public ICollection<StudentModel> Students { get; set; }
}
