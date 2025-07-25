using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ProjectDto
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required string Client { get; set; }
	public required bool RequiresNda { get; set; }
	public required int MinStudents { get; set; }
	public required int MaxStudents { get; set; }
	public required int MinInstances { get; set; }
	public required int MaxInstances { get; set; }
}

[ExportTsClass]
public class CreateProjectDto
{
	public required string Name { get; set; }
	public required string Client { get; set; }
	public required bool RequiresNda { get; set; }
	public required int MinStudents { get; set; }
	public required int MaxStudents { get; set; }
	public required int MinInstances { get; set; }
	public required int MaxInstances { get; set; }
}
