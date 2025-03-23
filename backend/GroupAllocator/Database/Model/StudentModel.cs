using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database.Model;

[PrimaryKey(nameof(Id))]
public class StudentModel
{
	[ForeignKey(nameof(UserModel.Id))]
	public int Id { get; set; }
	public required bool WillSignContract { get; set; }
}