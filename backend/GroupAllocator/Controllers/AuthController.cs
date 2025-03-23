using Google.Apis.Auth;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService, IAutheticationService tokenService) : ControllerBase
{
    [HttpGet("login-google")]
    public async Task<IActionResult> LoginGoogle(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new[] { "516503324384-h73aa00v5gectf0oqr0c9fber709gm0s.apps.googleusercontent.com" }
        };
        var googleToken = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        
        var user = await userService.GetOrCreateUserAsync(googleToken.Name, googleToken.Email);

        var principal = tokenService.GetPrincipal(user);

        // Generate our own token rather than use the one obtained from google so we can add our own claims (e.g. IsAdmin) to it
        // and to keep it standard with dev login or other auth methods
        await HttpContext.SignInAsync(principal);

        return new JsonResult(new UserInfoDto()
        {
            Name = user.Name,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
        });
    }

    [HttpGet("login-dev")]
    public async Task<IActionResult> LoginDev(string name, string email, bool? isAdmin)
    {
        var user = await userService.GetOrCreateUserAsync(name, email, isAdmin);

        var principal = tokenService.GetPrincipal(user);

        await HttpContext.SignInAsync(principal);

        return Ok();
    }
}
