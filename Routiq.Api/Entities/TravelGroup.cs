using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

public class TravelGroup
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(8)]
    public string InviteCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TravelGroupMember> Members { get; set; } = new List<TravelGroupMember>();
}
