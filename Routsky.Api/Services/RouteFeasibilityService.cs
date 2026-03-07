using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Routsky.Api.Data;
using Routsky.Api.Entities;

namespace Routsky.Api.Services;

/// <summary>
/// MCP Atom #1: Route Feasibility
/// Input (Origin, Destination, Passports) → Output (FlightTime, Cost, VisaInfo).
/// Flight data is synthesized by Gemini AI. Visa data comes from the DB.
/// Acts as a Semantic Kernel Plugin to feed facts to the Agent Orchestrator.
/// </summary>
public class RouteFeasibilityService
{
    private readonly RoutskyDbContext _context;
    private readonly Kernel _kernel;
    private readonly ILogger<RouteFeasibilityService> _logger;
    private readonly Dictionary<string, (int Minutes, int CostUsd)> _flightCache = new(StringComparer.OrdinalIgnoreCase);

    public RouteFeasibilityService(RoutskyDbContext context, Kernel kernel, ILogger<RouteFeasibilityService> logger)
    {
        _context = context;
        _kernel = kernel;
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
    /// Pre-fetch flight estimates for many routes in a single Gemini API call.
    /// Call this before a batch of AnalyseAsync calls to avoid per-pair overhead.
    /// </summary>
    public async Task PreloadFlightEstimatesAsync(List<(string Origin, string Destination)> routePairs)
    {
        var uncached = routePairs
            .Select(p => (Origin: p.Origin.ToUpperInvariant(), Destination: p.Destination.ToUpperInvariant()))
            .Distinct()
            .Where(p => !_flightCache.ContainsKey($"{p.Origin}-{p.Destination}"))
            .ToList();

        if (uncached.Count == 0) return;

        _logger.LogInformation(
            "[GeminiClient] Request sent to Google AI — batch estimating {Count} flight routes",
            uncached.Count);

        var routeLines = string.Join("\n",
            uncached.Select((p, i) => $"  {i + 1}. {p.Origin} → {p.Destination}"));

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage(
$@"You are an aviation data expert. For each route below, estimate the approximate one-way flight time in minutes and the average economy round-trip ticket cost in USD.

Routes:
{routeLines}

Respond STRICTLY with a JSON array, no markdown wrapping, no explanation:
[{{""origin"":""XXX"",""destination"":""YYY"",""minutes"":N,""costUsd"":N}}, ...]");

        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object> { { "temperature", 0.1 }, { "topP", 0.9 } }
        };

        var response = await chatService.GetChatMessageContentAsync(history, settings);
        var json = StripMarkdownFences(response.Content?.Trim() ?? "[]");

        _logger.LogInformation(
            "[GeminiClient] Batch response received from Google AI ({Length} chars, {RouteCount} routes requested)",
            json.Length, uncached.Count);

        try
        {
            var estimates = JsonSerializer.Deserialize<List<BatchFlightEstimate>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (estimates != null)
            {
                foreach (var est in estimates)
                {
                    if (!string.IsNullOrEmpty(est.Origin) && !string.IsNullOrEmpty(est.Destination) && est.Minutes > 0)
                    {
                        var key = $"{est.Origin.ToUpperInvariant()}-{est.Destination.ToUpperInvariant()}";
                        _flightCache[key] = (est.Minutes, est.CostUsd);
                    }
                }
            }

            _logger.LogInformation("[GeminiClient] Cached {Count}/{Total} flight estimates from Gemini batch",
                _flightCache.Count, uncached.Count);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[GeminiClient] Failed to parse batch flight estimate response from Google AI");
            throw new InvalidOperationException(
                "Gemini AI returned an unparseable flight estimate response. No local fallback.", ex);
        }
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

        // ── 1. Flight time & cost from Gemini AI ──
        var estimate = await GetFlightEstimateAsync(origin, destination);
        result.FlightTimeMinutes = estimate.Minutes;
        result.FlightTimeFormatted = $"{estimate.Minutes / 60}h {estimate.Minutes % 60:D2}m";
        result.EstimatedCostUsd = estimate.CostUsd;

        // ── 2. Visa check from DB ──
        var visaType = "VisaFree";
        var isVisaRequired = false;

        foreach (var passport in passportCodes)
        {
            var rule = await _context.VisaRules
                .FirstOrDefaultAsync(v => v.PassportCountryCode == passport && v.DestinationCountryCode == destinationCountryCode);

            if (rule != null)
            {
                visaType = rule.Requirement.ToString();
                if (rule.Requirement == VisaRequirement.Banned || rule.Requirement == VisaRequirement.Required)
                {
                    isVisaRequired = true;
                }

                if (rule.Requirement == VisaRequirement.VisaFree || rule.Requirement == VisaRequirement.OnArrival)
                {
                    visaType = rule.Requirement.ToString();
                    isVisaRequired = false;
                    break;
                }
            }
            else if (passport == "TR")
            {
                if (new[] { "FR", "ES", "IT", "DE", "NL", "CH", "AT", "CZ", "PL", "HU" }.Contains(destinationCountryCode))
                {
                    isVisaRequired = true;
                    visaType = "Schengen Visa Required";
                }
                else if (destinationCountryCode == "GB")
                {
                    isVisaRequired = true;
                    visaType = "UK Visa Required";
                }
                else if (destinationCountryCode == "US")
                {
                    isVisaRequired = true;
                    visaType = "US Visa Required";
                }
                else if (new[] { "AU", "NZ", "CA" }.Contains(destinationCountryCode))
                {
                    isVisaRequired = true;
                    visaType = "Visa Required";
                }
            }
        }

        result.VisaRequired = isVisaRequired;
        result.VisaType = visaType;

        return result;
    }

    /// <summary>
    /// Get flight estimate from the preloaded cache, or call Gemini for a single route.
    /// Never falls back to local computation.
    /// </summary>
    private async Task<(int Minutes, int CostUsd)> GetFlightEstimateAsync(string origin, string destination)
    {
        var key = $"{origin.ToUpperInvariant()}-{destination.ToUpperInvariant()}";
        if (_flightCache.TryGetValue(key, out var cached))
            return cached;

        _logger.LogInformation(
            "[GeminiClient] Request sent to Google AI — single flight estimate for {Origin} → {Destination}",
            origin, destination);

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage(
$@"Estimate the one-way flight time in minutes and average economy round-trip ticket cost in USD from airport {origin} to airport {destination}.
Respond ONLY with JSON, no markdown: {{""minutes"": N, ""costUsd"": N}}");

        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object> { { "temperature", 0.1 } }
        };

        var response = await chatService.GetChatMessageContentAsync(history, settings);
        var json = StripMarkdownFences(response.Content?.Trim() ?? "{}");

        _logger.LogInformation(
            "[GeminiClient] Response received from Google AI for {Origin} → {Destination}: {Json}",
            origin, destination, json);

        var parsed = JsonSerializer.Deserialize<BatchFlightEstimate>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (parsed == null || parsed.Minutes <= 0)
            throw new InvalidOperationException(
                $"Gemini AI failed to estimate flight data for {origin} → {destination}. Raw: {json}");

        var result = (parsed.Minutes, parsed.CostUsd);
        _flightCache[key] = result;
        return result;
    }

    private static string StripMarkdownFences(string text)
    {
        if (!text.StartsWith("```")) return text;
        var firstNewline = text.IndexOf('\n');
        var lastFence = text.LastIndexOf("```");
        if (firstNewline != -1 && lastFence > firstNewline)
            return text.Substring(firstNewline + 1, lastFence - firstNewline - 1).Trim();
        return text;
    }

    private class BatchFlightEstimate
    {
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public int Minutes { get; set; }
        public int CostUsd { get; set; }
    }
}
