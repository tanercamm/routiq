using Routiq.Api.Entities;

namespace Routiq.Api.Services;

public interface ICostService
{
    decimal CalculateTripCost(Destination destination, int days, decimal totalBudget);
    bool IsBudgetSufficient(Destination destination, int days, decimal totalBudget);
}
