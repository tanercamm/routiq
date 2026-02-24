using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

/// <summary>
/// Static, manually-maintained budget range per region + cost level.
/// This is the engine's single source of truth for cost estimation.
/// Values are intentionally ranges (min/max), never exact prices.
/// </summary>
public class RegionPriceTier
{
    public int Id { get; set; }

    /// <summary>Region name — must match Destination.Region exactly (used as a lookup key).</summary>
    [Required, MaxLength(80)]
    public string Region { get; set; } = string.Empty;

    public CostLevel CostLevel { get; set; }

    /// <summary>Minimum daily spend in USD for this tier (transport + food + accommodation).</summary>
    public int DailyBudgetUsdMin { get; set; }

    /// <summary>Maximum daily spend in USD for this tier.</summary>
    public int DailyBudgetUsdMax { get; set; }

    /// <summary>Short description surfaced in the UI to explain what the tier includes.</summary>
    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Admin audit: when this tier was last reviewed for accuracy.</summary>
    public DateTime LastReviewedAt { get; set; } = DateTime.UtcNow;
}
