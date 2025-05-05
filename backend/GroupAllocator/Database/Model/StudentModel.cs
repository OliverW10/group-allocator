using System.ComponentModel.DataAnnotations.Schema;
using GroupAllocator.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace GroupAllocator.Database.Model;

[PrimaryKey(nameof(Id))]
public class StudentModel
{
	public int Id { get; set; }
	public required bool WillSignContract { get; set; }
	[ForeignKey(nameof(StudentModel.Id))]
	public required UserModel User { get; set; }
	public ICollection<PreferenceModel> Preferences { get; } = new List<PreferenceModel>();
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
			WillSignContract = model.WillSignContract,
			OrderedPreferences = model.Preferences.OrderBy(p => p.Strength).Select(p => p.Project.Id).ToList(),
			Files = model.User.Files.Select(f => f.ToDto()),
		};
	}

	public static StudentInfoDto ToInfoDto(this StudentModel model)
	{
		return new StudentInfoDto
		{
			StudentId = model.Id,
			Name = model.User.Name,
			Email = model.User.Email,
			IsVerified = model.User.IsVerified,
		};
	}
}
