using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GroupAllocator.Services;

public interface IAutheticationService
{
    ClaimsPrincipal GetPrincipal(UserModel user);
}

public class AuthenticationService : IAutheticationService
{
    public ClaimsPrincipal GetPrincipal(UserModel user)
    {
        var claims = new[]
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
}
