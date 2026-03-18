using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Routsky.Api.Data;
using Routsky.Api.DTOs;
using Routsky.Api.Entities;

namespace Routsky.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> GetMeAsync(int userId);
    Task<AuthResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
    Microsoft.AspNetCore.Authentication.AuthenticationProperties GetSocialAuthProperties(string provider, string redirectUrl);
    Task<AuthResponseDto> HandleSocialAuthAsync(ClaimsPrincipal principal);
    string GetFrontendRedirectUrl();
}

public class AuthService : IAuthService
{
    private readonly RoutskyDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(RoutskyDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new Exception("User with this email already exists.");
        }

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var profile = new UserProfile
        {
            UserId = user.Id,
            Username = string.IsNullOrWhiteSpace(request.FirstName) ? request.Email.Split('@')[0] : request.FirstName,
            Email = request.Email,
            Passports = request.Passports != null && request.Passports.Any() ? request.Passports : new List<string> { "TR" },
            Origin = string.IsNullOrWhiteSpace(request.Origin)
                ? PassportHubResolver.Resolve(request.Passports?.FirstOrDefault() ?? "TR")
                : request.Origin
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return GenerateAuthResponse(user, profile);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password.");
        }

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

        return GenerateAuthResponse(user, profile);
    }

    public async Task<AuthResponseDto> GetMeAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        return GenerateAuthResponse(user, profile);
    }

    public async Task<AuthResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                Username = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email.Split('@')[0] : user.FirstName,
                Email = user.Email,
                Passports = request.Passports,
                Origin = string.IsNullOrWhiteSpace(request.Origin)
                    ? PassportHubResolver.Resolve(request.Passports?.FirstOrDefault() ?? "TR")
                    : request.Origin,
                PreferredCurrency = string.IsNullOrWhiteSpace(request.PreferredCurrency) ? "USD" : request.PreferredCurrency,
                UnitPreference = string.IsNullOrWhiteSpace(request.UnitPreference) ? "Metric" : request.UnitPreference,
                TravelStyle = string.IsNullOrWhiteSpace(request.TravelStyle) ? "Comfort" : request.TravelStyle,
                NotificationsEnabled = request.NotificationsEnabled,
                PriceAlertsEnabled = request.PriceAlertsEnabled
            };
            _context.UserProfiles.Add(profile);
        }
        else
        {
            profile.Passports = request.Passports;
            if (!string.IsNullOrWhiteSpace(request.Origin))
            {
                profile.Origin = request.Origin;
            }
            if (!string.IsNullOrWhiteSpace(request.PreferredCurrency))
            {
                profile.PreferredCurrency = request.PreferredCurrency;
            }
            if (!string.IsNullOrWhiteSpace(request.UnitPreference))
            {
                profile.UnitPreference = request.UnitPreference;
            }
            if (!string.IsNullOrWhiteSpace(request.TravelStyle))
            {
                profile.TravelStyle = request.TravelStyle;
            }
            // Update booleans directly
            profile.NotificationsEnabled = request.NotificationsEnabled;
            profile.PriceAlertsEnabled = request.PriceAlertsEnabled;
        }

        await _context.SaveChangesAsync();

        return GenerateAuthResponse(user, profile);
    }

    public Microsoft.AspNetCore.Authentication.AuthenticationProperties GetSocialAuthProperties(string provider, string redirectUrl)
    {
        return new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
    }

    public async Task<AuthResponseDto> HandleSocialAuthAsync(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            throw new Exception("Email not provided by social provider.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        bool isNewUser = user == null;

        if (isNewUser)
        {
            var name = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue(ClaimTypes.GivenName);
            var parts = name?.Split(' ', 2);
            
            user = new User
            {
                Email = email,
                FirstName = parts?.Length > 0 ? parts[0] : "",
                LastName = parts?.Length > 1 ? parts[1] : "",
                PasswordHash = "OAUTH_LOGIN", // Placeholder for external users
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.Id,
                Username = email.Split('@')[0],
                Email = email,
                Passports = new List<string> { "TR" },
                Origin = "IST" // Default for Routsky
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        var userProfile = await _context.UserProfiles.FirstAsync(p => p.UserId == user!.Id);
        return GenerateAuthResponse(user!, userProfile);
    }

    public string GetFrontendRedirectUrl()
    {
        return _configuration["FrontendUrl"] ?? "https://routsky.xyz";
    }

    private AuthResponseDto GenerateAuthResponse(User user, UserProfile? profile)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "SuperSecretKeyForDevelopmentOnly123!"); // Move to appsettings
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponseDto
        {
            Id = user.Id,
            Token = tokenHandler.WriteToken(token),
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}".Trim(),
            Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                AvatarBase64 = user.AvatarBase64,
            Passports = profile?.Passports ?? new List<string> { "TR" },
            Origin = profile?.Origin ?? "",
            PreferredCurrency = profile?.PreferredCurrency ?? "USD",
            UnitPreference = profile?.UnitPreference ?? "Metric",
            TravelStyle = profile?.TravelStyle ?? "Comfort",
            NotificationsEnabled = profile?.NotificationsEnabled ?? true,
            PriceAlertsEnabled = profile?.PriceAlertsEnabled ?? true
        };
    }
}
