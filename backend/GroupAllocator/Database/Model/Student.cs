using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database.Model;

[PrimaryKey(nameof(Id))]
public class Student
{
	[ForeignKey(nameof(User.Id))]
	public int Id { get; set; }
	public required bool WillSignContract { get; set; }
}