using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ProjectAllocationDto
{
    public required int ProjectId { get; set; }
    public required IEnumerable<string> StudentNames { get; set; }
}

[ExportTsClass]
public class SolveRunDto
{
    public required int Id { get; set; }
    public required DateTime RanAt { get; set; }
    public required double Evaluation { get; set; }
    public required IEnumerable<ProjectAllocationDto> Projects { get; set; }
}
