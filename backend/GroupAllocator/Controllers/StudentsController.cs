using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController(ApplicationDbContext db, IStudentService studentService, IGaAuthenticationService authService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await studentService.GetStudents().Select(s => s.ToDto()).ToListAsync());
    }

    [HttpPost("whitelist")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PostWhitelist([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var reader = new StreamReader(file.OpenReadStream());
        await authService.AddFromCsv(reader);

        return Ok(await studentService.GetStudents().Select(s => s.ToDto()).ToListAsync());
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (userEmail == null)
        {
            return BadRequest("Not logged in");
        }

        var student = await studentService.GetStudents().Where(s => s.User.Email == userEmail).FirstOrDefaultAsync();
        if (student == null)
        {
            return NotFound();
        }

        return Ok(student.ToDto());
    }

    [HttpPost("me")]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] StudentSubmissionDto preferences)
    {
        var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (userEmail == null)
        {
            return BadRequest("Not logged in");
        }

        var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

        if (user == null)
        {
            // database resets often in development
            return BadRequest("User doesn't exist");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Student.Where(s => s.User.Email == userEmail).ExecuteDeleteAsync();
        var student = new StudentModel()
        {
            User = user,
            WillSignContract = preferences.WillSignContract,
        };
        var preferenceModels = new List<PreferenceModel>();
        var allProjects = await db.Projects.ToListAsync();

        double strength = 1.0;
        foreach (var preference in preferences.OrderedPreferences.Take(10))
        {
            var proj = allProjects.FirstOrDefault(x => x.Id == preference);

            if (proj == null) throw new InvalidOperationException("Project doesn't exist");

            if (preferenceModels.Any(p => p.Project == proj)) throw new InvalidOperationException("Duplicate preferences");

            var newPreference = new PreferenceModel
            {
                Project = proj,
                Student = student,
                Strength = strength,
            };

            db.Add(newPreference);
            preferenceModels.Add(newPreference);
            strength -= 0.1;
        }

        // TODO: handle files

        db.Student.Add(student);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        await db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
        await db.SaveChangesAsync();
        return await GetAll();
    }
}
