using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Routsky.Api.Services;

namespace Routsky.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VisaController : ControllerBase
{
    private readonly TravelBuddyApiService _travelBuddyApiService;

    public VisaController(TravelBuddyApiService travelBuddyApiService)
    {
        _travelBuddyApiService = travelBuddyApiService;
    }

    [HttpGet("global-map/{passportCode}")]
    public async Task<IActionResult> GetGlobalMap(string passportCode)
    {
        if (string.IsNullOrWhiteSpace(passportCode))
            return BadRequest(new { message = "passportCode is required." });

        var normalized = passportCode.Trim().ToUpperInvariant();
        if (normalized.Length is < 2 or > 3)
            return BadRequest(new { message = "passportCode must be ISO alpha-2 or alpha-3." });

        var result = await _travelBuddyApiService.GetGlobalVisaStatusesAsync(normalized);
        return Ok(result);
    }
}
