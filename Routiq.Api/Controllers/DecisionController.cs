using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.Services;

namespace Routiq.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class DecisionController : ControllerBase
{
    private readonly RoutiqDbContext _context;
    private readonly DecisionSolverService _solver;

    public DecisionController(RoutiqDbContext context, DecisionSolverService solver)
    {
        _context = context;
        _solver = solver;
    }

    /// <summary>
    /// Dumb Backend endpoint: returns raw participant data for a group.
    /// No decision-making — just data.
    /// </summary>
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

    /// <summary>
    /// The Agent endpoint: POST /api/decision/run
    /// Triggers the full orchestration pipeline.
    /// Frontend only needs to call this — everything else is handled by the solver.
    /// </summary>
    [HttpPost("decision/run")]
    public async Task<IActionResult> RunDecision([FromBody] RunDecisionRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        // Verify caller is a member of this group
        var isMember = await _context.TravelGroupMembers
            .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == userId);
        if (!isMember) return Forbid();

        // Run the Decision Solver (the Agent brain)
        var result = await _solver.SolveAsync(request.GroupId);

        return Ok(result);
    }
}
