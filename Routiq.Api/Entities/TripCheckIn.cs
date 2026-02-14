using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class TripCheckIn
{
    public Guid Id { get; set; }

    /// <summary>FK to the parent trip.</summary>
    public Guid UserTripId { get; set; }
    [ForeignKey(nameof(UserTripId))]
    public UserTrip? UserTrip { get; set; }

    /// <summary>FK to the attraction the user checked in at.</summary>
    public int AttractionId { get; set; }
    [ForeignKey(nameof(AttractionId))]
    public Attraction? Attraction { get; set; }

    public int EarnedPoints { get; set; } = 50;

    /// <summary>Optional text post earns bonus points (+100).</summary>
    public string? UserPostText { get; set; }

    public DateTime CheckInDate { get; set; } = DateTime.UtcNow;
}
