namespace Routsky.Api.Services;

public interface IHybridFlightPriceService
{
    Task<FlightEstimate> EstimateAsync(string originCode, string destinationCode, DateTime? departureDate = null);

    Task PreloadBatchAsync(List<(string Origin, string Destination)> routePairs);
}
