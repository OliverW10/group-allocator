using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentPreferencesDto
{
	public required int[] Preferences { get; set; }
	public required bool WillingToSignContract { get; set; }
	public required string[] FileNames { get; set; }
	public required byte[][] FileBlobs { get; set; }
}
