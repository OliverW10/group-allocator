using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Database
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
	{
	}
}
