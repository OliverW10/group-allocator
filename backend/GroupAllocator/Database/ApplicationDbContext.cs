using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<ClassModel> Classes { get; set; } = null!;
	public DbSet<ClassTeacherModel> ClassTeachers { get; set; } = null!;
	public DbSet<ClientModel> Clients { get; set; } = null!;
	public DbSet<FileModel> Files { get; set; } = null!;
	public DbSet<PaymentModel> Payments { get; set; } = null!;
	public DbSet<PreferenceModel> Preferences { get; set; } = null!;
	public DbSet<ProjectModel> Projects { get; set; } = null!;
	public DbSet<SolveRunModel> SolveRuns { get; set; } = null!;
	public DbSet<StudentAssignmentModel> StudentAssignments { get; set; } = null!;
	public DbSet<StudentModel> Students { get; set; } = null!;
	public DbSet<UserModel> Users { get; set; } = null!;
}
