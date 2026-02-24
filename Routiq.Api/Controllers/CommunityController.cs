using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.Entities;

namespace Routiq.Api.Controllers;

/// <summary>
/// V2 Community Controller.
/// The community loop is for data validation, NOT social content.
/// Endpoints surface structured TraveledRoute feedback — no free-text feeds.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly RoutiqDbContext _context;

    public CommunityController(RoutiqDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all submitted TraveledRoute records for a destination, showing structured community feedback.
    /// </summary>
    [HttpGet("feedback/{destinationId:int}")]
    public async Task<IActionResult> GetDestinationFeedback(int destinationId)
    {
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

        return Ok(records);
    }

    /// <summary>
    /// Returns aggregated expense density stats for a destination (used to recalibrate heuristics).
    /// </summary>
    [HttpGet("stats/{destinationId:int}")]
    public async Task<IActionResult> GetDestinationStats(int destinationId)
    {
        var records = await _context.TraveledRoutes
            .Where(tr => tr.SavedRoute!.Stops.Any(s => s.DestinationId == destinationId))
            .ToListAsync();

        if (!records.Any())
            return Ok(new { Message = "No community records yet for this destination." });

        var total = records.Count;

        return Ok(new
        {
            TotalRecords = total,
            TransportFeedback = GroupExpenseDensity(records.Select(r => r.TransportExpense)),
            FoodFeedback = GroupExpenseDensity(records.Select(r => r.FoodExpense)),
            AccomFeedback = GroupExpenseDensity(records.Select(r => r.AccommodationExpense)),
            VisaFeedback = GroupVisaDifficulty(records.Select(r => r.VisaExperience))
        });
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
