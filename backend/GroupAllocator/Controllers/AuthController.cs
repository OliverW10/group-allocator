using System.Security.Claims;
using Google.Apis.Auth;
using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService, IConfiguration configuration, ApplicationDbContext db) : ControllerBase
{
	[HttpGet("me")]
	[Authorize]
	public ActionResult<UserInfoDto> GetCurrentUser()
	{
		var claims = HttpContext.User.Claims.ToList();
		if ((HttpContext.User.Identity is null || !claims.Any()))
		{
			return Unauthorized();
		}

		return new JsonResult(new UserInfoDto()
		{
			Name = claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value,
			Email = claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value,
			Role = Enum.Parse<AuthRole>(claims.First(c => c.Type == AuthRolesConstants.RoleClaimName).Value),
			IsAdmin = bool.Parse(claims.First(c => c.Type == AuthRolesConstants.AdminClaimName).Value),
		});
	}

	[HttpGet("login-google-student")]
	public async Task<ActionResult<UserInfoDto>> LoginGoogleStudent(string idToken)
	{
		return await LoginGoogleCommon(idToken, AuthRole.Student);
	}

	[HttpGet("login-google-teacher")]
	public async Task<ActionResult<UserInfoDto>> LoginGoogleTeacher(string idToken)
	{
		return await LoginGoogleCommon(idToken, AuthRole.Teacher);
	}

	async Task<UserInfoDto> LoginGoogleCommon(string idToken, AuthRole role)
	{
		var googleToken = await ValidateGoogleToken(idToken);
		var user = await userService.GetOrCreateUserAsync(googleToken.Name, googleToken.Email);
		await SignIn(user, role);
		return UserDto(role, user);
	}

	[HttpGet("login-dev")]
	public async Task<UserInfoDto> LoginDev(string name, string email, AuthRole role)
	{
		var user = await userService.GetOrCreateUserAsync(name, email);
		await SignIn(user, role);
		return UserDto(role, user);
	}

	private static UserInfoDto UserDto(AuthRole role, UserModel user)
	{
		return new UserInfoDto
		{
			Name = user.Name,
			Email = user.Email,
			Role = role,
			IsAdmin = user.IsAdmin,
		};
	}

	[HttpGet("logout")]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync();
		return Ok();
	}

	[HttpGet("role/set/{email}/{value}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> SetAdmin(string email, bool value)
	{
		var user = await userService.GetOrCreateUserAsync("Unknown", email);
		if (user is not null)
		{
			// Update user's role in the database
			user.IsAdmin = value;
			await db.SaveChangesAsync();

			// If the user is currently logged in, sign them out to force a re-login with new role
			if (User.FindFirst(JwtRegisteredClaimNames.Email)?.Value == email)
			{
				await HttpContext.SignOutAsync();
			}

			return Ok($"Set {email} admin state to {value}.");
		}

		return Ok($"User not found, no action taken.");
	}

	async Task SignIn(UserModel user, AuthRole role)
	{
		var principal = GetPrincipal(user, role, user.IsAdmin);
		var authProperties = new AuthenticationProperties
		{
			IsPersistent = true,
		};
		await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
	}

	ClaimsPrincipal GetPrincipal(UserModel user, AuthRole role, bool isAdmin)
	{
		var claims = new List<Claim>()
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Name, user.Name),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(AuthRolesConstants.RoleClaimName, role.ToString().ToLowerInvariant()),
			new Claim(AuthRolesConstants.AdminClaimName, isAdmin.ToString()),
		};

		var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		return new ClaimsPrincipal(id);
	}

	static async Task<GoogleJsonWebSignature.Payload> ValidateGoogleToken(string idToken)
	{
		var settings = new GoogleJsonWebSignature.ValidationSettings()
		{
			Audience = ["516503324384-h73aa00v5gectf0oqr0c9fber709gm0s.apps.googleusercontent.com"]
		};
		var googleToken = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
		return googleToken;
	}
}

public static class AuthRolesConstants
{
	public const string RoleClaimName = "role";
	public const string AdminClaimName = "admin";
	public static string Student => AuthRole.Student.ToString().ToLowerInvariant();
	public static string Teacher => AuthRole.Teacher.ToString().ToLowerInvariant();
}
