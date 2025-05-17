using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IStudentService
{
	IQueryable<UserModel> GetStudents();
}

public class StudentService(ApplicationDbContext db) : IStudentService
{
	public IQueryable<UserModel> GetStudents()
	{
		return db.Users.Where(u => !u.IsAdmin)
			.Include(s => s.Files)
			.Include(s => s.StudentModel)
				.ThenInclude(s => s!.Preferences)
					.ThenInclude(p => p.Project)
			;
	}
}
