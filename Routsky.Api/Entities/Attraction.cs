using System.ComponentModel.DataAnnotations;

namespace Routsky.Api.Entities;

/// <summary>
/// A notable attraction or activity within a city, with cost and duration estimates.
/// Seeded from attractions.json.
/// </summary>
public class Attraction
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string CityName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public double EstimatedCost { get; set; }

    public double EstimatedDurationInHours { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Historical, Museum, Nature, or Entertainment.</summary>
    [Required, MaxLength(30)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Morning, Afternoon, Evening, or Anytime.</summary>
    [MaxLength(20)]
    public string BestTimeOfDay { get; set; } = string.Empty;
}
