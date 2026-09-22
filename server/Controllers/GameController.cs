using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    
    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetGamesList()
    {
        return Ok(await _gameService.GetActiveGamesListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGameById(int id)
    {
        return Ok(await _gameService.GetGameById(id));
    }

    [HttpPost("bet")]
    [Authorize]
    public async Task<IActionResult> PostBetById(PlayDTO dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _gameService.PlayRoundByGameIdAsync(dto, userId);
        return Ok(new {result});
    }
    
    [HttpGet("{gameId}/outcomes")]
    public async Task<IActionResult> GetOutcomes(int gameId)
    {
        var outcomes = await _gameService.GetOutcomesAsync(gameId);
        return Ok(outcomes);
    }
}