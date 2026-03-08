namespace Routsky.Api.Services;

public record FlightEstimate(int FlightTimeMinutes, int CostUsd, string Source);

public interface IFlightPriceProvider
{
    Task<FlightEstimate?> EstimateAsync(string originCode, string destinationCode, DateTime? departureDate = null);
}
