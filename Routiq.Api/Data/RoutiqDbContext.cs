using Microsoft.EntityFrameworkCore;
using Routiq.Api.Entities;

namespace Routiq.Api.Data;

public class RoutiqDbContext : DbContext
{
    public RoutiqDbContext(DbContextOptions<RoutiqDbContext> options) : base(options)
    {
    }

    public DbSet<Destination> Destinations { get; set; }
    public DbSet<VisaRule> VisaRules { get; set; }
    public DbSet<UserRequest> UserRequests { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Attraction> Attractions { get; set; }
    public DbSet<AccommodationZone> AccommodationZones { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserTrip> UserTrips { get; set; }
    public DbSet<TripCheckIn> TripCheckIns { get; set; }
    public DbSet<DestinationTip> DestinationTips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Destination
        modelBuilder.Entity<Destination>()
            .Property(d => d.AvgDailyCostLow)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Destination>()
            .Property(d => d.AvgDailyCostMid)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Destination>()
            .Property(d => d.AvgDailyCostHigh)
            .HasPrecision(18, 2);

        // Configure UserRequest
        modelBuilder.Entity<UserRequest>()
            .Property(ur => ur.TotalBudget)
            .HasPrecision(18, 2);

        // Configure User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Configure Flight
        modelBuilder.Entity<Flight>()
            .Property(f => f.AveragePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Flight>()
            .Property(f => f.MinPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Flight>()
            .Property(f => f.MaxPrice)
            .HasPrecision(18, 2);

        // Configure Attraction
        modelBuilder.Entity<Attraction>()
            .Property(a => a.EstimatedCost)
            .HasPrecision(18, 2);

        // Configure AccommodationZone
        modelBuilder.Entity<AccommodationZone>()
            .Property(az => az.AverageNightlyCost)
            .HasPrecision(18, 2);

        // ── Gamification Entities ──

        // UserProfile: unique FK to User
        modelBuilder.Entity<UserProfile>()
            .HasIndex(up => up.UserId)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasIndex(up => up.Username)
            .IsUnique();

        // UserProfile → many UserTrips
        modelBuilder.Entity<UserProfile>()
            .HasMany(up => up.Trips)
            .WithOne(t => t.UserProfile)
            .HasForeignKey(t => t.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserTrip → many TripCheckIns
        modelBuilder.Entity<UserTrip>()
            .HasMany(t => t.CheckIns)
            .WithOne(ci => ci.UserTrip)
            .HasForeignKey(ci => ci.UserTripId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserTrip.TotalBudget precision
        modelBuilder.Entity<UserTrip>()
            .Property(t => t.TotalBudget)
            .HasPrecision(18, 2);

        // ── Community Entities ──

        // UserProfile → many DestinationTips
        modelBuilder.Entity<UserProfile>()
            .HasMany(up => up.Tips)
            .WithOne(t => t.UserProfile)
            .HasForeignKey(t => t.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // DestinationTip.Content max length
        modelBuilder.Entity<DestinationTip>()
            .Property(t => t.Content)
            .HasMaxLength(500);
    }
}
