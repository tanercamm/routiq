using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

/// <summary>
/// Extended profile for a registered user.
/// V2: Gamification points removed. Community loop is validation, not social scoring.
/// </summary>
public class UserProfile
{
    public Guid Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required, MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 passport country. Used as the default in RouteQuery generation.</summary>
    [MaxLength(3)]
    public string PassportCountryCode { get; set; } = "TR";

    /// <summary>Preferred display currency for the UI (does not affect engine logic — engine uses USD).</summary>
    [MaxLength(3)]
    public string PreferredCurrency { get; set; } = "USD";

    /// <summary>ISO 3166-1 alpha-2 for emoji flag rendering in the UI.</summary>
    [MaxLength(3)]
    public string CountryCode { get; set; } = "TR";

    public int? Age { get; set; }

    // Navigation
    public ICollection<SavedRoute> SavedRoutes { get; set; } = new List<SavedRoute>();
}
