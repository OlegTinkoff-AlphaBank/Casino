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

    [HttpGet("bet")]
    [Authorize]
    public async Task<IActionResult> GetBetById(PlayDTO dto)
    {
        return Ok(await _gameService.PlayRoundByGameIdAsync(dto));
    }
}