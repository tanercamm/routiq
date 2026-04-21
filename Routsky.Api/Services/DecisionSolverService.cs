using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Routsky.Api.Configuration;
using Routsky.Api.Data;

namespace Routsky.Api.Services;

/// <summary>
/// The Orchestrator: Decision Solver
/// Fetches member data → calls MCP atoms for facts → passes to Gemini Agent Brain for reasoning → picks winner with explanation.
/// This is a true "Agent Brain" — it does NOT hardcode winners.
/// </summary>
public class DecisionSolverService : IDecisionSolverService
{
    private readonly RoutskyDbContext _context;
    private readonly IRouteFeasibilityService _feasibility;
    private readonly IBudgetConsistencyService _budget;
    private readonly ITimeOverlapService _timeOverlap;
    private readonly Kernel _kernel;
    private readonly ILogger<DecisionSolverService> _logger;
    private readonly BudgetDefaults _budgetDefaults;
    private readonly CurrencyRates _currencyRates;
    private readonly PrestigeMapping _prestigeMapping;
    private readonly DiscoverDefaults _discoverDefaults;
    private readonly GeminiSettings _geminiSettings;

    public DecisionSolverService(
        RoutskyDbContext context,
        IRouteFeasibilityService feasibility,
        IBudgetConsistencyService budget,
        ITimeOverlapService timeOverlap,
        Kernel kernel,
        ILogger<DecisionSolverService> logger,
        IOptions<BudgetDefaults> budgetDefaults,
        IOptions<CurrencyRates> currencyRates,
        IOptions<PrestigeMapping> prestigeMapping,
        IOptions<DiscoverDefaults> discoverDefaults,
        IOptions<GeminiSettings> geminiSettings)
    {
        _context = context;
        _feasibility = feasibility;
        _budget = budget;
        _timeOverlap = timeOverlap;
        _kernel = kernel;
        _logger = logger;
        _budgetDefaults = budgetDefaults.Value;
        _currencyRates = currencyRates.Value;
        _prestigeMapping = prestigeMapping.Value;
        _discoverDefaults = discoverDefaults.Value;
        _geminiSettings = geminiSettings.Value;
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
        public int ConvertedCost { get; set; }
        public string Currency { get; set; } = "USD";
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
        public int AvgConvertedCost { get; set; }
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
        /// <summary>"gemini" when AI produced the result, "fallback" when deterministic scoring was used.</summary>
        public string Source { get; set; } = "gemini";
    }

    public class DiscoverRequest
    {
        public string Passport { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string BudgetLimit { get; set; } = "Any";
        public string Duration { get; set; } = "Any";
        public string Region { get; set; } = "All";

        /// <summary>Backpacker, Comfort, or Luxury. Null = no preference.</summary>
        public string? TravelStyle { get; set; }
        /// <summary>Relaxed, Moderate, or Fast. Null = Moderate.</summary>
        public string? Pace { get; set; }
        /// <summary>Historical, Nature, Entertainment, Museum, etc. Null = no preference.</summary>
        public List<string>? Interests { get; set; }
    }

    public class MemberInfo
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public List<string> Passports { get; set; } = new();
        public int Budget { get; set; } = 0;
        public string PreferredCurrency { get; set; } = "USD";
    }

    /// <summary>
    /// Load candidate destinations from the database.
    /// Only cities with an IATA code can be evaluated by the flight engine.
    /// PopularityWeight is mapped to a 0-100 PrestigeScore.
    /// </summary>
    private async Task<List<(string Code, string City, string Country, string CountryCode, int PrestigeScore)>> LoadCandidatesAsync(string? regionFilter = null)
    {
        var query = _context.Destinations
            .Where(d => d.IsActive && d.IataCode != null);

        if (!string.IsNullOrEmpty(regionFilter) && regionFilter != "All")
            query = query.Where(d => d.Region == regionFilter);

        var destinations = await query
            .Select(d => new { d.IataCode, d.City, d.Country, d.CountryCode, d.PopularityWeight })
            .ToListAsync();

        return destinations.Select(d => (
            Code: d.IataCode!,
            City: d.City,
            Country: d.Country,
            CountryCode: d.CountryCode,
            PrestigeScore: (int)Math.Clamp(d.PopularityWeight * _prestigeMapping.Multiplier, _prestigeMapping.MinScore, _prestigeMapping.MaxScore)
        )).ToList();
    }

