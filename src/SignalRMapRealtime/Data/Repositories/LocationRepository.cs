// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Repository for location data access with specialized queries.
/// </summary>
public class LocationRepository : BaseRepository<Location>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of LocationRepository.
    /// </summary>
    public LocationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the latest location for a specific vehicle.
    /// </summary>
    public async Task<Location?> GetLatestLocationByVehicleAsync(int vehicleId)
    {
        return await _context.Locations
            .Where(l => l.VehicleId == vehicleId)
            .OrderByDescending(l => l.RecordedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets locations recorded within a time range for a vehicle.
    /// </summary>
    public async Task<IEnumerable<Location>> GetLocationsByTimeRangeAsync(int vehicleId, DateTime startTime, DateTime endTime)
    {
        return await _context.Locations
            .Where(l => l.VehicleId == vehicleId && l.RecordedAt >= startTime && l.RecordedAt <= endTime)
            .OrderBy(l => l.RecordedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets locations for a tracking session.
    /// </summary>
    public async Task<IEnumerable<Location>> GetLocationsBySessionAsync(int sessionId)
    {
        return await _context.Locations
            .Where(l => l.TrackingSessionId == sessionId)
            .OrderBy(l => l.RecordedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets locations by type within a geographic area.
    /// </summary>
    public async Task<IEnumerable<Location>> GetLocationsByTypeAsync(LocationType locationType)
    {
        return await _context.Locations
            .Where(l => l.LocationType == locationType)
            .Include(l => l.Vehicle)
            .ToListAsync();
    }

    /// <summary>
    /// Gets locations recorded in the last N hours for a vehicle.
    /// </summary>
    public async Task<IEnumerable<Location>> GetRecentLocationsAsync(int vehicleId, int hoursBack = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hoursBack);
        return await _context.Locations
            .Where(l => l.VehicleId == vehicleId && l.RecordedAt >= cutoffTime)
            .OrderBy(l => l.RecordedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Finds locations within a geographic radius from a point.
    /// </summary>
    public async Task<IEnumerable<Location>> GetLocationsNearbyAsync(double centerLat, double centerLng, double radiusKm = 1.0)
    {
        var locations = await _context.Locations
            .Include(l => l.Vehicle)
            .ToListAsync();

        return locations.Where(l => CalculateDistance(centerLat, centerLng, l.Latitude, l.Longitude) <= radiusKm).ToList();
    }

    /// <summary>
    /// Gets location statistics for a vehicle over a time period.
    /// </summary>
    public async Task<(int count, double minSpeed, double maxSpeed, double avgSpeed)> GetLocationStatsAsync(int vehicleId, DateTime startTime, DateTime endTime)
    {
        var locations = await _context.Locations
            .Where(l => l.VehicleId == vehicleId && l.RecordedAt >= startTime && l.RecordedAt <= endTime && l.Speed.HasValue)
            .ToListAsync();

        if (!locations.Any())
            return (0, 0, 0, 0);

        return (
            locations.Count,
            locations.Min(l => l.Speed ?? 0),
            locations.Max(l => l.Speed ?? 0),
            locations.Average(l => l.Speed ?? 0)
        );
    }

    /// <summary>
    /// Deletes old location records older than specified days.
    /// </summary>
    public async Task<int> DeleteOldLocationsAsync(int daysOld = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var locationsToDelete = await _context.Locations
            .Where(l => l.RecordedAt < cutoffDate)
            .ToListAsync();

        _context.Locations.RemoveRange(locationsToDelete);
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calculates distance between two geographic points using Haversine formula.
    /// </summary>
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }
}
