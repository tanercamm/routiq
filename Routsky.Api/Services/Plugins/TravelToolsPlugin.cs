using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Routsky.Api.Configuration;
using Routsky.Api.Data;

namespace Routsky.Api.Services.Plugins;

/// <summary>
/// Semantic Kernel Plugin: Passive Data Tools for the Gemini Agent.
/// 
/// These tools contain ZERO decision logic, ZERO scoring, ZERO elimination.
/// They return raw facts. The Agent alone reasons over these facts,
/// decides which tools to call, and synthesizes the final decision.
///
/// Tool Inventory:
///   - GetCandidateDestinations  → catalog of possible destinations
///   - EvaluateDestination       → batch: full group analysis for one destination
///   - CheckRouteFeasibility     → atom: single member → single route
///   - AnalyzeBudgetFit          → atom: single cost vs single budget
///   - AnalyzeGroupFlightFairness→ atom: compare flight times across members
///   - GetCityIntelligence       → safety, cost of living, seasonal data
///   - CheckVisaMatrix           → visa status lookup
///   - GetCurrentWeather         → live weather conditions
///   - GetAccommodationZones    → accommodation options by city
///   - GetAttractions           → activities and attractions by city
/// </summary>
public class TravelToolsPlugin
{
    private readonly IRouteFeasibilityService _feasibility;
    private readonly IBudgetConsistencyService _budget;
    private readonly ITimeOverlapService _timeOverlap;
    private readonly RoutskyDbContext _context;
    private readonly IAgentInsightService _insight;
    private readonly List<MemberContext> _members;
    private readonly CurrencyRates _currencyRates;

    public record MemberContext(
        string Name, int UserId, string Origin,
        List<string> Passports, int Budget, string PreferredCurrency);

    public class DestinationEvaluation
    {
        public string DestinationCode { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public int PrestigeScore { get; set; }
        public List<MemberRouteAnalysis> Members { get; set; } = new();
        public GroupFairnessAnalysis Fairness { get; set; } = new();
    }

    public class MemberRouteAnalysis
    {
        public string Name { get; set; } = "";
        public int UserId { get; set; }
        public string Origin { get; set; } = "";
        public string FlightTime { get; set; } = "";
        public int FlightTimeMinutes { get; set; }
        public int CostUsd { get; set; }
        public int ConvertedCost { get; set; }
        public string Currency { get; set; } = "USD";
        public string VisaType { get; set; } = "VisaFree";
        public bool VisaRequired { get; set; }
        public double BudgetPercentUsed { get; set; }
        public string BudgetSeverity { get; set; } = "comfortable";
        public bool IsWithinBudget { get; set; }
    }

    public class GroupFairnessAnalysis
    {
        public string AvgFlightTime { get; set; } = "";
        public int MaxDifferenceMinutes { get; set; }
        public double FrictionScore { get; set; }
        public double FairnessScore { get; set; }
    }

    private readonly Dictionary<string, DestinationEvaluation> _evaluationCache = new();
    public Dictionary<string, DestinationEvaluation> EvaluationCache => _evaluationCache;

    private static readonly List<(string Code, string City, string Country, string CountryCode, int Prestige)> Destinations = new()
    {
        ("TBS", "Tbilisi",       "Georgia",              "GE", 40),
        ("GYD", "Baku",          "Azerbaijan",           "AZ", 42),
        ("SJJ", "Sarajevo",      "Bosnia & Herzegovina", "BA", 45),
        ("CMN", "Casablanca",    "Morocco",              "MA", 50),
        ("SOF", "Sofia",         "Bulgaria",             "BG", 43),
        ("BEG", "Belgrade",      "Serbia",               "RS", 46),
        ("SIN", "Singapore",     "Singapore",            "SG", 75),
        ("BKK", "Bangkok",       "Thailand",             "TH", 68),
        ("KUL", "Kuala Lumpur",  "Malaysia",             "MY", 62),
        ("HAN", "Hanoi",         "Vietnam",              "VN", 60),
        ("DPS", "Bali",          "Indonesia",            "ID", 72),
        ("CEB", "Cebu",          "Philippines",          "PH", 55),
        ("DXB", "Dubai",         "UAE",                  "AE", 82),
        ("DOH", "Doha",          "Qatar",                "QA", 78),
        ("CDG", "Paris",         "France",               "FR", 92),
        ("BCN", "Barcelona",     "Spain",                "ES", 85),
        ("LHR", "London",        "United Kingdom",       "GB", 90),
        ("FCO", "Rome",          "Italy",                "IT", 88),
        ("NRT", "Tokyo",         "Japan",                "JP", 95),
        ("ICN", "Seoul",         "South Korea",          "KR", 80),
        ("JFK", "New York",      "United States",        "US", 93),
        ("MEX", "Mexico City",   "Mexico",               "MX", 65),
        ("EZE", "Buenos Aires",  "Argentina",            "AR", 70),
        ("BOG", "Bogotá",        "Colombia",             "CO", 58),
        ("CPT", "Cape Town",     "South Africa",         "ZA", 74),
        ("AKL", "Auckland",      "New Zealand",          "NZ", 76),
    };

