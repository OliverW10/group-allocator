namespace GroupAllocator.Database.Model;

public class ClassModel
{
	public int Id { get; set; }
	public required string Code { get; set; }
	public required string Name { get; set; }
	public required DateTimeOffset CreatedAt { get; set; }

	public ICollection<ClassTeacherModel> Teachers { get; set; } = null!;
	public ICollection<StudentModel> Students { get; set; } = null!;
	public ICollection<ProjectModel> Projects { get; set; } = null!;
	public ICollection<ClientModel> Clients{ get; set; } = null!;
	public ICollection<PaymentModel> Payments { get; set; } = null!;
}
