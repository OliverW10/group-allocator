using TypeGen.Core.TypeAnnotations;

namespace GroupAllocator.DTOs;

[ExportTsClass]
public class StudentInfoAndSubmission
{
	public required StudentInfoDto StudentInfo { get; set; }
	public required StudentSubmissionDto StudentSubmission { get; set; }
}