    public TravelToolsPlugin(
        IRouteFeasibilityService feasibility,
        IBudgetConsistencyService budget,
        ITimeOverlapService timeOverlap,
        RoutskyDbContext context,
        IAgentInsightService insight,
        List<MemberContext> members,
        CurrencyRates currencyRates)
    {
        _feasibility = feasibility;
        _budget = budget;
        _timeOverlap = timeOverlap;
        _context = context;
        _insight = insight;
        _members = members;
        _currencyRates = currencyRates;
    }

    // ════════════════════════════════════════════════════════════════
    //  CATALOG TOOL
    // ════════════════════════════════════════════════════════════════

    [KernelFunction("GetCandidateDestinations")]
    [Description("Returns the catalog of candidate travel destinations. Each entry includes airport Code, City, Country, CountryCode, Prestige (0-100), and Region. Use this first to see what destinations exist. Optionally filter by region.")]
    public string GetCandidateDestinations(
        [Description("Region filter: Asia, Europe, Africa, Americas, Oceania, or All")] string region = "All")
    {
        var memberOrigins = _members.Select(m => m.Origin).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var results = Destinations
            .Where(d => !memberOrigins.Contains(d.Code))
            .Where(d => region == "All" || GetRegion(d.CountryCode) == region)
            .Select(d => new
            {
                d.Code, d.City, d.Country, d.CountryCode,
                d.Prestige, Region = GetRegion(d.CountryCode)
            })
            .ToList();

        return JsonSerializer.Serialize(results);
    }

    // ════════════════════════════════════════════════════════════════
    //  BATCH EVALUATION TOOL (primary workhorse)
    // ════════════════════════════════════════════════════════════════

