
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public class ProjectsService(ApplicationDbContext db)
{
	public async Task<List<ProjectDto>> GetProjects(int classId)
    {
        return await db.Projects.Include(p => p.Client)
			.Include(p => p.Class)
			.Where(x => x.Class.Id == classId)
			.OrderBy(x => x.Name)
			.Select(x => x.ToDto())
			.ToListAsync();
    }

    public async Task<ProjectDto?> CreateProject(int classId, CreateProjectDto createProjectDto)
    {
        
		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
			.FirstOrDefaultAsync(c => c.Id == classId);
			
		if (@class == null)
		{
			return null;
		}

		// Get or create client
		var client = await db.Clients.FirstOrDefaultAsync(c => c.Name == createProjectDto.Client && c.Class.Id == classId);
		if (client == null)
		{
			client = new ClientModel
			{
				Name = createProjectDto.Client,
				Class = @class,
			};
			db.Clients.Add(client);
		}

		var project = new ProjectModel
		{
			Name = createProjectDto.Name,
			Client = client,
			MinStudents = createProjectDto.MinStudents,
			MaxStudents = createProjectDto.MaxStudents,
			RequiresNda = createProjectDto.RequiresNda,
			MinInstances = createProjectDto.MinInstances,
			MaxInstances = createProjectDto.MaxInstances,
			Class = @class,
		};

		db.Projects.Add(project);
		await db.SaveChangesAsync();

        return project.ToDto();
    }
}