    /// <summary>
    /// Run the full decision pipeline for a travel group utilizing the Gemini Agent Orchestrator.
    /// </summary>
    public async Task<DecisionResult> SolveAsync(
        Guid groupId,
        Func<string, Task>? onStatus = null,
        CancellationToken ct = default)
    {
        // ── Phase 1: Search (Gather Constraints) ──
        var (members, skippedWarnings) = await FetchMembersAsync(groupId);
        if (members.Count < 2)
        {
            var reason = skippedWarnings.Count > 0
                ? $"Need at least 2 members with set origins. Skipped: {string.Join(", ", skippedWarnings)}"
                : "Need at least 2 members with set origins to run the intersection engine.";
            return new DecisionResult { Explanation = reason };
        }

        // ── Phase 2: Evaluation (Pre-fetch Facts from live APIs) ──
        var candidateDestinations = await LoadCandidatesAsync();
        var factsList = new List<object>();
        var storedTickets = new Dictionary<string, List<MemberTicket>>();

        var allRoutePairs = new List<(string Origin, string Destination)>();
        foreach (var (code, _, _, _, _) in candidateDestinations)
        {
            if (members.Any(m => m.Origin.Equals(code, StringComparison.OrdinalIgnoreCase)))
                continue;
            foreach (var member in members)
                allRoutePairs.Add((member.Origin, code));
        }
        if (onStatus != null) await onStatus("Consulting flight intelligence APIs...");
        await _feasibility.PreloadFlightEstimatesAsync(allRoutePairs);

        foreach (var (code, city, country, countryCode, prestigeScore) in candidateDestinations)
        {
            if (members.Any(m => m.Origin.Equals(code, StringComparison.OrdinalIgnoreCase)))
                continue;

            var rawTickets = new List<object>();
            var hydratedTickets = new List<MemberTicket>();

            foreach (var member in members)
            {
                RouteFeasibilityService.FeasibilityResult feasibility;
                try
                {
                    feasibility = await _feasibility.AnalyseAsync(member.Origin, code, member.Passports, countryCode);
                }
                catch (Exception)
                {
                    continue;
                }

                var currency = member.PreferredCurrency;
                var convertedCost = _currencyRates.Convert(feasibility.EstimatedCostUsd, currency);

                rawTickets.Add(new
                {
                    MemberName = member.Name,
                    FlightTimeMinutes = feasibility.FlightTimeMinutes,
                    EstimatedFlightCostUsd = feasibility.EstimatedCostUsd,
                    VisaRequired = feasibility.VisaRequired,
                    MemberBudgetUsd = member.Budget
                });

                hydratedTickets.Add(new MemberTicket
                {
                    MemberName = member.Name,
                    MemberId = member.UserId,
                    Origin = member.Origin,
                    Destination = code,
                    FlightTime = feasibility.FlightTimeFormatted,
                    FlightTimeMinutes = feasibility.FlightTimeMinutes,
                    CostUsd = feasibility.EstimatedCostUsd,
                    ConvertedCost = convertedCost,
                    Currency = currency,
                    VisaType = feasibility.VisaType,
                    VisaRequired = feasibility.VisaRequired,
                    BudgetSeverity = feasibility.EstimatedCostUsd > member.Budget ? "over" : "comfortable",
                    BudgetPercentUsed = member.Budget > 0 ? (feasibility.EstimatedCostUsd / (double)member.Budget) * 100 : 50
                });
            }

            if (rawTickets.Count < members.Count) continue; // Skip if a flight couldn't be routed

            factsList.Add(new
            {
                DestinationCode = code,
                City = city,
                PrestigeScore = prestigeScore,
                Tickets = rawTickets
            });
            storedTickets[code] = hydratedTickets;
        }

        // ── Phase 3: Decision & Synthesis (Agent Prompt) ──
        var prompt = $@"
ROLE: Group travel decision engine for {members.Count} members. TERMINAL-STYLE OUTPUT ONLY.
STRICT OUTPUT RULES: No greetings, no salutations, no filler phrases, no conversational tone. Zero ""Hello"", ""I've reviewed"", ""I'm excited"", ""Great news"". Write like a flight operations terminal — direct, factual, concise.

Raw Facts:
{JsonSerializer.Serialize(factsList)}

Decision Rules:
1. YOU are the sole authority on the logical winner. No hardcoded logic.
2. Eliminate destinations requiring Visa when VisaRequired = true.
3. Majority must stay within their personal MemberBudgetUsd. Cost = typical return flight.
4. Minimize FlightTimeMinutes variance between members for fairness.

Explanation MUST be 2-4 sentences, max 150 words. State the winner, the key metric that decided it, and one reason runners-up lost. No bullet points, no numbering.

Respond STRICTLY with valid JSON, NO markdown. Do not wrap in ```json.
{{
  ""Winner"": {{ ""DestinationCode"": ""XYZ"", ""City"": ""CityName"", ""Country"": ""CountryName"", ""CompositeScore"": 95, ""AvgCostUsd"": 1000, ""AvgFlightTime"": ""2h 30m"" }},
  ""Alternatives"": [ {{ ""DestinationCode"": ""ABC"", ""City"": ""CityName"", ""Country"": ""CountryName"", ""CompositeScore"": 85, ""AvgCostUsd"": 1200, ""AvgFlightTime"": ""3h"" }} ],
  ""Explanation"": ""Direct factual reasoning. No filler."",
  ""EliminatedReasons"": {{ ""CDE"": ""Visa required for Member1"", ""FGH"": ""Over budget for Member2"" }}
}}
";
        return await ExecuteAgentPrompt(prompt, storedTickets, candidateDestinations);
    }


    /// <summary>
    /// Discover route logic - Single User Agent orchestration
    /// </summary>
    public async Task<DecisionResult> SolveDiscoverAsync(
        DiscoverRequest request,
        Func<string, Task>? onStatus = null,
        CancellationToken ct = default)
    {
        var passports = string.IsNullOrWhiteSpace(request.Passport)
            ? new List<string> { "TR" }
            : new List<string> { request.Passport };
        var origin = request.Origin;
        if (string.IsNullOrWhiteSpace(origin))
            origin = PassportHubResolver.Resolve(passports[0]);

        int maxBudget = request.BudgetLimit switch
        {
            "< $500" => 500,
            "< $1000" => 1000,
            "< $1500" => 1500,
            "< $3000" => 3000,
            "< $5000" => 5000,
            _ => 10000
        };

        var durationDays = request.Duration == "2-3 Days" ? 3 : request.Duration == "4-7 Days" ? 5 : request.Duration == "1-2 Weeks" ? 10 : 7;

        var candidateDestinations = await LoadCandidatesAsync(request.Region);
        var factsList = new List<object>();
        var storedTickets = new Dictionary<string, List<MemberTicket>>();

        var discoverPairs = candidateDestinations
            .Where(d => !origin.Equals(d.Code, StringComparison.OrdinalIgnoreCase))
            .Select(d => (Origin: origin, Destination: d.Code))
            .ToList();
        if (onStatus != null) await onStatus("Consulting flight intelligence APIs...");
        await _feasibility.PreloadFlightEstimatesAsync(discoverPairs);

        foreach (var (code, city, country, countryCode, prestigeScore) in candidateDestinations)
        {
            if (origin.Equals(code, StringComparison.OrdinalIgnoreCase)) continue;

            var intelligence = await _context.CityIntelligences.FirstOrDefaultAsync(c => c.CityName == city);
            if (intelligence == null) continue;

            RouteFeasibilityService.FeasibilityResult feasibility;
            try { feasibility = await _feasibility.AnalyseAsync(origin, code, passports, countryCode); }
            catch (Exception) { continue; }

            double dailyCostUsd = _discoverDefaults.DailyCostBaseUsd * (intelligence.CostOfLivingIndex / 100.0);
            double totalLandCost = dailyCostUsd * durationDays;
            double projectedTotalCost = feasibility.EstimatedCostUsd + totalLandCost;
            bool isVisaRequired = feasibility.VisaRequired;
            var visaType = feasibility.VisaType;

            var currency = origin == "SYD" ? "AUD" : origin == "BER" ? "EUR" : origin == "IST" ? "TRY" : "USD";
            var convertedCost = _currencyRates.Convert(feasibility.EstimatedCostUsd, currency);

            var accommodations = await _context.AccommodationZones
                .Where(z => z.CityName == city)
                .Select(z => new { z.ZoneName, z.Category, z.AverageNightlyCost })
                .ToListAsync();

            var cityAttractions = await _context.Attractions
                .Where(a => a.CityName == city)
                .Select(a => new { a.Name, a.Category, a.EstimatedCost, a.EstimatedDurationInHours })
                .ToListAsync();

            factsList.Add(new
            {
                DestinationCode = code,
                City = city,
                EstimatedTotalTripCostUsd = (int)projectedTotalCost,
                FlightCostUsd = feasibility.EstimatedCostUsd,
                FlightTimeMinutes = feasibility.FlightTimeMinutes,
                VisaRequired = isVisaRequired,
                SafetyIndex = intelligence.SafetyIndex,
                PrestigeScore = prestigeScore,
                AccommodationZones = accommodations,
                Attractions = cityAttractions
            });

            storedTickets[code] = new List<MemberTicket> { new MemberTicket
            {
                MemberName = "You",
                Origin = origin,
                Destination = code,
                FlightTime = feasibility.FlightTimeFormatted,
                FlightTimeMinutes = feasibility.FlightTimeMinutes,
                CostUsd = feasibility.EstimatedCostUsd,
                ConvertedCost = convertedCost,
                Currency = currency,
                VisaType = visaType,
                VisaRequired = isVisaRequired,
                BudgetSeverity = projectedTotalCost > maxBudget ? "over" : "comfortable",
                BudgetPercentUsed = maxBudget < _discoverDefaults.MaxBudgetCap ? (projectedTotalCost / maxBudget) * 100 : _discoverDefaults.DefaultBudgetPercentUsed
            }};
        }

        var travelStyle = request.TravelStyle ?? "Any";
        var pace = request.Pace ?? "Moderate";
        var interests = request.Interests is { Count: > 0 }
            ? string.Join(", ", request.Interests)
            : "Any";

        var prompt = $@"
ROLE: Solo travel decision engine with preference-aware reasoning. TERMINAL-STYLE OUTPUT ONLY.
STRICT OUTPUT RULES: No greetings, no salutations, no filler phrases, no conversational tone. Zero ""Hello"", ""I've reviewed"", ""I'm excited"", ""Great news"". Write like a flight operations terminal — direct, factual, concise.

═══ USER CONSTRAINTS ═══
Budget: ≤ ${maxBudget} total trip cost
Passports: {string.Join(",", passports)}
Duration: {request.Duration} ({durationDays} days)

═══ USER PREFERENCES ═══
TravelStyle: {travelStyle}
  → Map to AccommodationZone categories: Backpacker=Budget zones, Comfort=Mid-Range zones, Luxury=Luxury zones. ""Any"" = no preference.
Pace: {pace}
  → Relaxed = favor cities with fewer but longer attractions (≥2h each). Fast = favor cities with many short attractions. Moderate = balanced.
Interests: {interests}
  → Match against Attraction.Category values in each city's data: Historical, Museum, Nature, Entertainment. ""Any"" = no preference.

═══ RAW FACTS (per candidate) ═══
Each candidate includes: flight cost, total trip cost, visa status, safety index, prestige, AccommodationZones (with Category and NightlyCost), and Attractions (with Category, Cost, DurationInHours).

{JsonSerializer.Serialize(factsList)}

═══ MANDATORY 3-STEP REASONING ═══

STEP 1 — HARD ELIMINATION:
Discard any destination where:
  (a) VisaRequired = true, OR
  (b) EstimatedTotalTripCostUsd > {maxBudget}.
Record every eliminated destination and reason in EliminatedReasons.

STEP 2 — PREFERENCE SCORING (survivors only):
For each surviving destination, compute a rational composite score (0-100) considering:
  (a) Budget efficiency: lower EstimatedTotalTripCostUsd relative to budget cap = higher score.
  (b) TravelStyle alignment: if TravelStyle != ""Any"", check AccommodationZones array. A city with zones matching the mapped category scores higher. No matching zones = penalty.
  (c) Interest alignment: if Interests != ""Any"", count how many of the user's Interests appear in the city's Attractions[].Category values. More matches = higher score. Zero matches = significant penalty.
  (d) Pace fit: if Pace=""Relaxed"", prefer cities with fewer attractions of longer duration. If Pace=""Fast"", prefer cities with many attractions. Moderate = neutral.
  (e) SafetyIndex and PrestigeScore as tiebreakers.
  (f) Flight time: shorter is better, but secondary to preference alignment.

STEP 3 — WINNER SELECTION:
Pick the destination with the highest composite score. In the Explanation, you MUST cite:
  - At least one specific Attraction by name that matches the user's Interests (if Interests != ""Any"" and data exists).
  - The AccommodationZone category that matches TravelStyle (if TravelStyle != ""Any"" and data exists).
  - The concrete metric that separated the winner from the runner-up.
If no accommodation/attraction data exists for the winner, state this explicitly and justify using other metrics.

═══ OUTPUT FORMAT ═══
Explanation MUST be 2-4 sentences, max 200 words. Cite specific attraction names and zone categories from the data.
ALL fields below are REQUIRED for Winner AND every Alternative. Do not omit City, Country, AvgCostUsd, or AvgFlightTime.
Respond STRICTLY in JSON, NO markdown wrapping. Do not wrap in ```json.
{{
  ""Winner"": {{ ""DestinationCode"": ""XYZ"", ""City"": ""CityName"", ""Country"": ""CountryName"", ""CompositeScore"": 95, ""AvgCostUsd"": 1000, ""AvgFlightTime"": ""2h 30m"" }},
  ""Alternatives"": [ {{ ""DestinationCode"": ""ABC"", ""City"": ""CityName"", ""Country"": ""CountryName"", ""CompositeScore"": 85, ""AvgCostUsd"": 1200, ""AvgFlightTime"": ""3h"" }} ],
  ""Explanation"": ""Direct factual reasoning citing specific attractions and zones."",
  ""EliminatedReasons"": {{ ""DEF"": ""Over budget ($X > ${maxBudget})"", ""GHI"": ""Visa required"" }}
}}
";
        return await ExecuteAgentPrompt(prompt, storedTickets, candidateDestinations);
    }

