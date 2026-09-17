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
            res+=$"{dt.Amount} — {dt.RawComment}";
        return Ok(res);
    }
    
}