    [KernelFunction("EvaluateDestination")]
    [Description("Evaluates a single destination for ALL group members at once. Returns per-member flight time, cost in USD, visa status, budget severity, and overall group fairness score. Use this as the primary analysis tool for shortlisted candidates.")]
    public async Task<string> EvaluateDestination(
        [Description("Destination airport IATA code, e.g. TBS")] string destinationCode,
        [Description("Destination ISO country code, e.g. GE")] string destinationCountryCode)
    {
        var dest = Destinations.FirstOrDefault(d =>
            d.Code.Equals(destinationCode, StringComparison.OrdinalIgnoreCase));
        if (dest == default)
            return JsonSerializer.Serialize(new { Error = $"Unknown destination: {destinationCode}" });

        var memberAnalyses = new List<MemberRouteAnalysis>();
        var flightTimes = new List<int>();

        foreach (var member in _members)
        {
            try
            {
                var route = await _feasibility.AnalyseAsync(
                    member.Origin, destinationCode, member.Passports, destinationCountryCode);

                var budgetResult = _budget.Analyse(route.EstimatedCostUsd, member.Budget);

                var convertedCost = ConvertCurrency(route.EstimatedCostUsd, member.PreferredCurrency);
                flightTimes.Add(route.FlightTimeMinutes);

                memberAnalyses.Add(new MemberRouteAnalysis
                {
                    Name = member.Name,
                    UserId = member.UserId,
                    Origin = member.Origin,
                    FlightTime = route.FlightTimeFormatted,
                    FlightTimeMinutes = route.FlightTimeMinutes,
                    CostUsd = route.EstimatedCostUsd,
                    ConvertedCost = convertedCost,
                    Currency = member.PreferredCurrency,
                    VisaType = route.VisaType,
                    VisaRequired = route.VisaRequired,
                    BudgetPercentUsed = budgetResult.PercentageUsed,
                    BudgetSeverity = budgetResult.Severity,
                    IsWithinBudget = budgetResult.IsWithinBudget
                });
            }
            catch
            {
                memberAnalyses.Add(new MemberRouteAnalysis
                {
                    Name = member.Name,
                    Origin = member.Origin,
                    FlightTime = "N/A",
                    BudgetSeverity = "unknown"
                });
            }
        }

        var allRoutable = memberAnalyses.All(m => m.FlightTime != "N/A");
        var fairness = allRoutable ? _timeOverlap.Analyse(flightTimes) : null;

        var evaluation = new DestinationEvaluation
        {
            DestinationCode = destinationCode,
            City = dest.City,
            Country = dest.Country,
            PrestigeScore = dest.Prestige,
            Members = memberAnalyses,
            Fairness = new GroupFairnessAnalysis
            {
                AvgFlightTime = fairness?.AvgFlightFormatted ?? "N/A",
                MaxDifferenceMinutes = fairness?.MaxDifferenceMinutes ?? 0,
                FrictionScore = fairness?.FrictionScore ?? 0,
                FairnessScore = fairness?.NormalizedScore ?? 0
            }
        };

        _evaluationCache[destinationCode] = evaluation;

        var avgCost = memberAnalyses.Where(m => m.CostUsd > 0).Select(m => m.CostUsd).DefaultIfEmpty(0).Average();

        return JsonSerializer.Serialize(new
        {
            evaluation.DestinationCode,
            evaluation.City,
            evaluation.Country,
            evaluation.PrestigeScore,
            AllMembersRoutable = allRoutable,
            AnyVisaRequired = memberAnalyses.Any(m => m.VisaRequired),
            VisaBlockedMembers = memberAnalyses.Where(m => m.VisaRequired).Select(m => $"{m.Name} ({m.VisaType})").ToList(),
            AllWithinBudget = memberAnalyses.All(m => m.IsWithinBudget),
            OverBudgetMembers = memberAnalyses.Where(m => !m.IsWithinBudget).Select(m => $"{m.Name} ({m.BudgetPercentUsed:F0}% used)").ToList(),
            AvgCostUsd = (int)avgCost,
            Fairness = evaluation.Fairness,
            Members = memberAnalyses.Select(m => new
            {
                m.Name, m.Origin, m.FlightTime, m.FlightTimeMinutes,
                m.CostUsd, m.VisaRequired, m.VisaType,
                m.BudgetSeverity, m.IsWithinBudget, m.BudgetPercentUsed
            })
        });
    }

    // ════════════════════════════════════════════════════════════════
    //  BULK EVALUATION TOOL (reduces Gemini round-trips)
    // ════════════════════════════════════════════════════════════════

