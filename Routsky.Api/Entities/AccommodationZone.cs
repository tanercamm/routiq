using System.ComponentModel.DataAnnotations;

namespace Routsky.Api.Entities;

/// <summary>
/// A named accommodation zone within a city, with cost tier and nightly rate.
/// Seeded from accommodation_zones.json.
/// </summary>
public class AccommodationZone
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string CityName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string ZoneName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Budget, Mid-Range, or Luxury.</summary>
    [Required, MaxLength(30)]
    public string Category { get; set; } = string.Empty;

    public double AverageNightlyCost { get; set; }

    [MaxLength(5)]
    public string Currency { get; set; } = "USD";
}
