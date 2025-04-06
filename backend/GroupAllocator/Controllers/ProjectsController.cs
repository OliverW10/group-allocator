using GroupAllocator.Database.Model;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
	private readonly IProjectService projectService;

	public ProjectsController(IProjectService projectService)
	{
		this.projectService = projectService;
	}

	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> GetProjects()
	{
		var projects = await projectService.GetProjects();
		if (projects == null)
		{
			return NotFound();
		}

		return Ok(projects.Select(x => x.ToDto()));
	}

	[HttpGet]
	[Route("get/{id}")]
	public async Task<IActionResult> GetProject(int id)
	{
		var project = await projectService.GetProject(id);
		if (project == null)
		{
			return NotFound();
		}

		return Ok(project.ToDto());
	}
}