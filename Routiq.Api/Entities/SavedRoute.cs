using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

/// <summary>Lifecycle state of a saved route.</summary>
public enum RouteStatus
{
    Saved,      // User saved it — intent signal
    Active,     // User is currently on this trip
    Traveled,   // User completed it and submitted a TraveledRoute record
    Archived    // User dismissed/archived it
}

/// <summary>
/// A route the user has saved from the engine's output.
/// Contains structured stops (RouteStop) — no JSON blobs.
/// The SelectionReason field surfaces the engine's "why this route" explanation in the UI.
/// </summary>
public class SavedRoute
{
    public Guid Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    /// <summary>The query parameters that generated this route.</summary>
    public Guid RouteQueryId { get; set; }
    [ForeignKey(nameof(RouteQueryId))]
    public RouteQuery? RouteQuery { get; set; }

    /// <summary>User-editable display name for this route.</summary>
    [Required, MaxLength(120)]
    public string RouteName { get; set; } = string.Empty;

    public RouteStatus Status { get; set; } = RouteStatus.Saved;

    /// <summary>
    /// Engine-generated explanation of WHY this route was selected over alternatives.
    /// E.g. "Visa-free for TR passport + fits $1500 budget + 14-day duration."
    /// </summary>
    [Required, MaxLength(600)]
    public string SelectionReason { get; set; } = string.Empty;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();

    /// <summary>Set when the user marks this route as traveled and submits a structured record.</summary>
    public TraveledRoute? TraveledRoute { get; set; }
}
