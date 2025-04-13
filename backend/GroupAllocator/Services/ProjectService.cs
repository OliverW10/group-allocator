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
	Task AddFromCsv(StreamReader csvStream);
}

public class ProjectService(ApplicationDbContext db) : IProjectService
{
    public async Task<ProjectModel?> GetProject(int id)
	{
		return await db.Projects
			.Include(p => p.Client)
			.FirstOrDefaultAsync(x => x.Id == id);
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

    public async Task AddFromCsv(StreamReader csvStream)
    {
		var allClients = db.Clients.ToList();
        string? line;
        while ((line = await csvStream.ReadLineAsync()) != null)
        {
			var fields = line.Split(',').Select(x => x.Trim()).ToArray();
			if (fields.Length != 1)
			{
				throw new InvalidOperationException("Invalid csv");
			}

            // project name, client, min_students, max_students, requires_nda
			var projectName = fields[0];
			var clientName = fields[1];
			var minStudents = int.Parse(fields[2]);
			var maxStudents = int.Parse(fields[3]);
			var requiredContract = bool.Parse(fields[4]);

			var client = GetOrAddClient(clientName);

			db.Projects.Add(new ProjectModel()
			{
				Name = projectName,
				MinStudents = minStudents,
				MaxStudents = maxStudents,
				RequiresNda = requiredContract,
				Client = client
			});
        }

		await db.SaveChangesAsync();

		ClientModel GetOrAddClient(string name)
		{
			var existingClient = allClients.FirstOrDefault(x => x.Name == name);
			if (existingClient is not null)
			{
				return existingClient;
			}

			var newClient = new ClientModel()
			{
				MaxProjects = 99,
				MinProjects = 0,
				Name = name
			};

            db.Clients.Add(newClient);
			allClients.Add(newClient);
			return newClient;
        }
    }
}