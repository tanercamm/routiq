using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routsky.Api.Data;
using Routsky.Api.Services;

namespace Routsky.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class DecisionController : ControllerBase
{
    private readonly RoutskyDbContext _context;
    private readonly DecisionSolverService _solver;

    private static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DecisionController(RoutskyDbContext context, DecisionSolverService solver)
    {
        _context = context;
        _solver = solver;
    }

    [HttpGet("groups/{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var isMember = await _context.TravelGroupMembers
            .AnyAsync(m => m.GroupId == id && m.UserId == userId);
        if (!isMember) return Forbid();

        var members = await _context.TravelGroupMembers
            .Where(m => m.GroupId == id)
            .Include(m => m.User)
                .ThenInclude(u => u!.Profile)
            .Select(m => new
            {
                id = m.UserId,
                name = (m.User!.FirstName + " " + m.User.LastName).Trim(),
                origin = !string.IsNullOrWhiteSpace(m.User.Profile!.Origin)
                    ? m.User.Profile.Origin
                    : m.User.Profile.Passports.FirstOrDefault() ?? "",
                originMissing = string.IsNullOrWhiteSpace(m.User.Profile!.Origin)
                    && (m.User.Profile.Passports == null || m.User.Profile.Passports.Count == 0),
                passports = m.User.Profile.Passports ?? new List<string>(),
                budget = m.User.Profile.Budget > 0 ? m.User.Profile.Budget : 1500
            })
            .ToListAsync();

        return Ok(members);
    }

    public class RunDecisionRequest
    {
        public Guid GroupId { get; set; }
    }

    // ════════════════════════════════════════════════════════════════
    //  STANDARD ENDPOINTS (JSON response, backward-compatible)
    // ════════════════════════════════════════════════════════════════

    [HttpPost("decision/run")]
    public async Task<IActionResult> RunDecision([FromBody] RunDecisionRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var isMember = await _context.TravelGroupMembers
            .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == userId);
        if (!isMember) return Forbid();

        var result = await _solver.SolveAsync(request.GroupId, ct: HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpPost("decision/discover")]
    [AllowAnonymous]
    public async Task<IActionResult> Discover([FromBody] DecisionSolverService.DiscoverRequest request)
    {
        var result = await _solver.SolveDiscoverAsync(request, ct: HttpContext.RequestAborted);
        return Ok(result);
    }

    // ════════════════════════════════════════════════════════════════
    //  SSE STREAMING ENDPOINTS (real-time status updates)
    //
    //  Events pushed to the client:
    //    data: {"type":"status","data":{"message":"Scanning destinations..."}}
    //    data: {"type":"status","data":{"message":"Batch-evaluating TBS,SJJ,BKK..."}}
    //    data: {"type":"result","data":{...DecisionResult...}}
    //    data: {"type":"error","data":{"message":"..."}}
    //
    //  Frontend consumes via EventSource or fetch + ReadableStream.
    // ════════════════════════════════════════════════════════════════

    [HttpPost("decision/run/stream")]
    public async Task RunDecisionStream([FromBody] RunDecisionRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
        {
            Response.StatusCode = 401;
            return;
        }

        var isMember = await _context.TravelGroupMembers
            .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == userId);
        if (!isMember)
        {
            Response.StatusCode = 403;
            return;
        }

        SetupSseResponse();

        try
        {
            await SendSseEvent("status", new { message = "Initializing group decision agent..." });

            var result = await _solver.SolveAsync(
                request.GroupId,
                async message => await SendSseEvent("status", new { message }),
                HttpContext.RequestAborted);

            await SendSseEvent("result", result);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await SendSseEvent("error", new { message = $"Decision failed: {ex.Message}" });
        }
    }

    [HttpPost("decision/discover/stream")]
    [AllowAnonymous]
    public async Task DiscoverStream([FromBody] DecisionSolverService.DiscoverRequest request)
    {
        SetupSseResponse();

        try
        {
            await SendSseEvent("status", new { message = "Initializing discovery agent..." });

            var result = await _solver.SolveDiscoverAsync(
                request,
                async message => await SendSseEvent("status", new { message }),
                HttpContext.RequestAborted);

            await SendSseEvent("result", result);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await SendSseEvent("error", new { message = $"Discovery failed: {ex.Message}" });
        }
    }

    // ── SSE Helpers ──

    private void SetupSseResponse()
    {
        Response.StatusCode = 200;
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");
    }

    private async Task SendSseEvent(string eventType, object data)
    {
        var payload = JsonSerializer.Serialize(new { type = eventType, data }, CamelCase);
        await Response.WriteAsync($"data: {payload}\n\n");
        await Response.Body.FlushAsync();
    }
}
