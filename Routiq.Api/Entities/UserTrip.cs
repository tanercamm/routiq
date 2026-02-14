using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class UserTrip
{
    public Guid Id { get; set; }

    /// <summary>FK to the owning UserProfile.</summary>
    public Guid UserProfileId { get; set; }
    [ForeignKey(nameof(UserProfileId))]
    public UserProfile? UserProfile { get; set; }

    /// <summary>FK to the destination city.</summary>
    public int DestinationCityId { get; set; }
    [ForeignKey(nameof(DestinationCityId))]
    public Destination? DestinationCity { get; set; }

    public decimal TotalBudget { get; set; }
    public int Days { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TripCheckIn> CheckIns { get; set; } = new List<TripCheckIn>();
}
