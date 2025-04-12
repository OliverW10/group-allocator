using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentDto
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public required IEnumerable<int> OrderedPreferences { get; set; }
    public required bool WillSignContract { get; set; }
    public required IEnumerable<string> FileNames { get; set; }
}