    private async Task<DecisionResult> ExecuteAgentPrompt(string prompt, Dictionary<string, List<MemberTicket>> storedTickets, List<(string Code, string City, string Country, string CountryCode, int PrestigeScore)>? candidatesList = null)
    {
        try
        {
            _logger.LogInformation("[GeminiClient] Request sent to Google AI — decision synthesis for {CandidateCount} candidates", storedTickets.Count);
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            history.AddSystemMessage(
                "You are Routsky Decision Engine. Output valid JSON only. " +
                "Never use greetings, salutations, or conversational filler in the Explanation field. " +
                "No \"Hello\", \"Hi there\", \"I've reviewed\", \"Great news\", \"I'm excited\". " +
                "Explanation: max 3 sentences of direct analytical logic.");
            history.AddUserMessage(prompt);

            var executionSettings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object> { { "temperature", _geminiSettings.Temperature }, { "topP", _geminiSettings.TopP } }
            };

            var response = await chatService.GetChatMessageContentAsync(history, executionSettings);
            var json = response.Content ?? "{}";
            json = json.Trim();

            if (json.StartsWith("```"))
            {
                var firstNewline = json.IndexOf('\n');
                var lastBackticks = json.LastIndexOf("```");
                if (firstNewline != -1 && lastBackticks > firstNewline)
                {
                    json = json.Substring(firstNewline + 1, lastBackticks - firstNewline - 1).Trim();
                }
            }

            var result = JsonSerializer.Deserialize<DecisionResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result != null)
            {
                result.DecidedAt = DateTime.UtcNow;
                result.Source = "gemini";

                if (!string.IsNullOrEmpty(result.Winner.DestinationCode))
                {
                    if (storedTickets.TryGetValue(result.Winner.DestinationCode, out var winnerTickets))
                        result.Winner.MemberTickets = winnerTickets;

                    if (candidatesList != null)
                    {
                        var knownWinner = candidatesList.FirstOrDefault(c => c.Code == result.Winner.DestinationCode);
                        if (knownWinner != default)
                        {
                            if (string.IsNullOrEmpty(result.Winner.City)) result.Winner.City = knownWinner.City;
                            if (string.IsNullOrEmpty(result.Winner.Country)) result.Winner.Country = knownWinner.Country;
                        }
                    }

                    if (result.Winner.MemberTickets.Count > 0)
                    {
                        if (result.Winner.AvgCostUsd <= 0)
                            result.Winner.AvgCostUsd = (int)result.Winner.MemberTickets.Average(t => t.CostUsd);
                        if (result.Winner.AvgConvertedCost <= 0)
                            result.Winner.AvgConvertedCost = (int)result.Winner.MemberTickets.Average(t => t.ConvertedCost);
                        if (string.IsNullOrEmpty(result.Winner.AvgFlightTime))
                        {
                            var avgMin = (int)result.Winner.MemberTickets.Average(t => t.FlightTimeMinutes);
                            result.Winner.AvgFlightTime = $"{avgMin / 60}h {avgMin % 60}m";
                        }
                    }
                }

                foreach (var alt in result.Alternatives)
                {
                    if (string.IsNullOrEmpty(alt.DestinationCode)) continue;

                    if (storedTickets.TryGetValue(alt.DestinationCode, out var altTickets))
                        alt.MemberTickets = altTickets;

                    if (candidatesList != null)
                    {
                        var known = candidatesList.FirstOrDefault(c => c.Code == alt.DestinationCode);
                        if (known != default)
                        {
                            if (string.IsNullOrEmpty(alt.City)) alt.City = known.City;
                            if (string.IsNullOrEmpty(alt.Country)) alt.Country = known.Country;
                        }
                    }

                    if (alt.MemberTickets.Count > 0)
                    {
                        if (alt.AvgCostUsd <= 0)
                            alt.AvgCostUsd = (int)alt.MemberTickets.Average(t => t.CostUsd);
                        if (alt.AvgConvertedCost <= 0)
                            alt.AvgConvertedCost = (int)alt.MemberTickets.Average(t => t.ConvertedCost);
                        if (string.IsNullOrEmpty(alt.AvgFlightTime))
                        {
                            var avgMin = (int)alt.MemberTickets.Average(t => t.FlightTimeMinutes);
                            alt.AvgFlightTime = $"{avgMin / 60}h {avgMin % 60}m";
                        }
                    }
                }

                // Remove alternatives that couldn't be hydrated from storedTickets
                // This is the primary fix for the "$0" bug — alternatives returned
                // by Gemini that aren't in our candidates can't be costed.
                result.Alternatives.RemoveAll(a =>
                    string.IsNullOrEmpty(a.City) ||
                    a.AvgCostUsd <= 0 ||
                    !storedTickets.ContainsKey(a.DestinationCode));

                if (!string.IsNullOrEmpty(result.Explanation))
                    result.Explanation = StripConversationalFiller(result.Explanation);

                _logger.LogInformation("[GeminiClient] Response received from Google AI — winner: {Winner}", result.Winner.City);
                return result;
            }

