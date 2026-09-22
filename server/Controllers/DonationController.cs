using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Services;
using serverDB;

namespace server.Controllers;

[ApiController]
[Route("api/v1.0/[controller]")]
public class DonationController : ControllerBase
{
    private readonly DonationService _donationService;

    public DonationController(DonationService donationService)
    {
        _donationService = donationService;
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> GetGamesList()
    {
        var d = await _donationService.GetLastDonationsAsync();
        var res = "";
        foreach (var dt in d)
            res+=$"{dt.Amount} — {dt.RawComment}\n";
        return Ok(res);
    }

    [HttpPost("donate")]
    [Authorize]
    public async Task<ActionResult> DonateByID(decimal amount)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string code = await _donationService.PostDonationAsync(userId, amount);
        return Ok(code);
    }

    [HttpPost("check_donate")]
    [Authorize]
    public async Task<ActionResult> ChekDonateByID()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string code = await _donationService.ChackDonationAsync(userId);
        return Ok(code);
    }
    
}