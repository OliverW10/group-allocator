using TypeGen.Core.TypeAnnotations;
namespace GroupAllocator.DTOs;


[ExportTsClass]
public class UserInfoDto
{
	public required string Name { get; set; }
	public required string Email { get; set; }
	public required AuthRole Role { get; set; }
	public required bool IsAdmin { get; set; }
}

public enum AuthRole
{
	Student, Teacher
}
