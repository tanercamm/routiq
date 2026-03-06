namespace Routiq.Api.Entities;

public class CityIntelligence
{
    public Guid Id { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Safety Index (0-100)
    public double SafetyIndex { get; set; }

    // Cost of Living Indexes relative to NYC
    public double CostOfLivingIndex { get; set; }
    public double AverageMealCostUSD { get; set; }
    public double AverageTransportCostUSD { get; set; }

    // Comma-separated list of optimal months (1-12)
    public string BestMonthsToVisit { get; set; } = string.Empty;
}
