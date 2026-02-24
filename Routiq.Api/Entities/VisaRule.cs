using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

/// <summary>
/// Visa requirement level for a passport → destination country pair.
/// This is the primary hard-filter signal in the route engine.
/// </summary>
public enum VisaRequirement
{
    VisaFree,       // No visa needed at all
    EVisa,          // Can apply online before travel
    OnArrival,      // Visa issued at border (generally easy)
    Required,       // Must apply at consulate in advance
    Banned          // Entry not permitted (e.g. diplomatic restrictions)
}

/// <summary>
/// Static, manually-maintained rule for passport X → destination country Y.
/// Hard filter: if Requirement == Required or Banned and user hasn't declared visa, destination is eliminated.
/// </summary>
public class VisaRule
{
    public int Id { get; set; }

    /// <summary>ISO 3166-1 alpha-2 code of the traveler's passport. E.g. "TR", "US", "IN".</summary>
    [Required, MaxLength(3)]
    public string PassportCountryCode { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 code of the destination country.</summary>
    [Required, MaxLength(3)]
    public string DestinationCountryCode { get; set; } = string.Empty;

    public VisaRequirement Requirement { get; set; }

    /// <summary>Maximum allowed stay in days. 0 = not applicable (e.g. Banned).</summary>
    public int MaxStayDays { get; set; }

    /// <summary>How many days it typically takes to get the visa approved. 0 for VisaFree/OnArrival.</summary>
    public int AvgProcessingDays { get; set; }

    /// <summary>Official eVisa portal URL if applicable. Null otherwise.</summary>
    public string? EVisaUrl { get; set; }

    /// <summary>Human-readable notes surfaced in the UI. E.g. "Onward ticket proof required."</summary>
    public string? Notes { get; set; }

    /// <summary>Admin audit: when this rule was last verified against official sources.</summary>
    public DateTime LastReviewedAt { get; set; } = DateTime.UtcNow;
}
