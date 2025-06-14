using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentSubmissionDto
{
	public required IEnumerable<int> OrderedPreferences { get; set; }
	public required bool WillSignContract { get; set; }
	public required IEnumerable<FileDetailsDto> Files { get; set; }
	public required int ClassId { get; set; }
}
