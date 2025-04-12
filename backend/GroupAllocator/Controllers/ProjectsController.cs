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

	[HttpPost("upload")]
	public async Task<IActionResult> UploadProjects([FromForm] IFormFile file)
	{
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

		using var reader = new StreamReader(file.OpenReadStream());
		await _projectService.AddFromCsv(reader);

		return await GetProjects();
    }

	[HttpGet]
	[Route("get")]
	public async Task<IActionResult> GetProjects()
	{
		var projects = await _projectService.GetProjects();
		return Ok(projects.Select(x => x.ToDto()));
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
		return await GetProjects();
	}
}