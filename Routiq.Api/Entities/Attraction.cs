using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class Attraction
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Destination table.
    /// </summary>
    public int CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public Destination? City { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public decimal EstimatedCost { get; set; }

    public double EstimatedDurationInHours { get; set; }
}
