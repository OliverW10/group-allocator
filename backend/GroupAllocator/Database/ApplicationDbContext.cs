using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
	{
		public DbSet<UserModel> Users { get; set; }
		public DbSet<StudentModel> Student { get; set; }
		public DbSet<ClientModel> Clients { get; set; }
		public DbSet<ProjectModel> Projects { get; set; }
		public DbSet<PreferenceModel> Preferences { get; set; }
		public DbSet<SolveRunModel> SolveRuns { get; set; }
		public DbSet<StudentAssignmentModel> StudentAssignments { get; set; }
		public DbSet<FileModel> Files { get; set; }
	}
}
