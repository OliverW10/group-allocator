using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public interface IUserService
{
    Task<UserModel> GetOrCreateUserAsync(string name, string email, bool? isAdmin = null);
}

public class UserService(ApplicationDbContext db) : IUserService
{
    public async Task<UserModel> GetOrCreateUserAsync(string name, string email, bool? isAdmin = null)
    {
        var knownIsAdmin = isAdmin ?? ShouldBeAdmin(email);
        return await db.Users.FirstOrDefaultAsync(x => x.Email == email) ?? await CreateNewUser(name, email, knownIsAdmin);
    }

    async Task<UserModel> CreateNewUser(string name, string email, bool isAdmin)
    {
        var newUser = new UserModel { Email = email, Name = name, IsAdmin = ShouldBeAdmin(email) };
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser;
    }

    bool ShouldBeAdmin(string email)
    {
        // TODO: should maybe just seed which users are admin
        string[] adminEmails = [
            "marc.carmicheal@uts.edu.au",
        ];
        return adminEmails.Contains(email);
    }
}
