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
    private readonly ILogger<VisaController> _logger;

    public VisaController(
        TravelBuddyApiService travelBuddyApiService,
        ILogger<VisaController> logger)
    {
        _travelBuddyApiService = travelBuddyApiService;
        _logger = logger;
    }

    [HttpGet("global-map/{passportCode}")]
    public async Task<IActionResult> GetGlobalMap(string passportCode)
    {
        if (string.IsNullOrWhiteSpace(passportCode))
            return BadRequest(new { message = "passportCode is required." });

        var normalized = passportCode.Trim().ToUpperInvariant();
        if (normalized.Length is < 2 or > 3)
            return BadRequest(new { message = "passportCode must be ISO alpha-2 or alpha-3." });

        try
        {
            var result = await _travelBuddyApiService.GetGlobalVisaStatusesAsync(normalized);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            // RapidAPI auth failures (401/403) are thrown as HttpRequestException
            _logger.LogError(ex, "[VisaController] RapidAPI request failed for {Passport}", normalized);
            return StatusCode(502, new { message = $"Visa API error: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VisaController] Unexpected error for {Passport}", normalized);
            return StatusCode(500, new { message = "Internal server error while fetching visa data." });
        }
    }
}
