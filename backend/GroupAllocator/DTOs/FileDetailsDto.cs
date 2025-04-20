
using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class FileDetailsDto
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required int UserId { get; set; }
}
