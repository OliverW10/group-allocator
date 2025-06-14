using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IUserService
{
	Task<UserModel> GetOrCreateUserAsync(string name, string email);
	Task CreateStudentAllowlist(int classId, StreamReader reader);
}

public class UserService(ApplicationDbContext db, IConfiguration configuration) : IUserService
{
	public async Task<UserModel> GetOrCreateUserAsync(string name, string email)
	{
		var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

		if (existingUser is null)
		{
			return await CreateNewUser(name, email);
		}

		existingUser.Name = name;
		await db.SaveChangesAsync();
		
		return existingUser;

	}
	async Task<UserModel> CreateNewUser(string name, string email)
	{
		var newUser = new UserModel { Email = email, Name = name, IsAdmin = IsPredeterminedAdmin(email) };
		db.Users.Add(newUser);
		await db.SaveChangesAsync();
		return newUser;
	}

	bool IsPredeterminedAdmin(string email)
	{
		string[] adminEmails = configuration.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();
		return adminEmails.Contains(email);
	}

	public async Task CreateStudentAllowlist(int classId, StreamReader csvStream)
	{
		var @class = db.Classes.Find(classId) ?? throw new InvalidOperationException($"Class {classId} does not exist");

		string? line;
		while ((line = await csvStream.ReadLineAsync()) != null)
		{
			var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == line);
			if (existingUser == null)
			{
				existingUser = new UserModel
				{
					Email = line,
					Name = line.Split("@").First(),
					IsAdmin = IsPredeterminedAdmin(line),
				};
				db.Users.Add(existingUser);
				await db.SaveChangesAsync();
			}

			var existingStudent = await db.Students.FirstOrDefaultAsync(s => s.Id == existingUser.Id);
			if (existingStudent == null)
			{
				db.Students.Add(new StudentModel
				{
					Class = @class,
					User = existingUser,
				});
			}
		}

		await db.SaveChangesAsync();
	}
}
