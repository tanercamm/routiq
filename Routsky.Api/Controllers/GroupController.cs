using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Routsky.Api.Configuration;
using Routsky.Api.Data;
using Routsky.Api.Entities;

namespace Routsky.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly RoutskyDbContext _context;
    private readonly int _defaultBudgetUsd;
    private readonly InviteCodeSettings _inviteCodeSettings;

    public GroupController(
        RoutskyDbContext context,
        IOptions<BudgetDefaults> budgetDefaults,
        IOptions<InviteCodeSettings> inviteCodeSettings)
    {
        _context = context;
        _defaultBudgetUsd = budgetDefaults.Value.DefaultBudgetUsd;
        _inviteCodeSettings = inviteCodeSettings.Value;
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

        // Step 3: Pre-fetch all shortlist routes for these groups in a single query (avoids N+1)
        var allShortlists = await _context.GroupShortlistRoutes
            .AsNoTracking()
            .Where(sr => groupIds.Contains(sr.GroupId))
            .Include(sr => sr.Votes)
            .ToListAsync();

        var shortlistByGroup = allShortlists
            .GroupBy(sr => sr.GroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Step 4: Project to DTOs in memory (safe for JSON ValueConverter columns)
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
                budget = (m.User?.Profile?.Budget ?? 0) > 0 ? m.User!.Profile!.Budget : _defaultBudgetUsd
            }).ToList(),
            isEngineReady = g.Members.Count > 1,
            avatars = g.Members.Select(m =>
                m.User?.AvatarUrl ?? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{m.User?.FirstName} {m.User?.LastName}".Trim())}&background=random&color=fff"
            ).ToList(),
            shortlist = (shortlistByGroup.TryGetValue(g.Id, out var routes) ? routes : new List<GroupShortlistRoute>())
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
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.DestinationId))
            return BadRequest("Destination is required.");

        var isMember = await IsGroupMemberAsync(groupId, userId.Value);
        if (!isMember) return Forbid();

        var existing = await _context.GroupShortlistRoutes.FirstOrDefaultAsync(s => s.GroupId == groupId && s.DestinationId == request.DestinationId);
        if (existing != null) return Ok(new { message = "Already in shortlist", id = existing.Id });

        var shortlistRoute = new GroupShortlistRoute
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            DestinationId = request.DestinationId,
            AddedByUserId = userId.Value,
            AddedAt = DateTime.UtcNow
        };

        _context.GroupShortlistRoutes.Add(shortlistRoute);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Added to shortlist", id = shortlistRoute.Id });
    }

    public class VoteRequest
    {
        public bool? IsUpvote { get; set; }
        public string? VoteType { get; set; }
    }

    [HttpGet("{groupId}/shortlist")]
    public async Task<IActionResult> GetShortlist(Guid groupId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var isMember = await IsGroupMemberAsync(groupId, userId.Value);
        if (!isMember) return Forbid();

        var shortlist = await _context.GroupShortlistRoutes
            .AsNoTracking()
            .Where(s => s.GroupId == groupId)
            .Include(s => s.Votes)
            .OrderByDescending(s => s.AddedAt)
            .ToListAsync();

        var response = shortlist.Select(sr => ToShortlistDto(sr, userId.Value));
        return Ok(response);
    }

    [HttpPost("{groupId}/shortlist/{routeId}/vote")]
    [HttpPost("{groupId}/routes/{routeId}/vote")]
    public async Task<IActionResult> VoteShortlistRoute(Guid groupId, Guid routeId, [FromBody] VoteRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var isMember = await IsGroupMemberAsync(groupId, userId.Value);
        if (!isMember) return Forbid();

        var parsedVote = ParseVote(request);
        if (parsedVote == null)
            return BadRequest(new { message = "Provide voteType=Upvote|Downvote or isUpvote=true|false." });

        var route = await _context.GroupShortlistRoutes.FirstOrDefaultAsync(r => r.Id == routeId && r.GroupId == groupId);
        if (route == null) return NotFound("Route not found in shortlist.");

        var existingVote = await _context.RouteVotes.FirstOrDefaultAsync(v => v.GroupShortlistRouteId == routeId && v.UserId == userId.Value);
        if (existingVote != null)
        {
            existingVote.IsUpvote = parsedVote.Value;
            existingVote.VotedAt = DateTime.UtcNow;
        }
        else
        {
            _context.RouteVotes.Add(new RouteVote
            {
                Id = Guid.NewGuid(),
                GroupShortlistRouteId = routeId,
                UserId = userId.Value,
                IsUpvote = parsedVote.Value,
                VotedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        var updated = await _context.GroupShortlistRoutes
            .AsNoTracking()
            .Include(s => s.Votes)
            .FirstAsync(s => s.Id == routeId);

        return Ok(new
        {
            message = "Vote cast successfully",
            route = ToShortlistDto(updated, userId.Value)
        });
    }

    [HttpDelete("{groupId}/shortlist/{routeId}/vote")]
    [HttpDelete("{groupId}/routes/{routeId}/vote")]
    public async Task<IActionResult> ClearShortlistVote(Guid groupId, Guid routeId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var isMember = await IsGroupMemberAsync(groupId, userId.Value);
        if (!isMember) return Forbid();

        var route = await _context.GroupShortlistRoutes
            .Include(r => r.Votes)
            .FirstOrDefaultAsync(r => r.Id == routeId && r.GroupId == groupId);
        if (route == null) return NotFound("Route not found in shortlist.");

        var existingVote = route.Votes.FirstOrDefault(v => v.UserId == userId.Value);
        if (existingVote == null)
        {
            return Ok(new
            {
                message = "No existing vote to clear",
                route = ToShortlistDto(route, userId.Value)
            });
        }

        _context.RouteVotes.Remove(existingVote);
        await _context.SaveChangesAsync();

        var updated = await _context.GroupShortlistRoutes
            .AsNoTracking()
            .Include(s => s.Votes)
            .FirstAsync(s => s.Id == routeId);

        return Ok(new
        {
            message = "Vote cleared successfully",
            route = ToShortlistDto(updated, userId.Value)
        });
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new string(Enumerable.Repeat(chars, _inviteCodeSettings.CodeLength)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        return _inviteCodeSettings.Prefix + code;
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }

    private Task<bool> IsGroupMemberAsync(Guid groupId, int userId) =>
        _context.TravelGroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

    private static bool? ParseVote(VoteRequest request)
    {
        if (request.IsUpvote.HasValue) return request.IsUpvote.Value;
        if (string.IsNullOrWhiteSpace(request.VoteType)) return null;

        return request.VoteType.Trim().ToLowerInvariant() switch
        {
            "upvote" => true,
            "downvote" => false,
            _ => null
        };
    }

    private static object ToShortlistDto(GroupShortlistRoute route, int currentUserId)
    {
        var upvoters = route.Votes.Where(v => v.IsUpvote).Select(v => v.UserId).Distinct().ToList();
        var downvoters = route.Votes.Where(v => !v.IsUpvote).Select(v => v.UserId).Distinct().ToList();

        return new
        {
            id = route.Id,
            destinationId = route.DestinationId,
            addedByUserId = route.AddedByUserId,
            addedAt = route.AddedAt,
            upvotes = upvoters.Count,
            downvotes = downvoters.Count,
            upvoterIds = upvoters,
            downvoterIds = downvoters,
            currentUserVote = route.Votes
                .Where(v => v.UserId == currentUserId)
                .Select(v => v.IsUpvote ? "Upvote" : "Downvote")
                .FirstOrDefault(),
            votes = route.Votes.Select(v => new
            {
                userId = v.UserId,
                isUpvote = v.IsUpvote
            })
        };
    }
}
