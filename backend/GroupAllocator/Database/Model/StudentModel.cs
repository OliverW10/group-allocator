using System.ComponentModel.DataAnnotations.Schema;
using GroupAllocator.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace GroupAllocator.Database.Model;

[PrimaryKey(nameof(Id))]
public class StudentModel
{
	public int Id { get; set; }
	[ForeignKey(nameof(StudentModel.Id))] // Share PK with UserModel
	public required UserModel User { get; set; }
	public required ClassModel Class { get; set; }
	public bool? WillSignContract { get; set; }

	public ICollection<PreferenceModel> Preferences { get; } = new List<PreferenceModel>();
	public ICollection<FileModel> Files { get; } = new List<FileModel>();
}