            _logger.LogError("[GeminiClient] Gemini returned a response but deserialization produced null.");
            throw new Exception("Gemini AI failed to produce a valid decision response. No local fallback.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GeminiClient] Gemini AI call FAILED — {Message}. No local fallback, propagating error.", ex.Message);
            throw;
        }
    }



    private async Task<(List<MemberInfo> Members, List<string> Warnings)> FetchMembersAsync(Guid groupId)
    {
        var dbMembers = await _context.TravelGroupMembers
            .Where(m => m.GroupId == groupId)
            .Include(m => m.User).ThenInclude(u => u!.Profile).ToListAsync();

        var members = new List<MemberInfo>();
        var warnings = new List<string>();

        foreach (var m in dbMembers.Where(m => m.User != null))
        {
            var name = $"{m.User!.FirstName} {m.User.LastName}".Trim();
            var origin = m.User.Profile?.Origin;
            var passports = m.User.Profile?.Passports;

            if (string.IsNullOrWhiteSpace(origin) && (passports == null || passports.Count == 0))
            {
                warnings.Add($"{name} (no origin/passport set)");
                continue;
            }

            var resolvedOrigin = !string.IsNullOrWhiteSpace(origin) ? origin : PassportHubResolver.Resolve(passports!.First());
            var budget = m.User.Profile?.Budget ?? 0;

            members.Add(new MemberInfo
            {
                UserId = m.UserId,
                Name = name,
                Origin = resolvedOrigin,
                Passports = passports ?? new List<string>(),
                Budget = budget > 0 ? budget : _budgetDefaults.DefaultBudgetUsd,
                PreferredCurrency = m.User.Profile?.PreferredCurrency ?? "USD"
            });
        }

        return (members, warnings);
    }

    private static readonly Regex FillerPattern = new(
        @"^(Hello[^.!]*[.!]\s*|Hi[^.!]*[.!]\s*|Hey[^.!]*[.!]\s*|Greetings[^.!]*[.!]\s*|I've reviewed[^.!]*[.!]\s*|I have reviewed[^.!]*[.!]\s*|Great news[^.!]*[.!]\s*|I'm excited[^.!]*[.!]\s*|Good news[^.!]*[.!]\s*|Welcome[^.!]*[.!]\s*|Sure[^.!]*[.!]\s*|Absolutely[^.!]*[.!]\s*|Of course[^.!]*[.!]\s*)+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static string StripConversationalFiller(string text)
    {
        var cleaned = FillerPattern.Replace(text, "").TrimStart();
        return string.IsNullOrWhiteSpace(cleaned) ? text.Trim() : cleaned;
    }

}
