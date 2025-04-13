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
	public static StudentSubmissionDto ToDto(this StudentModel model)
	{
		return new StudentSubmissionDto
		{
			Id = model.Id,
			Email = model.User.Email,
			Name = model.User.Name,
			WillSignContract = model.WillSignContract,
			OrderedPreferences = model.Preferences.OrderBy(p => p.Strength).Select(p => p.Project.Id).ToList(),
			FileNames = model.Files.Select(f => f.Name).ToArray()
		};
	}

	public static StudentInfoDto ToInfoDto(this StudentModel model)
	{
		return new StudentInfoDto
		{
			StudentId = model.Id,
			UserId = model.User.Id,
			Name = model.User.Name,
			Email = model.User.Email
		};
	}
}
