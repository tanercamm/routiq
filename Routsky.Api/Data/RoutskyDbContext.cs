using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Routsky.Api.Entities;

namespace Routsky.Api.Data;

public class RoutskyDbContext : DbContext
{
    public RoutskyDbContext(DbContextOptions<RoutskyDbContext> options) : base(options)
    {
    }

    // ── Core Lookup ──
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<VisaRule> VisaRules { get; set; }
    public DbSet<RegionPriceTier> RegionPriceTiers { get; set; }

    // ── Auth ──
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    // ── Travel Groups ──
    public DbSet<TravelGroup> TravelGroups { get; set; }
    public DbSet<TravelGroupMember> TravelGroupMembers { get; set; }
    public DbSet<GroupShortlistRoute> GroupShortlistRoutes { get; set; }
    public DbSet<RouteVote> RouteVotes { get; set; }

    // ── Route Engine ──
    public DbSet<RouteQuery> RouteQueries { get; set; }
    public DbSet<SavedRoute> SavedRoutes { get; set; }
    public DbSet<RouteStop> RouteStops { get; set; }
    public DbSet<RouteElimination> RouteEliminations { get; set; }

    // ── Community Loop ──
    public DbSet<TraveledRoute> TraveledRoutes { get; set; }

    // ── City Intelligence ──
    public DbSet<CityIntelligence> CityIntelligences { get; set; }
    public DbSet<VisaMatrix> VisaMatrices { get; set; }

    // ── Accommodation & Attractions ──
    public DbSet<AccommodationZone> AccommodationZones { get; set; }
    public DbSet<Attraction> Attractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // ── Travel Groups ──
        modelBuilder.Entity<TravelGroup>()
            .HasIndex(tg => tg.InviteCode)
            .IsUnique();

        modelBuilder.Entity<TravelGroupMember>()
            .HasKey(tgm => new { tgm.GroupId, tgm.UserId });

        modelBuilder.Entity<TravelGroupMember>()
            .HasOne(tgm => tgm.Group)
            .WithMany(tg => tg.Members)
            .HasForeignKey(tgm => tgm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TravelGroupMember>()
            .HasOne(tgm => tgm.User)
            .WithMany() // or add navigation to User
            .HasForeignKey(tgm => tgm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupShortlistRoute>()
            .HasOne(sr => sr.Group)
            .WithMany()
            .HasForeignKey(sr => sr.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RouteVote>()
            .HasOne(rv => rv.ShortlistRoute)
            .WithMany(sr => sr.Votes)
            .HasForeignKey(rv => rv.GroupShortlistRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RouteVote>()
            .HasOne(rv => rv.User)
            .WithMany()
            .HasForeignKey(rv => rv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── List<string> ValueConverters ──
        // EF Core / Npgsql does not auto-map List<string> to text[].
        // Serialise as JSON array string so the column is a simple VARCHAR.
        var stringListConverter = new ValueConverter<List<string>, string>(
            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
            v => string.IsNullOrEmpty(v)
                     ? new List<string>()
                     : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<string>()
        );

        modelBuilder.Entity<UserProfile>()
            .Property(up => up.Passports)
            .HasConversion(stringListConverter)
            .HasColumnType("text");

        modelBuilder.Entity<RouteQuery>()
            .Property(rq => rq.Passports)
            .HasConversion(stringListConverter)
            .HasColumnType("text");

        // ── UserProfile ──
        modelBuilder.Entity<UserProfile>()
            .HasIndex(up => up.UserId)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasIndex(up => up.Username)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── RouteQuery ──
        modelBuilder.Entity<RouteQuery>()
            .HasOne(rq => rq.User)
            .WithMany()
            .HasForeignKey(rq => rq.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // RouteQuery → many SavedRoutes
        modelBuilder.Entity<SavedRoute>()
            .HasOne(sr => sr.RouteQuery)
            .WithMany(rq => rq.SavedRoutes)
            .HasForeignKey(sr => sr.RouteQueryId)
            .OnDelete(DeleteBehavior.Restrict); // keep RouteQuery even if SavedRoute deleted

        // RouteQuery → many RouteEliminations
        modelBuilder.Entity<RouteElimination>()
            .HasOne(re => re.RouteQuery)
            .WithMany(rq => rq.Eliminations)
            .HasForeignKey(re => re.RouteQueryId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── SavedRoute ──
        modelBuilder.Entity<SavedRoute>()
            .HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.NoAction); // avoid multiple cascade paths

        // SavedRoute → many RouteStops
        modelBuilder.Entity<RouteStop>()
            .HasOne(rs => rs.SavedRoute)
            .WithMany(sr => sr.Stops)
            .HasForeignKey(rs => rs.SavedRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        // RouteStop → Destination
        modelBuilder.Entity<RouteStop>()
            .HasOne(rs => rs.Destination)
            .WithMany(d => d.RouteStops)
            .HasForeignKey(rs => rs.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);

        // RouteElimination → Destination
        modelBuilder.Entity<RouteElimination>()
            .HasOne(re => re.Destination)
            .WithMany(d => d.Eliminations)
            .HasForeignKey(re => re.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── TraveledRoute ──
        // SavedRoute → one TraveledRoute (1:1)
        modelBuilder.Entity<TraveledRoute>()
            .HasOne(tr => tr.SavedRoute)
            .WithOne(sr => sr.TraveledRoute)
            .HasForeignKey<TraveledRoute>(tr => tr.SavedRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes for common query patterns ──
        // Visa lookups by passport + destination
        modelBuilder.Entity<VisaRule>()
            .HasIndex(vr => new { vr.PassportCountryCode, vr.DestinationCountryCode })
            .IsUnique();

        // Region price tier lookups
        modelBuilder.Entity<RegionPriceTier>()
            .HasIndex(rpt => new { rpt.Region, rpt.CostLevel })
            .IsUnique();

        // SavedRoutes by user
        modelBuilder.Entity<SavedRoute>()
            .HasIndex(sr => sr.UserId);

        // RouteStops ordering within a route
        modelBuilder.Entity<RouteStop>()
            .HasIndex(rs => new { rs.SavedRouteId, rs.StopOrder })
            .IsUnique();

        // ── Accommodation Zones ──
        modelBuilder.Entity<AccommodationZone>()
            .HasIndex(az => new { az.CityName, az.ZoneName })
            .IsUnique();

        // ── Attractions ──
        modelBuilder.Entity<Attraction>()
            .HasIndex(a => new { a.CityName, a.Name })
            .IsUnique();
    }
}
