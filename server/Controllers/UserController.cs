using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _userService.GetByIdAsync(userId);
        
        if (user == null) return NotFound();
        
        return Ok(new { user.Id, user.Username, user.Balance });
    }

    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh()
    {
        await _userService.RefreshUsersRateAsync();
        return Ok();
    }
}