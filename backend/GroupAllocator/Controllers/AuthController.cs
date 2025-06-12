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
public class AuthController(IUserService userService, IGroupAllocatorAuthenticationService tokenService, IConfiguration configuration, ApplicationDbContext db) : ControllerBase
{
	[HttpGet("me")]
	[Authorize]
	public IActionResult GetCurrentUser()
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
			Role = claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "student"
		});
	}

	[HttpGet("login-google")]
	public async Task<IActionResult> LoginGoogle(string idToken)
	{
		var googleToken = await ValidateGoogleToken(idToken);

		var user = await userService.GetOrCreateUserAsync(googleToken.Name, googleToken.Email);

		if (user is null)
		{
			return Unauthorized();
		}

		await SignIn(user);

		return UserDto(user);
	}

	[HttpGet("login-dev")]
	public async Task<IActionResult> LoginDev(string name, string email, string? role = "student")
	{
		var user = await userService.GetOrCreateUserAsync(name, email, role);

		if (user is null)
		{
			return Unauthorized();
		}

		await SignIn(user);

		return UserDto(user);
	}

	[HttpGet("logout")]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync();
		return Ok();
	}

	[HttpGet("role/set/{email}/{role}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> SetRole(string email, string role)
	{
		if (!new[] { "admin", "teacher", "student" }.Contains(role.ToLower()))
		{
			return BadRequest("Invalid role. Must be one of: admin, teacher, student");
		}

		var user = await userService.GetOrCreateUserAsync("Unknown", email);
		if (user is not null)
		{
			// Update user's role in the database
			user.Role = role.ToLower();
			await db.SaveChangesAsync();

			// If the user is currently logged in, sign them out to force a re-login with new role
			if (User.FindFirst(JwtRegisteredClaimNames.Email)?.Value == email)
			{
				await HttpContext.SignOutAsync();
			}

			return Ok($"Set {email} to {role}.");
		}

		return Ok($"User not found, no action taken.");
	}

	static IActionResult UserDto(UserModel user)
	{
		return new JsonResult(new UserInfoDto()
		{
			Name = user.Name,
			Email = user.Email,
			Role = user.Role
		});
	}

	async Task SignIn(UserModel user)
	{
		var principal = tokenService.GetPrincipal(user);
		var authProperties = new AuthenticationProperties
		{
			IsPersistent = true,
		};
		await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
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
