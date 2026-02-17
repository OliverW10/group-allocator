
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public class StudentsService(ApplicationDbContext db)
{
    public async Task<List<StudentInfoAndSubmission>> GetStudents(int classId)
    {
        return await db.Students
            .Include(s => s.Files)
            .Include(s => s.User)
            .Include(s => s.Preferences)
                .ThenInclude(p => p.Project)
            .Include(s => s.Class)
            .Where(s => s.Class.Id == classId)
            .Select(s => s.ToDto())
            .ToListAsync();
    }
}