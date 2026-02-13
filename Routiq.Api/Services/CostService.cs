using Routiq.Api.Entities;

namespace Routiq.Api.Services;

public class CostService : ICostService
{
    // Budget Distribution Constants
    private const decimal AccommodationShare = 0.35m;
    private const decimal TransportationShare = 0.25m;
    private const decimal FoodShare = 0.25m;
    private const decimal InnerCityTransitShare = 0.10m;
    private const decimal BufferShare = 0.05m;

    public decimal CalculateTripCost(Destination destination, int days, decimal totalBudget)
    {
        // Determine user's daily budget level
        decimal dailyBudget = totalBudget / days;

        // Select appropriate cost tier based on budget
        // Heuristic: If daily budget > High cost, use High. If > Mid, use Mid. Else Low.
        decimal dailyCostEstimate;

        if (dailyBudget >= destination.AvgDailyCostHigh)
        {
            dailyCostEstimate = destination.AvgDailyCostHigh;
        }
        else if (dailyBudget >= destination.AvgDailyCostMid)
        {
            dailyCostEstimate = destination.AvgDailyCostMid;
        }
        else
        {
            dailyCostEstimate = destination.AvgDailyCostLow;
        }

        return dailyCostEstimate * days;
    }

    public bool IsBudgetSufficient(Destination destination, int days, decimal totalBudget)
    {
        decimal minCost = destination.AvgDailyCostLow * days;
        // Add a 10% safety margin for the absolute minimum check
        decimal safetyMargin = minCost * 0.10m;

        return totalBudget >= (minCost + safetyMargin);
    }
}
