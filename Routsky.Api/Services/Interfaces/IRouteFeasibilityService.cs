namespace Routsky.Api.Services;

public interface IRouteFeasibilityService
{
    Task<RouteFeasibilityService.FeasibilityResult> AnalyseAsync(
        string origin,
        string destination,
        List<string> passportCodes,
        string destinationCountryCode);

    Task PreloadFlightEstimatesAsync(List<(string Origin, string Destination)> routePairs);
}
