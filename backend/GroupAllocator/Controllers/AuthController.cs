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
			IsAdmin = bool.Parse(claims.First(c => c.Type == "admin").Value)
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
	public async Task<IActionResult> LoginDev(string name, string email, bool? isAdmin)
	{
		var user = await userService.GetOrCreateUserAsync(name, email, isAdmin);

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

	[HttpGet("role/claim-admin/{email}")]
	public async Task<IActionResult> ClaimAdmin(string email)
	{
		if (!configuration.GetValue<bool>("AdminClaimable") && !User.HasClaim("admin", "true"))
		{
			return NotFound();
		}

		await userService.GetOrCreateUserAsync("Unknown", email, true);
		return Ok($"Set {email} to admin.");
	}

	[HttpGet("role/drop-admin/{email}")]
	public async Task<IActionResult> DropAdmin(string email)
	{
		if (!configuration.GetValue<bool>("AdminClaimable") && !User.HasClaim("admin", "true"))
		{
			return NotFound();
		}

		var user = await userService.GetOrCreateUserAsync("Unknown", email);
		if (user is not null)
		{
			user.IsAdmin = false;
			await db.SaveChangesAsync();
			return Ok($"Set {email} to student.");
		}

		return Ok($"User not found, no action taken.");

	}

	static IActionResult UserDto(UserModel user)
	{
		return new JsonResult(new UserInfoDto()
		{
			Name = user.Name,
			Email = user.Email,
			IsAdmin = user.IsAdmin,
		});
	}

	async Task SignIn(UserModel user)
	{
		// Generate our own token rather than use the one obtained from google so we can add our own claims (e.g. IsAdmin) to it
		// and to keep it standard with dev login or other auth methods
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
