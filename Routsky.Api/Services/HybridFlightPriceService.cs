using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Routsky.Api.Configuration;

namespace Routsky.Api.Services;

/// <summary>
/// Orchestrates flight price estimation using multiple providers.
/// Strategy: Try Turkish Airlines first (real prices), fall back to Gemini (AI estimates).
/// All results are cached in-memory to avoid redundant calls.
/// </summary>
public class HybridFlightPriceService : IHybridFlightPriceService
{
    private readonly TurkishAirlinesFlightPriceProvider _tkProvider;
    private readonly GeminiFlightPriceProvider _geminiProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HybridFlightPriceService> _logger;
    private readonly FlightDefaults _flightDefaults;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

    public HybridFlightPriceService(
        TurkishAirlinesFlightPriceProvider tkProvider,
        GeminiFlightPriceProvider geminiProvider,
        IMemoryCache cache,
        ILogger<HybridFlightPriceService> logger,
        IOptions<FlightDefaults> flightDefaults)
    {
        _tkProvider = tkProvider;
        _geminiProvider = geminiProvider;
        _cache = cache;
        _logger = logger;
        _flightDefaults = flightDefaults.Value;
    }

    public async Task<FlightEstimate> EstimateAsync(string originCode, string destinationCode, DateTime? departureDate = null)
    {
        var key = $"flight:{originCode.ToUpperInvariant()}-{destinationCode.ToUpperInvariant()}";

        if (_cache.TryGetValue(key, out FlightEstimate? cached) && cached != null)
            return cached;

        // Strategy 1: Turkish Airlines (real prices)
        if (_tkProvider.IsConfigured)
        {
            var tkResult = await _tkProvider.EstimateAsync(originCode, destinationCode, departureDate);
            if (tkResult != null)
            {
                _cache.Set(key, tkResult, CacheTtl);
                return tkResult;
            }
        }

        // Strategy 2: Gemini AI estimation (fallback)
        var geminiResult = await _geminiProvider.EstimateAsync(originCode, destinationCode, departureDate);
        if (geminiResult != null)
        {
            _cache.Set(key, geminiResult, CacheTtl);
            return geminiResult;
        }

        _logger.LogWarning("[HybridFlight] All providers failed for {Origin}→{Dest}, using defaults",
            originCode, destinationCode);

        var fallback = new FlightEstimate(_flightDefaults.FallbackFlightMinutes, _flightDefaults.FallbackCostUsd, "fallback");
        _cache.Set(key, fallback, CacheTtl);
        return fallback;
    }

    /// <summary>
    /// Batch preload via Gemini. TK doesn't support batch, so only Gemini is used for preloading.
    /// </summary>
    public async Task PreloadBatchAsync(List<(string Origin, string Destination)> routePairs)
    {
        await _geminiProvider.PreloadBatchAsync(routePairs);
    }
}
