using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Routsky.Api.Configuration;
using Routsky.Api.Data;

namespace Routsky.Api.Services;

public interface IAnalyticsService
{
    Task<object> GetUserAnalyticsAsync(int userId);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly RoutskyDbContext _context;
    private readonly double _kgCo2PerRoute;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        RoutskyDbContext context,
        IOptions<CarbonEstimation> carbonEstimation,
        ILogger<AnalyticsService> logger)
    {
        _context = context;
        _kgCo2PerRoute = carbonEstimation.Value.KgCo2PerRoute;
        _logger = logger;
    }

    public async Task<object> GetUserAnalyticsAsync(int userId)
    {
        _logger.LogInformation("Computing analytics for user {UserId}", userId);

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        var savings = await CalculateUserSavingsAsync(userId);
        var carbonMetrics = await CalculateCarbonFootprintAsync(userId);
        var popularRegions = await CalculatePopularRegionsAsync(userId);

        var travelGroupsCount = await _context.TravelGroupMembers
            .Where(m => m.UserId == userId)
            .CountAsync();

        var totalTripsCount = await _context.SavedRoutes
            .Where(sr => sr.UserId == userId)
            .CountAsync();

        var recentActivity = await _context.SavedRoutes
            .Where(sr => sr.UserId == userId)
            .OrderByDescending(sr => sr.SavedAt)
            .Take(5)
            .Select(sr => new
            {
                routeName = sr.RouteName,
                savedAt = sr.SavedAt,
                status = sr.Status.ToString()
            })
            .ToListAsync();

        return new
        {
            totalGroupSavings = savings,
            carbonFootprintEstimate = carbonMetrics,
            popularRegions,
            travelGroupsCount,
            totalTripsCount,
            recentActivity,
            preferredCurrency = profile?.PreferredCurrency ?? "USD",
            unitPreference = profile?.UnitPreference ?? "Metric"
        };
    }

    private async Task<double> CalculateUserSavingsAsync(int userId)
    {
        return await _context.SavedRoutes
            .Include(sr => sr.RouteQuery)
            .Where(sr => sr.UserId == userId
                && (sr.Status == Entities.RouteStatus.Active
                    || sr.Status == Entities.RouteStatus.Traveled
                    || sr.Status == Entities.RouteStatus.Saved)
                && sr.RouteQuery != null && sr.RouteQuery.TotalBudgetUsd > 0)
            .SumAsync(sr => (double)sr.RouteQuery!.TotalBudgetUsd);
    }

    private async Task<object> CalculateCarbonFootprintAsync(int userId)
    {
        var userRoutesCount = await _context.SavedRoutes
           .Where(sr => sr.UserId == userId)
           .CountAsync();

        double estimatedKgCo2 = userRoutesCount * _kgCo2PerRoute;

        return new
        {
            totalKgCo2 = estimatedKgCo2,
            tripsTracked = userRoutesCount
        };
    }

    private async Task<List<object>> CalculatePopularRegionsAsync(int userId)
    {
        var regions = await _context.RouteStops
            .Include(rs => rs.SavedRoute)
            .Include(rs => rs.Destination)
            .Where(rs => rs.SavedRoute!.UserId == userId && rs.Destination != null)
            .GroupBy(rs => rs.Destination!.Region)
            .Select(g => new { Region = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return regions.Select(r => new { name = r.Region, value = r.Count } as object).ToList();
    }
}
