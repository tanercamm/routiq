using Microsoft.EntityFrameworkCore;
using Routsky.Api.Data;
using Routsky.Api.Entities;

namespace Routsky.Api.Services;

public interface ICommunityService
{
    Task<List<object>> GetDestinationFeedbackAsync(int destinationId);
    Task<object> GetDestinationStatsAsync(int destinationId);
}

public class CommunityService : ICommunityService
{
    private readonly RoutskyDbContext _context;
    private readonly ILogger<CommunityService> _logger;

    public CommunityService(RoutskyDbContext context, ILogger<CommunityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<object>> GetDestinationFeedbackAsync(int destinationId)
    {
        _logger.LogInformation("Fetching community feedback for destination {DestinationId}", destinationId);

        var records = await _context.TraveledRoutes
            .Include(tr => tr.SavedRoute)
                .ThenInclude(sr => sr!.Stops.Where(s => s.DestinationId == destinationId))
            .Where(tr => tr.SavedRoute!.Stops.Any(s => s.DestinationId == destinationId))
            .Select(tr => new
            {
                tr.TraveledAt,
                tr.TransportExpense,
                tr.FoodExpense,
                tr.AccommodationExpense,
                tr.VisaExperience,
                tr.WhyThisRegion,
                tr.WhatWasChallenging,
                tr.WhatIWouldDoDifferently,
                tr.SubmittedAt
            })
            .OrderByDescending(tr => tr.SubmittedAt)
            .ToListAsync();

        return records.Cast<object>().ToList();
    }

    public async Task<object> GetDestinationStatsAsync(int destinationId)
    {
        _logger.LogInformation("Computing destination stats for destination {DestinationId}", destinationId);

        var records = await _context.TraveledRoutes
            .Where(tr => tr.SavedRoute!.Stops.Any(s => s.DestinationId == destinationId))
            .ToListAsync();

        if (records.Count == 0)
            return new { Message = "No community records yet for this destination." };

        return new
        {
            TotalRecords = records.Count,
            TransportFeedback = GroupExpenseDensity(records.Select(r => r.TransportExpense)),
            FoodFeedback = GroupExpenseDensity(records.Select(r => r.FoodExpense)),
            AccomFeedback = GroupExpenseDensity(records.Select(r => r.AccommodationExpense)),
            VisaFeedback = GroupVisaDifficulty(records.Select(r => r.VisaExperience))
        };
    }

    private static object GroupExpenseDensity(IEnumerable<ExpenseDensity> values)
    {
        var list = values.ToList();
        return new
        {
            UnderBudget = (int)Math.Round(100.0 * list.Count(v => v == ExpenseDensity.UnderBudget) / list.Count),
            AsExpected = (int)Math.Round(100.0 * list.Count(v => v == ExpenseDensity.AsExpected) / list.Count),
            OverBudget = (int)Math.Round(100.0 * list.Count(v => v == ExpenseDensity.OverBudget) / list.Count)
        };
    }

    private static object GroupVisaDifficulty(IEnumerable<VisaDifficulty> values)
    {
        var list = values.ToList();
        return new
        {
            Easy = (int)Math.Round(100.0 * list.Count(v => v == VisaDifficulty.Easy) / list.Count),
            Moderate = (int)Math.Round(100.0 * list.Count(v => v == VisaDifficulty.Moderate) / list.Count),
            Hard = (int)Math.Round(100.0 * list.Count(v => v == VisaDifficulty.Hard) / list.Count),
            Failed = (int)Math.Round(100.0 * list.Count(v => v == VisaDifficulty.Failed) / list.Count)
        };
    }
}
