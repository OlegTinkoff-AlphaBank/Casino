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
        return Ok(await _donationService.GetLast());
    }
    
}