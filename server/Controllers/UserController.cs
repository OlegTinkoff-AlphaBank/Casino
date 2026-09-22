using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _env;

    public UserController(UserService userService, IWebHostEnvironment env)
    {
        _userService = userService;
        _env = env;
    }
    
    [HttpPost("outcomes/{outcomeId}/image")]
    public async Task<IActionResult> UploadOutcomeImage(int outcomeId, IFormFile file)
    {
        var imageUrl = await _userService.SaveOutcomeImageAsync(outcomeId, file, _env.WebRootPath);
        if (imageUrl is null) return NotFound();
        return Ok(new { imageUrl });
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> History()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok();
    }

    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh()
    {
        await _userService.RefreshUsersRateAsync();
        return Ok();
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Username, user.Email, user.Balance, user.Role });
    }
    
    [HttpPost("add-balance")]
    public async Task<IActionResult> AddBalance(AdminAddBalanceDto dto)
    {
        try
        {
            var newBalance = await _userService.AddBalanceAsync(dto.Username, dto.Amount);
            return Ok(new { balance = newBalance });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("games")]
    public async Task<IActionResult> CreateGame(CreateGameDto dto)
    {
        var game = await _userService.CreateGameAsync(dto);
        return Ok(game);
    }
    
    [HttpPost("games/outcomes")]
    public async Task<IActionResult> CreateOutcome(CreateGameOutcomeDto dto)
    {
        try
        {
            var outcome = await _userService.CreateOutcomeAsync(dto);
            return Ok(outcome);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}