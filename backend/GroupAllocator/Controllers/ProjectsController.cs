using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController(ApplicationDbContext db) : ControllerBase
{
	[HttpPost("upload")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<ProjectDto>>> UploadProjects(int classId, [FromForm] IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest("No file uploaded.");
		}

		using var reader = new StreamReader(file.OpenReadStream());
		var @class = db.Classes.Find(classId) ?? throw new InvalidOperationException($"No class with id {classId}");
		var allClients = db.Clients.ToList();
		string? line;
		var header = "project_name,client,min_students,max_students,requires_nda,min_instances,max_instances";
		var expectedCols = header.Split(',').Length;
		while ((line = await reader.ReadLineAsync()) != null)
		{
			if (RemoveWhitespace(line).Equals(header, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}

			var fields = line.Split(',').Select(x => x.Trim()).ToArray();
			if (fields.Length != expectedCols)
			{
				throw new InvalidOperationException("Invalid csv");
			}

			var clientName = fields[1];
			var client = GetOrAddClient(clientName);

			db.Projects.Add(new ProjectModel()
			{
				Name = fields[0],
				Client = client,
				MinStudents = int.Parse(fields[2]),
				MaxStudents = int.Parse(fields[3]),
				RequiresNda = bool.Parse(fields[4]),
				MinInstances = int.Parse(fields[5]),
				MaxInstances = int.Parse(fields[6]),
				Class = @class,
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
				Name = name,
				Class = @class,
			};

			db.Clients.Add(newClient);
			allClients.Add(newClient);
			return newClient;
		}

		return await GetProjects();
	}

	string RemoveWhitespace(string s) => new string(s.Where(c => !Char.IsWhiteSpace(c)).ToArray());

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<List<ProjectDto>>> GetProjects()
	{
		return await db.Projects.Include(p => p.Client).Select(x => x.ToDto()).ToListAsync();
	}

	[HttpPut]
	[Route("update/{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] ProjectDto projectDto)
	{
		var project = await db.Projects
			.Include(p => p.Client)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (project == null)
		{
			return NotFound();
		}

		db.Projects.Update(project);
		await db.SaveChangesAsync();

		return project.ToDto();
	}

	[HttpDelete]
	[Route("delete/{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<ProjectDto>>> DeleteProject(int id)
	{
		var project = await db.Projects
			.Include(p => p.Client)
			.FirstOrDefaultAsync(x => x.Id == id);
		if (project != null)
		{
			db.Projects.Remove(project);
			await db.SaveChangesAsync();
		}
		return await GetProjects();
	}
}
