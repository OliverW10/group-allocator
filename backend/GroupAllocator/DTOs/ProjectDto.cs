using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ProjectDto
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required bool RequiresNda { get; set; }
	public required int MinStudents { get; set; }
	public required int MaxStudents { get; set; }
}