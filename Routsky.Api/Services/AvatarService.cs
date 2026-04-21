using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Routsky.Api.Configuration;
using Routsky.Api.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Routsky.Api.Services;

public interface IAvatarService
{
    Task<string> UploadAsync(int userId, IFormFile file);
    Task RemoveAsync(int userId);
}

public class AvatarService : IAvatarService
{
    private readonly RoutskyDbContext _context;
    private readonly AvatarSettings _settings;
    private readonly ILogger<AvatarService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".heic", ".heif" };

    public AvatarService(
        RoutskyDbContext context,
        IOptions<AvatarSettings> settings,
        ILogger<AvatarService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public long MaxFileSizeBytes => _settings.MaxFileSizeBytes;

    public static bool IsExtensionAllowed(string extension) =>
        AllowedExtensions.Contains(extension);

    public async Task<string> UploadAsync(int userId, IFormFile file)
    {
        _logger.LogInformation("Processing avatar upload for user {UserId}", userId);

        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new SixLabors.ImageSharp.Size(_settings.ResizePx, _settings.ResizePx),
            Mode = ResizeMode.Crop
        }));

        await using var ms = new MemoryStream();
        await image.SaveAsWebpAsync(ms, new WebpEncoder { Quality = _settings.WebpQuality });
        ms.Seek(0, SeekOrigin.Begin);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var dataUri = $"data:image/webp;base64,{base64}";

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
            if (File.Exists(oldFile))
                File.Delete(oldFile);
        }

        user.AvatarBase64 = dataUri;
        user.AvatarUrl = null;
        _context.Users.Update(user);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.ProfilePictureBase64 = dataUri;
            profile.ProfilePictureUrl = null;
            _context.UserProfiles.Update(profile);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Avatar uploaded successfully for user {UserId}", userId);
        return dataUri;
    }

    public async Task RemoveAsync(int userId)
    {
        _logger.LogInformation("Removing avatar for user {UserId}", userId);

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var fileName = Path.GetFileName(user.AvatarUrl);
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars", fileName);

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }

        user.AvatarUrl = null;
        user.AvatarBase64 = null;
        _context.Users.Update(user);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.ProfilePictureUrl = null;
            profile.ProfilePictureBase64 = null;
            _context.UserProfiles.Update(profile);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Avatar removed for user {UserId}", userId);
    }
}
