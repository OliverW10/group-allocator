using System.ComponentModel.DataAnnotations.Schema;

namespace GroupAllocator.Database.Model;

public class TeacherModel
{
	public int Id { get; set; }
	[ForeignKey(nameof(StudentModel.Id))] // Share PK with UserModel
	public required UserModel User { get; set; }
}
