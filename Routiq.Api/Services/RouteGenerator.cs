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
/// V2 Deterministic Route Generator.
/// Rules-based, explainable. Zero fake data. Zero live API calls.
/// Outputs: viable route options + eliminated destinations with reasons.
/// </summary>
public class RouteGenerator : IRouteGenerator
{
    private readonly RoutiqDbContext _context;

    public RouteGenerator(RoutiqDbContext context)
    {
        _context = context;
    }

    public async Task<RouteResponseDto> GenerateRoutesAsync(RouteRequestDto request)
    {
        var response = new RouteResponseDto();

        // ── Step 1: Load static data ──
        var allDestinations = await _context.Destinations
            .Where(d => d.IsActive)
            .ToListAsync();

        var visaRules = await _context.VisaRules
            .Where(v => v.PassportCountryCode == request.PassportCountryCode)
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
            var rule = visaRules.FirstOrDefault(v => v.DestinationCountryCode == dest.CountryCode);
            var tier = priceTiers.FirstOrDefault(t => t.Region == dest.Region && t.CostLevel == dest.DailyCostLevel);

            // Visa filter
            if (!IsVisaAccessible(dest, rule, request))
            {
                eliminations.Add(new RouteElimination
                {
                    RouteQueryId = Guid.Empty, // filled by caller when persisting
                    DestinationId = dest.Id,
                    Reason = EliminationReason.VisaRequired,
                    ExplanationText = BuildVisaEliminationText(dest, rule, request.PassportCountryCode)
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
            // Surface eliminations even when no route found
            response.Eliminations = eliminations
                .Select(e => MapEliminationToDto(e, allDestinations))
                .ToList();
            return response;
        }

        // ── Step 4: Build route options ──
        // Sort by popularity weight descending, then by cost level ascending (cheaper is better tie-break)
        var sorted = eligible
            .OrderByDescending(d => d.PopularityWeight)
            .ThenBy(d => (int)d.DailyCostLevel)
            .ToList();

        // Option 1: Best single city focus
        var top = sorted.First();
        response.Options.Add(BuildSingleCityOption(top, request, priceTiers, visaRules));

        // Option 2: Multi-city loop (if 6+ days)
        if (request.DurationDays >= 6)
        {
            var multiOption = TryBuildMultiCityOption(sorted, request, priceTiers, visaRules);
            if (multiOption != null)
                response.Options.Add(multiOption);
        }

        // Option 3: Best per-region pick (different region from option 1, if any)
        var altRegion = sorted.FirstOrDefault(d => d.Region != top.Region);
        if (altRegion != null)
            response.Options.Add(BuildSingleCityOption(altRegion, request, priceTiers, visaRules, isAlt: true));

        response.Eliminations = eliminations
            .Select(e => MapEliminationToDto(e, allDestinations))
            .ToList();

        return response;
    }

    // ── Option Builders ──

    private RouteOptionDto BuildSingleCityOption(
        Destination dest, RouteRequestDto req,
        List<RegionPriceTier> tiers, List<VisaRule> rules,
        bool isAlt = false)
    {
        var tier = tiers.FirstOrDefault(t => t.Region == dest.Region && t.CostLevel == dest.DailyCostLevel);
        var rule = rules.FirstOrDefault(r => r.DestinationCountryCode == dest.CountryCode);
        var days = Math.Clamp(req.DurationDays, dest.MinRecommendedDays, dest.MaxRecommendedDays);

        return new RouteOptionDto
        {
            RouteName = isAlt
                ? $"Alternative: {dest.City} Focus"
                : $"{dest.City} Focus",
            SelectionReason = BuildSelectionReason(dest, rule, req, days),
            EstimatedBudgetRange = tier != null
                ? $"${tier.DailyBudgetUsdMin * days}–${tier.DailyBudgetUsdMax * days}"
                : "See cost tier",
            Stops = new List<RouteStopDto>
            {
                MapStopToDto(dest, days, 1, tier, rule)
            }
        };
    }

    private RouteOptionDto? TryBuildMultiCityOption(
        List<Destination> sorted, RouteRequestDto req,
        List<RegionPriceTier> tiers, List<VisaRule> rules)
    {
        // Take top 2–4 destinations and split days proportionally
        var picks = sorted
            .Take(4)
            .ToList();

        if (picks.Count < 2) return null;

        var totalMinDays = picks.Sum(d => d.MinRecommendedDays);
        if (totalMinDays > req.DurationDays) picks = picks.Take(2).ToList();

        // Recalculate: distribute days proportionally
        var stops = new List<RouteStopDto>();
        int daysLeft = req.DurationDays;
        int totalMinNow = picks.Sum(d => d.MinRecommendedDays);

        for (int i = 0; i < picks.Count; i++)
        {
            var dest = picks[i];
            var tier = tiers.FirstOrDefault(t => t.Region == dest.Region && t.CostLevel == dest.DailyCostLevel);
            var rule = rules.FirstOrDefault(r => r.DestinationCountryCode == dest.CountryCode);

            int daysForStop = i == picks.Count - 1
                ? daysLeft
                : (int)Math.Round((double)dest.MinRecommendedDays / totalMinNow * req.DurationDays);

            daysForStop = Math.Clamp(daysForStop, dest.MinRecommendedDays, dest.MaxRecommendedDays);
            daysLeft -= daysForStop;

            stops.Add(MapStopToDto(dest, daysForStop, i + 1, tier, rule));
        }

        // Budget check: sum of min estimates vs total budget
        int totalMinCost = stops.Sum(s =>
        {
            var tier = tiers.FirstOrDefault(t => t.Region == stops.First().Region && t.CostLevel == Enum.Parse<CostLevel>(s.CostLevel));
            return tier?.DailyBudgetUsdMin * s.RecommendedDays ?? 0;
        });

        if (totalMinCost > req.TotalBudgetUsd) return null;

        var regionNames = stops.Select(s => s.Region).Distinct().ToList();
        return new RouteOptionDto
        {
            RouteName = $"{picks.Count}-City Loop ({string.Join(" → ", stops.Select(s => s.City))})",
            SelectionReason = $"All {picks.Count} stops are visa-accessible for {req.PassportCountryCode} passport. " +
                              $"Combined minimum cost estimate within ${req.TotalBudgetUsd} budget.",
            EstimatedBudgetRange = $"See individual stops",
            Stops = stops
        };
    }

    // ── Mapping Helpers ──

    private RouteStopDto MapStopToDto(Destination dest, int days, int order,
        RegionPriceTier? tier, VisaRule? rule)
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
            VisaStatus = rule != null
                ? FormatVisaStatus(rule)
                : "Check requirements",
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

    private bool IsVisaAccessible(Destination dest, VisaRule? rule, RouteRequestDto req)
    {
        if (rule == null) return false; // Unknown = assume required (strict/safe default)

        return rule.Requirement switch
        {
            VisaRequirement.VisaFree => true,
            VisaRequirement.EVisa => true,  // eVisa = accessible (user can get before trip)
            VisaRequirement.OnArrival => true,
            VisaRequirement.Required => IsSpecialVisaHeld(dest, req),
            VisaRequirement.Banned => false,
            _ => false
        };
    }

    private bool IsSpecialVisaHeld(Destination dest, RouteRequestDto req)
    {
        // If destination country is Schengen and user declared Schengen visa
        var schengenCountries = new[] { "DE", "FR", "AT", "NL", "ES", "IT", "PT", "BE", "SE", "NO", "FI", "DK", "LU", "CH", "CZ", "PL", "HU", "SK", "SI", "LV", "LT", "EE", "MT", "HR" };
        if (req.HasSchengenVisa && schengenCountries.Contains(dest.CountryCode)) return true;
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

    private string BuildSelectionReason(Destination dest, VisaRule? rule, RouteRequestDto req, int days)
    {
        var visaPart = rule?.Requirement switch
        {
            VisaRequirement.VisaFree => $"Visa-free for {req.PassportCountryCode} passport",
            VisaRequirement.EVisa => $"eVisa available online for {req.PassportCountryCode} passport",
            VisaRequirement.OnArrival => $"Visa on arrival for {req.PassportCountryCode} passport",
            _ => "Visa accessible"
        };

        return $"{visaPart}. {days} days within {dest.MinRecommendedDays}–{dest.MaxRecommendedDays} day recommended range. " +
               $"{dest.DailyCostLevel} cost tier fits ${req.TotalBudgetUsd} budget.";
    }

    private string BuildVisaEliminationText(Destination dest, VisaRule? rule, string passportCode)
    {
        if (rule == null)
            return $"{dest.City} eliminated: No visa rule found for {passportCode} passport → {dest.CountryCode}. Strict default applied.";

        return rule.Requirement switch
        {
            VisaRequirement.Required =>
                $"{dest.City} eliminated: {dest.Country} requires a visa for {passportCode} passport " +
                $"(avg {rule.AvgProcessingDays} days processing). You declared no qualifying visa. " +
                (rule.Notes != null ? rule.Notes : ""),
            VisaRequirement.Banned =>
                $"{dest.City} eliminated: {dest.Country} does not permit entry for {passportCode} passport holders.",
            _ => $"{dest.City} eliminated: Visa accessibility could not be confirmed."
        };
    }

    private string FormatVisaStatus(VisaRule rule)
    {
        return rule.Requirement switch
        {
            VisaRequirement.VisaFree => $"Visa-Free (up to {rule.MaxStayDays} days)",
            VisaRequirement.EVisa => $"eVisa Required",
            VisaRequirement.OnArrival => $"Visa on Arrival",
            VisaRequirement.Required => $"Visa Required",
            VisaRequirement.Banned => "Entry Not Permitted",
            _ => "Unknown"
        };
    }
}
