using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ClassInfoDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required int StudentCount { get; set; }
    public required int StudentsWithPreferencesCount { get; set; }
}
