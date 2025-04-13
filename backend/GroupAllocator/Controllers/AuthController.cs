using Google.Apis.Auth;
using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService, IGaAuthenticationService tokenService) : ControllerBase
{
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
