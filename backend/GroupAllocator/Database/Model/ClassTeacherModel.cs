namespace GroupAllocator.Database.Model;

public enum ClassTeacherRole
{
	Owner, NonOwner
}

public class ClassTeacherModel
{
	public int Id { get; set; }
	public required TeacherModel Teacher { get; set; }
	public required ClassModel Class { get; set; }
	public required ClassTeacherRole Role { get; set; }
}
