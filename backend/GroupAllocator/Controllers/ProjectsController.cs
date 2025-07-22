using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

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

		return await GetProjects(classId);
	}

	[HttpPost]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ProjectDto>> CreateProject([FromQuery, BindRequired] int classId, [FromBody] CreateProjectDto createProjectDto)
	{
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		
		var @class = await db.Classes
			.Include(c => c.Teachers)
				.ThenInclude(t => t.Teacher)
			.FirstOrDefaultAsync(c => c.Id == classId);
			
		if (@class == null)
		{
			return NotFound($"No class with id {classId}");
		}

		// Check if the teacher is the owner of the class
		var isTeacher = @class.Teachers.Any(t => t.Teacher.Id == userId);
		if (!isTeacher)
		{
			return Forbid("Only the class owner can create projects");
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

	string RemoveWhitespace(string s) => new string(s.Where(c => !Char.IsWhiteSpace(c)).ToArray());

	[HttpGet]
	[Authorize]
	public async Task<ActionResult<List<ProjectDto>>> GetProjects([FromQuery, BindRequired] int classId)
	{
		return await db.Projects.Include(p => p.Client).Include(p => p.Class).Where(x => x.Class.Id == classId).Select(x => x.ToDto()).ToListAsync();
	}

	[HttpGet("clients")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<List<ClientDto>>> GetClients([FromQuery, BindRequired] int classId)
	{
		var clients = await db.Clients
			.Where(c => c.Class.Id == classId)
			.Select(c => new ClientDto { Id = c.Id, Name = c.Name })
			.ToListAsync();
		return clients;
	}

	[HttpPut]
	[Route("update/{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] ProjectDto projectDto)
	{
		var project = await db.Projects
			.Include(p => p.Client)
			.Include(p => p.Class)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (project == null)
		{
			return NotFound();
		}

		// Update project properties
		project.Name = projectDto.Name;
		project.RequiresNda = projectDto.RequiresNda;
		project.MinStudents = projectDto.MinStudents;
		project.MaxStudents = projectDto.MaxStudents;
		project.MinInstances = projectDto.MinInstances;
		project.MaxInstances = projectDto.MaxInstances;

		// Handle client update - get or create client
		var client = await db.Clients.FirstOrDefaultAsync(c => c.Name == projectDto.Client && c.Class.Id == project.Class.Id);
		if (client == null)
		{
			client = new ClientModel
			{
				Name = projectDto.Client,
				Class = project.Class,
			};
			db.Clients.Add(client);
		}
		project.Client = client;

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
			.Include(p => p.Class)
			.FirstOrDefaultAsync(x => x.Id == id);
		if (project == null)
		{
			return NotFound();
		}
		
		db.Projects.Remove(project);
		await db.SaveChangesAsync();
		return await GetProjects(project.Class.Id);
	}
}
