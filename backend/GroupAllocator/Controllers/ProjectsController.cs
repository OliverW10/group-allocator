using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
	private readonly ApplicationDbContext _dbContext;
	public ProjectsController(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	[HttpGet]
	public IActionResult GetProjects()
	{
		return Ok(_dbContext.Projects.ToList().Select(p => new ProjectDto
		{
			Id = p.Id,
			Name = p.Name,
			RequiresNda = p.RequiresNda,
			MinStudents = p.MinStudents,
			MaxStudents = p.MaxStudents
		}));
	}

	[HttpGet("{id}")]
	public IActionResult GetProject(int id)
	{
		var project = _dbContext.Projects.Find(id);
		if (project == null)
		{
			return NotFound();
		}

		return Ok(new ProjectDto
		{
			Id = project.Id,
			Name = project.Name,
			RequiresNda = project.RequiresNda,
			MinStudents = project.MinStudents,
			MaxStudents = project.MaxStudents
		});
	}

	[HttpPost]
	public IActionResult AddProject([FromBody] ProjectDto projectDto)
	{
		var project = new ProjectModel
		{
			Name = projectDto.Name,
			RequiresNda = projectDto.RequiresNda,
			MinStudents = projectDto.MinStudents,
			MaxStudents = projectDto.MaxStudents,
			Client = null
		};
		
		_dbContext.Projects.Add(project);
		_dbContext.SaveChanges();
		
		return CreatedAtAction(nameof(GetProjects), new ProjectDto {
			Id = project.Id,
			Name = project.Name,
			RequiresNda = project.RequiresNda,
			MinStudents = project.MinStudents,
			MaxStudents = project.MaxStudents
		});
	}
}