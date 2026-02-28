using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Routiq.Api.DTOs;
using Routiq.Api.Services;

namespace Routiq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponseDto>> GetMe()
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var response = await _authService.GetMeAsync(userId);

            // Explicitly map the URL
            var dbContext = HttpContext.RequestServices.GetRequiredService<Routiq.Api.Data.RoutiqDbContext>();
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                response.AvatarUrl = user.AvatarUrl;
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var response = await _authService.UpdateProfileAsync(userId, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [Authorize]
    [HttpPost("profile/avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/avatars/{uniqueFileName}";

            // Inject DbContext to update User entity
            var dbContext = HttpContext.RequestServices.GetRequiredService<Routiq.Api.Data.RoutiqDbContext>();
            var user = await dbContext.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.AvatarUrl = avatarUrl;
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            return Ok(new { avatarUrl = user.AvatarUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [Authorize]
    [HttpDelete("profile/avatar")]
    public async Task<IActionResult> RemoveAvatar()
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var dbContext = HttpContext.RequestServices.GetRequiredService<Routiq.Api.Data.RoutiqDbContext>();
            var user = await dbContext.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                // Physical file deletion
                var fileName = Path.GetFileName(user.AvatarUrl);
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars", fileName);

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                user.AvatarUrl = null;
                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync();
            }

            return Ok(new { avatarUrl = user.AvatarUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
