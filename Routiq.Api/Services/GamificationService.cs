using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.DTOs;
using Routiq.Api.Entities;

namespace Routiq.Api.Services;

public interface IGamificationService
{
    /// <summary>
    /// Awards check-in points: base 50 pts, +100 bonus if userPostText is provided.
    /// </summary>
    Task AwardCheckInPointsAsync(Guid userProfileId, int attractionId, string? userPostText = null);

    /// <summary>
    /// Returns the top travelers ordered by TotalPoints descending.
    /// </summary>
    Task<List<LeaderboardDto>> GetTopTravelersAsync(int topCount = 10);

    /// <summary>
    /// Returns the Top 3 users with a mock coupon code reward.
    /// </summary>
    Task<List<LeaderboardWinnerDto>> GetLeaderboardWinnersAsync();
}

public class GamificationService : IGamificationService
{
    private const int BaseCheckInPoints = 50;
    private const int TextPostBonusPoints = 100;
    private const string MockCouponCode = "ROUTIQ-TOP3-DISCOUNT";

    private readonly RoutiqDbContext _context;

    public GamificationService(RoutiqDbContext context)
    {
        _context = context;
    }

    public async Task AwardCheckInPointsAsync(Guid userProfileId, int attractionId, string? userPostText = null)
    {
        var profile = await _context.UserProfiles.FindAsync(userProfileId)
            ?? throw new Exception($"UserProfile {userProfileId} not found.");

        // Find the user's most recent trip to attach the check-in to
        var latestTrip = await _context.UserTrips
            .Where(t => t.UserProfileId == userProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new Exception("No active trip found for this user.");

        // Verify the attraction exists
        var attraction = await _context.Attractions.FindAsync(attractionId)
            ?? throw new Exception($"Attraction {attractionId} not found.");

        // Calculate points: base + bonus for text post
        var points = BaseCheckInPoints;
        if (!string.IsNullOrWhiteSpace(userPostText))
        {
            points += TextPostBonusPoints;
        }

        // Create the check-in record
        var checkIn = new TripCheckIn
        {
            Id = Guid.NewGuid(),
            UserTripId = latestTrip.Id,
            AttractionId = attractionId,
            EarnedPoints = points,
            UserPostText = userPostText?.Trim(),
            CheckInDate = DateTime.UtcNow
        };

        _context.TripCheckIns.Add(checkIn);

        // Award points to the profile
        profile.TotalPoints += points;

        await _context.SaveChangesAsync();
    }

    public async Task<List<LeaderboardDto>> GetTopTravelersAsync(int topCount = 10)
    {
        return await _context.UserProfiles
            .OrderByDescending(p => p.TotalPoints)
            .Take(topCount)
            .Select(p => new LeaderboardDto
            {
                Username = p.Username,
                CountryCode = p.CountryCode,
                Age = p.Age,
                TotalPoints = p.TotalPoints,
                TripCount = p.Trips.Count,
                CheckInCount = p.Trips.SelectMany(t => t.CheckIns).Count()
            })
            .ToListAsync();
    }

    public async Task<List<LeaderboardWinnerDto>> GetLeaderboardWinnersAsync()
    {
        return await _context.UserProfiles
            .OrderByDescending(p => p.TotalPoints)
            .Take(3)
            .Select(p => new LeaderboardWinnerDto
            {
                Username = p.Username,
                CountryCode = p.CountryCode,
                TotalPoints = p.TotalPoints,
                CouponCode = MockCouponCode
            })
            .ToListAsync();
    }
}
