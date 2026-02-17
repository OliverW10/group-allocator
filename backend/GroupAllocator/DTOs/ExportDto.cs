using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ExportDto
{
    public required ClassInfoDto ClassInfo { get; set; }
    public required IEnumerable<ProjectDto> Projects { get; set; }
    public required IEnumerable<StudentInfoAndSubmission> Students { get; set; }
}