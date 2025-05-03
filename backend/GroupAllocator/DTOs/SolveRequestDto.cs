using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;


[ExportTsClass]
public class ClientLimitsDto
{
	public required int MinProjects { get; set; }
	public required int MaxProjects { get; set; }
}

[ExportTsClass]
public class SolveRequestDto
{
	public required IEnumerable<AllocationDto> PreAllocations { get; set; }
	public required IEnumerable<ClientLimitsDto> ClientLimits { get; set; }
	public required double PreferenceExponent { get; set; }
}
