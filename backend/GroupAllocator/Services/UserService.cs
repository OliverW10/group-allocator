using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IUserService
{
	Task<UserModel?> GetOrCreateUserAsync(string name, string email, bool? isAdmin = null);
	Task CreateStudentAllowlist(StreamReader reader);
}

public class UserService(ApplicationDbContext db, IConfiguration configuration) : IUserService
{
	public async Task<UserModel?> GetOrCreateUserAsync(string name, string email, bool? isAdmin = null)
	{
		var knownIsAdmin = isAdmin ?? ShouldBeAdmin(email);
		var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

		if (existingUser is null)
		{
			return await CreateNewUser(name, email, knownIsAdmin);
		}

		existingUser.Name = name;
		if (knownIsAdmin && !existingUser.IsAdmin)
		{
			existingUser.IsAdmin = knownIsAdmin;
		}
		await db.SaveChangesAsync();
		

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
		string[] adminEmails = configuration.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();
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
					Name = line.Split("@").First(),
				});
			}
			else
			{
				existingUser.IsVerified = true;
			}
		}

		await db.SaveChangesAsync();
	}
}
