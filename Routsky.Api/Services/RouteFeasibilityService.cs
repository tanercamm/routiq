using Microsoft.SemanticKernel;

namespace Routsky.Api.Services;

/// <summary>
/// MCP Atom #1: Route Feasibility
/// Input (Origin, Destination, Passports) → Output (FlightTime, Cost, VisaInfo).
/// Flight data sourced from HybridFlightPriceService (TK + Gemini).
/// Visa data sourced from TravelBuddyApiService (live API).
/// </summary>
public class RouteFeasibilityService
{
    private readonly TravelBuddyApiService _travelBuddy;
    private readonly HybridFlightPriceService _flightService;
    private readonly ILogger<RouteFeasibilityService> _logger;

    public RouteFeasibilityService(
        TravelBuddyApiService travelBuddy,
        HybridFlightPriceService flightService,
        ILogger<RouteFeasibilityService> logger)
    {
        _travelBuddy = travelBuddy;
        _flightService = flightService;
        _logger = logger;
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
    }

    /// <summary>
    /// Pre-fetch flight estimates for many routes via the hybrid service.
    /// </summary>
    public async Task PreloadFlightEstimatesAsync(List<(string Origin, string Destination)> routePairs)
    {
        await _flightService.PreloadBatchAsync(routePairs);
    }

    /// <summary>
    /// Analyse feasibility of a single origin→destination route for a set of passports.
    /// </summary>
    [Microsoft.SemanticKernel.KernelFunction("GetFlightAndVisaFacts")]
    [System.ComponentModel.Description("Gets flight time, cost, and visa requirements for a given origin, destination, and passports.")]
    public async Task<FeasibilityResult> AnalyseAsync(
        [System.ComponentModel.Description("Origin airport code")] string origin,
        [System.ComponentModel.Description("Destination airport code")] string destination,
        [System.ComponentModel.Description("List of passport country codes")] List<string> passportCodes,
        [System.ComponentModel.Description("Destination country code")] string destinationCountryCode)
    {
        var result = new FeasibilityResult
        {
            Origin = origin,
            Destination = destination
        };

        // ── 1. Flight time & cost from Hybrid service (TK → Gemini fallback) ──
        var estimate = await _flightService.EstimateAsync(origin, destination);
        result.FlightTimeMinutes = estimate.FlightTimeMinutes;
        result.FlightTimeFormatted = $"{estimate.FlightTimeMinutes / 60}h {estimate.FlightTimeMinutes % 60:D2}m";
        result.EstimatedCostUsd = estimate.CostUsd;

        // ── 2. Visa check from Travel Buddy API ──
        var visaType = "VisaFree";
        var isVisaRequired = false;

        foreach (var passport in passportCodes)
        {
            var visaResult = await _travelBuddy.CheckVisaAsync(passport, destinationCountryCode);

            visaType = TravelBuddyApiService.MapColorToVisaType(visaResult.Color);
            isVisaRequired = TravelBuddyApiService.IsVisaRequired(visaResult.Color);

            // If any passport grants visa-free or on-arrival access, use that
            if (visaResult.Color is "green" or "blue")
            {
                visaType = TravelBuddyApiService.MapColorToVisaType(visaResult.Color);
                isVisaRequired = false;
                break;
            }
        }

        result.VisaRequired = isVisaRequired;
        result.VisaType = visaType;

        return result;
    }
}
