using KidsTown;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    [Route("verify")]
    public async Task<IActionResult> VerifyUser([FromBody] User user)
    {
        var isValid = await userRepository.IsValidLogin(user.Username, user.PasswordHash);
        return isValid ? Ok() : Unauthorized();
    }
}

public record User(string Username, string PasswordHash);