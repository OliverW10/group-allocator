using Microsoft.AspNetCore.Mvc;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController
{
    [HttpGet("login-google")]
    void LoginGoogle(string token)
    {
        Console.WriteLine("yep");
        // verify token
        // generate and send our own jwt
    }

    [HttpGet("login-dev")]
    void LoginDev()
    {
        // verify and login with a microsoft sso jwt
        // https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens#validate-tokens
    }
}
