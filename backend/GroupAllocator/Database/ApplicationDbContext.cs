using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Student> Student { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<Preference> Preferences { get; set; }
		public DbSet<SolveRun> SolveRuns { get; set; }
		public DbSet<StudentAssignment> StudentAssignments { get; set; }
	}
}
