using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Routsky.Api.Configuration;
using Routsky.Api.DTOs;
using Routsky.Api.Services;
using SixLabors.ImageSharp;

namespace Routsky.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAvatarService _avatarService;
    private readonly AvatarSettings _avatarSettings;

    public AuthController(
        IAuthService authService,
        IAvatarService avatarService,
        IOptions<AvatarSettings> avatarSettings)
    {
        _authService = authService;
        _avatarService = avatarService;
        _avatarSettings = avatarSettings.Value;
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

    public class GoogleAuthRequest
    {
        public string Credential { get; set; } = string.Empty;
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        try
        {
            var response = await _authService.HandleGoogleAuthAsync(request.Credential);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    public class GitHubAuthRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    [HttpPost("github")]
    public async Task<ActionResult<AuthResponseDto>> GitHubLogin([FromBody] GitHubAuthRequest request)
    {
        try
        {
            var response = await _authService.HandleGitHubAuthAsync(request.Code);
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

            var dbContext = HttpContext.RequestServices.GetRequiredService<Routsky.Api.Data.RoutskyDbContext>();
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                response.AvatarBase64 = user.AvatarBase64;
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
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        if (file.Length > _avatarSettings.MaxFileSizeBytes)
            return BadRequest(new { message = $"File exceeds {_avatarSettings.MaxFileSizeBytes / (1024 * 1024)} MB limit." });

        var ext = Path.GetExtension(file.FileName);
        if (!AvatarService.IsExtensionAllowed(ext))
            return BadRequest(new { message = $"Unsupported image format: {ext}" });

        try
        {
            var dataUri = await _avatarService.UploadAsync(userId, file);
            return Ok(new { avatarBase64 = dataUri });
        }
        catch (UnknownImageFormatException)
        {
            return BadRequest(new { message = "Cannot decode image. Supported: JPEG, PNG, WebP, GIF, BMP." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User not found");
        }
        catch (IOException ioEx)
        {
            return StatusCode(500, new { message = "Disk storage failure. Please check volume permissions.", details = ioEx.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            return StatusCode(500, new { message = "Database persistence failure.", details = dbEx.InnerException?.Message ?? dbEx.Message });
        }
    }

    [Authorize]
    [HttpDelete("profile/avatar")]
    public async Task<IActionResult> RemoveAvatar()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        try
        {
            await _avatarService.RemoveAsync(userId);
            return Ok(new { avatarUrl = (string?)null });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User not found");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
