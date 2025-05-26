using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;


[ExportTsClass]
public class AllocationDto
{
	public required ProjectDto? Project { get; set; }
	public required IEnumerable<StudentInfoDto> Students { get; set; }
	public required int InstanceId { get; set; }
}
