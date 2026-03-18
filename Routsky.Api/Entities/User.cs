using System.ComponentModel.DataAnnotations;

namespace Routsky.Api.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = "User"; // "Admin", "User"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? AvatarUrl { get; set; }

    // Base64 data URI for embedded avatars (e.g. "data:image/webp;base64,...")
    public string? AvatarBase64 { get; set; }

    // Navigation
    public UserProfile? Profile { get; set; }
    public ICollection<TravelGroupMember> GroupMemberships { get; set; } = new List<TravelGroupMember>();
}
