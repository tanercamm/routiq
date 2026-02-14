using Routiq.Api.Entities;

namespace Routiq.Api.DTOs;

public class RouteRequestDto
{
    public string PassportCountry { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public int DurationDays { get; set; }
}

public class RouteResponseDto
{
    public List<RouteOptionDto> Options { get; set; } = new();
}

public class RouteOptionDto
{
    public string RouteType { get; set; } = string.Empty; // "Single City", "Two-City Combo", "Economy Saver"
    public string Description { get; set; } = string.Empty;
    public decimal TotalEstimatedCost { get; set; }
    public List<RouteStopDto> Stops { get; set; } = new();
}

public class RouteStopDto
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Days { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Climate { get; set; } = string.Empty;
    public string VisaStatus { get; set; } = string.Empty;
}

public class LeaderboardDto
{
    public string Username { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public int? Age { get; set; }
    public int TotalPoints { get; set; }
    public int TripCount { get; set; }
    public int CheckInCount { get; set; }
}

public class DestinationTipDto
{
    public string Username { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaderboardWinnerDto
{
    public string Username { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public string CouponCode { get; set; } = string.Empty;
}
