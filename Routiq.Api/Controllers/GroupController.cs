using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.Entities;

namespace Routiq.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly RoutiqDbContext _context;

    public GroupController(RoutiqDbContext context)
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

        var groups = await _context.TravelGroupMembers
            .Where(m => m.UserId == userId)
            .Include(m => m.Group)
                .ThenInclude(g => g.Members)
                    .ThenInclude(gm => gm.User)
            .Select(m => m.Group)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var result = groups.Select(g => new
        {
            id = g.Id,
            name = g.Name,
            inviteCode = g.InviteCode,
            members = g.Members.Count,
            isEngineReady = g.Members.Count > 1, // At least 2 members for intersection logic 
            avatars = g.Members.Select(m =>
                $"https://ui-avatars.com/api/?name={Uri.EscapeDataString((m.User?.FirstName + " " + m.User?.LastName).Trim())}&background=random&color=fff"
            ).ToList()
        });

        return Ok(result);
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
