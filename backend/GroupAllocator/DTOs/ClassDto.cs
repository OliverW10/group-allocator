using GroupAllocator.Database.Model;
using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ClassDto
{
    public required string Name { get; set; }
}

[ExportTsClass]
public class ClassResponseDto
{
    public required int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required ClassTeacherRole? TeacherRole { get; set; }
} 
