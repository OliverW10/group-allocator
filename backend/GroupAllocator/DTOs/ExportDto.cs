namespace GroupAllocator.DTOs;

public class ExportDto
{
    public required ClassInfoDto ClassInfo { get; set; }
    public required IEnumerable<ProjectDto> Projects { get; set; }
    public required IEnumerable<StudentInfoAndSubmission> Students { get; set; }
}