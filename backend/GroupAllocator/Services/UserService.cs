using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IUserService
{
	Task<UserModel?> GetOrCreateUserAsync(string name, string email);
	Task SetAdmin(string email, bool admin);
	Task CreateStudentAllowlist(StreamReader reader);
}

public class UserService(ApplicationDbContext db) : IUserService
{
	public async Task<UserModel?> GetOrCreateUserAsync(string name, string email)
	{
		var knownIsAdmin = ShouldBeAdmin(email);
		var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

		if (existingUser is null)
		{
			return await CreateNewUser(name, email, knownIsAdmin);
		}

		if (existingUser is not null)
		{
			existingUser.Name = name;
			existingUser.IsAdmin = knownIsAdmin;
			await db.SaveChangesAsync();
		}

		return existingUser;
	}
	async Task<UserModel> CreateNewUser(string name, string email, bool isAdmin)
	{
		var newUser = new UserModel { Email = email, Name = name, IsAdmin = isAdmin, IsVerified = false };
		db.Users.Add(newUser);
		await db.SaveChangesAsync();
		return newUser;
	}

	bool ShouldBeAdmin(string email)
	{
		// TODO: should maybe just seed which users are admin
		string[] adminEmails = [
			"marc.carmicheal@uts.edu.au"
		];
		return adminEmails.Contains(email);
	}

	public async Task CreateStudentAllowlist(StreamReader csvStream)
	{
		// Handles the case a student un-enrols and the list is updated
		await db.Users.ExecuteUpdateAsync(s => s.SetProperty(u => u.IsVerified, u => false));

		string? line;
		while ((line = await csvStream.ReadLineAsync()) != null)
		{
			var existingUser = db.Users.FirstOrDefault(u => u.Email == line);
			if (existingUser == null)
			{
				db.Users.Add(new UserModel
				{
					Email = line,
					IsAdmin = false,
					IsVerified = true,
					Name = "Unknown"
				});
			} else
			{
				existingUser.IsVerified = true;
			}
		}

		await db.SaveChangesAsync();
	}

	public async Task SetAdmin(string email, bool admin)
	{
		var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

		if (existingUser is not null)
		{
			var initialValue = existingUser.IsAdmin;
			existingUser.IsAdmin = admin;
			await db.SaveChangesAsync();
		}

		// New users will be non admin by default so don't need to create one
		if (admin)
		{
			await CreateNewUser(email.Split("@").FirstOrDefault() ?? string.Empty, email, admin);
		}
	}
}
