using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ClassInfoDto
{
    public required string Code { get; set; }
    public int StudentCount { get; set; }
    public int StudentsWithPreferencesCount { get; set; }
}
