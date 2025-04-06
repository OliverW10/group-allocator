using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ProjectDto
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required bool RequiresContract { get; set; }
}