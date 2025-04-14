using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentSubmissionDto
{
	public required int Id { get; set; }
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required IEnumerable<int> OrderedPreferences { get; set; }
	public required bool WillSignContract { get; set; }
	public required IEnumerable<FileDetailsDto> Files { get; set; }
	public required bool IsVerified { get; set; }
}
