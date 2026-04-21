namespace Routsky.Api.Services;

public interface IAgentInsightService
{
    Task<string> GenerateInsightAsync(string city);
}
