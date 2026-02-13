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
    }
}
