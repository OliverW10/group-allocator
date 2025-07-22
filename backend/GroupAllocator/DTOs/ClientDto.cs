using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class ClientDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
} 