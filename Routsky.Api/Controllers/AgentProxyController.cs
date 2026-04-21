using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Routsky.Api.Services;

namespace Routsky.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/agent")]
public class AgentProxyController : ControllerBase
{
    private readonly IAgentInsightService _agentInsightService;

    public AgentProxyController(IAgentInsightService agentInsightService)
    {
        _agentInsightService = agentInsightService;
    }

    [HttpGet("insight/{city}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInsight(string city)
    {
        var insight = await _agentInsightService.GenerateInsightAsync(city);
        return Ok(new { text = insight });
    }
}
