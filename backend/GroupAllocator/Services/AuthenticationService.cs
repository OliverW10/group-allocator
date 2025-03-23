using GroupAllocator.Database.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

        //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("asdfqwerty"));
        //var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //return new JwtSecurityTokenHandler().WriteToken(token);

        var id = new ClaimsIdentity(claims, "ApplicationCookie");
        return new ClaimsPrincipal(id);
    }
}
