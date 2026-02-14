using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class AccommodationZone
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Destination table.
    /// </summary>
    public int CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public Destination? City { get; set; }

    [Required]
    public string ZoneName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty; // "Budget", "Mid-Range", "Luxury"

    public decimal AverageNightlyCost { get; set; }

    public string Currency { get; set; } = "USD";
}
