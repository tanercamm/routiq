using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

public class Flight
{
    public int Id { get; set; }

    [Required]
    public string Origin { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string AirlineName { get; set; } = string.Empty;

    public TimeSpan DepartureTime { get; set; }

    public decimal AveragePrice { get; set; }

    public decimal MinPrice { get; set; }

    public decimal MaxPrice { get; set; }

    public string Currency { get; set; } = "USD";
}
