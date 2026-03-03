using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;

namespace Routiq.Api.Services;

/// <summary>
/// The Orchestrator: Decision Solver
/// Fetches member data → calls MCP atoms → scores candidates → picks winner with explanation.
/// This is the "Agent Brain" — it does NOT hardcode winners.
/// </summary>
public class DecisionSolverService
{
    private readonly RoutiqDbContext _context;
    private readonly RouteFeasibilityService _feasibility;
    private readonly BudgetConsistencyService _budget;
    private readonly TimeOverlapService _timeOverlap;

    public DecisionSolverService(
        RoutiqDbContext context,
        RouteFeasibilityService feasibility,
        BudgetConsistencyService budget,
        TimeOverlapService timeOverlap)
    {
        _context = context;
        _feasibility = feasibility;
        _budget = budget;
        _timeOverlap = timeOverlap;
    }

    // ── DTOs ──

    public class MemberTicket
    {
        public string MemberName { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string FlightTime { get; set; } = string.Empty;
        public int FlightTimeMinutes { get; set; }
        public int CostUsd { get; set; }
        public string VisaType { get; set; } = "VisaFree";
        public bool VisaRequired { get; set; }
        public string BudgetSeverity { get; set; } = "comfortable";
        public double BudgetPercentUsed { get; set; }
    }

    public class CandidateResult
    {
        public string DestinationCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double CompositeScore { get; set; }
        public int AvgCostUsd { get; set; }
        public string AvgFlightTime { get; set; } = string.Empty;
        public double FrictionScore { get; set; }
        public List<MemberTicket> MemberTickets { get; set; } = new();
    }

    public class DecisionResult
    {
        public CandidateResult Winner { get; set; } = new();
        public List<CandidateResult> Alternatives { get; set; } = new();
        public string Explanation { get; set; } = string.Empty;
        public Dictionary<string, string> EliminatedReasons { get; set; } = new();
        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
    }

    public class MemberInfo
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public List<string> Passports { get; set; } = new();
        public int Budget { get; set; } = 0;
    }

    // ── Candidate destinations to evaluate ──
    // Using a curated list with IATA codes and country codes for visa lookups
    private static readonly List<(string Code, string City, string Country, string CountryCode)> CandidateDestinations = new()
    {
        ("SIN", "Singapore", "Singapore", "SG"),
        ("GYD", "Baku", "Azerbaijan", "AZ"),
        ("SJJ", "Sarajevo", "Bosnia & Herzegovina", "BA"),
        ("CMN", "Casablanca", "Morocco", "MA"),
        ("BKK", "Bangkok", "Thailand", "TH"),
        ("TBS", "Tbilisi", "Georgia", "GE"),
        ("KUL", "Kuala Lumpur", "Malaysia", "MY"),
    };

