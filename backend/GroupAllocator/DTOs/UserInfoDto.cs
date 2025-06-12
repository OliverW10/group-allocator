using TypeGen.Core.TypeAnnotations;
namespace GroupAllocator.DTOs;

[ExportTsClass]
public class UserInfoDto
{
	public required string Role { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
}
