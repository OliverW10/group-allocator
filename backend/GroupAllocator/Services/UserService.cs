using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IUserService
{
	Task<UserModel> GetOrCreateUserAsync(string name, string email);
	Task CreateStudentAllowlist(int classId, StreamReader reader);
	Task AddStudentToClass(int classId, string email);
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

	private async Task<ClassModel> GetClassById(int classId)
	{
		return await db.Classes.FindAsync(classId) ?? throw new InvalidOperationException($"Class {classId} does not exist");
	}

	private async Task<UserModel> GetOrCreateUserByEmail(string email)
	{
		var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
		if (existingUser == null)
		{
			existingUser = new UserModel
			{
				Email = email,
				Name = email.Split("@").First(),
				IsAdmin = IsPredeterminedAdmin(email),
			};
			db.Users.Add(existingUser);
			await db.SaveChangesAsync();
		}
		return existingUser;
	}

	private async Task<bool> IsStudentInClass(UserModel user, ClassModel @class)
	{
		return await db.Students.Include(s => s.User).Include(s => s.Class).AnyAsync(s => s.User == user && s.Class == @class);
	}

	private async Task AddStudentToClassIfNotExists(UserModel user, ClassModel @class)
	{
		var hasExistingStudent = await IsStudentInClass(user, @class);
		if (!hasExistingStudent)
		{
			db.Students.Add(new StudentModel
			{
				Class = @class,
				User = user,
				IsVerified = true,
				Notes = "",
			});
		}
	}

	public async Task CreateStudentAllowlist(int classId, StreamReader csvStream)
	{
		var @class = await GetClassById(classId);

		string? line;
		while ((line = await csvStream.ReadLineAsync()) != null)
		{
			var user = await GetOrCreateUserByEmail(line);
			await AddStudentToClassIfNotExists(user, @class);
		}

		await db.SaveChangesAsync();
	}

	public async Task AddStudentToClass(int classId, string email)
	{
		var @class = await GetClassById(classId);
		var user = await GetOrCreateUserByEmail(email);
		await AddStudentToClassIfNotExists(user, @class);
		await db.SaveChangesAsync();
	}
}
