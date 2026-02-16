using Routiq.Api.Entities;

namespace Routiq.Api.Data;

public static class DbInitializer
{
    public static void Initialize(RoutiqDbContext context)
    {
        context.Database.EnsureCreated();

        // Seed destinations and visa rules if they don't exist
        if (!context.Destinations.Any())
        {

            var destinations = new Destination[]
            {
            // Balkans
            new() { Country = "Serbia", City = "Belgrade", Region = "Balkans", AvgDailyCostLow = 40, AvgDailyCostMid = 70, AvgDailyCostHigh = 120, PopularityScore = 8, ClimateTags = ["Continental", "Mild"], Notes = "Great nightlife and food." },
            new() { Country = "Montenegro", City = "Kotor", Region = "Balkans", AvgDailyCostLow = 45, AvgDailyCostMid = 80, AvgDailyCostHigh = 150, PopularityScore = 9, ClimateTags = ["Coastal", "Mediterranean"], Notes = "Stunning bay views." },
            new() { Country = "Albania", City = "Tirana", Region = "Balkans", AvgDailyCostLow = 35, AvgDailyCostMid = 60, AvgDailyCostHigh = 100, PopularityScore = 7, ClimateTags = ["Mediterranean"], Notes = "Up and coming gem." },
            new() { Country = "Bosnia & Herzegovina", City = "Sarajevo", Region = "Balkans", AvgDailyCostLow = 40, AvgDailyCostMid = 70, AvgDailyCostHigh = 110, PopularityScore = 8, ClimateTags = ["Continental"], Notes = "Rich history." },
            
            // Western Europe
            new() { Country = "France", City = "Paris", Region = "Western Europe", AvgDailyCostLow = 80, AvgDailyCostMid = 150, AvgDailyCostHigh = 300, PopularityScore = 10, ClimateTags = ["Temperate"], Notes = "The city of lights." },
            new() { Country = "Italy", City = "Rome", Region = "Western Europe", AvgDailyCostLow = 75, AvgDailyCostMid = 140, AvgDailyCostHigh = 280, PopularityScore = 10, ClimateTags = ["Mediterranean"], Notes = "Eternal city." },
            new() { Country = "Germany", City = "Berlin", Region = "Western Europe", AvgDailyCostLow = 70, AvgDailyCostMid = 130, AvgDailyCostHigh = 250, PopularityScore = 9, ClimateTags = ["Continental"], Notes = "Cool vibes." },
            new() { Country = "Germany", City = "Stuttgart", Region = "Western Europe", AvgDailyCostLow = 65, AvgDailyCostMid = 120, AvgDailyCostHigh = 240, PopularityScore = 8, ClimateTags = ["Continental"], Notes = "Automotive hub & vineyards." }, // Added Stuttgart
            new() { Country = "Spain", City = "Barcelona", Region = "Western Europe", AvgDailyCostLow = 70, AvgDailyCostMid = 130, AvgDailyCostHigh = 260, PopularityScore = 10, ClimateTags = ["Mediterranean"], Notes = "Gaudí architecture." },

            // Asia
            new() { Country = "Thailand", City = "Bangkok", Region = "Southeast Asia", AvgDailyCostLow = 30, AvgDailyCostMid = 60, AvgDailyCostHigh = 150, PopularityScore = 10, ClimateTags = ["Tropical"], Notes = "Street food heaven." },
            new() { Country = "Japan", City = "Tokyo", Region = "East Asia", AvgDailyCostLow = 90, AvgDailyCostMid = 180, AvgDailyCostHigh = 400, PopularityScore = 10, ClimateTags = ["Temperate"], Notes = "Futuristic yet traditional." },
            new() { Country = "Vietnam", City = "Hanoi", Region = "Southeast Asia", AvgDailyCostLow = 25, AvgDailyCostMid = 50, AvgDailyCostHigh = 120, PopularityScore = 8, ClimateTags = ["Tropical"], Notes = "Chaotic and charming." },
            new() { Country = "Indonesia", City = "Bali", Region = "Southeast Asia", AvgDailyCostLow = 40, AvgDailyCostMid = 80, AvgDailyCostHigh = 250, PopularityScore = 10, ClimateTags = ["Tropical"], Notes = "Island paradise." },

            // Turkey (Domestic)
            new() { Country = "Turkey", City = "Istanbul", Region = "Turkey", AvgDailyCostLow = 50, AvgDailyCostMid = 100, AvgDailyCostHigh = 200, PopularityScore = 10, ClimateTags = ["Temperate"], Notes = "Where East meets West." },
            new() { Country = "Turkey", City = "Antalya", Region = "Turkey", AvgDailyCostLow = 45, AvgDailyCostMid = 90, AvgDailyCostHigh = 180, PopularityScore = 9, ClimateTags = ["Mediterranean"], Notes = "Beautiful coast." },
            new() { Country = "Turkey", City = "Cappadocia", Region = "Turkey", AvgDailyCostLow = 60, AvgDailyCostMid = 120, AvgDailyCostHigh = 300, PopularityScore = 10, ClimateTags = ["Continental"], Notes = "Fairy chimneys." }
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

        SeedUsersAndProfiles(context);
        SeedCommunityData(context);
    }

    private static void SeedUsersAndProfiles(RoutiqDbContext context)
    {
        var usersData = new[]
        {
            new { Email = "admin@routiq.com", Username = "TanerCam", Role = "Admin", Country = "Turkey", Currency = "USD", Code = "TR", Points = 850 },
            new { Email = "klaus@routiq.com", Username = "Klaus_M", Role = "User", Country = "Germany", Currency = "EUR", Code = "DE", Points = 450 },
            new { Email = "yuki@routiq.com", Username = "YukiTravels", Role = "User", Country = "Japan", Currency = "JPY", Code = "JP", Points = 1200 }
        };

        foreach (var u in usersData)
        {
            // Ensure User
            var user = context.Users.FirstOrDefault(x => x.Email == u.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = u.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Default password for all seed users
                    FirstName = u.Username,
                    LastName = "User",
                    Role = u.Role,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            // Ensure Profile
            var profile = context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user!.Id,
                    Username = u.Username,
                    Email = u.Email,
                    PassportCountry = u.Country,
                    PreferredCurrency = u.Currency,
                    CountryCode = u.Code,
                    TotalPoints = u.Points
                };
                context.UserProfiles.Add(profile);
            }
            else
            {
                // Update existing profile
                profile.Username = u.Username;
                profile.CountryCode = u.Code;
                profile.PassportCountry = u.Country;
                profile.PreferredCurrency = u.Currency;
                profile.TotalPoints = u.Points;
                context.UserProfiles.Update(profile);
            }
        }
        context.SaveChanges();
    }

    private static void SeedCommunityData(RoutiqDbContext context)
    {
        var taner = context.UserProfiles.Single(p => p.Username == "TanerCam");
        var klaus = context.UserProfiles.Single(p => p.Username == "Klaus_M");
        var yuki = context.UserProfiles.Single(p => p.Username == "YukiTravels");

        // Ensure Destinations Exist for Trips
        var stuttgart = context.Destinations.FirstOrDefault(d => d.City == "Stuttgart");
        var belgrade = context.Destinations.FirstOrDefault(d => d.City == "Belgrade");
        var tokyo = context.Destinations.FirstOrDefault(d => d.City == "Tokyo");

        // If Stuttgart missing (e.g. strict seed didn't run because Destinations existed), add it now
        if (stuttgart == null)
        {
            stuttgart = new Destination { Country = "Germany", City = "Stuttgart", Region = "Western Europe", AvgDailyCostLow = 65, AvgDailyCostMid = 120, AvgDailyCostHigh = 240, PopularityScore = 8, ClimateTags = ["Continental"], Notes = "Automotive hub." };
            context.Destinations.Add(stuttgart);
            context.SaveChanges();
        }

        // Ensure Attractions
        var mercedes = context.Attractions.FirstOrDefault(a => a.Name == "Mercedes-Benz Museum");
        if (mercedes == null && stuttgart != null)
        {
            mercedes = new Attraction { Name = "Mercedes-Benz Museum", CityId = stuttgart.Id, Description = "Automobile history.", Category = "Museum", EstimatedCost = 12, EstimatedDurationInHours = 3, BestTimeOfDay = "Morning" };
            context.Attractions.Add(mercedes);
        }

        var fortress = context.Attractions.FirstOrDefault(a => a.Name == "Kalemegdan Fortress");
        if (fortress == null && belgrade != null)
        {
            fortress = new Attraction { Name = "Kalemegdan Fortress", CityId = belgrade.Id, Description = "Historic fortress.", Category = "Landmark", EstimatedCost = 0, EstimatedDurationInHours = 2, BestTimeOfDay = "Sunset" };
            context.Attractions.Add(fortress);
        }
        context.SaveChanges();

        // Seed Trips
        if (!context.UserTrips.Any())
        {
            if (stuttgart != null) context.UserTrips.Add(new UserTrip { UserProfileId = taner.Id, DestinationCityId = stuttgart.Id, TotalBudget = 1500, Days = 5, CreatedAt = DateTime.UtcNow.AddDays(-10) });
            if (belgrade != null) context.UserTrips.Add(new UserTrip { UserProfileId = yuki.Id, DestinationCityId = belgrade.Id, TotalBudget = 600, Days = 4, CreatedAt = DateTime.UtcNow.AddDays(-20) });
            if (tokyo != null) context.UserTrips.Add(new UserTrip { UserProfileId = klaus.Id, DestinationCityId = tokyo.Id, TotalBudget = 2500, Days = 10, CreatedAt = DateTime.UtcNow.AddDays(-40) });
            context.SaveChanges();
        }

        // Seed Tips
        if (!context.DestinationTips.Any())
        {
            if (stuttgart != null) context.DestinationTips.Add(new DestinationTip { CityId = stuttgart.Id, UserProfileId = klaus.Id, Content = "The classic Mercedes-Benz Museum is an absolute must-visit. Dedicate at least half a day to see all the historical models!", Upvotes = 89 });
            if (tokyo != null) context.DestinationTips.Add(new DestinationTip { CityId = tokyo.Id, UserProfileId = taner.Id, Content = "Found some amazing healthy eating spots near the university dorms. Great macro-friendly meals for cheap.", Upvotes = 124 });
            context.SaveChanges();
        }

        // Seed CheckIns
        if (!context.TripCheckIns.Any())
        {
            // Find Taner's Stuttgart trip
            var tanerTrip = context.UserTrips.FirstOrDefault(t => t.UserProfileId == taner.Id && t.DestinationCityId == stuttgart!.Id);
            if (tanerTrip != null && mercedes != null)
            {
                context.TripCheckIns.Add(new TripCheckIn { UserTripId = tanerTrip.Id, AttractionId = mercedes.Id, EarnedPoints = 100, UserPostText = "Visiting the legends!", CheckInDate = DateTime.UtcNow.AddDays(-8) });
            }

            // Find Yuki's Belgrade trip
            var yukiTrip = context.UserTrips.FirstOrDefault(t => t.UserProfileId == yuki.Id && t.DestinationCityId == belgrade!.Id);
            if (yukiTrip != null && fortress != null)
            {
                context.TripCheckIns.Add(new TripCheckIn { UserTripId = yukiTrip.Id, AttractionId = fortress.Id, EarnedPoints = 50, UserPostText = "Sunset at Kalemegdan.", CheckInDate = DateTime.UtcNow.AddDays(-18) });
            }
            context.SaveChanges();
        }
    }
}
