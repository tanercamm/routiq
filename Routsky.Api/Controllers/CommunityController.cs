using Microsoft.AspNetCore.Mvc;
using Routsky.Api.Services;

namespace Routsky.Api.Controllers;

/// <summary>
/// V2 Community Controller.
/// The community loop is for data validation, NOT social content.
/// Endpoints surface structured TraveledRoute feedback — no free-text feeds.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;

    public CommunityController(ICommunityService communityService)
    {
        _communityService = communityService;
    }

    [HttpGet("feedback/{destinationId:int}")]
    public async Task<IActionResult> GetDestinationFeedback(int destinationId)
    {
        var records = await _communityService.GetDestinationFeedbackAsync(destinationId);
        return Ok(records);
    }

    [HttpGet("stats/{destinationId:int}")]
    public async Task<IActionResult> GetDestinationStats(int destinationId)
    {
        var result = await _communityService.GetDestinationStatsAsync(destinationId);
        return Ok(result);
    }
}
