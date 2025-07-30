using GroupAllocator.Database.Model;
using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class TeacherDto
{
    public required string Email { get; set; }
    public required bool IsOwner { get; set; }
} 