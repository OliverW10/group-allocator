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
			Notes = model.Notes
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
