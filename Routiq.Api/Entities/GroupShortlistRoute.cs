using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class GroupShortlistRoute
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }
    [ForeignKey(nameof(GroupId))]
    public TravelGroup? Group { get; set; }

    [Required]
    [MaxLength(10)]
    public string DestinationId { get; set; } = string.Empty; // e.g., "SIN", "SJJ"

    public int AddedByUserId { get; set; }
    [ForeignKey(nameof(AddedByUserId))]
    public User? AddedByUser { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<RouteVote> Votes { get; set; } = new List<RouteVote>();
}