    /// <summary>
    /// Run the full decision pipeline for a travel group.
    /// </summary>
    public async Task<DecisionResult> SolveAsync(Guid groupId)
    {
        // ── Step 1: Fetch member data from DB ──
        var (members, skippedWarnings) = await FetchMembersAsync(groupId);
        if (members.Count < 2)
        {
            var reason = skippedWarnings.Count > 0
                ? $"Need at least 2 members with set origins. Skipped: {string.Join(", ", skippedWarnings)}"
                : "Need at least 2 members with set origins to run the intersection engine.";
            return new DecisionResult { Explanation = reason };
        }

        // ── Step 2: Evaluate each candidate with MCP atoms ──
        var scoredCandidates = new List<CandidateResult>();
        var eliminated = new Dictionary<string, string>();

        foreach (var (code, city, country, countryCode) in CandidateDestinations)
        {
            // Skip if any member's origin IS the destination
            if (members.Any(m => m.Origin.Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                eliminated[code] = $"{city} skipped: a group member already lives there.";
                continue;
            }

            var tickets = new List<MemberTicket>();
            var flightTimes = new List<int>();
            var budgetScores = new List<double>();
            var visaIssues = new List<string>();
            var isFeasible = true;

            foreach (var member in members)
            {
                // MCP #1: Route Feasibility
                var feasibility = await _feasibility.AnalyseAsync(
                    member.Origin, code, member.Passports, countryCode);

                if (!feasibility.IsFeasible)
                {
                    eliminated[code] = $"{city} eliminated: {feasibility.BlockReason} ({member.Name}).";
                    isFeasible = false;
                    break;
                }

                // MCP #2: Budget Consistency
                var budgetResult = _budget.Analyse(feasibility.EstimatedCostUsd, member.Budget);

                // MCP #3: collect flight times for overlap analysis
                flightTimes.Add(feasibility.FlightTimeMinutes);
                budgetScores.Add(budgetResult.Score);

                if (feasibility.VisaRequired)
                    visaIssues.Add($"{member.Name} needs visa for {city}");

                tickets.Add(new MemberTicket
                {
                    MemberName = member.Name,
                    MemberId = member.UserId,
                    Origin = member.Origin,
                    Destination = code,
                    FlightTime = feasibility.FlightTimeFormatted,
                    FlightTimeMinutes = feasibility.FlightTimeMinutes,
                    CostUsd = feasibility.EstimatedCostUsd,
                    VisaType = feasibility.VisaType,
                    VisaRequired = feasibility.VisaRequired,
                    BudgetSeverity = budgetResult.Severity,
                    BudgetPercentUsed = budgetResult.PercentageUsed
                });
            }

            if (!isFeasible) continue;

            // MCP #3: Time Overlap
            var timeResult = _timeOverlap.Analyse(flightTimes);

            // ── Step 3: Composite Score ──
            // Weights: 40% budget, 30% time fairness, 30% visa ease
            var avgBudgetScore = budgetScores.Average();
            var timeScore = timeResult.NormalizedScore;
            var visaScore = visaIssues.Count == 0 ? 100.0 : Math.Max(0, 100 - (visaIssues.Count * 30.0));

            var composite = (0.4 * avgBudgetScore) + (0.3 * timeScore) + (0.3 * visaScore);

            // Penalize extreme flight times (>20h for any member)
            if (tickets.Any(t => t.FlightTimeMinutes > 1200))
                composite *= 0.8; // 20% penalty

            scoredCandidates.Add(new CandidateResult
            {
                DestinationCode = code,
                City = city,
                Country = country,
                CompositeScore = Math.Round(composite, 1),
                AvgCostUsd = (int)tickets.Average(t => t.CostUsd),
                AvgFlightTime = timeResult.AvgFlightFormatted,
                FrictionScore = timeResult.FrictionScore,
                MemberTickets = tickets
            });
        }

        // ── Step 4: Rank and pick winner ──
        var ranked = scoredCandidates.OrderByDescending(c => c.CompositeScore).ToList();

        if (ranked.Count == 0)
        {
            return new DecisionResult
            {
                Explanation = "No feasible destinations found for this group's constraints.",
                EliminatedReasons = eliminated
            };
        }

        var winner = ranked[0];
        var alternatives = ranked.Skip(1).Take(2).ToList();

        // ── Step 5: Generate explanation ──
        var explanation = GenerateExplanation(winner, alternatives, eliminated, members);

        return new DecisionResult
        {
            Winner = winner,
            Alternatives = alternatives,
            Explanation = explanation,
            EliminatedReasons = eliminated,
            DecidedAt = DateTime.UtcNow
        };
    }

    private async Task<(List<MemberInfo> Members, List<string> Warnings)> FetchMembersAsync(Guid groupId)
    {
        var dbMembers = await _context.TravelGroupMembers
            .Where(m => m.GroupId == groupId)
            .Include(m => m.User)
                .ThenInclude(u => u!.Profile)
            .ToListAsync();

        var members = new List<MemberInfo>();
        var warnings = new List<string>();

        foreach (var m in dbMembers.Where(m => m.User != null))
        {
            var name = $"{m.User!.FirstName} {m.User.LastName}".Trim();
            var origin = m.User.Profile?.Origin;
            var passports = m.User.Profile?.Passports;

            // Skip members with no origin AND no passports — engine can't route them
            if (string.IsNullOrWhiteSpace(origin) && (passports == null || passports.Count == 0))
            {
                warnings.Add($"{name} (no origin/passport set)");
                continue;
            }

            // Use Origin if set, else fall back to first passport code as IATA hint
            var resolvedOrigin = !string.IsNullOrWhiteSpace(origin)
                ? origin
                : passports!.First(); // We know passports is non-empty due to the check above

            var budget = m.User.Profile?.Budget ?? 0;

            members.Add(new MemberInfo
            {
                UserId = m.UserId,
                Name = name,
                Origin = resolvedOrigin,
                Passports = passports ?? new List<string>(),
                Budget = budget > 0 ? budget : 1500 // Default $1500 only when user hasn't set budget
            });
        }

        return (members, warnings);
    }

    private static string GenerateExplanation(
        CandidateResult winner,
        List<CandidateResult> alternatives,
        Dictionary<string, string> eliminated,
        List<MemberInfo> members)
    {
        var lines = new List<string>();

        // Winner explanation
        lines.Add($"🏆 {winner.City} ({winner.DestinationCode}) scored highest at {winner.CompositeScore}/100.");

        // Budget insight
        var allUnderBudget = winner.MemberTickets.All(t => t.BudgetSeverity != "over");
        if (allUnderBudget)
            lines.Add($"✅ All {members.Count} members' tickets are within budget (avg ${winner.AvgCostUsd}/person).");
        else
            lines.Add($"⚠️ Some members' tickets exceed their budget.");

        // Visa insight
        var visaNeeded = winner.MemberTickets.Where(t => t.VisaRequired).ToList();
        if (visaNeeded.Count == 0)
            lines.Add($"🛂 Visa-free entry for all passport holders.");
        else
            lines.Add($"🛂 Visa required for: {string.Join(", ", visaNeeded.Select(t => t.MemberName))}.");

        // Time fairness
        var maxDiff = winner.MemberTickets.Max(t => t.FlightTimeMinutes) - winner.MemberTickets.Min(t => t.FlightTimeMinutes);
        lines.Add($"⏱️ Flight time spread: {maxDiff / 60}h {maxDiff % 60}m difference between members (avg {winner.AvgFlightTime}).");

        // Per-member tickets
        lines.Add("");
        lines.Add("📋 Individual tickets:");
        foreach (var ticket in winner.MemberTickets)
        {
            lines.Add($"  • {ticket.MemberName}: {ticket.Origin} ➔ {ticket.Destination} ({ticket.FlightTime}, ${ticket.CostUsd})");
        }

        // Why alternatives lost
        if (alternatives.Count > 0)
        {
            lines.Add("");
            lines.Add("📊 Runner-ups:");
            foreach (var alt in alternatives)
            {
                var diff = winner.CompositeScore - alt.CompositeScore;
                lines.Add($"  • {alt.City} ({alt.DestinationCode}): scored {alt.CompositeScore}/100 (−{diff:F1} points). Avg cost ${alt.AvgCostUsd}, friction {alt.FrictionScore:F0}.");
            }
        }

        // Eliminated
        if (eliminated.Count > 0)
        {
            lines.Add("");
            lines.Add("❌ Eliminated:");
            foreach (var (code, reason) in eliminated)
            {
                lines.Add($"  • {reason}");
            }
        }

        return string.Join("\n", lines);
    }
}
