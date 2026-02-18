using Routiq.Api.DTOs;

namespace Routiq.Api.DTOs;

public class SaveRouteDto
{
    public int UserId { get; set; }
    public int DestinationCityId { get; set; }
    public decimal TotalBudget { get; set; }
    public int Days { get; set; }
    public RouteOptionDto RouteDetails { get; set; } = new();
}

public class UserTripDto
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string DestinationCity { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public int Days { get; set; }
    public DateTime CreatedAt { get; set; }
    public RouteOptionDto? RouteDetails { get; set; }
}
