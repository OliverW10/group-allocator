using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class SolveRunDto
{
	public required int Id { get; set; }
	public required DateTime RanAt { get; set; }
	public required IEnumerable<AllocationDto> Projects { get; set; }
	public required IEnumerable<int> Histogram { get; set; }
}
