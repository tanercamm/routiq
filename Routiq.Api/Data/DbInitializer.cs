using Routiq.Api.Entities;

namespace Routiq.Api.Data;

public static class DbInitializer
{
    public static void Initialize(RoutiqDbContext context)
    {
        context.Database.EnsureCreated();

        // Look for any destinations.
        if (context.Destinations.Any())
        {
            return;   // DB has been seeded
        }

        var destinations = new Destination[]
        {
            // Balkans
            new Destination { Country = "Serbia", City = "Belgrade", Region = "Balkans", AvgDailyCostLow = 40, AvgDailyCostMid = 70, AvgDailyCostHigh = 120, PopularityScore = 8, ClimateTags = new[] { "Continental", "Mild" }, Notes = "Great nightlife and food." },
            new Destination { Country = "Montenegro", City = "Kotor", Region = "Balkans", AvgDailyCostLow = 45, AvgDailyCostMid = 80, AvgDailyCostHigh = 150, PopularityScore = 9, ClimateTags = new[] { "Coastal", "Mediterranean" }, Notes = "Stunning bay views." },
            new Destination { Country = "Albania", City = "Tirana", Region = "Balkans", AvgDailyCostLow = 35, AvgDailyCostMid = 60, AvgDailyCostHigh = 100, PopularityScore = 7, ClimateTags = new[] { "Mediterranean" }, Notes = "Up and coming gem." },
            new Destination { Country = "Bosnia & Herzegovina", City = "Sarajevo", Region = "Balkans", AvgDailyCostLow = 40, AvgDailyCostMid = 70, AvgDailyCostHigh = 110, PopularityScore = 8, ClimateTags = new[] { "Continental" }, Notes = "Rich history." },
            
            // Western Europe
            new Destination { Country = "France", City = "Paris", Region = "Western Europe", AvgDailyCostLow = 80, AvgDailyCostMid = 150, AvgDailyCostHigh = 300, PopularityScore = 10, ClimateTags = new[] { "Temperate" }, Notes = "The city of lights." },
            new Destination { Country = "Italy", City = "Rome", Region = "Western Europe", AvgDailyCostLow = 75, AvgDailyCostMid = 140, AvgDailyCostHigh = 280, PopularityScore = 10, ClimateTags = new[] { "Mediterranean" }, Notes = "Eternal city." },
            new Destination { Country = "Germany", City = "Berlin", Region = "Western Europe", AvgDailyCostLow = 70, AvgDailyCostMid = 130, AvgDailyCostHigh = 250, PopularityScore = 9, ClimateTags = new[] { "Continental" }, Notes = "Cool vibes." },
            new Destination { Country = "Spain", City = "Barcelona", Region = "Western Europe", AvgDailyCostLow = 70, AvgDailyCostMid = 130, AvgDailyCostHigh = 260, PopularityScore = 10, ClimateTags = new[] { "Mediterranean" }, Notes = "Gaudí architecture." },

            // Asia
            new Destination { Country = "Thailand", City = "Bangkok", Region = "Southeast Asia", AvgDailyCostLow = 30, AvgDailyCostMid = 60, AvgDailyCostHigh = 150, PopularityScore = 10, ClimateTags = new[] { "Tropical" }, Notes = "Street food heaven." },
            new Destination { Country = "Japan", City = "Tokyo", Region = "East Asia", AvgDailyCostLow = 90, AvgDailyCostMid = 180, AvgDailyCostHigh = 400, PopularityScore = 10, ClimateTags = new[] { "Temperate" }, Notes = "Futuristic yet traditional." },
            new Destination { Country = "Vietnam", City = "Hanoi", Region = "Southeast Asia", AvgDailyCostLow = 25, AvgDailyCostMid = 50, AvgDailyCostHigh = 120, PopularityScore = 8, ClimateTags = new[] { "Tropical" }, Notes = "Chaotic and charming." },
            new Destination { Country = "Indonesia", City = "Bali", Region = "Southeast Asia", AvgDailyCostLow = 40, AvgDailyCostMid = 80, AvgDailyCostHigh = 250, PopularityScore = 10, ClimateTags = new[] { "Tropical" }, Notes = "Island paradise." },

            // Turkey (Domestic)
            new Destination { Country = "Turkey", City = "Istanbul", Region = "Turkey", AvgDailyCostLow = 50, AvgDailyCostMid = 100, AvgDailyCostHigh = 200, PopularityScore = 10, ClimateTags = new[] { "Temperate" }, Notes = "Where East meets West." },
            new Destination { Country = "Turkey", City = "Antalya", Region = "Turkey", AvgDailyCostLow = 45, AvgDailyCostMid = 90, AvgDailyCostHigh = 180, PopularityScore = 9, ClimateTags = new[] { "Mediterranean" }, Notes = "Beautiful coast." },
            new Destination { Country = "Turkey", City = "Cappadocia", Region = "Turkey", AvgDailyCostLow = 60, AvgDailyCostMid = 120, AvgDailyCostHigh = 300, PopularityScore = 10, ClimateTags = new[] { "Continental" }, Notes = "Fairy chimneys." }
        };

        context.Destinations.AddRange(destinations);

        var visaRules = new VisaRule[]
        {
            // Turkey Passport Rules
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Serbia", VisaType = VisaType.VisaFree, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Montenegro", VisaType = VisaType.VisaFree, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Albania", VisaType = VisaType.VisaFree, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Bosnia & Herzegovina", VisaType = VisaType.VisaFree, MaxStayDays = 90 },

            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "France", VisaType = VisaType.Required, MaxStayDays = 90 }, // Schengen
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Italy", VisaType = VisaType.Required, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Germany", VisaType = VisaType.Required, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Spain", VisaType = VisaType.Required, MaxStayDays = 90 },

            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Thailand", VisaType = VisaType.VisaFree, MaxStayDays = 30 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Japan", VisaType = VisaType.VisaFree, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Vietnam", VisaType = VisaType.Evisa, MaxStayDays = 90 },
            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Indonesia", VisaType = VisaType.OnArrival, MaxStayDays = 30 },

            new VisaRule { PassportCountry = "Turkey", DestinationCountry = "Turkey", VisaType = VisaType.VisaFree, MaxStayDays = 365 }
        };

        context.VisaRules.AddRange(visaRules);
        context.SaveChanges();
    }
}
