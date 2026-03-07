using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routsky.Api.Data;
using Routsky.Api.Entities;

namespace Routsky.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly RoutskyDbContext _context;

    public GroupController(RoutskyDbContext context)
    {
        _context = context;
    }

    public class CreateGroupRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class JoinGroupRequest
    {
        public string InviteCode { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Group name is required." });

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        string inviteCode = GenerateInviteCode();

        // Ensure uniqueness
        while (await _context.TravelGroups.AnyAsync(g => g.InviteCode == inviteCode))
        {
            inviteCode = GenerateInviteCode();
        }

        var group = new TravelGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            InviteCode = inviteCode,
            CreatedAt = DateTime.UtcNow
        };

        var member = new TravelGroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.TravelGroups.Add(group);
        _context.TravelGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Workspace created.", groupId = group.Id, inviteCode = group.InviteCode });
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InviteCode))
            return BadRequest(new { message = "Invite code is required." });

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var code = request.InviteCode.ToUpper().Trim();

        var group = await _context.TravelGroups.FirstOrDefaultAsync(g => g.InviteCode == code);
        if (group == null)
            return NotFound(new { message = "Workspace not found or invalid invite code." });

        bool isMember = await _context.TravelGroupMembers.AnyAsync(m => m.GroupId == group.Id && m.UserId == userId);
        if (isMember)
            return Conflict(new { message = "You are already a member of this workspace." });

        var member = new TravelGroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.TravelGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Joined workspace successfully.", groupId = group.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyGroups()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        // Step 1: Get the group IDs this user belongs to
        var groupIds = await _context.TravelGroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .Distinct()
            .ToListAsync();

        // Step 2: Load groups with full navigation graph from TravelGroups root
        var groups = await _context.TravelGroups
            .AsNoTracking()
            .Where(g => groupIds.Contains(g.Id))
            .Include(g => g.Members)
                .ThenInclude(m => m.User!)
                    .ThenInclude(u => u.Profile)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        // Step 3: Project to DTOs in memory (safe for JSON ValueConverter columns)
        var result = groups.Select(g => new
        {
            id = g.Id,
            name = g.Name,
            inviteCode = g.InviteCode,
            ownerId = g.Members.OrderBy(m => m.JoinedAt).FirstOrDefault()?.UserId ?? 0,
            members = g.Members.Select(m => new
            {
                id = m.UserId,
                name = $"{m.User?.FirstName} {m.User?.LastName}".Trim(),
                avatar = m.User?.AvatarUrl ?? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{m.User?.FirstName} {m.User?.LastName}".Trim())}&background=random&color=fff",
                origin = !string.IsNullOrWhiteSpace(m.User?.Profile?.Origin)
                    ? m.User!.Profile!.Origin
                    : (m.User?.Profile?.Passports?.FirstOrDefault() ?? ""),
                budget = (m.User?.Profile?.Budget ?? 0) > 0 ? m.User!.Profile!.Budget : 1500
            }).ToList(),
            isEngineReady = g.Members.Count > 1,
            avatars = g.Members.Select(m =>
                m.User?.AvatarUrl ?? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{m.User?.FirstName} {m.User?.LastName}".Trim())}&background=random&color=fff"
            ).ToList(),
            shortlist = _context.GroupShortlistRoutes
                .AsNoTracking()
                .Where(sr => sr.GroupId == g.Id)
                .Include(sr => sr.Votes)
                .ToList()
                .Select(sr => new
                {
                    id = sr.Id,
                    destinationId = sr.DestinationId,
                    addedByUserId = sr.AddedByUserId,
                    addedAt = sr.AddedAt,
                    votes = sr.Votes.Select(v => new
                    {
                        userId = v.UserId,
                        isUpvote = v.IsUpvote
                    })
                })
        });

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var group = await _context.TravelGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound(new { message = "Group not found." });

        // Check if the current user is the owner (first member who joined)
        var ownerId = group.Members.OrderBy(m => m.JoinedAt).FirstOrDefault()?.UserId ?? 0;
        if (ownerId != userId)
            return Forbid(); // Only the owner can delete the group

        _context.TravelGroups.Remove(group);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Group deleted successfully." });
    }

    public class AddShortlistRequest
    {
        public string DestinationId { get; set; } = string.Empty;
    }

    [HttpPost("{groupId}/shortlist")]
    public async Task<IActionResult> AddToShortlist(Guid groupId, [FromBody] AddShortlistRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.DestinationId))
            return BadRequest("Destination is required.");

        var isMember = await _context.TravelGroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
        if (!isMember) return Forbid();

        var existing = await _context.GroupShortlistRoutes.FirstOrDefaultAsync(s => s.GroupId == groupId && s.DestinationId == request.DestinationId);
        if (existing != null) return Ok(new { message = "Already in shortlist", id = existing.Id });

        var shortlistRoute = new GroupShortlistRoute
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            DestinationId = request.DestinationId,
            AddedByUserId = userId,
            AddedAt = DateTime.UtcNow
        };

        _context.GroupShortlistRoutes.Add(shortlistRoute);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Added to shortlist", id = shortlistRoute.Id });
    }

    public class VoteRequest
    {
        public bool IsUpvote { get; set; }
    }

    [HttpPost("{groupId}/shortlist/{routeId}/vote")]
    public async Task<IActionResult> VoteShortlistRoute(Guid groupId, Guid routeId, [FromBody] VoteRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var isMember = await _context.TravelGroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
        if (!isMember) return Forbid();

        var route = await _context.GroupShortlistRoutes.FirstOrDefaultAsync(r => r.Id == routeId && r.GroupId == groupId);
        if (route == null) return NotFound("Route not found in shortlist.");

        var existingVote = await _context.RouteVotes.FirstOrDefaultAsync(v => v.GroupShortlistRouteId == routeId && v.UserId == userId);
        if (existingVote != null)
        {
            existingVote.IsUpvote = request.IsUpvote;
            existingVote.VotedAt = DateTime.UtcNow;
        }
        else
        {
            _context.RouteVotes.Add(new RouteVote
            {
                Id = Guid.NewGuid(),
                GroupShortlistRouteId = routeId,
                UserId = userId,
                IsUpvote = request.IsUpvote,
                VotedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Vote cast successfully" });
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new string(Enumerable.Repeat(chars, 4)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        return "RTQ-" + code;
    }
}
