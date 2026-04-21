using System.Text.Json;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Routsky.Api.Entities;

namespace Routsky.Api.Data;

/// <summary>
/// V3 Seed Data — API-Driven Intelligence Skeleton.
/// Seeds 325 global destination nodes across 152 countries and 7 regions.
/// Visa and cost data are now sourced from live APIs (Travel Buddy, Gemini).
/// Only static geography (city, country, lat/lng, IATA) is seeded here.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(RoutskyDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedRegionPriceTiersAsync(context);
        await SeedDestinationsAsync(context);
        await SeedUsersAsync(context);
        await SeedCityIntelligenceAsync(context);
        await SeedAccommodationZonesAsync(context);
        await SeedAttractionsAsync(context);
    }

    // ─────────────────────────────────────────────
    // REGION PRICE TIERS (27 rows — truth table)
    // ─────────────────────────────────────────────
    private static async Task SeedRegionPriceTiersAsync(RoutskyDbContext context)
    {
        if (await context.RegionPriceTiers.AnyAsync()) return;

        var now = new DateTime(2026, 3, 8, 0, 0, 0, DateTimeKind.Utc);

        var tiers = new List<RegionPriceTier>
        {
            // Southeast Asia
            new() { Region = "Southeast Asia", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 20,  DailyBudgetUsdMax = 45,  Description = "Hostel + street food + local transport.",                       LastReviewedAt = now },
            new() { Region = "Southeast Asia", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 45,  DailyBudgetUsdMax = 100, Description = "Private room + restaurant meals + occasional rideshare.",        LastReviewedAt = now },
            new() { Region = "Southeast Asia", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 100, DailyBudgetUsdMax = 250, Description = "Boutique hotel + fine dining + private transfers.",              LastReviewedAt = now },

            // Eastern Europe
            new() { Region = "Eastern Europe", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 25,  DailyBudgetUsdMax = 50,  Description = "Hostel + local eateries + buses/trains.",                       LastReviewedAt = now },
            new() { Region = "Eastern Europe", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 120, Description = "Mid-range hotels + sit-down restaurants + occasional taxi.",     LastReviewedAt = now },
            new() { Region = "Eastern Europe", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 120, DailyBudgetUsdMax = 280, Description = "Upscale accommodation + fine dining.",                          LastReviewedAt = now },

            // Western Europe
            new() { Region = "Western Europe", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 80,  DailyBudgetUsdMax = 130,  Description = "Hostel or budget hotel + supermarket meals + metro pass.",     LastReviewedAt = now },
            new() { Region = "Western Europe", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 130, DailyBudgetUsdMax = 280,  Description = "3-star hotel + restaurant dining + trains/Uber.",              LastReviewedAt = now },
            new() { Region = "Western Europe", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 280, DailyBudgetUsdMax = 700,  Description = "5-star hotel + fine dining + private concierge.",              LastReviewedAt = now },

            // Turkiye
            new() { Region = "Turkiye", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 25,  DailyBudgetUsdMax = 50,  Description = "Pansiyon + lokanta meals + dolmus transport.",                        LastReviewedAt = now },
            new() { Region = "Turkiye", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 130, Description = "Boutique hotel + restaurant dining + domestic flights.",              LastReviewedAt = now },
            new() { Region = "Turkiye", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 130, DailyBudgetUsdMax = 350, Description = "5-star resort + fine dining + private transfers.",                    LastReviewedAt = now },

            // Latin America
            new() { Region = "Latin America", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 25,  DailyBudgetUsdMax = 50,  Description = "Hostel + street food + colectivos.",                            LastReviewedAt = now },
            new() { Region = "Latin America", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 120, Description = "Budget hotel + restaurant meals + Uber/bus.",                   LastReviewedAt = now },
            new() { Region = "Latin America", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 120, DailyBudgetUsdMax = 280, Description = "Boutique hotel + fine dining + private transfers.",             LastReviewedAt = now },

            // Africa & Middle East
            new() { Region = "Africa & Middle East", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 20,  DailyBudgetUsdMax = 50,  Description = "Guesthouse + local food + shared transport.",             LastReviewedAt = now },
            new() { Region = "Africa & Middle East", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 150, Description = "Mid-range hotel + restaurant meals + taxi.",              LastReviewedAt = now },
            new() { Region = "Africa & Middle East", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 150, DailyBudgetUsdMax = 500, Description = "Luxury resort + fine dining + private drivers.",          LastReviewedAt = now },

            // East Asia
            new() { Region = "East Asia", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 50,  DailyBudgetUsdMax = 100, Description = "Budget guesthouse + street food + subway.",                         LastReviewedAt = now },
            new() { Region = "East Asia", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 100, DailyBudgetUsdMax = 220, Description = "Business hotel + restaurants + shinkansen day trips.",              LastReviewedAt = now },
            new() { Region = "East Asia", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 220, DailyBudgetUsdMax = 600, Description = "Ryokan/luxury hotel + kaiseki dining + private transport.",         LastReviewedAt = now },

            // South Asia & Indian Ocean
            new() { Region = "South Asia & Indian Ocean", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 15,  DailyBudgetUsdMax = 40,  Description = "Guesthouse + local food + tuk-tuks.",               LastReviewedAt = now },
            new() { Region = "South Asia & Indian Ocean", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 40,  DailyBudgetUsdMax = 120, Description = "3-star hotel + restaurant meals + rideshare.",       LastReviewedAt = now },
            new() { Region = "South Asia & Indian Ocean", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 120, DailyBudgetUsdMax = 800, Description = "Overwater bungalow / 5-star + all-inclusive.",       LastReviewedAt = now },

            // Oceania
            new() { Region = "Oceania", CostLevel = CostLevel.Low,    DailyBudgetUsdMin = 60,  DailyBudgetUsdMax = 120, Description = "Hostel + supermarket meals + public transport.",                      LastReviewedAt = now },
            new() { Region = "Oceania", CostLevel = CostLevel.Medium, DailyBudgetUsdMin = 120, DailyBudgetUsdMax = 250, Description = "3-star hotel + restaurant dining + domestic flights.",                LastReviewedAt = now },
            new() { Region = "Oceania", CostLevel = CostLevel.High,   DailyBudgetUsdMin = 250, DailyBudgetUsdMax = 600, Description = "Luxury lodge + fine dining + scenic flights.",                        LastReviewedAt = now },
        };

        context.RegionPriceTiers.AddRange(tiers);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // DESTINATIONS (325 cities — geography skeleton)
    // Only static data: City, Country, CountryCode, Region, Lat, Lng, IataCode
    // ─────────────────────────────────────────────
    private static async Task SeedDestinationsAsync(RoutskyDbContext context)
    {
        if (await context.Destinations.AnyAsync()) return;

        var destinations = new List<Destination>
        {
            // ══════════════════════════════════════════════════════════════════
            // WESTERN EUROPE (~40 cities) — High density
            // ══════════════════════════════════════════════════════════════════
            D("France",        "FR", "Paris",          "Western Europe", 48.8566,  2.3522,   "CDG", 1),
            D("France",        "FR", "Nice",           "Western Europe", 43.7102,  7.2620,   "NCE", 3),
            D("France",        "FR", "Lyon",           "Western Europe", 45.7640,  4.8357,   "LYS", 3),
            D("France",        "FR", "Marseille",      "Western Europe", 43.2965,  5.3698,   "MRS", 3),
            D("France",        "FR", "Bordeaux",       "Western Europe", 44.8378, -0.5792,   "BOD", 3),
            D("France",        "FR", "Strasbourg",     "Western Europe", 48.5734,  7.7521,   "SXB", 3),
            D("Switzerland",   "CH", "Zurich",         "Western Europe", 47.3769,  8.5417,   "ZRH", 2),
            D("Switzerland",   "CH", "Geneva",         "Western Europe", 46.2044,  6.1432,   "GVA", 3),
            D("Austria",       "AT", "Vienna",         "Western Europe", 48.2082, 16.3738,   "VIE", 2),
            D("Austria",       "AT", "Salzburg",       "Western Europe", 47.8095, 13.0550,   "SZG", 3),
            D("Netherlands",   "NL", "Amsterdam",      "Western Europe", 52.3676,  4.9041,   "AMS", 1),
            D("Netherlands",   "NL", "Rotterdam",      "Western Europe", 51.9225,  4.4792,   "RTM", 3),
            D("Italy",         "IT", "Rome",           "Western Europe", 41.9028, 12.4964,   "FCO", 1),
            D("Italy",         "IT", "Milan",          "Western Europe", 45.4642,  9.1900,   "MXP", 2),
            D("Italy",         "IT", "Florence",       "Western Europe", 43.7696, 11.2558,   "FLR", 3),
            D("Italy",         "IT", "Venice",         "Western Europe", 45.4408, 12.3155,   "VCE", 3),
            D("Italy",         "IT", "Naples",         "Western Europe", 40.8518, 14.2681,   "NAP", 3),
            D("Italy",         "IT", "Bologna",        "Western Europe", 44.4949, 11.3426,   "BLQ", 3),
            D("Italy",         "IT", "Palermo",        "Western Europe", 38.1157, 13.3615,   "PMO", 3),
            D("Italy",         "IT", "Amalfi Coast",   "Western Europe", 40.6333, 14.6029,   null,  3),
            D("Spain",         "ES", "Barcelona",      "Western Europe", 41.3851,  2.1734,   "BCN", 1),
            D("Spain",         "ES", "Madrid",         "Western Europe", 40.4168, -3.7038,   "MAD", 1),
            D("Spain",         "ES", "Seville",        "Western Europe", 37.3891, -5.9845,   "SVQ", 3),
            D("Spain",         "ES", "Valencia",       "Western Europe", 39.4699, -0.3763,   "VLC", 3),
            D("Spain",         "ES", "Malaga",         "Western Europe", 36.7213, -4.4214,   "AGP", 3),
            D("Spain",         "ES", "Bilbao",         "Western Europe", 43.2630, -2.9350,   "BIO", 3),
            D("Germany",       "DE", "Frankfurt",      "Western Europe", 50.1109,  8.6821,   "FRA", 1),
            D("Germany",       "DE", "Munich",         "Western Europe", 48.1351, 11.5820,   "MUC", 2),
            D("Germany",       "DE", "Berlin",         "Western Europe", 52.5200, 13.4050,   "BER", 1),
            D("Germany",       "DE", "Hamburg",        "Western Europe", 53.5511,  9.9937,   "HAM", 3),
            D("Germany",       "DE", "Dusseldorf",     "Western Europe", 51.2277,  6.7735,   "DUS", 3),
            D("Portugal",      "PT", "Lisbon",         "Western Europe", 38.7223, -9.1393,   "LIS", 2),
            D("Portugal",      "PT", "Porto",          "Western Europe", 41.1579, -8.6291,   "OPO", 3),
            D("Belgium",       "BE", "Brussels",       "Western Europe", 50.8503,  4.3517,   "BRU", 2),
            D("Belgium",       "BE", "Antwerp",        "Western Europe", 51.2194,  4.4025,   "ANR", 3),
            D("Denmark",       "DK", "Copenhagen",     "Western Europe", 55.6761, 12.5683,   "CPH", 2),
            D("Sweden",        "SE", "Stockholm",      "Western Europe", 59.3293, 18.0686,   "ARN", 2),
            D("Norway",        "NO", "Oslo",           "Western Europe", 59.9139, 10.7522,   "OSL", 2),
            D("Finland",       "FI", "Helsinki",       "Western Europe", 60.1699, 24.9384,   "HEL", 2),
            D("Ireland",       "IE", "Dublin",         "Western Europe", 53.3498, -6.2603,   "DUB", 2),
            D("Greece",        "GR", "Athens",         "Western Europe", 37.9838, 23.7275,   "ATH", 2),
            D("Greece",        "GR", "Thessaloniki",   "Western Europe", 40.6401, 22.9444,   "SKG", 3),
            D("Luxembourg",    "LU", "Luxembourg City","Western Europe", 49.6116,  6.1300,   "LUX", 3),
            D("Iceland",       "IS", "Reykjavik",      "Western Europe", 64.1466,-21.9426,   "KEF", 3),

            // ══════════════════════════════════════════════════════════════════
            // UNITED KINGDOM (~12 cities) — High density
            // ══════════════════════════════════════════════════════════════════
            D("United Kingdom","GB", "London",         "Western Europe", 51.5074, -0.1278,   "LHR", 1),
            D("United Kingdom","GB", "Edinburgh",      "Western Europe", 55.9533, -3.1883,   "EDI", 3),
            D("United Kingdom","GB", "Manchester",     "Western Europe", 53.4808, -2.2426,   "MAN", 2),
            D("United Kingdom","GB", "Birmingham",     "Western Europe", 52.4862, -1.8904,   "BHX", 3),
            D("United Kingdom","GB", "Glasgow",        "Western Europe", 55.8642, -4.2518,   "GLA", 3),
            D("United Kingdom","GB", "Bristol",        "Western Europe", 51.4545, -2.5879,   "BRS", 3),
            D("United Kingdom","GB", "Liverpool",      "Western Europe", 53.4084, -2.9916,   "LPL", 3),
            D("United Kingdom","GB", "Leeds",          "Western Europe", 53.8008, -1.5491,   "LBA", 3),
            D("United Kingdom","GB", "Cardiff",        "Western Europe", 51.4816, -3.1791,   "CWL", 3),
            D("United Kingdom","GB", "Belfast",        "Western Europe", 54.5973, -5.9301,   "BFS", 3),
            D("United Kingdom","GB", "Cambridge",      "Western Europe", 52.2053,  0.1218,   "CBG", 3),
            D("United Kingdom","GB", "Oxford",         "Western Europe", 51.7520, -1.2577,   null,  3),

            // ══════════════════════════════════════════════════════════════════
            // TURKIYE (~15 cities) — High density
            // ══════════════════════════════════════════════════════════════════
            D("Turkiye",       "TR", "Istanbul",       "Turkiye", 41.0082, 28.9784, "IST", 1),
            D("Turkiye",       "TR", "Ankara",         "Turkiye", 39.9334, 32.8597, "ESB", 2),
            D("Turkiye",       "TR", "Antalya",        "Turkiye", 36.8969, 30.7133, "AYT", 2),
            D("Turkiye",       "TR", "Izmir",          "Turkiye", 38.4192, 27.1287, "ADB", 2),
            D("Turkiye",       "TR", "Bodrum",         "Turkiye", 37.0344, 27.4305, "BJV", 3),
            D("Turkiye",       "TR", "Cappadocia",     "Turkiye", 38.6431, 34.8289, "NAV", 3),
            D("Turkiye",       "TR", "Trabzon",        "Turkiye", 41.0027, 39.7168, "TZX", 3),
            D("Turkiye",       "TR", "Bursa",          "Turkiye", 40.1885, 29.0610, "YEI", 3),
            D("Turkiye",       "TR", "Gaziantep",      "Turkiye", 37.0662, 37.3833, "GZT", 3),
            D("Turkiye",       "TR", "Fethiye",        "Turkiye", 36.6538, 29.1231, "DLM", 3),
            D("Turkiye",       "TR", "Kas",            "Turkiye", 36.2002, 29.6383, null,  3),
            D("Turkiye",       "TR", "Marmaris",       "Turkiye", 36.8510, 28.2740, "DLM", 3),
            D("Turkiye",       "TR", "Konya",          "Turkiye", 37.8714, 32.4846, "KYA", 3),
            D("Turkiye",       "TR", "Adana",          "Turkiye", 37.0000, 35.3213, "ADA", 3),
            D("Turkiye",       "TR", "Diyarbakir",     "Turkiye", 37.9250, 40.2100, "DIY", 3),

            // ══════════════════════════════════════════════════════════════════
            // USA (~25 cities) — High density
            // ══════════════════════════════════════════════════════════════════
            D("United States", "US", "New York",       "Americas", 40.7128, -74.0060, "JFK", 1),
            D("United States", "US", "Los Angeles",    "Americas", 33.9425,-118.4080, "LAX", 1),
            D("United States", "US", "Chicago",        "Americas", 41.8781, -87.6298, "ORD", 1),
            D("United States", "US", "Miami",          "Americas", 25.7617, -80.1918, "MIA", 2),
            D("United States", "US", "San Francisco",  "Americas", 37.7749,-122.4194, "SFO", 2),
            D("United States", "US", "Seattle",        "Americas", 47.6062,-122.3321, "SEA", 2),
            D("United States", "US", "Boston",         "Americas", 42.3601, -71.0589, "BOS", 2),
            D("United States", "US", "Washington DC",  "Americas", 38.9072, -77.0369, "IAD", 2),
            D("United States", "US", "Dallas",         "Americas", 32.7767, -96.7970, "DFW", 2),
            D("United States", "US", "Denver",         "Americas", 39.7392,-104.9903, "DEN", 2),
            D("United States", "US", "Atlanta",        "Americas", 33.7490, -84.3880, "ATL", 1),
            D("United States", "US", "Las Vegas",      "Americas", 36.1699,-115.1398, "LAS", 2),
            D("United States", "US", "Orlando",        "Americas", 28.5383, -81.3792, "MCO", 2),
            D("United States", "US", "Nashville",      "Americas", 36.1627, -86.7816, "BNA", 3),
            D("United States", "US", "Portland",       "Americas", 45.5152,-122.6784, "PDX", 3),
            D("United States", "US", "Houston",        "Americas", 29.7604, -95.3698, "IAH", 2),
            D("United States", "US", "Phoenix",        "Americas", 33.4484,-112.0740, "PHX", 3),
            D("United States", "US", "Minneapolis",    "Americas", 44.9778, -93.2650, "MSP", 3),
            D("United States", "US", "Charlotte",      "Americas", 35.2271, -80.8431, "CLT", 3),
            D("United States", "US", "Austin",         "Americas", 30.2672, -97.7431, "AUS", 3),
            D("United States", "US", "San Diego",      "Americas", 32.7157,-117.1611, "SAN", 3),
            D("United States", "US", "Honolulu",       "Americas", 21.3069,-157.8583, "HNL", 3),
            D("United States", "US", "New Orleans",    "Americas", 29.9511, -90.0715, "MSY", 3),
            D("United States", "US", "Philadelphia",   "Americas", 39.9526, -75.1652, "PHL", 3),
            D("United States", "US", "Detroit",        "Americas", 42.3314, -83.0458, "DTW", 3),

            // ══════════════════════════════════════════════════════════════════
            // EASTERN EUROPE & BALKANS (~25 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Hungary",       "HU", "Budapest",       "Eastern Europe", 47.4979, 19.0402, "BUD", 2),
            D("Czech Republic","CZ", "Prague",         "Eastern Europe", 50.0755, 14.4378, "PRG", 2),
            D("Poland",        "PL", "Krakow",         "Eastern Europe", 50.0647, 19.9450, "KRK", 3),
            D("Poland",        "PL", "Warsaw",         "Eastern Europe", 52.2297, 21.0122, "WAW", 2),
            D("Poland",        "PL", "Gdansk",         "Eastern Europe", 54.3520, 18.6466, "GDN", 3),
            D("Poland",        "PL", "Wroclaw",        "Eastern Europe", 51.1079, 17.0385, "WRO", 3),
            D("Romania",       "RO", "Bucharest",      "Eastern Europe", 44.4268, 26.1025, "OTP", 3),
            D("Romania",       "RO", "Cluj-Napoca",    "Eastern Europe", 46.7712, 23.6236, "CLJ", 3),
            D("Bulgaria",      "BG", "Sofia",          "Eastern Europe", 42.6977, 23.3219, "SOF", 3),
            D("Georgia",       "GE", "Tbilisi",        "Eastern Europe", 41.7151, 44.8271, "TBS", 3),
            D("Georgia",       "GE", "Batumi",         "Eastern Europe", 41.6168, 41.6367, "BUS", 3),
            D("Serbia",        "RS", "Belgrade",       "Eastern Europe", 44.7866, 20.4489, "BEG", 3),
            D("Croatia",       "HR", "Zagreb",         "Eastern Europe", 45.8150, 15.9819, "ZAG", 3),
            D("Croatia",       "HR", "Dubrovnik",      "Eastern Europe", 42.6507, 18.0944, "DBV", 3),
            D("Croatia",       "HR", "Split",          "Eastern Europe", 43.5081, 16.4402, "SPU", 3),
            D("Slovenia",      "SI", "Ljubljana",      "Eastern Europe", 46.0569, 14.5058, "LJU", 3),
            D("North Macedonia","MK","Skopje",         "Eastern Europe", 41.9973, 21.4280, "SKP", 3),
            D("Albania",       "AL", "Tirana",         "Eastern Europe", 41.3275, 19.8187, "TIA", 3),
            D("Bosnia",        "BA", "Sarajevo",       "Eastern Europe", 43.8563, 18.4131, "SJJ", 3),
            D("Montenegro",    "ME", "Kotor",          "Eastern Europe", 42.4247, 18.7712, null,  3),
            D("Montenegro",    "ME", "Podgorica",      "Eastern Europe", 42.4304, 19.2594, "TGD", 3),
            D("Slovakia",      "SK", "Bratislava",     "Eastern Europe", 48.1486, 17.1077, "BTS", 3),
            D("Ukraine",       "UA", "Kyiv",           "Eastern Europe", 50.4501, 30.5234, "KBP", 2),
            D("Latvia",        "LV", "Riga",           "Eastern Europe", 56.9496, 24.1052, "RIX", 3),
            D("Lithuania",     "LT", "Vilnius",        "Eastern Europe", 54.6872, 25.2797, "VNO", 3),
            D("Estonia",       "EE", "Tallinn",        "Eastern Europe", 59.4370, 24.7536, "TLL", 3),
            D("Moldova",       "MD", "Chisinau",       "Eastern Europe", 47.0105, 28.8638, "KIV", 3),
            D("Azerbaijan",    "AZ", "Baku",           "Eastern Europe", 40.4093, 49.8671, "GYD", 3),
            D("Armenia",       "AM", "Yerevan",        "Eastern Europe", 40.1792, 44.4991, "EVN", 3),

            // ══════════════════════════════════════════════════════════════════
            // SOUTHEAST ASIA (~20 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Thailand",      "TH", "Bangkok",        "Southeast Asia", 13.7563, 100.5018, "BKK", 1),
            D("Thailand",      "TH", "Chiang Mai",     "Southeast Asia", 18.7883,  98.9853, "CNX", 3),
            D("Thailand",      "TH", "Phuket",         "Southeast Asia",  7.8804,  98.3923, "HKT", 3),
            D("Thailand",      "TH", "Pattaya",        "Southeast Asia", 12.9236, 100.8825, "UTP", 3),
            D("Vietnam",       "VN", "Hanoi",          "Southeast Asia", 21.0285, 105.8542, "HAN", 2),
            D("Vietnam",       "VN", "Ho Chi Minh City","Southeast Asia",10.8231, 106.6297, "SGN", 2),
            D("Vietnam",       "VN", "Hoi An",         "Southeast Asia", 15.8801, 108.3380, "DAD", 3),
            D("Vietnam",       "VN", "Da Nang",        "Southeast Asia", 16.0544, 108.2022, "DAD", 3),
            D("Vietnam",       "VN", "Nha Trang",      "Southeast Asia", 12.2388, 109.1967, "CXR", 3),
            D("Cambodia",      "KH", "Siem Reap",      "Southeast Asia", 13.3671, 103.8448, "REP", 3),
            D("Cambodia",      "KH", "Phnom Penh",     "Southeast Asia", 11.5564, 104.9282, "PNH", 3),
            D("Indonesia",     "ID", "Bali",           "Southeast Asia", -8.3405, 115.0920, "DPS", 2),
            D("Indonesia",     "ID", "Jakarta",        "Southeast Asia", -6.2088, 106.8456, "CGK", 2),
            D("Indonesia",     "ID", "Yogyakarta",     "Southeast Asia", -7.7956, 110.3695, "JOG", 3),
            D("Malaysia",      "MY", "Kuala Lumpur",   "Southeast Asia",  3.1390, 101.6869, "KUL", 2),
            D("Malaysia",      "MY", "Penang",         "Southeast Asia",  5.4164, 100.3327, "PEN", 3),
            D("Singapore",     "SG", "Singapore",      "Southeast Asia",  1.3521, 103.8198, "SIN", 1),
            D("Philippines",   "PH", "Manila",         "Southeast Asia", 14.5995, 120.9842, "MNL", 2),
            D("Philippines",   "PH", "Cebu",           "Southeast Asia", 10.3157, 123.8854, "CEB", 3),
            D("Laos",          "LA", "Luang Prabang",  "Southeast Asia", 19.8856, 102.1347, "LPQ", 3),
            D("Myanmar",       "MM", "Yangon",         "Southeast Asia", 16.8661,  96.1951, "RGN", 3),

            // ══════════════════════════════════════════════════════════════════
            // EAST ASIA (~12 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Japan",         "JP", "Tokyo",          "East Asia", 35.6762, 139.6503, "NRT", 1),
            D("Japan",         "JP", "Kyoto",          "East Asia", 35.0116, 135.7681, "KIX", 3),
            D("Japan",         "JP", "Osaka",          "East Asia", 34.6937, 135.5023, "KIX", 2),
            D("Japan",         "JP", "Fukuoka",        "East Asia", 33.5904, 130.4017, "FUK", 3),
            D("Japan",         "JP", "Sapporo",        "East Asia", 43.0618, 141.3545, "CTS", 3),
            D("South Korea",   "KR", "Seoul",          "East Asia", 37.5665, 126.9780, "ICN", 1),
            D("South Korea",   "KR", "Busan",          "East Asia", 35.1796, 129.0756, "PUS", 3),
            D("China",         "CN", "Beijing",        "East Asia", 39.9042, 116.4074, "PEK", 1),
            D("China",         "CN", "Shanghai",       "East Asia", 31.2304, 121.4737, "PVG", 1),
            D("China",         "CN", "Hong Kong",      "East Asia", 22.3193, 114.1694, "HKG", 1),
            D("China",         "CN", "Guangzhou",      "East Asia", 23.1291, 113.2644, "CAN", 2),
            D("Taiwan",        "TW", "Taipei",         "East Asia", 25.0330, 121.5654, "TPE", 2),
            D("Mongolia",      "MN", "Ulaanbaatar",    "East Asia", 47.8864, 106.9057, "UBN", 3),

            // ══════════════════════════════════════════════════════════════════
            // SOUTH ASIA & INDIAN OCEAN (~15 cities)
            // ══════════════════════════════════════════════════════════════════
            D("India",         "IN", "Mumbai",         "South Asia & Indian Ocean", 19.0760,  72.8777, "BOM", 1),
            D("India",         "IN", "New Delhi",      "South Asia & Indian Ocean", 28.6139,  77.2090, "DEL", 1),
            D("India",         "IN", "Bangalore",      "South Asia & Indian Ocean", 12.9716,  77.5946, "BLR", 2),
            D("India",         "IN", "Goa",            "South Asia & Indian Ocean", 15.2993,  74.1240, "GOI", 3),
            D("India",         "IN", "Jaipur",         "South Asia & Indian Ocean", 26.9124,  75.7873, "JAI", 3),
            D("India",         "IN", "Kolkata",        "South Asia & Indian Ocean", 22.5726,  88.3639, "CCU", 2),
            D("India",         "IN", "Chennai",        "South Asia & Indian Ocean", 13.0827,  80.2707, "MAA", 3),
            D("India",         "IN", "Kochi",          "South Asia & Indian Ocean",  9.9312,  76.2673, "COK", 3),
            D("Sri Lanka",     "LK", "Colombo",        "South Asia & Indian Ocean",  6.9271,  79.8612, "CMB", 3),
            D("Nepal",         "NP", "Kathmandu",      "South Asia & Indian Ocean", 27.7172,  85.3240, "KTM", 3),
            D("Maldives",      "MV", "Male Atolls",    "South Asia & Indian Ocean",  4.1755,  73.5093, "MLE", 3),
            D("Bangladesh",    "BD", "Dhaka",          "South Asia & Indian Ocean", 23.8103,  90.4125, "DAC", 3),
            D("Pakistan",      "PK", "Karachi",        "South Asia & Indian Ocean", 24.8607,  67.0011, "KHI", 3),
            D("Pakistan",      "PK", "Lahore",         "South Asia & Indian Ocean", 31.5204,  74.3587, "LHE", 3),
            D("Pakistan",      "PK", "Islamabad",      "South Asia & Indian Ocean", 33.6844,  73.0479, "ISB", 3),

            // ══════════════════════════════════════════════════════════════════
            // AMERICAS — Latin America + Canada (~30 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Canada",        "CA", "Toronto",        "Americas", 43.6532, -79.3832, "YYZ", 2),
            D("Canada",        "CA", "Vancouver",      "Americas", 49.2827,-123.1207, "YVR", 2),
            D("Canada",        "CA", "Montreal",       "Americas", 45.5017, -73.5673, "YUL", 2),
            D("Canada",        "CA", "Calgary",        "Americas", 51.0447,-114.0719, "YYC", 3),
            D("Mexico",        "MX", "Mexico City",    "Americas", 19.4326, -99.1332, "MEX", 1),
            D("Mexico",        "MX", "Cancun",         "Americas", 21.1619, -86.8515, "CUN", 2),
            D("Mexico",        "MX", "Guadalajara",    "Americas", 20.6597,-103.3496, "GDL", 3),
            D("Mexico",        "MX", "Oaxaca",         "Americas", 17.0732, -96.7266, "OAX", 3),
            D("Colombia",      "CO", "Bogota",         "Americas",  4.7110, -74.0721, "BOG", 2),
            D("Colombia",      "CO", "Medellin",       "Americas",  6.2476, -75.5658, "MDE", 3),
            D("Colombia",      "CO", "Cartagena",      "Americas", 10.3910, -75.5144, "CTG", 3),
            D("Peru",          "PE", "Lima",           "Americas",-12.0464, -77.0428, "LIM", 2),
            D("Peru",          "PE", "Cusco",          "Americas",-13.5320, -71.9675, "CUZ", 3),
            D("Argentina",     "AR", "Buenos Aires",   "Americas",-34.6037, -58.3816, "EZE", 1),
            D("Argentina",     "AR", "Cordoba",        "Americas",-31.4201, -64.1888, "COR", 3),
            D("Brazil",        "BR", "Sao Paulo",      "Americas",-23.5505, -46.6333, "GRU", 1),
            D("Brazil",        "BR", "Rio de Janeiro", "Americas",-22.9068, -43.1729, "GIG", 2),
            D("Brazil",        "BR", "Salvador",       "Americas",-12.9714, -38.5124, "SSA", 3),
            D("Chile",         "CL", "Santiago",       "Americas",-33.4489, -70.6693, "SCL", 2),
            D("Ecuador",       "EC", "Quito",          "Americas", -0.1807, -78.4678, "UIO", 3),
            D("Bolivia",       "BO", "La Paz",         "Americas",-16.4897, -68.1193, "LPB", 3),
            D("Uruguay",       "UY", "Montevideo",     "Americas",-34.9011, -56.1645, "MVD", 3),
            D("Venezuela",     "VE", "Caracas",        "Americas", 10.4806, -66.9036, "CCS", 3),
            D("Costa Rica",    "CR", "San Jose",       "Americas",  9.9281, -84.0907, "SJO", 3),
            D("Costa Rica",    "CR", "La Fortuna",     "Americas", 10.4719, -84.6427, null,  3),
            D("Panama",        "PA", "Panama City",    "Americas",  8.9824, -79.5199, "PTY", 2),
            D("Guatemala",     "GT", "Guatemala City", "Americas", 14.6349, -90.5069, "GUA", 3),
            D("Guatemala",     "GT", "Antigua",        "Americas", 14.5586, -90.7295, null,  3),
            D("Nicaragua",     "NI", "Granada",        "Americas", 11.9344, -85.9560, "MGA", 3),
            D("Cuba",          "CU", "Havana",         "Americas", 23.1136, -82.3666, "HAV", 3),
            D("Dominican Republic","DO","Santo Domingo","Americas", 18.4861, -69.9312, "SDQ", 3),
            D("Jamaica",       "JM", "Kingston",       "Americas", 18.0179, -76.8099, "KIN", 3),
            D("Trinidad and Tobago","TT","Port of Spain","Americas",10.6596, -61.5086, "POS", 3),

            // ══════════════════════════════════════════════════════════════════
            // AFRICA & MIDDLE EAST (~45 cities)
            // ══════════════════════════════════════════════════════════════════
            // North Africa
            D("Morocco",       "MA", "Marrakech",      "Africa & Middle East", 31.6295, -7.9811, "RAK", 2),
            D("Morocco",       "MA", "Fez",            "Africa & Middle East", 34.0181, -5.0078, "FEZ", 3),
            D("Morocco",       "MA", "Tangier",        "Africa & Middle East", 35.7595, -5.8340, "TNG", 3),
            D("Morocco",       "MA", "Casablanca",     "Africa & Middle East", 33.5731, -7.5898, "CMN", 2),
            D("Tunisia",       "TN", "Tunis",          "Africa & Middle East", 36.8065, 10.1815, "TUN", 3),
            D("Egypt",         "EG", "Cairo",          "Africa & Middle East", 30.0444, 31.2357, "CAI", 2),
            D("Egypt",         "EG", "Luxor",          "Africa & Middle East", 25.6872, 32.6396, "LXR", 3),
            D("Algeria",       "DZ", "Algiers",        "Africa & Middle East", 36.7538,  3.0588, "ALG", 3),
            D("Libya",         "LY", "Tripoli",        "Africa & Middle East", 32.8872, 13.1802, "TIP", 3),
            // Sub-Saharan Africa
            D("South Africa",  "ZA", "Cape Town",      "Africa & Middle East",-33.9249, 18.4241, "CPT", 2),
            D("South Africa",  "ZA", "Johannesburg",   "Africa & Middle East",-26.2041, 28.0473, "JNB", 2),
            D("South Africa",  "ZA", "Durban",         "Africa & Middle East",-29.8587, 31.0218, "DUR", 3),
            D("Kenya",         "KE", "Nairobi",        "Africa & Middle East", -1.2921, 36.8219, "NBO", 2),
            D("Kenya",         "KE", "Mombasa",        "Africa & Middle East", -4.0435, 39.6682, "MBA", 3),
            D("Tanzania",      "TZ", "Zanzibar",       "Africa & Middle East", -6.1659, 39.2026, "ZNZ", 3),
            D("Tanzania",      "TZ", "Dar es Salaam",  "Africa & Middle East", -6.7924, 39.2083, "DAR", 3),
            D("Ethiopia",      "ET", "Addis Ababa",    "Africa & Middle East",  8.9806, 38.7578, "ADD", 2),
            D("Ghana",         "GH", "Accra",          "Africa & Middle East",  5.6037, -0.1870, "ACC", 3),
            D("Nigeria",       "NG", "Lagos",          "Africa & Middle East",  6.5244,  3.3792, "LOS", 2),
            D("Nigeria",       "NG", "Abuja",          "Africa & Middle East",  9.0765,  7.3986, "ABV", 3),
            D("Rwanda",        "RW", "Kigali",         "Africa & Middle East", -1.9403, 29.8739, "KGL", 3),
            D("Senegal",       "SN", "Dakar",          "Africa & Middle East", 14.7167,-17.4677, "DSS", 3),
            D("Uganda",        "UG", "Kampala",        "Africa & Middle East",  0.3476, 32.5825, "EBB", 3),
            D("Mauritius",     "MU", "Port Louis",     "Africa & Middle East",-20.1609, 57.5012, "MRU", 3),
            D("Seychelles",    "SC", "Victoria",       "Africa & Middle East", -4.6191, 55.4513, "SEZ", 3),
            D("Madagascar",    "MG", "Antananarivo",   "Africa & Middle East",-18.8792, 47.5079, "TNR", 3),
            D("Namibia",       "NA", "Windhoek",       "Africa & Middle East",-22.5609, 17.0658, "WDH", 3),
            D("Botswana",      "BW", "Gaborone",       "Africa & Middle East",-24.6282, 25.9231, "GBE", 3),
            D("Ivory Coast",   "CI", "Abidjan",        "Africa & Middle East",  5.3599, -4.0083, "ABJ", 3),
            D("Cameroon",      "CM", "Douala",         "Africa & Middle East",  4.0511,  9.7679, "DLA", 3),
            D("Zimbabwe",      "ZW", "Harare",         "Africa & Middle East",-17.8252, 31.0335, "HRE", 3),
            D("Mozambique",    "MZ", "Maputo",         "Africa & Middle East",-25.9692, 32.5732, "MPM", 3),
            D("Zambia",        "ZM", "Lusaka",         "Africa & Middle East",-15.3875, 28.3228, "LUN", 3),
            D("DR Congo",      "CD", "Kinshasa",       "Africa & Middle East", -4.4419, 15.2663, "FIH", 3),
            D("Angola",        "AO", "Luanda",         "Africa & Middle East", -8.8399, 13.2894, "LAD", 3),
            // Middle East
            D("UAE",           "AE", "Dubai",          "Africa & Middle East", 25.2048, 55.2708, "DXB", 1),
            D("UAE",           "AE", "Abu Dhabi",      "Africa & Middle East", 24.4539, 54.3773, "AUH", 2),
            D("Qatar",         "QA", "Doha",           "Africa & Middle East", 25.2854, 51.5310, "DOH", 2),
            D("Saudi Arabia",  "SA", "Riyadh",         "Africa & Middle East", 24.7136, 46.6753, "RUH", 2),
            D("Saudi Arabia",  "SA", "Jeddah",         "Africa & Middle East", 21.4858, 39.1925, "JED", 2),
            D("Oman",          "OM", "Muscat",         "Africa & Middle East", 23.5880, 58.3829, "MCT", 3),
            D("Bahrain",       "BH", "Manama",         "Africa & Middle East", 26.2285, 50.5860, "BAH", 3),
            D("Kuwait",        "KW", "Kuwait City",    "Africa & Middle East", 29.3759, 47.9774, "KWI", 3),
            D("Jordan",        "JO", "Amman",          "Africa & Middle East", 31.9454, 35.9284, "AMM", 3),
            D("Lebanon",       "LB", "Beirut",         "Africa & Middle East", 33.8938, 35.5018, "BEY", 3),
            D("Israel",        "IL", "Tel Aviv",       "Africa & Middle East", 32.0853, 34.7818, "TLV", 2),
            D("Iran",          "IR", "Tehran",         "Africa & Middle East", 35.6892, 51.3890, "IKA", 2),
            D("Iraq",          "IQ", "Baghdad",        "Africa & Middle East", 33.3152, 44.3661, "BGW", 3),
            D("Iraq",          "IQ", "Erbil",          "Africa & Middle East", 36.1912, 44.0119, "EBL", 3),

            // ══════════════════════════════════════════════════════════════════
            // CENTRAL ASIA (~8 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Kazakhstan",    "KZ", "Almaty",         "East Asia", 43.2551, 76.9126, "ALA", 3),
            D("Kazakhstan",    "KZ", "Astana",         "East Asia", 51.1694, 71.4491, "NQZ", 3),
            D("Uzbekistan",    "UZ", "Tashkent",       "East Asia", 41.2995, 69.2401, "TAS", 3),
            D("Uzbekistan",    "UZ", "Samarkand",      "East Asia", 39.6542, 66.9597, "SKD", 3),
            D("Kyrgyzstan",    "KG", "Bishkek",        "East Asia", 42.8746, 74.5698, "FRU", 3),
            D("Tajikistan",    "TJ", "Dushanbe",       "East Asia", 38.5598, 68.7740, "DYU", 3),
            D("Turkmenistan",  "TM", "Ashgabat",       "East Asia", 37.9601, 58.3261, "ASB", 3),

            // ══════════════════════════════════════════════════════════════════
            // OCEANIA (~12 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Australia",     "AU", "Sydney",         "Oceania",-33.8688, 151.2093, "SYD", 1),
            D("Australia",     "AU", "Melbourne",      "Oceania",-37.8136, 144.9631, "MEL", 2),
            D("Australia",     "AU", "Brisbane",       "Oceania",-27.4698, 153.0251, "BNE", 3),
            D("Australia",     "AU", "Perth",          "Oceania",-31.9505, 115.8605, "PER", 3),
            D("Australia",     "AU", "Adelaide",       "Oceania",-34.9285, 138.6007, "ADL", 3),
            D("Australia",     "AU", "Gold Coast",     "Oceania",-28.0167, 153.4000, "OOL", 3),
            D("Australia",     "AU", "Cairns",         "Oceania",-16.9186, 145.7781, "CNS", 3),
            D("New Zealand",   "NZ", "Auckland",       "Oceania",-36.8485, 174.7633, "AKL", 2),
            D("New Zealand",   "NZ", "Queenstown",     "Oceania",-45.0312, 168.6626, "ZQN", 3),
            D("New Zealand",   "NZ", "Wellington",     "Oceania",-41.2865, 174.7762, "WLG", 3),
            D("Fiji",          "FJ", "Suva",           "Oceania",-18.1416, 178.4419, "SUV", 3),
            D("Fiji",          "FJ", "Nadi",           "Oceania",-17.7765, 177.9653, "NAN", 3),

            // ══════════════════════════════════════════════════════════════════
            // RUSSIA (~5 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Russia",        "RU", "Moscow",         "Eastern Europe", 55.7558, 37.6173, "SVO", 1),
            D("Russia",        "RU", "St. Petersburg", "Eastern Europe", 59.9343, 30.3351, "LED", 2),
            D("Russia",        "RU", "Novosibirsk",    "Eastern Europe", 55.0084, 82.9357, "OVB", 3),
            D("Russia",        "RU", "Vladivostok",    "Eastern Europe", 43.1332,131.9113, "VVO", 3),
            D("Russia",        "RU", "Kazan",          "Eastern Europe", 55.8304, 49.0661, "KZN", 3),

            // ══════════════════════════════════════════════════════════════════
            // CARIBBEAN & PACIFIC ISLANDS (~5 cities)
            // ══════════════════════════════════════════════════════════════════
            D("Barbados",      "BB", "Bridgetown",     "Americas", 13.1132, -59.5988, "BGI", 3),
            D("Bahamas",       "BS", "Nassau",         "Americas", 25.0343, -77.3963, "NAS", 3),
            D("Bermuda",       "BM", "Hamilton",       "Americas", 32.2949, -64.7820, "BDA", 3),
            D("French Polynesia","PF","Papeete",       "Oceania", -17.5516,-149.5585, "PPT", 3),
            D("Papua New Guinea","PG","Port Moresby",  "Oceania",  -6.3149, 143.9556, "POM", 3),
            D("Samoa",          "WS", "Apia",          "Oceania", -13.8333,-171.7500, "APW", 3),

            // ══════════════════════════════════════════════════════════════════
            // ADDITIONAL COVERAGE — reaching 325+ nodes
            // ══════════════════════════════════════════════════════════════════
            // More Western Europe
            D("Italy",         "IT", "Turin",          "Western Europe", 45.0703,  7.6869, "TRN", 3),
            D("Italy",         "IT", "Catania",        "Western Europe", 37.5079, 15.0830, "CTA", 3),
            D("Spain",         "ES", "Granada",        "Western Europe", 37.1773, -3.5986, "GRX", 3),
            D("Spain",         "ES", "Ibiza",          "Western Europe", 38.9067,  1.4206, "IBZ", 3),
            D("France",        "FR", "Toulouse",       "Western Europe", 43.6047,  1.4442, "TLS", 3),
            D("Germany",       "DE", "Cologne",        "Western Europe", 50.9375,  6.9603, "CGN", 3),
            D("Austria",       "AT", "Innsbruck",      "Western Europe", 47.2692, 11.4041, "INN", 3),
            D("Switzerland",   "CH", "Interlaken",     "Western Europe", 46.6863,  7.8632, null,  3),
            D("Malta",         "MT", "Valletta",       "Western Europe", 35.8989, 14.5146, "MLA", 3),
            D("Cyprus",        "CY", "Nicosia",        "Western Europe", 35.1856, 33.3823, "LCA", 3),
            D("Monaco",        "MC", "Monte Carlo",    "Western Europe", 43.7384,  7.4246, null,  3),

            // More UK
            D("United Kingdom","GB", "Bath",           "Western Europe", 51.3811, -2.3590, null,  3),

            // More Americas
            D("United States", "US", "Savannah",       "Americas", 32.0809, -81.0912, "SAV", 3),
            D("United States", "US", "Asheville",      "Americas", 35.5951, -82.5515, "AVL", 3),
            D("United States", "US", "Salt Lake City", "Americas", 40.7608,-111.8910, "SLC", 3),
            D("Brazil",        "BR", "Brasilia",       "Americas",-15.7975, -47.8919, "BSB", 3),
            D("Brazil",        "BR", "Recife",         "Americas", -8.0476, -34.8770, "REC", 3),
            D("Brazil",        "BR", "Fortaleza",      "Americas", -3.7319, -38.5267, "FOR", 3),
            D("Paraguay",      "PY", "Asuncion",       "Americas",-25.2637, -57.5759, "ASU", 3),
            D("Honduras",      "HN", "Tegucigalpa",    "Americas", 14.0723, -87.1921, "TGU", 3),
            D("El Salvador",   "SV", "San Salvador",   "Americas", 13.6929, -89.2182, "SAL", 3),
            D("Guyana",        "GY", "Georgetown",     "Americas",  6.8013, -58.1551, "GEO", 3),
            D("Suriname",      "SR", "Paramaribo",     "Americas",  5.8520, -55.2038, "PBM", 3),
            D("Belize",        "BZ", "Belize City",    "Americas", 17.4989, -88.1886, "BZE", 3),
            D("Haiti",         "HT", "Port-au-Prince", "Americas", 18.5944, -72.3074, "PAP", 3),

            // More Africa
            D("Morocco",       "MA", "Rabat",          "Africa & Middle East", 34.0209, -6.8416, "RBA", 3),
            D("Mali",          "ML", "Bamako",         "Africa & Middle East", 12.6392, -8.0029, "BKO", 3),
            D("Niger",         "NE", "Niamey",         "Africa & Middle East", 13.5116,  2.1254, "NIM", 3),
            D("Burkina Faso",  "BF", "Ouagadougou",    "Africa & Middle East", 12.3714, -1.5197, "OUA", 3),
            D("Togo",          "TG", "Lome",           "Africa & Middle East",  6.1375,  1.2123, "LFW", 3),
            D("Benin",         "BJ", "Cotonou",        "Africa & Middle East",  6.3654,  2.4183, "COO", 3),
            D("Congo",         "CG", "Brazzaville",    "Africa & Middle East", -4.2634, 15.2429, "BZV", 3),
            D("Gabon",         "GA", "Libreville",     "Africa & Middle East",  0.4162,  9.4673, "LBV", 3),
            D("Eritrea",       "ER", "Asmara",         "Africa & Middle East", 15.3229, 38.9251, "ASM", 3),
        };

        context.Destinations.AddRange(destinations);
        await context.SaveChangesAsync();
    }

    private static Destination D(string country, string code, string city, string region,
        double lat, double lng, string? iata, int tier) => new()
    {
        Country = country,
        CountryCode = code,
        City = city,
        Region = region,
        Latitude = lat,
        Longitude = lng,
        IataCode = iata,
        DailyCostLevel = region switch
        {
            "Western Europe" => CostLevel.Medium,
            "Turkiye" => CostLevel.Low,
            "Eastern Europe" => CostLevel.Low,
            "Southeast Asia" => CostLevel.Low,
            "East Asia" => CostLevel.Medium,
            "South Asia & Indian Ocean" => CostLevel.Low,
            "Americas" => CostLevel.Medium,
            "Africa & Middle East" => CostLevel.Low,
            "Oceania" => CostLevel.Medium,
            _ => CostLevel.Medium,
        },
        PopularityWeight = tier switch { 1 => 2.0, 2 => 1.5, _ => 1.0 },
        MinRecommendedDays = 2,
        MaxRecommendedDays = 7,
    };

    // ─────────────────────────────────────────────
    // USERS & PROFILES (2 demo users)
    // ─────────────────────────────────────────────
    private static async Task SeedUsersAsync(RoutskyDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var user1 = new User
        {
            Email = "admin@routsky.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "Routsky",
            Role = "Admin",
            CreatedAt = new DateTime(2026, 2, 25, 0, 0, 0, DateTimeKind.Utc)
        };
        context.Users.Add(user1);
        await context.SaveChangesAsync();

        context.UserProfiles.Add(new UserProfile
        {
            UserId = user1.Id,
            Username = "admin",
            Email = user1.Email,
            Passports = new List<string> { "TR" },
            CountryCode = "TR",
            PreferredCurrency = "USD",
            Age = 29
        });

        var user2 = new User
        {
            Email = "arjun@routsky.app",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
            FirstName = "Arjun",
            LastName = "Demo",
            Role = "User",
            CreatedAt = new DateTime(2026, 2, 25, 0, 0, 0, DateTimeKind.Utc)
        };
        context.Users.Add(user2);
        await context.SaveChangesAsync();

        context.UserProfiles.Add(new UserProfile
        {
            UserId = user2.Id,
            Username = "arjun",
            Email = user2.Email,
            Passports = new List<string> { "IN" },
            CountryCode = "IN",
            PreferredCurrency = "USD",
            Age = 26
        });

        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────
    // CITY INTELLIGENCE (Safety, Cost, Climate)
    // Expanded to cover key cities from the 325-node skeleton
    // ─────────────────────────────────────────────
    private static async Task SeedCityIntelligenceAsync(RoutskyDbContext context)
    {
        if (await context.CityIntelligences.AnyAsync()) return;

        var intelligences = new List<CityIntelligence>
        {
            // Western Europe
            CI("London",         "United Kingdom",  65.5, 80.0, 25.0, 5.0, "5,6,7,8,9"),
            CI("Paris",          "France",          58.2, 75.0, 22.0, 2.5, "5,6,7,8,9,10"),
            CI("Amsterdam",      "Netherlands",     70.0, 78.0, 22.0, 4.0, "5,6,7,8,9"),
            CI("Rome",           "Italy",           60.5, 60.0, 18.0, 2.0, "4,5,6,9,10"),
            CI("Barcelona",      "Spain",           55.4, 55.0, 15.0, 2.5, "4,5,6,9,10"),
            CI("Milan",          "Italy",           62.0, 65.0, 18.0, 2.5, "4,5,6,9,10"),
            CI("Munich",         "Germany",         80.0, 72.0, 18.0, 3.0, "5,6,7,8,9"),
            CI("Zurich",         "Switzerland",     82.3,120.0, 40.0, 6.0, "6,7,8,9"),
            CI("Geneva",         "Switzerland",     75.4,115.0, 38.0, 5.0, "6,7,8,9"),
            CI("Vienna",         "Austria",         80.1, 65.0, 20.0, 3.0, "5,6,7,8,9"),
            CI("Berlin",         "Germany",         72.0, 60.0, 15.0, 3.0, "5,6,7,8,9"),
            CI("Frankfurt",      "Germany",         72.0, 70.0, 18.0, 3.5, "5,6,7,8,9"),
            CI("Lisbon",         "Portugal",        74.0, 48.0, 14.0, 2.0, "4,5,6,9,10"),
            CI("Porto",          "Portugal",        78.0, 42.0, 12.0, 1.5, "5,6,7,8,9"),
            CI("Madrid",         "Spain",           58.0, 52.0, 14.0, 2.0, "4,5,6,9,10"),
            CI("Copenhagen",     "Denmark",         78.0, 85.0, 25.0, 4.0, "5,6,7,8,9"),
            CI("Stockholm",      "Sweden",          80.0, 82.0, 22.0, 4.0, "5,6,7,8,9"),
            CI("Oslo",           "Norway",          82.0, 95.0, 28.0, 5.0, "5,6,7,8,9"),
            CI("Helsinki",       "Finland",         85.0, 80.0, 20.0, 4.0, "5,6,7,8,9"),
            CI("Dublin",         "Ireland",         75.0, 72.0, 20.0, 3.5, "5,6,7,8,9"),
            CI("Brussels",       "Belgium",         65.0, 68.0, 18.0, 3.0, "5,6,7,8,9"),
            CI("Athens",         "Greece",          60.0, 50.0, 14.0, 2.0, "4,5,6,9,10"),
            CI("Edinburgh",      "United Kingdom",  70.0, 68.0, 20.0, 3.0, "6,7,8,9"),
            CI("Manchester",     "United Kingdom",  60.0, 65.0, 18.0, 3.0, "5,6,7,8,9"),
            CI("Nice",           "France",          64.0, 70.0, 20.0, 2.5, "5,6,7,8,9"),
            CI("Seville",        "Spain",           60.0, 45.0, 12.0, 1.5, "3,4,5,9,10,11"),
            CI("Amalfi Coast",   "Italy",           75.0, 85.0, 30.0, 5.0, "5,6,7,8,9"),

            // Turkiye
            CI("Istanbul",       "Turkiye",         55.0, 35.0,  8.0, 1.5, "4,5,6,9,10"),
            CI("Ankara",         "Turkiye",         65.0, 30.0,  6.0, 1.0, "4,5,6,9,10"),
            CI("Antalya",        "Turkiye",         60.0, 30.0,  7.0, 1.5, "4,5,6,9,10"),
            CI("Izmir",          "Turkiye",         62.0, 32.0,  7.0, 1.0, "4,5,6,9,10"),
            CI("Bodrum",         "Turkiye",         65.0, 40.0, 10.0, 2.0, "5,6,7,8,9"),
            CI("Cappadocia",     "Turkiye",         70.0, 28.0,  6.0, 3.0, "4,5,6,9,10"),
            CI("Trabzon",        "Turkiye",         68.0, 22.0,  5.0, 0.8, "5,6,7,8,9"),
            CI("Gaziantep",      "Turkiye",         58.0, 20.0,  4.0, 0.8, "3,4,5,10,11"),

            // Eastern Europe & Balkans
            CI("Budapest",       "Hungary",         68.3, 40.0, 10.0, 1.2, "5,6,9,10"),
            CI("Prague",         "Czech Republic",  75.8, 45.0, 12.0, 1.5, "5,6,9,10"),
            CI("Krakow",         "Poland",          78.9, 35.0,  8.0, 1.0, "5,6,7,8,9"),
            CI("Warsaw",         "Poland",          72.0, 40.0, 10.0, 1.2, "5,6,9,10"),
            CI("Bucharest",      "Romania",         66.2, 32.0,  9.0, 0.8, "5,6,9,10"),
            CI("Cluj-Napoca",    "Romania",         77.4, 34.0,  9.0, 0.8, "5,6,9,10"),
            CI("Sofia",          "Bulgaria",        62.1, 30.0,  8.0, 1.0, "5,6,9,10"),
            CI("Tbilisi",        "Georgia",         73.5, 25.0,  8.0, 0.5, "5,6,9,10"),
            CI("Belgrade",       "Serbia",          63.8, 35.0,  9.0, 1.0, "5,6,9,10"),
            CI("Dubrovnik",      "Croatia",         79.5, 60.0, 20.0, 2.0, "5,6,9,10"),
            CI("Zagreb",         "Croatia",         75.0, 45.0, 12.0, 1.5, "5,6,9,10"),
            CI("Ljubljana",      "Slovenia",        82.3, 50.0, 12.0, 1.5, "5,6,7,8,9"),
            CI("Sarajevo",       "Bosnia",          67.2, 29.0,  6.0, 1.0, "5,6,9,10"),
            CI("Tirana",         "Albania",         64.9, 30.0,  7.0, 0.5, "5,6,9,10"),
            CI("Skopje",         "North Macedonia", 65.4, 28.0,  6.0, 0.7, "5,6,9,10"),
            CI("Kotor",          "Montenegro",      72.1, 40.0, 12.0, 1.5, "5,6,9,10"),
            CI("Baku",           "Azerbaijan",      60.0, 32.0,  8.0, 0.8, "4,5,9,10"),
            CI("Riga",           "Latvia",          72.0, 38.0, 10.0, 1.2, "5,6,7,8,9"),
            CI("Vilnius",        "Lithuania",       74.0, 35.0,  9.0, 1.0, "5,6,7,8,9"),
            CI("Tallinn",        "Estonia",         78.0, 42.0, 11.0, 1.5, "5,6,7,8,9"),

            // Southeast Asia
            CI("Bangkok",        "Thailand",        60.1, 35.0,  4.0, 1.0, "11,12,1,2"),
            CI("Chiang Mai",     "Thailand",        75.8, 30.0,  3.0, 0.8, "11,12,1,2"),
            CI("Phuket",         "Thailand",        58.0, 40.0,  5.0, 2.0, "11,12,1,2,3,4"),
            CI("Hanoi",          "Vietnam",         63.4, 28.0,  3.0, 0.5, "10,11,12,3,4"),
            CI("Ho Chi Minh City","Vietnam",        58.9, 30.0,  3.5, 0.5, "12,1,2,3"),
            CI("Hoi An",         "Vietnam",         74.2, 25.0,  4.0, 0.5, "2,3,4,5"),
            CI("Da Nang",        "Vietnam",         72.0, 26.0,  3.0, 0.5, "2,3,4,5"),
            CI("Siem Reap",      "Cambodia",        65.5, 26.0,  5.0, 1.5, "11,12,1,2"),
            CI("Bali",           "Indonesia",       68.2, 32.0,  5.0, 0.5, "5,6,7,8,9,10"),
            CI("Kuala Lumpur",   "Malaysia",        48.7, 32.0,  4.0, 0.8, "5,6,7"),
            CI("Singapore",      "Singapore",       84.0, 80.0,  8.0, 2.0, "2,3,4,5,6,7"),
            CI("Manila",         "Philippines",     42.0, 28.0,  4.0, 0.5, "11,12,1,2,3"),
            CI("Cebu",           "Philippines",     55.0, 25.0,  3.0, 0.5, "11,12,1,2,3,4,5"),
            CI("Penang",         "Malaysia",        55.0, 28.0,  3.0, 0.5, "12,1,2,3"),
            CI("Luang Prabang",  "Laos",            70.0, 22.0,  4.0, 0.5, "11,12,1,2,3"),

            // East Asia
            CI("Tokyo",          "Japan",           82.5, 60.0,  8.0, 2.0, "3,4,5,9,10,11"),
            CI("Kyoto",          "Japan",           85.2, 55.0,  7.0, 1.5, "3,4,5,9,10,11"),
            CI("Osaka",          "Japan",           80.4, 50.0,  6.0, 1.5, "3,4,5,9,10,11"),
            CI("Seoul",          "South Korea",     81.1, 65.0,  7.0, 1.2, "4,5,9,10,11"),
            CI("Hong Kong",      "China",           78.0, 75.0, 10.0, 2.0, "10,11,12,1,2,3"),
            CI("Taipei",         "Taiwan",          82.0, 45.0,  5.0, 1.0, "3,4,5,9,10,11"),
            CI("Beijing",        "China",           68.0, 50.0,  5.0, 1.0, "4,5,9,10"),
            CI("Shanghai",       "China",           72.0, 55.0,  6.0, 1.0, "4,5,9,10,11"),

            // South Asia & Indian Ocean
            CI("Mumbai",         "India",           48.0, 25.0,  3.0, 0.5, "10,11,12,1,2,3"),
            CI("New Delhi",      "India",           45.0, 22.0,  2.0, 0.5, "10,11,12,1,2,3"),
            CI("Goa",            "India",           55.0, 20.0,  4.0, 0.5, "11,12,1,2,3"),
            CI("Colombo",        "Sri Lanka",       60.5, 30.0,  4.0, 0.5, "1,2,3,4"),
            CI("Kathmandu",      "Nepal",           60.0, 18.0,  3.0, 0.3, "3,4,5,9,10,11"),
            CI("Male Atolls",    "Maldives",        65.0, 75.0, 30.0,25.0, "11,12,1,2,3,4"),

            // Americas
            CI("New York",       "United States",   55.0, 90.0, 25.0, 4.0, "4,5,6,9,10"),
            CI("Los Angeles",    "United States",   50.0, 85.0, 22.0, 3.0, "3,4,5,9,10,11"),
            CI("Chicago",        "United States",   48.0, 80.0, 20.0, 3.0, "5,6,7,8,9"),
            CI("Miami",          "United States",   52.0, 75.0, 20.0, 2.5, "11,12,1,2,3,4"),
            CI("San Francisco",  "United States",   55.0, 88.0, 22.0, 3.5, "4,5,6,9,10"),
            CI("Toronto",        "Canada",          72.0, 70.0, 20.0, 3.0, "5,6,7,8,9"),
            CI("Vancouver",      "Canada",          70.0, 72.0, 20.0, 3.0, "6,7,8,9"),
            CI("Mexico City",    "Mexico",          42.6, 35.0,  8.0, 0.5, "3,4,5,10,11"),
            CI("Cancun",         "Mexico",          48.0, 40.0, 10.0, 1.5, "11,12,1,2,3,4"),
            CI("Buenos Aires",   "Argentina",       47.9, 20.0,  6.0, 0.5, "3,4,5,9,10,11"),
            CI("Sao Paulo",      "Brazil",          38.0, 35.0,  8.0, 1.0, "4,5,6,7,8,9"),
            CI("Rio de Janeiro", "Brazil",          35.0, 35.0,  8.0, 1.0, "5,6,7,8,9"),
            CI("Bogota",         "Colombia",        44.0, 25.0,  5.0, 0.5, "12,1,2,3"),
            CI("Medellin",       "Colombia",        42.1, 22.0,  5.0, 0.8, "1,2,3,12"),
            CI("Cartagena",      "Colombia",        46.5, 28.0,  8.0, 1.0, "1,2,3,12"),
            CI("Lima",           "Peru",            33.8, 30.0,  6.0, 0.6, "12,1,2,3,4"),
            CI("Cusco",          "Peru",            49.2, 25.0,  5.0, 0.5, "5,6,7,8,9,10"),
            CI("Santiago",       "Chile",           50.0, 40.0, 10.0, 1.5, "10,11,12,1,2,3"),
            CI("Panama City",    "Panama",          51.5, 55.0, 10.0, 1.5, "1,2,3,4"),
            CI("San Jose",       "Costa Rica",      44.1, 50.0, 10.0, 1.5, "12,1,2,3,4"),
            CI("La Fortuna",     "Costa Rica",      65.5, 45.0, 12.0, 5.0, "12,1,2,3,4"),
            CI("Havana",         "Cuba",            58.0, 22.0,  6.0, 0.5, "11,12,1,2,3,4"),
            CI("Guatemala City", "Guatemala",       38.5, 35.0,  6.0, 0.5, "11,12,1,2,3,4"),
            CI("Antigua",        "Guatemala",       52.4, 40.0,  8.0, 1.0, "11,12,1,2,3,4"),
            CI("Granada",        "Nicaragua",       55.0, 25.0,  5.0, 0.5, "12,1,2,3,4"),

            // Africa & Middle East
            CI("Marrakech",      "Morocco",         63.4, 25.0,  5.0, 0.4, "3,4,5,9,10,11"),
            CI("Fez",            "Morocco",         60.1, 22.0,  4.0, 0.3, "3,4,5,9,10,11"),
            CI("Tangier",        "Morocco",         65.5, 24.0,  5.0, 0.5, "4,5,6,9,10"),
            CI("Casablanca",     "Morocco",         60.0, 28.0,  6.0, 0.5, "4,5,6,9,10"),
            CI("Tunis",          "Tunisia",         58.2, 20.0,  4.0, 0.2, "4,5,6,9,10"),
            CI("Cairo",          "Egypt",           54.3, 18.0,  3.0, 0.2, "10,11,12,1,2,3,4"),
            CI("Cape Town",      "South Africa",    42.0, 35.0,  8.0, 1.5, "10,11,12,1,2,3"),
            CI("Johannesburg",   "South Africa",    35.0, 32.0,  8.0, 1.0, "4,5,8,9,10"),
            CI("Nairobi",        "Kenya",           40.0, 28.0,  5.0, 0.5, "1,2,7,8,9,10"),
            CI("Zanzibar",       "Tanzania",        55.0, 22.0,  5.0, 0.5, "6,7,8,9,10"),
            CI("Addis Ababa",    "Ethiopia",        45.0, 18.0,  3.0, 0.3, "10,11,12,1,2,3"),
            CI("Accra",          "Ghana",           52.0, 25.0,  4.0, 0.5, "11,12,1,2"),
            CI("Kigali",         "Rwanda",          68.0, 22.0,  4.0, 0.5, "6,7,8,9"),
            CI("Dakar",          "Senegal",         50.0, 25.0,  5.0, 0.5, "11,12,1,2,3,4"),
            CI("Dubai",          "UAE",             84.0, 70.0, 15.0, 3.0, "11,12,1,2,3"),
            CI("Doha",           "Qatar",           82.0, 65.0, 12.0, 2.0, "11,12,1,2,3"),
            CI("Amman",          "Jordan",          68.0, 35.0,  6.0, 1.0, "3,4,5,10,11"),
            CI("Muscat",         "Oman",            80.0, 50.0,  8.0, 1.5, "10,11,12,1,2,3"),
            CI("Tel Aviv",       "Israel",          65.0, 65.0, 18.0, 2.0, "3,4,5,9,10,11"),
            CI("Beirut",         "Lebanon",         50.0, 40.0, 10.0, 1.5, "4,5,6,9,10,11"),

            // Oceania
            CI("Sydney",         "Australia",       78.0, 80.0, 22.0, 4.0, "9,10,11,12,1,2,3"),
            CI("Melbourne",      "Australia",       75.0, 75.0, 20.0, 3.5, "10,11,12,1,2,3"),
            CI("Auckland",       "New Zealand",     74.0, 68.0, 18.0, 3.0, "11,12,1,2,3"),
            CI("Queenstown",     "New Zealand",     80.0, 72.0, 22.0, 3.0, "12,1,2,3,6,7,8"),
        };

        context.CityIntelligences.AddRange(intelligences);
        await context.SaveChangesAsync();
    }

    private static CityIntelligence CI(string city, string country, double safety,
        double costIndex, double mealCost, double transportCost, string bestMonths) => new()
    {
        Id = Guid.NewGuid(),
        CityName = city,
        Country = country,
        SafetyIndex = safety,
        CostOfLivingIndex = costIndex,
        AverageMealCostUSD = mealCost,
        AverageTransportCostUSD = transportCost,
        BestMonthsToVisit = bestMonths,
    };

    // ─────────────────────────────────────────────
    // ACCOMMODATION ZONES (from JSON seed file)
    // ─────────────────────────────────────────────
    private static async Task SeedAccommodationZonesAsync(RoutskyDbContext context)
    {
        if (await context.AccommodationZones.AnyAsync()) return;

        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "accommodation_zones.json");
        var json = await File.ReadAllTextAsync(path);
        var zones = JsonSerializer.Deserialize<List<AccommodationZone>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (zones is { Count: > 0 })
        {
            context.AccommodationZones.AddRange(zones);
            await context.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────
    // ATTRACTIONS (from JSON seed file)
    // ─────────────────────────────────────────────
    private static async Task SeedAttractionsAsync(RoutskyDbContext context)
    {
        if (await context.Attractions.AnyAsync()) return;

        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "attractions.json");
        var json = await File.ReadAllTextAsync(path);
        var attractions = JsonSerializer.Deserialize<List<Attraction>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (attractions is { Count: > 0 })
        {
            context.Attractions.AddRange(attractions);
            await context.SaveChangesAsync();
        }
    }
}
