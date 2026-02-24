using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

/// <summary>
/// Cost bracket for a destination or region. Never use exact decimal prices.
/// </summary>
public enum CostLevel { Low, Medium, High }

/// <summary>
/// A city that can be part of a generated route.
/// Cost is expressed as a tier (Low/Medium/High) — never as an exact price.
/// </summary>
public class Destination
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string CountryCode { get; set; } = string.Empty; // ISO 3166-1 alpha-2, e.g. "TH"

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Region { get; set; } = string.Empty; // e.g. "Southeast Asia", "Balkans"

    /// <summary>The daily cost tier for this city. Used by the route engine for budget filtering.</summary>
    public CostLevel DailyCostLevel { get; set; }

    /// <summary>Minimum days the engine will allocate to this city in a generated route.</summary>
    public int MinRecommendedDays { get; set; } = 2;

    /// <summary>Soft upper ceiling for days at this city.</summary>
    public int MaxRecommendedDays { get; set; } = 7;

    /// <summary>
    /// Weight used by the route engine scoring. Increases as community validates the city via TraveledRoute records.
    /// Default 1.0 — no community signal yet.
    /// </summary>
    public double PopularityWeight { get; set; } = 1.0;

    /// <summary>Admin-authored notes about this destination (quirks, best season, etc.).</summary>
    public string? Notes { get; set; }

    /// <summary>Soft-delete flag. Inactive destinations are excluded from route generation.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
    public ICollection<RouteElimination> Eliminations { get; set; } = new List<RouteElimination>();
}
