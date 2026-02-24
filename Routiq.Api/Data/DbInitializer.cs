using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Entities;

namespace Routiq.Api.Data;

/// <summary>
/// V2 Seed Data. Populates:
/// - RegionPriceTiers (6 regions × 3 cost levels = 18 rows)
/// - Destinations (~40 cities across 6 regions)
/// - VisaRules (~4 passport types × key destination countries)
/// - Users, UserProfiles (2 demo users)
/// - Sample RouteQueries, SavedRoutes, RouteStops, RouteEliminations
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(RoutiqDbContext context)
    {
        // Ensure DB created
        await context.Database.EnsureCreatedAsync();

        await SeedRegionPriceTiersAsync(context);
        await SeedDestinationsAsync(context);
        await SeedVisaRulesAsync(context);
        await SeedUsersAsync(context);
    }

    // ─────────────────────────────────────────────
    // REGION PRICE TIERS (18 rows — truth table)
    // ─────────────────────────────────────────────
    private static async Task SeedRegionPriceTiersAsync(RoutiqDbContext context)
    {
        if (await context.RegionPriceTiers.AnyAsync()) return;

        var now = new DateTime(2026, 2, 24, 0, 0, 0, DateTimeKind.Utc);

        var tiers = new List<RegionPriceTier>
        {
            // Southeast Asia
            new() { Region = "Southeast Asia", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 20,  DailyBudgetUsdMax = 45,  Description = "Hostel + street food + local transport. Very achievable.",           LastReviewedAt = now },
            new() { Region = "Southeast Asia", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 45,  DailyBudgetUsdMax = 100, Description = "Private room + restaurant meals + occasional tuk-tuk/rideshare.", LastReviewedAt = now },
            new() { Region = "Southeast Asia", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 100, DailyBudgetUsdMax = 250, Description = "Boutique hotel + fine dining + private transfers.",                LastReviewedAt = now },

            // Balkans
            new() { Region = "Balkans", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 25,  DailyBudgetUsdMax = 50,  Description = "Hostel + local eateries + buses/trains. Extremely good value.",   LastReviewedAt = now },
            new() { Region = "Balkans", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 110, Description = "Mid-range hotels + sit-down restaurants + occasional taxi.",       LastReviewedAt = now },
            new() { Region = "Balkans", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 110, DailyBudgetUsdMax = 220, Description = "Upscale accommodation + fine dining.",                             LastReviewedAt = now },

            // Eastern Europe
            new() { Region = "Eastern Europe", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 30,  DailyBudgetUsdMax = 55,  Description = "Budget hotel or hostel + cheap local food + metro/tram.",    LastReviewedAt = now },
            new() { Region = "Eastern Europe", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 55,  DailyBudgetUsdMax = 130, Description = "3-star hotel + varied dining + occasional tours.",             LastReviewedAt = now },
            new() { Region = "Eastern Europe", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 130, DailyBudgetUsdMax = 300, Description = "4-5 star hotel + restaurant dining + private transport.",     LastReviewedAt = now },

            // Latin America
            new() { Region = "Latin America", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 25,  DailyBudgetUsdMax = 50,  Description = "Hostel + street food + colectivos. Budget-friendly region.",  LastReviewedAt = now },
            new() { Region = "Latin America", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 120, Description = "Budget hotel + restaurant meals + Uber/bus.",                  LastReviewedAt = now },
            new() { Region = "Latin America", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 120, DailyBudgetUsdMax = 280, Description = "Boutique hotel + fine dining + private transfers.",            LastReviewedAt = now },

            // North Africa
            new() { Region = "North Africa", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 20,  DailyBudgetUsdMax = 40,  Description = "Cheap riad room + street food + shared transport.",            LastReviewedAt = now },
            new() { Region = "North Africa", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 40,  DailyBudgetUsdMax = 90,  Description = "Mid-range riad + restaurant meals + taxi.",                    LastReviewedAt = now },
            new() { Region = "North Africa", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 90,  DailyBudgetUsdMax = 200, Description = "Luxury riad + fine dining + private drivers.",                 LastReviewedAt = now },

            // Central America
            new() { Region = "Central America", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 30,  DailyBudgetUsdMax = 55,  Description = "Hostel + comida corrida + chicken buses.",                  LastReviewedAt = now },
            new() { Region = "Central America", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 55,  DailyBudgetUsdMax = 120, Description = "Budget hotel + restaurant meals + shuttle services.",        LastReviewedAt = now },
            new() { Region = "Central America", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 120, DailyBudgetUsdMax = 260, Description = "Eco-lodge/boutique hotel + fine dining + private transport.", LastReviewedAt = now },
        };

        context.RegionPriceTiers.AddRange(tiers);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // DESTINATIONS (~40 cities)
    // ─────────────────────────────────────────────
    private static async Task SeedDestinationsAsync(RoutiqDbContext context)
    {
        if (await context.Destinations.AnyAsync()) return;

        var destinations = new List<Destination>
        {
            // Southeast Asia
            new() { Country = "Thailand",    CountryCode = "TH", City = "Bangkok",      Region = "Southeast Asia", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 3, MaxRecommendedDays = 7,  PopularityWeight = 1.8 },
            new() { Country = "Thailand",    CountryCode = "TH", City = "Chiang Mai",   Region = "Southeast Asia", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 3, MaxRecommendedDays = 7,  PopularityWeight = 1.6 },
            new() { Country = "Vietnam",     CountryCode = "VN", City = "Hanoi",        Region = "Southeast Asia", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 3, MaxRecommendedDays = 5,  PopularityWeight = 1.5 },
            new() { Country = "Vietnam",     CountryCode = "VN", City = "Ho Chi Minh City", Region = "Southeast Asia", DailyCostLevel = CostLevel.Low, MinRecommendedDays = 3, MaxRecommendedDays = 5, PopularityWeight = 1.5 },
            new() { Country = "Vietnam",     CountryCode = "VN", City = "Hoi An",       Region = "Southeast Asia", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.4 },
            new() { Country = "Cambodia",    CountryCode = "KH", City = "Siem Reap",    Region = "Southeast Asia", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.3 },
            new() { Country = "Indonesia",   CountryCode = "ID", City = "Bali",         Region = "Southeast Asia", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 5, MaxRecommendedDays = 14, PopularityWeight = 1.9 },
            new() { Country = "Malaysia",    CountryCode = "MY", City = "Kuala Lumpur", Region = "Southeast Asia", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 3, MaxRecommendedDays = 5,  PopularityWeight = 1.4 },

            // Balkans
            new() { Country = "Serbia",      CountryCode = "RS", City = "Belgrade",     Region = "Balkans", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.5, Notes = "No visa for most passports. Excellent nightlife hub." },
            new() { Country = "North Macedonia", CountryCode = "MK", City = "Skopje",   Region = "Balkans", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 1, MaxRecommendedDays = 3,  PopularityWeight = 1.1 },
            new() { Country = "Albania",     CountryCode = "AL", City = "Tirana",       Region = "Balkans", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 3,  PopularityWeight = 1.2 },
            new() { Country = "Bosnia",      CountryCode = "BA", City = "Sarajevo",     Region = "Balkans", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.3 },
            new() { Country = "Montenegro",  CountryCode = "ME", City = "Kotor",        Region = "Balkans", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.4 },
            new() { Country = "Croatia",     CountryCode = "HR", City = "Dubrovnik",    Region = "Balkans", DailyCostLevel = CostLevel.High,   MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.6, Notes = "Schengen zone. Visa required for some passports." },
            new() { Country = "Slovenia",    CountryCode = "SI", City = "Ljubljana",    Region = "Balkans", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 2, MaxRecommendedDays = 3,  PopularityWeight = 1.2, Notes = "Schengen zone." },

            // Eastern Europe
            new() { Country = "Hungary",     CountryCode = "HU", City = "Budapest",    Region = "Eastern Europe", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 3, MaxRecommendedDays = 5, PopularityWeight = 1.7, Notes = "Schengen zone." },
            new() { Country = "Czech Republic", CountryCode = "CZ", City = "Prague",   Region = "Eastern Europe", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 3, MaxRecommendedDays = 5, PopularityWeight = 1.8, Notes = "Schengen zone." },
            new() { Country = "Poland",      CountryCode = "PL", City = "Krakow",      Region = "Eastern Europe", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4, PopularityWeight = 1.4, Notes = "Schengen zone." },
            new() { Country = "Romania",     CountryCode = "RO", City = "Bucharest",   Region = "Eastern Europe", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4, PopularityWeight = 1.3 },
            new() { Country = "Romania",     CountryCode = "RO", City = "Cluj-Napoca", Region = "Eastern Europe", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 3, PopularityWeight = 1.1 },
            new() { Country = "Bulgaria",    CountryCode = "BG", City = "Sofia",       Region = "Eastern Europe", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 3, PopularityWeight = 1.2 },
            new() { Country = "Georgia",     CountryCode = "GE", City = "Tbilisi",     Region = "Eastern Europe", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 3, MaxRecommendedDays = 6, PopularityWeight = 1.6, Notes = "Visa-free for most passports up to 1 year." },

            // Latin America
            new() { Country = "Colombia",    CountryCode = "CO", City = "Medellín",    Region = "Latin America", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 4, MaxRecommendedDays = 7,  PopularityWeight = 1.6 },
            new() { Country = "Colombia",    CountryCode = "CO", City = "Cartagena",   Region = "Latin America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 3, MaxRecommendedDays = 5,  PopularityWeight = 1.4 },
            new() { Country = "Peru",        CountryCode = "PE", City = "Lima",        Region = "Latin America", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.3 },
            new() { Country = "Peru",        CountryCode = "PE", City = "Cusco",       Region = "Latin America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 3, MaxRecommendedDays = 5,  PopularityWeight = 1.5 },
            new() { Country = "Argentina",   CountryCode = "AR", City = "Buenos Aires",Region = "Latin America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 4, MaxRecommendedDays = 7,  PopularityWeight = 1.5 },
            new() { Country = "Mexico",      CountryCode = "MX", City = "Mexico City", Region = "Latin America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 4, MaxRecommendedDays = 7,  PopularityWeight = 1.6 },

            // North Africa
            new() { Country = "Morocco",     CountryCode = "MA", City = "Marrakech",   Region = "North Africa", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 3, MaxRecommendedDays = 6,  PopularityWeight = 1.6 },
            new() { Country = "Morocco",     CountryCode = "MA", City = "Fez",         Region = "North Africa", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 3,  PopularityWeight = 1.3 },
            new() { Country = "Morocco",     CountryCode = "MA", City = "Tangier",     Region = "North Africa", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 1, MaxRecommendedDays = 2,  PopularityWeight = 1.0 },
            new() { Country = "Tunisia",     CountryCode = "TN", City = "Tunis",       Region = "North Africa", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4,  PopularityWeight = 1.1 },
            new() { Country = "Egypt",       CountryCode = "EG", City = "Cairo",       Region = "North Africa", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 3, MaxRecommendedDays = 6,  PopularityWeight = 1.4, Notes = "eVisa available. Most nationalities can get on arrival." },

            // Central America
            new() { Country = "Guatemala",   CountryCode = "GT", City = "Guatemala City", Region = "Central America", DailyCostLevel = CostLevel.Low,  MinRecommendedDays = 1, MaxRecommendedDays = 2, PopularityWeight = 1.0 },
            new() { Country = "Guatemala",   CountryCode = "GT", City = "Antigua",      Region = "Central America", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 4, PopularityWeight = 1.4 },
            new() { Country = "Costa Rica",  CountryCode = "CR", City = "San José",     Region = "Central America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 2, MaxRecommendedDays = 3, PopularityWeight = 1.3 },
            new() { Country = "Costa Rica",  CountryCode = "CR", City = "La Fortuna",   Region = "Central America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 2, MaxRecommendedDays = 3, PopularityWeight = 1.3 },
            new() { Country = "Panama",      CountryCode = "PA", City = "Panama City",  Region = "Central America", DailyCostLevel = CostLevel.Medium, MinRecommendedDays = 2, MaxRecommendedDays = 4, PopularityWeight = 1.2 },
            new() { Country = "Nicaragua",   CountryCode = "NI", City = "Granada",      Region = "Central America", DailyCostLevel = CostLevel.Low,    MinRecommendedDays = 2, MaxRecommendedDays = 3, PopularityWeight = 1.1 },
        };

        context.Destinations.AddRange(destinations);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // VISA RULES (key passport × country pairs)
    // ─────────────────────────────────────────────
    private static async Task SeedVisaRulesAsync(RoutiqDbContext context)
    {
        if (await context.VisaRules.AnyAsync()) return;

        var now = new DateTime(2026, 2, 24, 0, 0, 0, DateTimeKind.Utc);

        var rules = new List<VisaRule>
        {
            // ── Turkish Passport (TR) ──
            new() { PassportCountryCode = "TR", DestinationCountryCode = "TH", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "VN", Requirement = VisaRequirement.EVisa,      MaxStayDays = 30,  AvgProcessingDays = 3, EVisaUrl = "https://evisa.xuatnhapcanh.gov.vn", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "KH", Requirement = VisaRequirement.OnArrival,  MaxStayDays = 30,  AvgProcessingDays = 0, Notes = "Fee ~$30 USD at airport.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "ID", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "MY", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "RS", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "AL", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "BA", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "ME", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "HR", Requirement = VisaRequirement.Required,   MaxStayDays = 90,  AvgProcessingDays = 15, Notes = "Schengen visa required. Apply at German/Austrian embassy.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "SI", Requirement = VisaRequirement.Required,   MaxStayDays = 90,  AvgProcessingDays = 15, Notes = "Schengen visa required.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "HU", Requirement = VisaRequirement.Required,   MaxStayDays = 90,  AvgProcessingDays = 15, Notes = "Schengen visa required.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "CZ", Requirement = VisaRequirement.Required,   MaxStayDays = 90,  AvgProcessingDays = 15, Notes = "Schengen visa required.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "PL", Requirement = VisaRequirement.Required,   MaxStayDays = 90,  AvgProcessingDays = 15, Notes = "Schengen visa required.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "RO", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  Notes = "Romania not fully Schengen. TR passport holders visa-free.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "BG", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "GE", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 365, AvgProcessingDays = 0,  Notes = "Visa-free up to 1 year.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "MA", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "TN", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "EG", Requirement = VisaRequirement.OnArrival,  MaxStayDays = 30,  AvgProcessingDays = 0,  Notes = "Fee ~$25 USD. Available at Cairo & Sharm airports.", LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "CO", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "PE", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "AR", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "MX", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 180, AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "CR", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "GT", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "TR", DestinationCountryCode = "PA", Requirement = VisaRequirement.VisaFree,   MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },

            // ── Indian Passport (IN) ──
            new() { PassportCountryCode = "IN", DestinationCountryCode = "TH", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 30,  AvgProcessingDays = 0, Notes = "Visa exemption extended for IN passport 2024.", LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "VN", Requirement = VisaRequirement.EVisa,     MaxStayDays = 90,  AvgProcessingDays = 3, EVisaUrl = "https://evisa.xuatnhapcanh.gov.vn", LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "KH", Requirement = VisaRequirement.OnArrival, MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "ID", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "MY", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "HR", Requirement = VisaRequirement.Required,  MaxStayDays = 90,  AvgProcessingDays = 20, Notes = "Schengen visa required.", LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "HU", Requirement = VisaRequirement.Required,  MaxStayDays = 90,  AvgProcessingDays = 20, Notes = "Schengen visa required.", LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "RS", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 30,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "GE", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 365, AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "MA", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "EG", Requirement = VisaRequirement.OnArrival, MaxStayDays = 30,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "CO", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0,  LastReviewedAt = now },
            new() { PassportCountryCode = "IN", DestinationCountryCode = "MX", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 180, AvgProcessingDays = 0,  LastReviewedAt = now },

            // ── US Passport (US) ──
            new() { PassportCountryCode = "US", DestinationCountryCode = "TH", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 30,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "VN", Requirement = VisaRequirement.EVisa,     MaxStayDays = 90,  AvgProcessingDays = 3, EVisaUrl = "https://evisa.xuatnhapcanh.gov.vn", LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "HR", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0, Notes = "Schengen rules apply.", LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "HU", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "GE", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 365, AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "RS", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "MA", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "CO", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 90,  AvgProcessingDays = 0, LastReviewedAt = now },
            new() { PassportCountryCode = "US", DestinationCountryCode = "MX", Requirement = VisaRequirement.VisaFree,  MaxStayDays = 180, AvgProcessingDays = 0, LastReviewedAt = now },
        };

        context.VisaRules.AddRange(rules);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // USERS & PROFILES (2 demo users)
    // ─────────────────────────────────────────────
    private static async Task SeedUsersAsync(RoutiqDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var hasher = new PasswordHasher<User>();

        // User 1: Turkish passport holder (demonstrates Schengen filtering)
        var user1 = new User
        {
            Email = "taner@routiq.app",
            PasswordHash = string.Empty,
            Role = "Admin",
            CreatedAt = new DateTime(2026, 2, 24, 0, 0, 0, DateTimeKind.Utc)
        };
        user1.PasswordHash = hasher.HashPassword(user1, "Admin123!");
        context.Users.Add(user1);
        await context.SaveChangesAsync();

        context.UserProfiles.Add(new UserProfile
        {
            UserId = user1.Id,
            Username = "taner",
            Email = user1.Email,
            PassportCountryCode = "TR",
            CountryCode = "TR",
            PreferredCurrency = "USD",
            Age = 29
        });

        // User 2: Indian passport holder (demonstrates different visa landscape)
        var user2 = new User
        {
            Email = "arjun@routiq.app",
            PasswordHash = string.Empty,
            Role = "User",
            CreatedAt = new DateTime(2026, 2, 24, 0, 0, 0, DateTimeKind.Utc)
        };
        user2.PasswordHash = hasher.HashPassword(user2, "User123!");
        context.Users.Add(user2);
        await context.SaveChangesAsync();

        context.UserProfiles.Add(new UserProfile
        {
            UserId = user2.Id,
            Username = "arjun",
            Email = user2.Email,
            PassportCountryCode = "IN",
            CountryCode = "IN",
            PreferredCurrency = "USD",
            Age = 26
        });

        await context.SaveChangesAsync();

        // ── Sample RouteQuery + SavedRoute + RouteStops + RouteEliminations ──
        await SeedSampleRouteAsync(context, user1.Id);
    }

    private static async Task SeedSampleRouteAsync(RoutiqDbContext context, int userId)
    {
        var bangkok = await context.Destinations.FirstAsync(d => d.City == "Bangkok");
        var chiangMai = await context.Destinations.FirstAsync(d => d.City == "Chiang Mai");
        var hanoi = await context.Destinations.FirstAsync(d => d.City == "Hanoi");
        var hoiAn = await context.Destinations.FirstAsync(d => d.City == "Hoi An");
        var budapest = await context.Destinations.FirstAsync(d => d.City == "Budapest");   // eliminated

        var queryId = Guid.NewGuid();
        var routeId = Guid.NewGuid();
        var queryDate = new DateTime(2026, 2, 24, 9, 0, 0, DateTimeKind.Utc);

        // Query: TR passport, Budget bracket, 21 days, $1400, SEA preference
        var query = new RouteQuery
        {
            Id = queryId,
            UserId = userId,
            PassportCountryCode = "TR",
            BudgetBracket = BudgetBracket.Budget,
            TotalBudgetUsd = 1400,
            DurationDays = 21,
            RegionPreference = RegionPreference.SoutheastAsia,
            HasSchengenVisa = false,
            HasUsVisa = false,
            HasUkVisa = false,
            CreatedAt = queryDate
        };

        var savedRoute = new SavedRoute
        {
            Id = routeId,
            UserId = userId,
            RouteQueryId = queryId,
            RouteName = "SEA Budget Loop: 21 Days",
            Status = RouteStatus.Saved,
            SelectionReason = "All 4 stops are visa-free for TR passport. Total cost estimate (Low tier × 21 days) = $945–$1365 USD, within $1400 cap. Minimum day requirements satisfied.",
            SavedAt = queryDate
        };

        var stops = new List<RouteStop>
        {
            new() { SavedRouteId = routeId, DestinationId = bangkok.Id,   StopOrder = 1, RecommendedDays = 5, ExpectedCostLevel = CostLevel.Low,  StopReason = "Gateway city. Low cost, no visa. Sets up the rest of the route." },
            new() { SavedRouteId = routeId, DestinationId = chiangMai.Id, StopOrder = 2, RecommendedDays = 5, ExpectedCostLevel = CostLevel.Low,  StopReason = "Budget-friendly northern hub. Visa-free continuation within Thailand." },
            new() { SavedRouteId = routeId, DestinationId = hanoi.Id,     StopOrder = 3, RecommendedDays = 5, ExpectedCostLevel = CostLevel.Low,  StopReason = "eVisa required (~3 days processing). Budget viable." },
            new() { SavedRouteId = routeId, DestinationId = hoiAn.Id,     StopOrder = 4, RecommendedDays = 4, ExpectedCostLevel = CostLevel.Low,  StopReason = "Within same VN eVisa. Completes the route on budget." },
        };

        // Elimination example: Budapest eliminated due to Schengen visa
        var eliminations = new List<RouteElimination>
        {
            new()
            {
                RouteQueryId  = queryId,
                DestinationId = budapest.Id,
                Reason        = EliminationReason.VisaRequired,
                ExplanationText = "Budapest eliminated: Hungary is Schengen zone. TR passport requires a Schengen visa (avg 15 days processing). You declared no Schengen visa."
            }
        };

        context.RouteQueries.Add(query);
        context.SavedRoutes.Add(savedRoute);
        context.RouteStops.AddRange(stops);
        context.RouteEliminations.AddRange(eliminations);
        await context.SaveChangesAsync();
    }
}
