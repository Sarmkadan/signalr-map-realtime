#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Extension methods for LocationRepository providing additional query capabilities
/// and convenience methods for location data operations.
/// </summary>
public static class LocationRepositoryExtensions
{
    /// <summary>
    /// Gets the latest location for each vehicle in a collection of vehicle IDs.
    /// </summary>
    public static async Task<Dictionary<int, Location?>> GetLatestLocationsByVehiclesAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds)
    {
        var results = new Dictionary<int, Location?>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var location = await repository.GetLatestLocationByVehicleAsync(vehicleId);
            results[vehicleId] = location;
        }

        return results;
    }

    /// <summary>
    /// Gets the latest location for each vehicle in a collection of vehicle IDs with vehicle details.
    /// </summary>
    public static async Task<IEnumerable<(int VehicleId, Location? Location)>> GetLatestLocationsWithVehicleDetailsAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds)
    {
        var results = new List<(int VehicleId, Location?)>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var location = await repository.GetLatestLocationByVehicleAsync(vehicleId);
            results.Add((vehicleId, location));
        }

        return results;
    }

    /// <summary>
    /// Gets locations for multiple vehicles within a time range.
    /// </summary>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsByTimeRangeAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds,
        DateTime startTime,
        DateTime endTime)
    {
        var result = new Dictionary<int, IEnumerable<Location>>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var locations = await repository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);
            result[vehicleId] = locations;
        }

        return result;
    }

    /// <summary>
    /// Gets locations for multiple tracking sessions.
    /// </summary>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsBySessionsAsync(
        this LocationRepository repository,
        IEnumerable<int> sessionIds)
    {
        var result = new Dictionary<int, IEnumerable<Location>>();

        foreach (var sessionId in sessionIds.Distinct())
        {
            var locations = await repository.GetLocationsBySessionAsync(sessionId);
            result[sessionId] = locations;
        }

        return result;
    }

    /// <summary>
    /// Gets locations by type for multiple vehicle IDs.
    /// </summary>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsByTypeAndVehicleAsync(
        this LocationRepository repository,
        LocationType locationType,
        IEnumerable<int> vehicleIds)
    {
        var result = new Dictionary<int, IEnumerable<Location>>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var locations = await repository.GetLocationsByTypeAsync(locationType);
            result[vehicleId] = locations.Where(l => l.VehicleId == vehicleId);
        }

        return result;
    }

    /// <summary>
    /// Gets recent locations for multiple vehicles within a specified time window.
    /// </summary>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetRecentLocationsAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds,
        int hoursBack = 24)
    {
        var result = new Dictionary<int, IEnumerable<Location>>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var locations = await repository.GetRecentLocationsAsync(vehicleId, hoursBack);
            result[vehicleId] = locations;
        }

        return result;
    }

    /// <summary>
    /// Gets locations within a geographic radius for multiple vehicles.
    /// </summary>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsNearbyAsync(
        this LocationRepository repository,
        double centerLat,
        double centerLng,
        double radiusKm,
        IEnumerable<int> vehicleIds)
    {
        var result = new Dictionary<int, IEnumerable<Location>>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var locations = await repository.GetLocationsNearbyAsync(centerLat, centerLng, radiusKm);
            result[vehicleId] = locations.Where(l => l.VehicleId == vehicleId);
        }

        return result;
    }

    /// <summary>
    /// Gets location statistics for multiple vehicles over a time period.
    /// </summary>
    public static async Task<Dictionary<int, (int count, double minSpeed, double maxSpeed, double avgSpeed)>> GetLocationStatsAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds,
        DateTime startTime,
        DateTime endTime)
    {
        var result = new Dictionary<int, (int count, double minSpeed, double maxSpeed, double avgSpeed)>();

        foreach (var vehicleId in vehicleIds.Distinct())
        {
            var stats = await repository.GetLocationStatsAsync(vehicleId, startTime, endTime);
            result[vehicleId] = stats;
        }

        return result;
    }

    /// <summary>
    /// Gets locations within a specific time range and radius for a vehicle.
    /// </summary>
    public static async Task<IEnumerable<Location>> GetLocationsInTimeRangeAndRadiusAsync(
        this LocationRepository repository,
        int vehicleId,
        DateTime startTime,
        DateTime endTime,
        double centerLat,
        double centerLng,
        double radiusKm)
    {
        var locations = await repository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);

        var distanceMethod = typeof(LocationRepository).GetMethod("CalculateDistance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return locations.Where(l =>
        {
            var distance = (double)distanceMethod.Invoke(repository, new object[] { centerLat, centerLng, l.Latitude, l.Longitude });
            return distance <= radiusKm;
        });
    }

    /// <summary>
    /// Gets the total distance traveled by a vehicle between two time points.
    /// </summary>
    public static async Task<double> GetTotalDistanceTraveledAsync(
        this LocationRepository repository,
        int vehicleId,
        DateTime startTime,
        DateTime endTime)
    {
        var locations = await repository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);

        if (locations.Count() < 2)
        {
            return 0;
        }

        double totalDistance = 0;
        Location? previousLocation = null;

        var distanceMethod = typeof(LocationRepository).GetMethod("CalculateDistance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var location in locations.OrderBy(l => l.RecordedAt))
        {
            if (previousLocation != null && location.Speed.HasValue)
            {
                var distance = (double)distanceMethod.Invoke(repository, new object[] {
                    previousLocation.Latitude,
                    previousLocation.Longitude,
                    location.Latitude,
                    location.Longitude
                });
                totalDistance += distance;
            }

            previousLocation = location;
        }

        return totalDistance;
    }

    /// <summary>
    /// Gets the average speed for a vehicle over a time period.
    /// </summary>
    public static async Task<double> GetAverageSpeedAsync(
        this LocationRepository repository,
        int vehicleId,
        DateTime startTime,
        DateTime endTime)
    {
        var stats = await repository.GetLocationStatsAsync(vehicleId, startTime, endTime);
        return stats.avgSpeed;
    }

    /// <summary>
    /// Gets locations filtered by type and time range.
    /// </summary>
    public static async Task<IEnumerable<Location>> GetLocationsByTypeAndTimeRangeAsync(
        this LocationRepository repository,
        LocationType locationType,
        DateTime startTime,
        DateTime endTime)
    {
        var locations = await repository.GetLocationsByTypeAsync(locationType);
        return locations.Where(l => l.RecordedAt >= startTime && l.RecordedAt <= endTime);
    }

}