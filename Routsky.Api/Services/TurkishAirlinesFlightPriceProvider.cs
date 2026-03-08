using System.Text.Json;

namespace Routsky.Api.Services;

/// <summary>
/// Flight price estimation via Turkish Airlines API (through Pipedream proxy).
/// This provider calls a Pipedream HTTP webhook that proxies requests to the
/// Turkish Airlines MCP search_flights tool.
///
/// Configuration: Set TURKISH_AIRLINES_PROXY_URL environment variable to the
/// Pipedream webhook URL. If not set, this provider gracefully returns null
/// so the HybridFlightPriceService falls back to Gemini.
/// </summary>
public class TurkishAirlinesFlightPriceProvider : IFlightPriceProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<TurkishAirlinesFlightPriceProvider> _logger;
    private readonly string? _proxyUrl;

    public TurkishAirlinesFlightPriceProvider(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TurkishAirlinesFlightPriceProvider> logger)
    {
        _http = httpClient;
        _logger = logger;

        var envVar = configuration["TurkishAirlines:ProxyUrlEnvVar"] ?? "TURKISH_AIRLINES_PROXY_URL";
        _proxyUrl = Environment.GetEnvironmentVariable(envVar);
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_proxyUrl);

    public async Task<FlightEstimate?> EstimateAsync(string originCode, string destinationCode, DateTime? departureDate = null)
    {
        if (!IsConfigured)
            return null;

        var date = departureDate ?? DateTime.UtcNow.AddDays(30);

        try
        {
            var payload = new
            {
                origin = originCode.ToUpperInvariant(),
                destination = destinationCode.ToUpperInvariant(),
                date = date.ToString("dd-MM-yyyy HH:mm")
            };

            var response = await _http.PostAsync(
                _proxyUrl,
                new StringContent(
                    JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[TurkishAirlines] Proxy returned {Status} for {Origin}→{Dest}",
                    response.StatusCode, originCode, destinationCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TkProxyResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.CostUsd <= 0)
                return null;

            _logger.LogInformation(
                "[TurkishAirlines] Flight estimate {Origin}→{Dest}: {Minutes}min, ${Cost}",
                originCode, destinationCode, result.Minutes, result.CostUsd);

            return new FlightEstimate(result.Minutes, result.CostUsd, "turkish-airlines");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[TurkishAirlines] Failed to get estimate for {Origin}→{Dest}, falling back",
                originCode, destinationCode);
            return null;
        }
    }

    private class TkProxyResponse
    {
        public int Minutes { get; set; }
        public int CostUsd { get; set; }
    }
}
