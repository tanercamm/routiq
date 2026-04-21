using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routsky.Api.Data;
using Routsky.Api.Services;

namespace Routsky.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly RoutskyDbContext _context;
    private readonly IAuthService _authService;

    public UserController(RoutskyDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPatch("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound("User profile not found.");

        if (!string.IsNullOrWhiteSpace(request.PreferredCurrency))
            profile.PreferredCurrency = request.PreferredCurrency;

        if (!string.IsNullOrWhiteSpace(request.UnitPreference))
            profile.UnitPreference = request.UnitPreference;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Preferences updated successfully" });
    }

    [HttpPatch("notifications")]
    public async Task<IActionResult> UpdateNotifications([FromBody] UpdateNotificationsRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound("User profile not found.");

        if (request.NotificationsEnabled.HasValue)
            profile.NotificationsEnabled = request.NotificationsEnabled.Value;

        if (request.PriceAlertsEnabled.HasValue)
            profile.PriceAlertsEnabled = request.PriceAlertsEnabled.Value;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Notification settings updated successfully" });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Incorrect current password." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }
}

public class UpdatePreferencesRequest
{
    public string? PreferredCurrency { get; set; }
    public string? UnitPreference { get; set; }
}

public class UpdateNotificationsRequest
{
    public bool? NotificationsEnabled { get; set; }
    public bool? PriceAlertsEnabled { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
