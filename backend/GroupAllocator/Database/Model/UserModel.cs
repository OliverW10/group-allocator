using GroupAllocator.DTOs;

namespace GroupAllocator.Database.Model;

public class UserModel
{
	public int Id { get; set; }
	public required string Role { get; set; } = "student";
	public required string Name { get; set; }
	public required string Email { get; set; }
	public required bool IsVerified { get; set; }

	public StudentModel? StudentModel { get; set; }
	public ICollection<FileModel> Files { get; } = new List<FileModel>();
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

	public static StudentInfoDto ToInfoDto(this UserModel model)
	{
		return new StudentInfoDto
		{
			StudentId = model.Id,
			Name = model.Name,
			Email = model.Email,
			IsVerified = model.IsVerified,
		};
	}
}
