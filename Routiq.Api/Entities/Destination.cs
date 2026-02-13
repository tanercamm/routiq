using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

public class Destination
{
    public int Id { get; set; }
    
    [Required]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    public string City { get; set; } = string.Empty;
    
    public string Region { get; set; } = string.Empty;

    public decimal AvgDailyCostLow { get; set; }
    public decimal AvgDailyCostMid { get; set; }
    public decimal AvgDailyCostHigh { get; set; }

    public int PopularityScore { get; set; } // 1-10

    public string[] ClimateTags { get; set; } = Array.Empty<string>();

    public string? Notes { get; set; }
}
