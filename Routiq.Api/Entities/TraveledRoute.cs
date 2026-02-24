using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

/// <summary>How the actual transport cost compared to the engine's prediction.</summary>
public enum ExpenseDensity { UnderBudget, AsExpected, OverBudget }

/// <summary>How difficult or smooth the visa process actually was.</summary>
public enum VisaDifficulty { Easy, Moderate, Hard, Failed }

/// <summary>
/// Community-submitted structured trip record.
/// Users CANNOT submit free-form blog posts — only structured enums + 3 capped free-text fields.
/// These records feed back into the heuristic recalibration system.
/// </summary>
public class TraveledRoute
{
    public Guid Id { get; set; }

    public Guid SavedRouteId { get; set; }
    [ForeignKey(nameof(SavedRouteId))]
    public SavedRoute? SavedRoute { get; set; }

    /// <summary>The actual travel start date (used for seasonal heuristics in the future).</summary>
    public DateTime TraveledAt { get; set; }

    // ── Structured expense feedback (feeds engine recalibration) ──
    public ExpenseDensity TransportExpense { get; set; }
    public ExpenseDensity FoodExpense { get; set; }
    public ExpenseDensity AccommodationExpense { get; set; }

    // ── Structured visa feedback ──
    public VisaDifficulty VisaExperience { get; set; }

    /// <summary>
    /// JSON array of per-stop day sufficiency feedback.
    /// Format: [{"stopOrder":1,"sufficiency":"JustRight"},{"stopOrder":2,"sufficiency":"TooFew"}]
    /// Enum values: TooFew | JustRight | TooMany
    /// </summary>
    public string DaySufficiencyJson { get; set; } = "[]";

    // ── Allowed free-text fields (Spec §4.2) — strictly capped ──

    /// <summary>Why did you choose this region? (max 500 chars)</summary>
    [MaxLength(500)]
    public string? WhyThisRegion { get; set; }

    /// <summary>What was unexpectedly challenging? (max 500 chars)</summary>
    [MaxLength(500)]
    public string? WhatWasChallenging { get; set; }

    /// <summary>What would you do differently? (max 500 chars)</summary>
    [MaxLength(500)]
    public string? WhatIWouldDoDifferently { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
