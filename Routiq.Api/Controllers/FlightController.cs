using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Routiq.Api.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public FlightController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("live")]
    public async Task<IActionResult> GetLiveFlight([FromQuery] string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
            return BadRequest(new { message = "Destination is required" });

        try
        {
            var apiKey = _configuration["FlightApi:ApiKey"] ?? "PLACEHOLDER_KEY";
            var baseUrl = _configuration["FlightApi:BaseUrl"] ?? "https://api.external-flight-provider.com/v1";

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/flights/live?destination={Uri.EscapeDataString(destination)}");
            request.Headers.Add("X-Api-Key", apiKey);
            request.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { message = "External API failed to return live flight data for this route." });
            }

            var content = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            var dto = new
            {
                flightNumber = root.TryGetProperty("flightNumber", out var fn) ? fn.GetString() : null,
                duration = root.TryGetProperty("duration", out var d) ? d.GetString() : null,
                price = root.TryGetProperty("price", out var p) ? p.GetString() : null
            };

            return Ok(dto);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "External flight service is unreachable at this time.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An internal error occurred while processing the live flight data.", error = ex.Message });
        }
    }
}
