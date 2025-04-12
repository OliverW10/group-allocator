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
public class ProjectsController(IProjectService projectService, ApplicationDbContext db) : ControllerBase
{
	[HttpPost("upload")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> UploadProjects([FromForm] IFormFile file)
	{
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

		using var reader = new StreamReader(file.OpenReadStream());
		await projectService.AddFromCsv(reader);

		return await GetProjects();
    }

	[HttpGet]
	[Route("get")]
	[Authorize]
	public async Task<IActionResult> GetProjects()
	{
		return Ok(await db.Projects.Include(p => p.Client).Select(x => x.ToDto()).ToListAsync());
	}

	[HttpPut]
	[Route("update/{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto projectDto)
	{
		var project = await projectService.GetProject(id);

		if (project == null)
		{
			return NotFound();
		}

		var updated = await projectService.UpdateProject(project);

		return Ok(updated.ToDto());
	}

	[HttpDelete]
	[Route("delete/{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> DeleteProject(int id)
	{
		await projectService.DeleteProject(id);
		return await GetProjects();
	}
}