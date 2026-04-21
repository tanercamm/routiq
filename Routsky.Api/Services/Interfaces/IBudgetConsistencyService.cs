namespace Routsky.Api.Services;

public interface IBudgetConsistencyService
{
    BudgetConsistencyService.BudgetResult Analyse(int ticketPrice, int userBudget);
}
