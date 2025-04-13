using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentInfoDto
{
	public required int StudentId { get; set; }
	public required int UserId { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}
