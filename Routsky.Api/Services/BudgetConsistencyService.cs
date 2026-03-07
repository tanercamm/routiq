namespace Routsky.Api.Services;

/// <summary>
/// MCP Atom #2: Budget Consistency
/// Stateless service. Input (TicketPrice, UserBudget) → Output (PercentageUsed, Severity).
/// Pure math — no external calls.
/// </summary>
public class BudgetConsistencyService
{
    public class BudgetResult
    {
        public int TicketPrice { get; set; }
        public int UserBudget { get; set; }
        public double PercentageUsed { get; set; }
        public bool IsWithinBudget { get; set; }
        public string Severity { get; set; } = "comfortable";
        /// <summary>Score 0-100. Higher = better (more budget remaining)</summary>
        public double Score { get; set; }
    }

    public BudgetResult Analyse(int ticketPrice, int userBudget)
    {
        // Default budget if user hasn't set one
        var effectiveBudget = userBudget > 0 ? userBudget : 1500;

        var percentageUsed = (double)ticketPrice / effectiveBudget * 100.0;
        var severity = percentageUsed switch
        {
            < 50 => "comfortable",
            < 80 => "moderate",
            <= 100 => "tight",
            _ => "over"
        };

        // Score: 100 when free, 0 when at budget, negative when over
        var score = Math.Max(0, Math.Min(100, 100 - percentageUsed));

        return new BudgetResult
        {
            TicketPrice = ticketPrice,
            UserBudget = effectiveBudget,
            PercentageUsed = Math.Round(percentageUsed, 1),
            IsWithinBudget = percentageUsed <= 100,
            Severity = severity,
            Score = Math.Round(score, 1)
        };
    }
}
