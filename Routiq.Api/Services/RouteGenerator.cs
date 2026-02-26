using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.DTOs;
using Routiq.Api.Entities;

namespace Routiq.Api.Services;

public interface IRouteGenerator
{
    Task<RouteResponseDto> GenerateRoutesAsync(RouteRequestDto request);
}

/// <summary>
/// V2.1 Deterministic Route Generator — Multi-Passport Edition.
/// Rules-based, explainable. Zero fake data. Zero live API calls.
///
/// Best-Case Visa Algorithm:
/// When a traveler holds multiple passports, the engine evaluates ALL of them
/// for each destination and selects the most favorable outcome:
///   1. VisaFree  (best — no action needed)
///   2. OnArrival (second — pay at border)
///   3. EVisa     (third — apply online beforehand)
///   4. Required  (worst — full embassy process)
///   5. Banned    (absolute — never overridable)
///
/// A destination is only eliminated on visa grounds if ALL passports result
/// in Required/Banned (after accounting for declared held visas).
/// </summary>
public class RouteGenerator : IRouteGenerator
{
    private readonly RoutiqDbContext _context;

    public RouteGenerator(RoutiqDbContext context)
    {
        _context = context;
    }

    // Favorability score — lower is better
    private static int VisaScore(VisaRequirement req) => req switch
    {
        VisaRequirement.VisaFree => 1,
        VisaRequirement.OnArrival => 2,
        VisaRequirement.EVisa => 3,
        VisaRequirement.Required => 4,
        VisaRequirement.Banned => 5,
        _ => 6
    };

