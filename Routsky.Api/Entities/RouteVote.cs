using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routsky.Api.Entities;

public class RouteVote
{
    public Guid Id { get; set; }

    public Guid GroupShortlistRouteId { get; set; }
    [ForeignKey(nameof(GroupShortlistRouteId))]
    public GroupShortlistRoute? ShortlistRoute { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public bool IsUpvote { get; set; }

    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}
