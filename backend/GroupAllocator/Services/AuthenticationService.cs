using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GroupAllocator.Services;

public interface IGaAuthenticationService
{
    ClaimsPrincipal GetPrincipal(UserModel user);
    Task AddFromCsv(StreamReader reader);
}

public class AuthenticationService(ApplicationDbContext db) : IGaAuthenticationService
{
    public ClaimsPrincipal GetPrincipal(UserModel user)
    {
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("admin", user.IsAdmin.ToString()),
        };

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(id);
    }
    public async Task AddFromCsv(StreamReader csvStream)
    {
        var allUsers = db.Users.ToList();
        string? line;
        while ((line = await csvStream.ReadLineAsync()) != null)
        {
            var fields = line.Split(',').Select(x => x.Trim()).ToArray();
            if (fields.Length != 1)
            {
                throw new InvalidOperationException("Invalid csv");
            }

            // student_email
            var studentEmail = fields[0];

            if (db.Users.FirstOrDefault(u => u.Email == studentEmail) == null)
            {
                db.Users.Add(new UserModel
                {
                    Email = studentEmail,
                    IsAdmin = false,
                    Name = "Unknown"
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
