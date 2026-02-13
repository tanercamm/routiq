using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.DTOs;
using Routiq.Api.Entities;

namespace Routiq.Api.Services;

public class RouteGenerator : IRouteGenerator
{
    private readonly RoutiqDbContext _context;
    private readonly ICostService _costService;

    public RouteGenerator(RoutiqDbContext context, ICostService costService)
    {
        _context = context;
        _costService = costService;
    }

    public async Task<RouteResponseDto> GenerateRoutesAsync(RouteRequestDto request)
    {
        var response = new RouteResponseDto();

        // Step 1: Fetch all data (small dataset, so feasible)
        var allDestinations = await _context.Destinations.ToListAsync();
        var allVisaRules = await _context.VisaRules
            .Where(v => v.PassportCountry == request.PassportCountry)
            .ToListAsync();

        // Step 2: Filter by Visa (Exclude Required)
        var accessibleDestinations = allDestinations.Where(d =>
        {
            var rule = allVisaRules.FirstOrDefault(v => v.DestinationCountry == d.Country);
            // Default to requiring visa if no rule found, or check specific rule
            if (rule == null) return true; // Assuming open if unknown, or strict? Let's assume strict: false. But for seed data coverage, let's keep it loose or strict. 
                                           // Actually, per requirements "Exclude 'Required' visas".
            return rule.VisaType != VisaType.Required;
        }).ToList();

        // Step 3: Filter by Budget and Score
        var eligibleDestinations = accessibleDestinations
            .Select(d => new
            {
                Destination = d,
                Cost = _costService.CalculateTripCost(d, request.DurationDays, request.TotalBudget),
                Score = d.PopularityScore // Simple scoring for now
            })
            .Where(x => x.Cost <= request.TotalBudget)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Cost) // Cheaper is better tie-breaker
            .ToList();

        if (!eligibleDestinations.Any())
        {
            return response; // Return empty if nothing fits
        }

        // Step 4: Generate 3 Options

        // Option 1: Single City Focus (Top Scorer)
        var bestSingle = eligibleDestinations.First();
        response.Options.Add(new RouteOptionDto
        {
            RouteType = "Single City Focus",
            Description = $"Enjoy a fully immersive {request.DurationDays}-day trip to {bestSingle.Destination.City}.",
            TotalEstimatedCost = bestSingle.Cost,
            Stops = new List<RouteStopDto>
            {
                CreateStopDto(bestSingle.Destination, request.DurationDays, bestSingle.Cost, allVisaRules)
            }
        });

        // Option 2: Two-City Combination (if budget/logistics allow)
        // Heuristic: Find two cities in the same region that fit in budget together.
        if (request.DurationDays >= 6)
        {
            var splitDays = request.DurationDays / 2;
            var regionGroups = eligibleDestinations
                .GroupBy(x => x.Destination.Region)
                .Where(g => g.Count() >= 2)
                .OrderByDescending(g => g.Max(x => x.Score))
                .FirstOrDefault();

            if (regionGroups != null)
            {
                var comboStats = regionGroups.Take(2).ToList();
                var city1 = comboStats[0];
                var city2 = comboStats[1];

                // Recalculate cost for split duration
                var cost1 = _costService.CalculateTripCost(city1.Destination, splitDays, request.TotalBudget);
                var cost2 = _costService.CalculateTripCost(city2.Destination, request.DurationDays - splitDays, request.TotalBudget);
                var flightBuffer = 150m; // Extra buffer for inter-city travel

                if (cost1 + cost2 + flightBuffer <= request.TotalBudget)
                {
                    response.Options.Add(new RouteOptionDto
                    {
                        RouteType = "Two-City Combination",
                        Description = $"Experience the best of {city1.Destination.Region} with {city1.Destination.City} and {city2.Destination.City}.",
                        TotalEstimatedCost = cost1 + cost2 + flightBuffer,
                        Stops = new List<RouteStopDto>
                        {
                            CreateStopDto(city1.Destination, splitDays, cost1, allVisaRules),
                            CreateStopDto(city2.Destination, request.DurationDays - splitDays, cost2, allVisaRules)
                        }
                    });
                }
            }
        }

        // Option 3: Economy Saver (Cheapest viable option that is still decent)
        var cheapest = eligibleDestinations.OrderBy(x => x.Cost).First();
        // Ensure it's not the same as Option 1
        if (cheapest.Destination.Id != bestSingle.Destination.Id)
        {
            response.Options.Add(new RouteOptionDto
            {
                RouteType = "Alternative Economy Route",
                Description = $"Save money while exploring {cheapest.Destination.City}.",
                TotalEstimatedCost = cheapest.Cost,
                Stops = new List<RouteStopDto>
                {
                    CreateStopDto(cheapest.Destination, request.DurationDays, cheapest.Cost, allVisaRules)
                }
            });
        }
        else if (eligibleDestinations.Count > 1)
        {
            // Pick the second best if cheapest is also the best
            var secondBest = eligibleDestinations.Skip(1).First();
            response.Options.Add(new RouteOptionDto
            {
                RouteType = "Alternative Choice",
                Description = $"Consider visiting {secondBest.Destination.City} as a great alternative.",
                TotalEstimatedCost = secondBest.Cost,
                Stops = new List<RouteStopDto>
                 {
                     CreateStopDto(secondBest.Destination, request.DurationDays, secondBest.Cost, allVisaRules)
                 }
            });
        }

        return response;
    }

    private RouteStopDto CreateStopDto(Destination d, int days, decimal cost, List<VisaRule> rules)
    {
        var rule = rules.FirstOrDefault(r => r.DestinationCountry == d.Country);
        return new RouteStopDto
        {
            City = d.City,
            Country = d.Country,
            Days = days,
            EstimatedCost = cost,
            Climate = d.ClimateTags.FirstOrDefault() ?? "Unknown",
            VisaStatus = rule?.VisaType.ToString() ?? "Unknown"
        };
    }
}
