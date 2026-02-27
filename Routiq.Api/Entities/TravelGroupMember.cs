using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

public class TravelGroupMember
{
    public Guid GroupId { get; set; }
    [ForeignKey(nameof(GroupId))]
    public TravelGroup? Group { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
