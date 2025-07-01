using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database.Model;

[PrimaryKey(nameof(Id))]
public class StudentModel
{
	public int Id { get; set; }
	public required UserModel User { get; set; }
	public required ClassModel Class { get; set; }
	public string Notes { get; set; } = "";
	public bool? WillSignContract { get; set; }
	public bool IsVerified { get; set; }

	public ICollection<PreferenceModel> Preferences { get; } = new List<PreferenceModel>();
	public ICollection<FileModel> Files { get; } = new List<FileModel>();
}
