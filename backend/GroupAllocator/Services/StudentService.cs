using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IStudentService
{
    IQueryable<StudentModel> GetStudents();
}

public class StudentService(ApplicationDbContext db) : IStudentService
{
    public IQueryable<StudentModel> GetStudents()
    {
        return db.Student
            .Include(s => s.User)
            .Include(s => s.Preferences)
                .ThenInclude(p => p.Project)
            .Include(s => s.Files);
    }
}
