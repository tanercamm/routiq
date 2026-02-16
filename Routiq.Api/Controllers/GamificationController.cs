using Microsoft.AspNetCore.Mvc;
using Routiq.Api.DTOs;
using Routiq.Api.Services;

namespace Routiq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _gamificationService;

    public GamificationController(IGamificationService gamificationService)
    {
        _gamificationService = gamificationService;
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardDto>>> GetLeaderboard([FromQuery] int topCount = 10)
    {
        var leaderboard = await _gamificationService.GetTopTravelersAsync(topCount);
        return Ok(leaderboard);
    }

    [HttpGet("winners")]
    public async Task<ActionResult<List<LeaderboardWinnerDto>>> GetWinners()
    {
        var winners = await _gamificationService.GetLeaderboardWinnersAsync();
        return Ok(winners);
    }
}
