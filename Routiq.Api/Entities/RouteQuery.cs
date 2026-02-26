using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

/// <summary>
/// Budget bracket labels — used instead of raw decimal amounts in queries.
/// The engine maps these to RegionPriceTier ranges.
/// </summary>
public enum BudgetBracket
{
    Shoestring,  // ~$0–30/day
    Budget,      // ~$30–60/day
    Mid,         // ~$60–120/day
    Comfort,     // ~$120–250/day
    Luxury       // $250+/day
}

/// <summary>
/// Geographic region preference for route generation.
/// </summary>
public enum RegionPreference
{
    Any,
    SoutheastAsia,
    EasternEurope,
    Balkans,
    LatinAmerica,
    NorthAfrica,
    CentralAsia,
    CentralAmerica,
    MiddleEast,
    Caribbean
}

/// <summary>
/// A structured set of inputs submitted by the user to generate a route.
/// Replaces the old UserRequest. All fields are typed — no free text.
/// </summary>
public class RouteQuery
{
    public Guid Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 passport codes for all passports the traveler holds.
    /// The engine applies best-case visa logic across all passports.
    /// e.g. ["TR", "DE"] for a Turkish-German dual citizen.
    /// </summary>
    [Required]
    public List<string> Passports { get; set; } = new();

    public BudgetBracket BudgetBracket { get; set; }

    /// <summary>Hard upper budget cap in USD. The engine eliminates any route exceeding this.</summary>
    public int TotalBudgetUsd { get; set; }

    /// <summary>Total trip duration in days.</summary>
    public int DurationDays { get; set; }

    public RegionPreference RegionPreference { get; set; } = RegionPreference.Any;

    // ── Visa context declared by the user ──
    /// <summary>User declares they already hold a valid Schengen visa.</summary>
    public bool HasSchengenVisa { get; set; }

    /// <summary>User declares they already hold a valid US visa.</summary>
    public bool HasUsVisa { get; set; }

    /// <summary>User declares they already hold a valid UK visa.</summary>
    public bool HasUkVisa { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<SavedRoute> SavedRoutes { get; set; } = new List<SavedRoute>();
    public ICollection<RouteElimination> Eliminations { get; set; } = new List<RouteElimination>();
}
