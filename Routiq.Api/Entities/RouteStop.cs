using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Routiq.Api.Entities;

/// <summary>
/// One city stop in a generated route. Ordered sequence of stops makes up a SavedRoute.
/// Replaces the ItinerarySnapshotJson blob with proper relational structure.
/// </summary>
public class RouteStop
{
    public int Id { get; set; }

    public Guid SavedRouteId { get; set; }
    [ForeignKey(nameof(SavedRouteId))]
    public SavedRoute? SavedRoute { get; set; }

    public int DestinationId { get; set; }
    [ForeignKey(nameof(DestinationId))]
    public Destination? Destination { get; set; }

    /// <summary>1-based ordering of this stop in the route. Stop 1 is the first city.</summary>
    public int StopOrder { get; set; }

    /// <summary>Engine-recommended days to spend at this stop.</summary>
    public int RecommendedDays { get; set; }

    /// <summary>Expected cost level at this stop — may differ from Destination.DailyCostLevel if context applies.</summary>
    public CostLevel ExpectedCostLevel { get; set; }

    /// <summary>
    /// Human-readable explanation of WHY this stop was included.
    /// E.g. "Budget buffer leg before expensive Japan segment."
    /// </summary>
    [MaxLength(500)]
    public string? StopReason { get; set; }
}
