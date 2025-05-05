using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentInfoDto
{
	public required int StudentId { get; set; }
	// TODO: student and user id should be the same, make sure that the mapped model for student has fk on its pk
	public required int UserId { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
	public required bool IsVerified { get; set; }
}
