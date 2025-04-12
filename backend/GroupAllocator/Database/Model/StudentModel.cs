using System.ComponentModel.DataAnnotations.Schema;
using GroupAllocator.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace GroupAllocator.Database.Model;

[PrimaryKey(nameof(Id))]
public class StudentModel
{
	[ForeignKey(nameof(UserModel.Id))]
	public int Id { get; set; }
	public required bool WillSignContract { get; set; }
	public required UserModel User { get; set; }

    public ICollection<PreferenceModel> Preferences { get; } = new List<PreferenceModel>();
    public ICollection<FileModel> Files { get; } = new List<FileModel>();
}

public static class StudentModelExtensions
{
    public static StudentDto ToDto(this StudentModel model)
    {
        return new StudentDto
        {
            Id = model.Id,
            Email = model.User.Email,
            WillSignContract = model.WillSignContract,
            OrderedPreferences = model.Preferences.OrderBy(p => p.Strength).Select(p => p.Project.Name).ToList(),
            FileNames = model.Files.Select(f => f.Name).ToArray()
        };
    }
}