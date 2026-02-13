using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Entities;

namespace Routiq.Api.Data;

/// <summary>
/// Reads flights.json and attractions.json from SeedData/ and inserts
/// them into the database if the respective tables are empty.
/// This class does NOT modify or interfere with DbInitializer.
/// </summary>
public static class FlightAttractionSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Seed(RoutiqDbContext context)
    {
        SeedFlights(context);
        SeedAttractions(context);
    }

    private static void SeedFlights(RoutiqDbContext context)
    {
        if (context.Flights.Any()) return;

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "flights.json");
        if (!File.Exists(jsonPath)) return;

        var json = File.ReadAllText(jsonPath);
        var flights = JsonSerializer.Deserialize<List<FlightSeedDto>>(json, JsonOptions);
        if (flights == null) return;

        foreach (var f in flights)
        {
            context.Flights.Add(new Flight
            {
                Origin = f.Origin,
                Destination = f.Destination,
                AirlineName = f.AirlineName,
                DepartureTime = TimeSpan.Parse(f.DepartureTime),
                AveragePrice = f.AveragePrice,
                MinPrice = f.MinPrice,
                MaxPrice = f.MaxPrice,
                Currency = f.Currency
            });
        }

        context.SaveChanges();
    }

    private static void SeedAttractions(RoutiqDbContext context)
    {
        if (context.Attractions.Any()) return;

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "attractions.json");
        if (!File.Exists(jsonPath)) return;

        var json = File.ReadAllText(jsonPath);
        var attractions = JsonSerializer.Deserialize<List<AttractionSeedDto>>(json, JsonOptions);
        if (attractions == null) return;

        foreach (var a in attractions)
        {
            // Look up the Destination by City name to get the CityId FK
            var destination = context.Destinations.FirstOrDefault(d => d.City == a.CityName);
            if (destination == null) continue; // skip if city not found

            context.Attractions.Add(new Attraction
            {
                CityId = destination.Id,
                Name = a.Name,
                EstimatedCost = a.EstimatedCost,
                EstimatedDurationInHours = a.EstimatedDurationInHours
            });
        }

        context.SaveChanges();
    }

    // ── Internal DTOs for JSON deserialization ──

    private class FlightSeedDto
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = "00:00:00";
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string Currency { get; set; } = "USD";
    }

    private class AttractionSeedDto
    {
        public string CityName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public double EstimatedDurationInHours { get; set; }
    }
}
