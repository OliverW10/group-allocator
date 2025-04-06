using GroupAllocator.DTOs;

namespace GroupAllocator.Database.Model;

public class ProjectModel
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required bool RequiresNda { get; set; }
	public required ClientModel? Client { get; set; }
	public required int MinStudents { get; set; }
	public required int MaxStudents { get; set; }
}
	public string Description { get; set; } = string.Empty;
	public required bool RequiresContract { get; set; }
	public required ClientModel Client { get; set; }
}

public static class ProjectModelExtensions
{
	public static ProjectDto ToDto(this ProjectModel model)
	{
		return new ProjectDto
		{
			Id = model.Id,
			Name = model.Name,
			Description = model.Client.Name,
			RequiresContract = model.RequiresContract
		};
	}
}