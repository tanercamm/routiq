using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.Entities;

namespace Routiq.Api.Services;

/// <summary>
/// MCP Atom #1: Route Feasibility
/// Stateless service. Input (Origin, Destination, Passports) → Output (FlightTime, Cost, VisaInfo).
/// Uses DB VisaRules for visa checks and distance-based estimates for flight data.
/// </summary>
public class RouteFeasibilityService
{
    private readonly RoutiqDbContext _context;

    public RouteFeasibilityService(RoutiqDbContext context)
    {
        _context = context;
    }

    public class FeasibilityResult
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int FlightTimeMinutes { get; set; }
        public string FlightTimeFormatted { get; set; } = string.Empty;
        public int EstimatedCostUsd { get; set; }
        public bool VisaRequired { get; set; }
        public string VisaType { get; set; } = "VisaFree";
        public bool IsFeasible { get; set; } = true;
        public string? BlockReason { get; set; }
    }

    /// <summary>
    /// Analyse feasibility of a single origin→destination route for a set of passports.
    /// </summary>
    public async Task<FeasibilityResult> AnalyseAsync(string origin, string destination, List<string> passportCodes, string destinationCountryCode)
    {
        var result = new FeasibilityResult
        {
            Origin = origin,
            Destination = destination
        };

        // ── 1. Flight time & cost estimation (distance-based heuristics) ──
        var estimate = EstimateFlightData(origin, destination);
        result.FlightTimeMinutes = estimate.minutes;
        result.FlightTimeFormatted = $"{estimate.minutes / 60}h {estimate.minutes % 60:D2}m";
        result.EstimatedCostUsd = estimate.costUsd;

        // ── 2. Visa check from DB ──
        var visaBlocked = false;
        var visaType = "VisaFree";

        foreach (var passport in passportCodes)
        {
            var rule = await _context.VisaRules
                .FirstOrDefaultAsync(v => v.PassportCountryCode == passport && v.DestinationCountryCode == destinationCountryCode);

            if (rule != null)
            {
                visaType = rule.Requirement.ToString();
                if (rule.Requirement == VisaRequirement.Banned)
                {
                    visaBlocked = true;
                    result.BlockReason = $"Entry banned for {passport} passport holders";
                    break;
                }
                if (rule.Requirement == VisaRequirement.Required)
                {
                    result.VisaRequired = true;
                    // Not blocked, but flagged — reduces score
                }
                if (rule.Requirement == VisaRequirement.VisaFree || rule.Requirement == VisaRequirement.OnArrival)
                {
                    // Best case — no penalty
                    visaType = rule.Requirement.ToString();
                    break; // One good passport is enough
                }
            }
        }

        result.VisaType = visaType;
        result.IsFeasible = !visaBlocked;

        return result;
    }

    /// <summary>
    /// Distance-based flight estimation using known airport pairs.
    /// This replaces the old hardcoded getFlightData() in the frontend.
    /// </summary>
    private static (int minutes, int costUsd) EstimateFlightData(string origin, string destination)
    {
        // Known route estimates (great-circle-based approximations)
        var routes = new Dictionary<string, (int min, int cost)>(StringComparer.OrdinalIgnoreCase)
        {
            // SIN routes
            ["SYD-SIN"] = (500, 700),
            ["MEL-SIN"] = (510, 720),
            ["IST-SIN"] = (550, 850),
            ["BER-SIN"] = (735, 1100),
            ["FRA-SIN"] = (730, 1050),
            ["BKK-SIN"] = (150, 200),
            ["KUL-SIN"] = (60, 80),

            // SJJ (Sarajevo) routes
            ["SYD-SJJ"] = (1350, 1800),
            ["MEL-SJJ"] = (1380, 1850),
            ["IST-SJJ"] = (120, 200),
            ["BER-SJJ"] = (130, 250),
            ["FRA-SJJ"] = (140, 280),

            // GYD (Baku) routes
            ["SYD-GYD"] = (1220, 1500),
            ["MEL-GYD"] = (1240, 1550),
            ["IST-GYD"] = (170, 150),
            ["BER-GYD"] = (375, 450),
            ["FRA-GYD"] = (365, 420),

            // CMN (Casablanca) routes
            ["SYD-CMN"] = (1560, 2100),
            ["MEL-CMN"] = (1580, 2150),
            ["IST-CMN"] = (315, 450),
            ["BER-CMN"] = (270, 380),
            ["FRA-CMN"] = (230, 350),

            // BKK (Bangkok) routes
            ["SYD-BKK"] = (540, 600),
            ["MEL-BKK"] = (560, 620),
            ["IST-BKK"] = (570, 750),
            ["BER-BKK"] = (640, 900),
            ["FRA-BKK"] = (630, 880),

            // TBS (Tbilisi) routes
            ["SYD-TBS"] = (1100, 1400),
            ["IST-TBS"] = (110, 180),
            ["BER-TBS"] = (280, 350),
            ["FRA-TBS"] = (270, 340),
        };

        var key = $"{origin.ToUpperInvariant()}-{destination.ToUpperInvariant()}";
        if (routes.TryGetValue(key, out var known))
            return known;

        // Reverse lookup
        var reverseKey = $"{destination.ToUpperInvariant()}-{origin.ToUpperInvariant()}";
        if (routes.TryGetValue(reverseKey, out var reverseKnown))
            return reverseKnown;

        // Default fallback for unknown routes
        return (600, 1000);
    }
}
