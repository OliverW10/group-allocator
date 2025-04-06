using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IProjectService
{
	Task<ProjectModel?> GetProject(int id);
	Task<List<ProjectModel>> GetProjects();
	Task<ProjectModel> AddProject(ProjectModel project);
	Task<ProjectModel> UpdateProject(ProjectModel project);
	Task DeleteProject(int id);
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

	public async Task<ProjectModel> AddProject(ProjectModel project)
	{
		db.Projects.Add(project);
		await db.SaveChangesAsync();
		return project;
	}

	public async Task<ProjectModel> UpdateProject(ProjectModel project)
	{
		db.Projects.Update(project);
		await db.SaveChangesAsync();
		return project;
	}

	public async Task DeleteProject(int id)
	{
		var project = await GetProject(id);
		if (project != null)
		{
			db.Projects.Remove(project);
			await db.SaveChangesAsync();
		}
	}
}