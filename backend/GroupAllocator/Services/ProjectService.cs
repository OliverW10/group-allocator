using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IProjectService
{
	Task<ProjectModel?> GetProject(int id);
	Task<List<ProjectModel>> GetProjects();
}

public class ProjectService : IProjectService
{
	public ProjectService(ApplicationDbContext db)
	{
		this.db = db;
	}

	private readonly ApplicationDbContext db;

	public async Task<ProjectModel?> GetProject(int id)
	{
		return await db.Projects.FirstOrDefaultAsync(x => x.Id == id);
	}

	public async Task<List<ProjectModel>> GetProjects()
	{
		return await db.Projects.ToListAsync();
	}
}