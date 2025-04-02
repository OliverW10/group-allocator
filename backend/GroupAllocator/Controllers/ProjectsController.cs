using GroupAllocator.Database;
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
}