using System.Text.Json;

namespace Routsky.Api.Services
{
    public class AgentInsightService : IAgentInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AgentInsightService> _logger;

        public AgentInsightService(HttpClient httpClient, ILogger<AgentInsightService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [Microsoft.SemanticKernel.KernelFunction("GetWeatherForecast")]
        [System.ComponentModel.Description("Gets the current weather forecast for a given city.")]
        public async Task<string> GenerateInsightAsync([System.ComponentModel.Description("The name of the city, e.g. London or Tokyo")] string city)
        {
            // 1. Fetch live weather
            var weatherPart = await GetWeatherAsync(city);

            if (string.IsNullOrWhiteSpace(weatherPart))
            {
                return $"Verified route details for {city}. Live weather data is currently unavailable.";
            }

            return weatherPart;
        }

        private async Task<string> GetWeatherAsync(string city)
        {
            try
            {
                // Clean city string (e.g. "Sarajevo, BA" -> "Sarajevo")
                var cleanCity = city.Split(',')[0].Split('(')[0].Trim();

                // Geocode
                var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cleanCity)}&count=1";
                var geoRes = await _httpClient.GetStringAsync(geoUrl);
                using var geoDoc = JsonDocument.Parse(geoRes);

                if (!geoDoc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Geocoding failed for city: {City}", cleanCity);
                    return "Unable to resolve geographic coordinates for live weather data.";
                }

                var firstResult = results[0];
                var lat = firstResult.GetProperty("latitude").GetDouble();
                var lon = firstResult.GetProperty("longitude").GetDouble();

                // Weather
                var wxUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
                var wxRes = await _httpClient.GetStringAsync(wxUrl);
                using var wxDoc = JsonDocument.Parse(wxRes);

                if (!wxDoc.RootElement.TryGetProperty("current_weather", out var current))
                {
                    return string.Empty;
                }

                var temp = Math.Round(current.GetProperty("temperature").GetDouble());
                var code = current.GetProperty("weathercode").GetInt32();

                string condition = "Clear";

                if (code >= 1 && code <= 3) { condition = "Partly Cloudy"; }
                else if (code >= 45 && code <= 48) { condition = "Foggy"; }
                else if (code >= 51 && code <= 67) { condition = "Rainy"; }
                else if (code >= 71 && code <= 77) { condition = "Snowy"; }
                else if (code >= 80 && code <= 82) { condition = "Rain Showers"; }
                else if (code >= 95 && code <= 99) { condition = "Stormy"; }

                return $"Current weather in {city}: {temp}°C, {condition}.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch weather for {City}", city);
                return string.Empty;
            }
        }

        // Mock methods completely removed to enforce Orchestrator-Agent integrity.
    }
}