    [KernelFunction("EvaluateMultipleDestinations")]
    [Description("Batch-evaluates MULTIPLE destinations for the entire group in a single call. MUCH more efficient than calling EvaluateDestination repeatedly. Pass comma-separated airport codes. Returns an array of full evaluation results.")]
    public async Task<string> EvaluateMultipleDestinations(
        [Description("Comma-separated destination airport IATA codes, e.g. 'TBS,SJJ,BKK,BEG,SOF,GYD'")] string destinationCodesCsv)
    {
        var codes = destinationCodesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var results = new List<JsonElement>();

        foreach (var code in codes)
        {
            var dest = Destinations.FirstOrDefault(d =>
                d.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (dest == default) continue;

            var json = await EvaluateDestination(code, dest.CountryCode);
            results.Add(JsonSerializer.Deserialize<JsonElement>(json));
        }

        return JsonSerializer.Serialize(results);
    }

    // ════════════════════════════════════════════════════════════════
    //  ATOMIC TOOLS (fine-grained investigation)
    // ════════════════════════════════════════════════════════════════

    [KernelFunction("CheckRouteFeasibility")]
    [Description("Checks flight feasibility for a SINGLE member on a SINGLE route. Returns flight time, estimated cost in USD, and visa requirements. Use this for targeted investigation of one specific member-route pair.")]
    public async Task<string> CheckRouteFeasibility(
        [Description("Origin airport IATA code, e.g. IST")] string originCode,
        [Description("Destination airport IATA code, e.g. CDG")] string destinationCode,
        [Description("Passport country code, e.g. TR")] string passportCode,
        [Description("Destination country code, e.g. FR")] string destinationCountryCode)
    {
        try
        {
            var result = await _feasibility.AnalyseAsync(
                originCode, destinationCode, new List<string> { passportCode }, destinationCountryCode);

            return JsonSerializer.Serialize(new
            {
                result.Origin,
                result.Destination,
                result.FlightTimeFormatted,
                result.FlightTimeMinutes,
                result.EstimatedCostUsd,
                result.VisaRequired,
                result.VisaType
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Route check failed: {ex.Message}" });
        }
    }

    [KernelFunction("AnalyzeBudgetFit")]
    [Description("Analyzes whether a specific flight cost fits within a member's travel budget. Returns percentage used and severity rating (comfortable / moderate / tight / over).")]
    public string AnalyzeBudgetFit(
        [Description("Estimated flight cost in USD")] int flightCostUsd,
        [Description("Member's total travel budget in USD")] int memberBudgetUsd)
    {
        var result = _budget.Analyse(flightCostUsd, memberBudgetUsd);
        return JsonSerializer.Serialize(new
        {
            result.TicketPrice,
            result.UserBudget,
            result.PercentageUsed,
            result.IsWithinBudget,
            result.Severity,
            result.Score
        });
    }

    [KernelFunction("AnalyzeGroupFlightFairness")]
    [Description("Analyzes how balanced/fair flight times are across group members for a destination. Lower FrictionScore means more equitable travel burden. Higher FairnessScore (0-100) is better.")]
    public string AnalyzeGroupFlightFairness(
        [Description("Comma-separated flight times in minutes for each member, e.g. '110,280,500'")] string flightTimesMinutesCsv)
    {
        var times = flightTimesMinutesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var v) ? v : 0)
            .Where(v => v > 0)
            .ToList();

        if (times.Count == 0)
            return JsonSerializer.Serialize(new { Error = "No valid flight times provided" });

        var result = _timeOverlap.Analyse(times);
        return JsonSerializer.Serialize(new
        {
            result.AvgFlightFormatted,
            result.MinFlightMinutes,
            result.MaxFlightMinutes,
            result.MaxDifferenceMinutes,
            result.FrictionScore,
            FairnessScore = result.NormalizedScore
        });
    }

    // ════════════════════════════════════════════════════════════════
    //  INTELLIGENCE TOOLS (deeper context for reasoning)
    // ════════════════════════════════════════════════════════════════

    [KernelFunction("GetCityIntelligence")]
    [Description("Gets city intelligence data: safety index (0-100, higher=safer), cost of living index (relative to NYC=100), average meal and transport costs in USD, and best months to visit. Use this to factor in daily expenses and livability.")]
    public async Task<string> GetCityIntelligence(
        [Description("City name exactly as listed in candidates, e.g. Bangkok or Tbilisi")] string cityName)
    {
        var intel = await _context.CityIntelligences
            .FirstOrDefaultAsync(c => c.CityName == cityName);

        if (intel == null)
            return JsonSerializer.Serialize(new { Error = $"No intelligence data available for {cityName}" });

        return JsonSerializer.Serialize(new
        {
            intel.CityName, intel.Country,
            intel.SafetyIndex, intel.CostOfLivingIndex,
            intel.AverageMealCostUSD, intel.AverageTransportCostUSD,
            intel.BestMonthsToVisit
        });
    }

    [KernelFunction("CheckVisaMatrix")]
    [Description("Checks detailed visa requirements from the visa matrix database for a specific passport country and destination country. Returns VisaFree, eVisa, or Required.")]
    public async Task<string> CheckVisaMatrix(
        [Description("Passport country name, e.g. Turkey")] string passportCountry,
        [Description("Destination country name, e.g. Georgia")] string destinationCountry)
    {
        var record = await _context.VisaMatrices
            .FirstOrDefaultAsync(v => v.PassportCountry == passportCountry && v.DestinationCountry == destinationCountry);

        if (record == null)
            return JsonSerializer.Serialize(new
            {
                PassportCountry = passportCountry,
                DestinationCountry = destinationCountry,
                VisaStatus = "Unknown — not in database"
            });

        return JsonSerializer.Serialize(new
        {
            record.PassportCountry,
            record.DestinationCountry,
            record.VisaStatus
        });
    }

    [KernelFunction("GetCurrentWeather")]
    [Description("Gets live current weather conditions for a city. Returns temperature in Celsius and condition (Clear, Cloudy, Rainy, Snowy, etc). Use this to factor real-time weather into the destination decision.")]
    public async Task<string> GetCurrentWeather(
        [Description("City name, e.g. Sarajevo or Bangkok")] string cityName)
    {
        var weather = await _insight.GenerateInsightAsync(cityName);
        return weather;
    }

    // ════════════════════════════════════════════════════════════════
    //  ACCOMMODATION & ATTRACTION TOOLS
    // ════════════════════════════════════════════════════════════════

    [KernelFunction("GetAccommodationZones")]
    [Description("Returns accommodation zone options for a given city, including zone name, description, category (Budget/Mid-Range/Luxury), and average nightly cost. Use this to recommend where group members should stay.")]
    public async Task<string> GetAccommodationZones(
        [Description("City name exactly as listed, e.g. Belgrade or Bangkok")] string cityName)
    {
        var zones = await _context.AccommodationZones
            .Where(z => z.CityName == cityName)
            .OrderBy(z => z.AverageNightlyCost)
            .Select(z => new
            {
                z.ZoneName,
                z.Description,
                z.Category,
                z.AverageNightlyCost,
                z.Currency
            })
            .ToListAsync();

        if (zones.Count == 0)
            return JsonSerializer.Serialize(new { Error = $"No accommodation data available for {cityName}" });

        return JsonSerializer.Serialize(zones);
    }

    [KernelFunction("GetAttractions")]
    [Description("Returns notable attractions and activities for a given city, including name, estimated cost, duration in hours, category (Historical/Museum/Nature/Entertainment), and best time of day. Use this to suggest daily itinerary activities.")]
    public async Task<string> GetAttractions(
        [Description("City name exactly as listed, e.g. Belgrade or Bangkok")] string cityName)
    {
        var attractions = await _context.Attractions
            .Where(a => a.CityName == cityName)
            .OrderBy(a => a.EstimatedDurationInHours)
            .Select(a => new
            {
                a.Name,
                a.EstimatedCost,
                a.EstimatedDurationInHours,
                a.Description,
                a.Category,
                a.BestTimeOfDay
            })
            .ToListAsync();

        if (attractions.Count == 0)
            return JsonSerializer.Serialize(new { Error = $"No attraction data available for {cityName}" });

        return JsonSerializer.Serialize(attractions);
    }

    // ════════════════════════════════════════════════════════════════
    //  INTERNAL HELPERS (not exposed to Agent)
    // ════════════════════════════════════════════════════════════════

    private int ConvertCurrency(int usdAmount, string targetCurrency) =>
        _currencyRates.Convert(usdAmount, targetCurrency);

    private static string GetRegion(string countryCode) => countryCode switch
    {
        "SG" or "TH" or "MY" or "VN" or "ID" or "PH" or "JP" or "KR" or "AZ" or "GE" or "AE" or "QA" => "Asia",
        "BA" or "BG" or "RS" or "FR" or "ES" or "GB" or "IT" => "Europe",
        "MA" or "ZA" => "Africa",
        "US" or "MX" or "CO" or "AR" => "Americas",
        "NZ" => "Oceania",
        _ => "Other"
    };
}
