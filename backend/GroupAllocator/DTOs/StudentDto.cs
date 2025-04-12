using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentDto
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public required IEnumerable<string> OrderedPreferences { get; set; }
    public required bool WillSignContract { get; set; }
}
