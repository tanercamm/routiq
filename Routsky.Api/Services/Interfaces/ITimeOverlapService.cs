namespace Routsky.Api.Services;

public interface ITimeOverlapService
{
    TimeOverlapService.TimeOverlapResult Analyse(List<int> memberFlightTimesMinutes);
}
