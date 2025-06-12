using GroupAllocator.DTOs;

namespace GroupAllocator.Database.Model;

public class UserModel
{
	public int Id { get; set; }
	public required bool IsAdmin { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}

public static class UserModelExtensions
{
	public static StudentInfoAndSubmission ToDto(this UserModel model)
	{
		return new StudentInfoAndSubmission
		{
			StudentInfo = model.ToInfoDto(),
			StudentSubmission = model.ToSubmissionDto(),
		};
	}

	public static StudentSubmissionDto ToSubmissionDto(this UserModel model)
	{
		return new StudentSubmissionDto
		{
			WillSignContract = model.StudentModel?.WillSignContract ?? false,
			OrderedPreferences = model.StudentModel?.Preferences.OrderBy(p => p.Strength).Select(p => p.Project.Id).ToList() ?? [],
			Files = model.Files.Select(f => f.ToDto()),
		};
	}
}
