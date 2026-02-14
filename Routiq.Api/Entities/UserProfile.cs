using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class UserProfile
{
    public Guid Id { get; set; }

    /// <summary>FK to the authentication User entity.</summary>
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PassportCountry { get; set; } = "Turkey";
    public string PreferredCurrency { get; set; } = "USD";
    public string CountryCode { get; set; } = "TR"; // 2-letter ISO for emoji flag
    public int? Age { get; set; }

    public int TotalPoints { get; set; } = 0;

    // Navigation
    public ICollection<UserTrip> Trips { get; set; } = new List<UserTrip>();
    public ICollection<DestinationTip> Tips { get; set; } = new List<DestinationTip>();
}
