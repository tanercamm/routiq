using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class DestinationTip
{
    public Guid Id { get; set; }

    /// <summary>FK to the destination city.</summary>
    public int CityId { get; set; }
    [ForeignKey(nameof(CityId))]
    public Destination? City { get; set; }

    /// <summary>FK to the user who wrote the tip.</summary>
    public Guid UserProfileId { get; set; }
    [ForeignKey(nameof(UserProfileId))]
    public UserProfile? UserProfile { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public int Upvotes { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