    public async Task<RouteResponseDto> GenerateRoutesAsync(RouteRequestDto request)
    {
        var response = new RouteResponseDto();

        // Normalise: ensure at least one passport
        if (request.Passports == null || request.Passports.Count == 0)
            request.Passports = new List<string> { "XX" };

        // Deduplicate and uppercase
        request.Passports = request.Passports
            .Select(p => p.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        // ── Step 1: Load static data ──
        var allDestinations = await _context.Destinations
            .Where(d => d.IsActive)
            .ToListAsync();

        // Load visa rules for ALL passports the user holds
        var allVisaRules = await _context.VisaRules
            .Where(v => request.Passports.Contains(v.PassportCountryCode))
            .ToListAsync();

        var priceTiers = await _context.RegionPriceTiers.ToListAsync();

        // ── Step 2: Filter by region preference ──
        var regionFiltered = request.RegionPreference == RegionPreference.Any
            ? allDestinations
            : allDestinations.Where(d => RegionMatches(d.Region, request.RegionPreference)).ToList();

        // ── Step 3: Apply hard filters — visa, budget, days ──
        var eligible = new List<Destination>();
        var eliminations = new List<RouteElimination>();

        foreach (var dest in regionFiltered)
        {
            // Collect all rules for this destination across all passports
            var rulesForDest = allVisaRules
                .Where(v => v.DestinationCountryCode == dest.CountryCode)
                .ToList();

            var tier = priceTiers.FirstOrDefault(t => t.Region == dest.Region && t.CostLevel == dest.DailyCostLevel);

            // Best-case visa evaluation
            var bestOutcome = GetBestVisaOutcome(dest, rulesForDest, request);

            if (!bestOutcome.IsAccessible)
            {
                eliminations.Add(new RouteElimination
                {
                    RouteQueryId = Guid.Empty,
                    DestinationId = dest.Id,
                    Reason = EliminationReason.VisaRequired,
                    ExplanationText = BuildVisaEliminationText(dest, rulesForDest, request.Passports)
                });
                continue;
            }

            // Minimum days filter
            if (request.DurationDays < dest.MinRecommendedDays)
            {
                eliminations.Add(new RouteElimination
                {
                    RouteQueryId = Guid.Empty,
                    DestinationId = dest.Id,
                    Reason = EliminationReason.DaysInsufficient,
                    ExplanationText = $"{dest.City} requires at least {dest.MinRecommendedDays} days. " +
                                      $"Your trip is only {request.DurationDays} days."
                });
                continue;
            }

            // Budget filter — use low tier × min days as the absolute floor
            if (tier != null)
            {
                var minCostEstimate = tier.DailyBudgetUsdMin * dest.MinRecommendedDays;
                if (minCostEstimate > request.TotalBudgetUsd)
                {
                    eliminations.Add(new RouteElimination
                    {
                        RouteQueryId = Guid.Empty,
                        DestinationId = dest.Id,
                        Reason = EliminationReason.BudgetInsufficient,
                        ExplanationText = $"{dest.City} minimum cost estimate (${minCostEstimate}) " +
                                          $"exceeds your budget of ${request.TotalBudgetUsd}."
                    });
                    continue;
                }
            }

            eligible.Add(dest);
        }

        if (!eligible.Any())
        {
            response.Eliminations = eliminations
                .Select(e => MapEliminationToDto(e, allDestinations))
                .ToList();
            return response;
        }

        // ── Step 4: Build route options ──
        var sorted = eligible
            .OrderByDescending(d => d.PopularityWeight)
            .ThenBy(d => (int)d.DailyCostLevel)
            .ToList();

        // Option 1: Best single city focus
        var top = sorted.First();
        response.Options.Add(BuildSingleCityOption(top, request, priceTiers, allVisaRules));

        // Option 2: Multi-city loop (if 6+ days or high budget)
        bool isHighBudget = request.TotalBudgetUsd >= 5000;
        if (request.DurationDays >= 6 || isHighBudget)
        {
            var multiOption = TryBuildMultiCityOption(sorted, request, priceTiers, allVisaRules);
            if (multiOption != null)
                response.Options.Add(multiOption);
        }

        // Option 3: Best per-region pick (different region from option 1, if any)
        var altRegion = sorted.FirstOrDefault(d => d.Region != top.Region);
        if (altRegion != null)
            response.Options.Add(BuildSingleCityOption(altRegion, request, priceTiers, allVisaRules, isAlt: true));

        // Option 4: Extended grand tour (high-budget premium — more stops, maximum cities)
        if (isHighBudget && sorted.Count >= 4)
        {
            var grandTour = TryBuildMultiCityOption(sorted, request, priceTiers, allVisaRules, maxStops: 6);
            if (grandTour != null && grandTour.RouteName != response.Options.ElementAtOrDefault(1)?.RouteName)
                response.Options.Add(grandTour);
        }

        response.Eliminations = eliminations
            .Select(e => MapEliminationToDto(e, allDestinations))
            .ToList();

        return response;
    }

    // ── Visa Outcome Types ──

    private record VisaOutcome(bool IsAccessible, VisaRequirement BestRequirement, string BestPassport);

    /// <summary>
    /// Evaluates all passports held against the destination and returns the most favorable outcome.
    /// Special-visa declarations (Schengen/US/UK held) are also checked.
    /// </summary>
    private VisaOutcome GetBestVisaOutcome(Destination dest, List<VisaRule> rulesForDest, RouteRequestDto req)
    {
        var best = new VisaOutcome(false, VisaRequirement.Banned, "");

        foreach (var passport in req.Passports)
        {
            var rule = rulesForDest.FirstOrDefault(r => r.PassportCountryCode == passport);
            VisaRequirement effective;

            if (rule == null)
            {
                // No rule found — default to VisaFree.
                // Our seed data explicitly records every Required/Banned case for each supported
                // passport. An absent row means the destination imposes no restriction on that
                // passport (common for strong passports like DE/US/GB/AU vs. small nations).
                // Treating unknown as Required was the source of the algorithm bias.
                effective = VisaRequirement.VisaFree;
            }
            else if (rule.Requirement == VisaRequirement.Required && IsSpecialVisaHeld(dest, req))
            {
                // User declared a held visa that covers this destination
                effective = VisaRequirement.EVisa; // treat held visa as eVisa-level accessibility
            }
            else
            {
                effective = rule.Requirement;
            }

            bool accessible = effective != VisaRequirement.Required && effective != VisaRequirement.Banned;

            // Update best if this passport gives a lower (more favorable) score
            if (best.BestPassport == "" || VisaScore(effective) < VisaScore(best.BestRequirement))
            {
                best = new VisaOutcome(accessible, effective, passport);
            }
        }

        return best;
    }

    // ── Option Builders ──

    private RouteOptionDto BuildSingleCityOption(
        Destination dest, RouteRequestDto req,
        List<RegionPriceTier> tiers, List<VisaRule> rules,
        bool isAlt = false)
    {
        var tier = tiers.FirstOrDefault(t => t.Region == dest.Region && t.CostLevel == dest.DailyCostLevel);
        var rulesForDest = rules.Where(r => r.DestinationCountryCode == dest.CountryCode).ToList();
        var outcome = GetBestVisaOutcome(dest, rulesForDest, req);
        var days = Math.Clamp(req.DurationDays, dest.MinRecommendedDays, dest.MaxRecommendedDays);

        return new RouteOptionDto
        {
            RouteName = isAlt
                ? $"Alternative: {dest.City} Focus"
                : $"{dest.City} Focus",
            SelectionReason = BuildSelectionReason(dest, outcome, req, days),
            EstimatedBudgetRange = tier != null
                ? $"${tier.DailyBudgetUsdMin * days}–${tier.DailyBudgetUsdMax * days}"
                : "See cost tier",
            Stops = new List<RouteStopDto>
            {
                MapStopToDto(dest, days, 1, tier, outcome)
            }
        };
    }

    private RouteOptionDto? TryBuildMultiCityOption(
        List<Destination> sorted, RouteRequestDto req,
        List<RegionPriceTier> tiers, List<VisaRule> rules,
        int? maxStops = null)
    {
        int stopLimit = maxStops ?? Math.Max(2, Math.Min(6, req.DurationDays / 3));

        var picks = sorted
            .Take(stopLimit + 2)
            .ToList();

        if (picks.Count < 2) return null;

        var totalMinDays = picks.Sum(d => d.MinRecommendedDays);
        while (picks.Count > 2 && totalMinDays > req.DurationDays)
        {
            picks.RemoveAt(picks.Count - 1);
            totalMinDays = picks.Sum(d => d.MinRecommendedDays);
        }

        if (picks.Count < 2) return null;

        var stops = new List<RouteStopDto>();
        int daysLeft = req.DurationDays;
        int totalMinNow = picks.Sum(d => d.MinRecommendedDays);

        for (int i = 0; i < picks.Count; i++)
        {
            var dest = picks[i];
            var tier = tiers.FirstOrDefault(t => t.Region == dest.Region && t.CostLevel == dest.DailyCostLevel);
            var rulesForDest = rules.Where(r => r.DestinationCountryCode == dest.CountryCode).ToList();
            var outcome = GetBestVisaOutcome(dest, rulesForDest, req);

            int daysForStop = i == picks.Count - 1
                ? daysLeft
                : (int)Math.Round((double)dest.MinRecommendedDays / totalMinNow * req.DurationDays);

            daysForStop = Math.Clamp(daysForStop, dest.MinRecommendedDays, dest.MaxRecommendedDays);
            daysLeft -= daysForStop;

            stops.Add(MapStopToDto(dest, daysForStop, i + 1, tier, outcome));
        }

        // Budget check
        int totalMinCost = stops.Sum(s =>
        {
            var t = tiers.FirstOrDefault(t => t.Region == s.Region && t.CostLevel == Enum.Parse<CostLevel>(s.CostLevel));
            return t?.DailyBudgetUsdMin * s.RecommendedDays ?? 0;
        });

        if (totalMinCost > req.TotalBudgetUsd) return null;

        var passportLabel = string.Join("+", req.Passports);
        string tourLabel = maxStops.HasValue && maxStops >= 5 ? "Grand Tour" : $"{picks.Count}-City Loop";
        return new RouteOptionDto
        {
            RouteName = $"{tourLabel} ({string.Join(" → ", stops.Select(s => s.City))})",
            SelectionReason = $"All {picks.Count} stops visa-accessible with [{passportLabel}] passport(s). " +
                              $"Best-case visa logic applied. Combined minimum cost within ${req.TotalBudgetUsd} budget.",
            EstimatedBudgetRange = "See individual stops",
            Stops = stops
        };
    }

    // ── Mapping Helpers ──

    private RouteStopDto MapStopToDto(Destination dest, int days, int order,
        RegionPriceTier? tier, VisaOutcome outcome)
    {
        return new RouteStopDto
        {
            Order = order,
            City = dest.City,
            Country = dest.Country,
            CountryCode = dest.CountryCode,
            Region = dest.Region,
            RecommendedDays = days,
            CostLevel = dest.DailyCostLevel.ToString(),
            DailyBudgetRange = tier != null
                ? $"${tier.DailyBudgetUsdMin}–${tier.DailyBudgetUsdMax}/day"
                : "See region tier",
            VisaStatus = FormatVisaStatus(outcome),
            BestPassport = outcome.BestPassport,
            StopReason = dest.Notes
        };
    }

    private EliminationSummaryDto MapEliminationToDto(RouteElimination e, List<Destination> destinations)
    {
        var dest = destinations.FirstOrDefault(d => d.Id == e.DestinationId);
        return new EliminationSummaryDto
        {
            City = dest?.City ?? "Unknown",
            Country = dest?.Country ?? "Unknown",
            Reason = e.Reason.ToString(),
            Explanation = e.ExplanationText
        };
    }

    // ── Logic Helpers ──

    private bool IsSpecialVisaHeld(Destination dest, RouteRequestDto req)
    {
        var schengen = new[] { "DE", "FR", "AT", "NL", "ES", "IT", "PT", "BE", "SE", "NO", "FI", "DK", "LU", "CH", "CZ", "PL", "HU", "SK", "SI", "LV", "LT", "EE", "MT", "HR" };
        if (req.HasSchengenVisa && schengen.Contains(dest.CountryCode)) return true;
        if (req.HasUsVisa && dest.CountryCode == "US") return true;
        if (req.HasUkVisa && dest.CountryCode == "GB") return true;
        return false;
    }

    private bool RegionMatches(string destRegion, RegionPreference pref)
    {
        return pref switch
        {
            RegionPreference.SoutheastAsia => destRegion == "Southeast Asia",
            RegionPreference.EasternEurope => destRegion == "Eastern Europe",
            RegionPreference.Balkans => destRegion == "Balkans",
            RegionPreference.LatinAmerica => destRegion == "Latin America",
            RegionPreference.NorthAfrica => destRegion == "North Africa",
            RegionPreference.CentralAmerica => destRegion == "Central America",
            RegionPreference.CentralAsia => destRegion == "Central Asia",
            RegionPreference.MiddleEast => destRegion == "Middle East",
            RegionPreference.Caribbean => destRegion == "Caribbean",
            _ => true
        };
    }

    private string BuildSelectionReason(Destination dest, VisaOutcome outcome, RouteRequestDto req, int days)
    {
        var visaPart = outcome.BestRequirement switch
        {
            VisaRequirement.VisaFree => $"Visa-free via {outcome.BestPassport} passport",
            VisaRequirement.EVisa => $"eVisa available online (best: {outcome.BestPassport})",
            VisaRequirement.OnArrival => $"Visa on arrival (best: {outcome.BestPassport})",
            _ => "Visa accessible"
        };

        var multiPassportNote = req.Passports.Count > 1
            ? $" Best-case evaluated across [{string.Join(", ", req.Passports)}]."
            : "";

        return $"{visaPart}.{multiPassportNote} {days} days within {dest.MinRecommendedDays}–{dest.MaxRecommendedDays} day recommended range. " +
               $"{dest.DailyCostLevel} cost tier fits ${req.TotalBudgetUsd} budget.";
    }

    private string BuildVisaEliminationText(Destination dest, List<VisaRule> rulesForDest, List<string> passports)
    {
        if (rulesForDest.Count == 0)
            return $"{dest.City} eliminated: No visa rule found for any of [{string.Join(", ", passports)}] → {dest.CountryCode}. Strict default applied.";

        var parts = rulesForDest.Select(r => r.Requirement switch
        {
            VisaRequirement.Required =>
                $"{r.PassportCountryCode}: visa required (≈{r.AvgProcessingDays}d processing)" +
                (r.Notes != null ? $" — {r.Notes}" : ""),
            VisaRequirement.Banned =>
                $"{r.PassportCountryCode}: entry not permitted",
            _ =>
                $"{r.PassportCountryCode}: {r.Requirement}"
        });

        return $"{dest.City} eliminated — all passports blocked: {string.Join("; ", parts)}.";
    }

    private string FormatVisaStatus(VisaOutcome outcome)
    {
        return outcome.BestRequirement switch
        {
            VisaRequirement.VisaFree => $"Visa-Free ({outcome.BestPassport})",
            VisaRequirement.EVisa => $"eVisa ({outcome.BestPassport})",
            VisaRequirement.OnArrival => $"On Arrival ({outcome.BestPassport})",
            VisaRequirement.Required => "Visa Required",
            VisaRequirement.Banned => "Entry Not Permitted",
            _ => "Unknown"
        };
    }
}
