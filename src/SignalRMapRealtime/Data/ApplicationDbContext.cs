// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Entity Framework Core database context for the location tracking system.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>Users and drivers in the system.</summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>Vehicles being tracked.</summary>
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    /// <summary>Location points recorded during tracking.</summary>
    public DbSet<Location> Locations { get; set; } = null!;

    /// <summary>Tracking sessions for vehicles.</summary>
    public DbSet<TrackingSession> TrackingSessions { get; set; } = null!;

    /// <summary>Planned routes for vehicles.</summary>
    public DbSet<Route> Routes { get; set; } = null!;

    /// <summary>Waypoints on routes.</summary>
    public DbSet<Waypoint> Waypoints { get; set; } = null!;

    /// <summary>Assets being tracked.</summary>
    public DbSet<Asset> Assets { get; set; } = null!;

    /// <summary>
    /// Configures the entity relationships and constraints.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeId);
        });

        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VIN).HasMaxLength(17);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.HasOne(e => e.Driver).WithMany(u => u.AssignedVehicles)
                .HasForeignKey(e => e.DriverId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsOnline);
        });

        // Location configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.Vehicle).WithMany(v => v.Locations)
                .HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TrackingSession).WithMany(ts => ts.Locations)
                .HasForeignKey(e => e.TrackingSessionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.VehicleId, e.RecordedAt });
            entity.HasIndex(e => e.LocationType);
        });

        // Asset configuration
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Condition).HasMaxLength(100);
            entity.HasOne(e => e.Vehicle).WithMany()
                .HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => e.AssetType);
        });

        // TrackingSession configuration
        modelBuilder.Entity<TrackingSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.Vehicle).WithMany(v => v.TrackingSessions)
                .HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Route).WithOne(r => r.TrackingSession)
                .HasForeignKey<TrackingSession>(e => e.RouteId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.VehicleId, e.StartTime });
        });

        // Route configuration
        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Origin).HasMaxLength(500);
            entity.Property(e => e.Destination).HasMaxLength(500);
            entity.HasOne(e => e.Vehicle).WithMany(v => v.Routes)
                .HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedUser).WithMany(u => u.AssignedRoutes)
                .HasForeignKey(e => e.AssignedUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsCompleted);
        });

        // Waypoint configuration
        modelBuilder.Entity<Waypoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Instructions).HasMaxLength(1000);
            entity.Property(e => e.ContactName).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.HasOne(e => e.Route).WithMany(r => r.Waypoints)
                .HasForeignKey(e => e.RouteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.RouteId, e.Sequence });
        });
    }
}
