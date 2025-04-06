using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
	private readonly IProjectService _projectService;
	public ProjectsController(IProjectService projectService)
	{
		_projectService = projectService;
	}

	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> GetProjects()
	{
		var projects = await _projectService.GetProjects();
		return Ok(projects.Select(x => x.ToDto()));
	}

	[HttpGet]
	[Route("get/{id}")]
	public async Task<IActionResult> GetProject(int id)
	{
		var project = await _projectService.GetProject(id);
		if (project == null)
		{
			return NotFound();
		}

		return Ok(project.ToDto());
	}

	[HttpPost]
	[Route("add")]
	public async Task<ActionResult> AddProject([FromBody] ProjectDto projectDto)
	{
		var project = new ProjectModel
		{
			Name = projectDto.Name,
			RequiresNda = projectDto.RequiresNda,
			MinStudents = projectDto.MinStudents,
			MaxStudents = projectDto.MaxStudents,
			RequiresContract = projectDto.RequiresContract,
			Client = null
		};

		await _projectService.AddProject(project);

		return CreatedAtAction(nameof(GetProject), new { id = project.Id }, new ProjectDto {
			Id = project.Id,
			Name = project.Name,
			RequiresNda = project.RequiresNda,
			MinStudents = project.MinStudents,
			MaxStudents = project.MaxStudents,
			RequiresContract = project.RequiresContract
		});
	}

	[HttpPut]
	[Route("update/{id}")]
	public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto projectDto)
	{
		var project = await _projectService.GetProject(id);

		if (project == null)
		{
			return NotFound();
		}

		var updated = await _projectService.UpdateProject(project);

		return Ok(updated.ToDto());
	}

	[HttpDelete]
	[Route("delete/{id}")]
	public async Task<IActionResult> DeleteProject(int id)
	{
		await _projectService.DeleteProject(id);
		return Ok();
	}
}