using GroupAllocator.DTOs;
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

public static class StudentModelExtensions
{
	public static StudentInfoAndSubmission ToDto(this StudentModel model)
	{
		return new StudentInfoAndSubmission
		{
			StudentInfo = model.ToInfoDto(),
			StudentSubmission = model.ToSubmissionDto(),
		};
	}

	public static StudentSubmissionDto ToSubmissionDto(this StudentModel model)
	{
		return new StudentSubmissionDto
		{
			WillSignContract = model.WillSignContract ?? false,
			OrderedPreferences = model.Preferences.OrderBy(p => p.Ordinal).Select(p => p.Project.Id).ToList() ?? [],
			Files = model.Files.Select(f => f.ToDto()),
			ClassId = model.Class.Id,
			Notes = model.Notes,
		};
	}

	public static StudentInfoDto ToInfoDto(this StudentModel model)
	{
		return new StudentInfoDto
		{
			StudentId = model.Id,
			Name = model.User.Name,
			Email = model.User.Email,
			IsVerified = model.IsVerified
		};
	}
}
