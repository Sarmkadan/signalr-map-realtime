#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="LocationRepository"/> providing additional query capabilities
/// and convenience methods for location data operations.
/// </summary>
public static class LocationRepositoryExtensions
{
    /// <summary>
    /// Gets the latest location for each vehicle in a collection of vehicle IDs.
    /// </summary>
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <returns>Dictionary mapping vehicle IDs to their latest locations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    public static async Task<Dictionary<int, Location?>> GetLatestLocationsByVehiclesAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <returns>Collection of tuples containing vehicle ID and its latest location.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    public static async Task<IEnumerable<(int VehicleId, Location? Location)>> GetLatestLocationsWithVehicleDetailsAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);

        return vehicleIds.Distinct().Select(vehicleId =>
            (VehicleId: vehicleId, Location: repository.GetLatestLocationByVehicleAsync(vehicleId).GetAwaiter().GetResult()));
    }

    /// <summary>
    /// Gets locations for multiple vehicles within a time range.
    /// </summary>
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <param name="startTime">Start of time range (inclusive).</param>
    /// <param name="endTime">End of time range (inclusive).</param>
    /// <returns>Dictionary mapping vehicle IDs to their locations within the time range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="startTime"/> is after <paramref name="endTime"/>.</exception>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsByTimeRangeAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds,
        DateTime startTime,
        DateTime endTime)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime, nameof(startTime));

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="sessionIds">Collection of tracking session IDs to query.</param>
    /// <returns>Dictionary mapping session IDs to their locations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionIds"/> is <see langword="null"/>.</exception>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsBySessionsAsync(
        this LocationRepository repository,
        IEnumerable<int> sessionIds)
    {
        ArgumentNullException.ThrowIfNull(sessionIds);

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="locationType">Type of locations to filter by.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <returns>Dictionary mapping vehicle IDs to their filtered locations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsByTypeAndVehicleAsync(
        this LocationRepository repository,
        LocationType locationType,
        IEnumerable<int> vehicleIds)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <param name="hoursBack">Number of hours to look back from current time.</param>
    /// <returns>Dictionary mapping vehicle IDs to their recent locations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="hoursBack"/> is less than 1.</exception>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetRecentLocationsAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds,
        int hoursBack = 24)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);
        ArgumentOutOfRangeException.ThrowIfLessThan(hoursBack, 1, nameof(hoursBack));

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="centerLat">Latitude of center point.</param>
    /// <param name="centerLng">Longitude of center point.</param>
    /// <param name="radiusKm">Radius in kilometers to search within.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <returns>Dictionary mapping vehicle IDs to locations within the specified radius.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="radiusKm"/> is less than or equal to 0, or
    /// <paramref name="centerLat"/> is outside valid latitude range (-90 to 90), or
    /// <paramref name="centerLng"/> is outside valid longitude range (-180 to 180).
    /// </exception>
    public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsNearbyAsync(
        this LocationRepository repository,
        double centerLat,
        double centerLng,
        double radiusKm,
        IEnumerable<int> vehicleIds)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(radiusKm, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(centerLat, -90);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(centerLat, 90);
        ArgumentOutOfRangeException.ThrowIfLessThan(centerLng, -180);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(centerLng, 180);

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleIds">Collection of vehicle IDs to query.</param>
    /// <param name="startTime">Start of time range (inclusive).</param>
    /// <param name="endTime">End of time range (inclusive).</param>
    /// <returns>Dictionary mapping vehicle IDs to their location statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicleIds"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="startTime"/> is after <paramref name="endTime"/>.</exception>
    public static async Task<Dictionary<int, (int count, double minSpeed, double maxSpeed, double avgSpeed)>> GetLocationStatsAsync(
        this LocationRepository repository,
        IEnumerable<int> vehicleIds,
        DateTime startTime,
        DateTime endTime)
    {
        ArgumentNullException.ThrowIfNull(vehicleIds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime, nameof(startTime));

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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleId">ID of the vehicle to query.</param>
    /// <param name="startTime">Start of time range (inclusive).</param>
    /// <param name="endTime">End of time range (inclusive).</param>
    /// <param name="centerLat">Latitude of center point.</param>
    /// <param name="centerLng">Longitude of center point.</param>
    /// <param name="radiusKm">Radius in kilometers to search within.</param>
    /// <returns>Collection of locations within the specified time range and radius.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="vehicleId"/> is less than or equal to 0, or
    /// <paramref name="radiusKm"/> is less than or equal to 0, or
    /// <paramref name="centerLat"/> is outside valid latitude range (-90 to 90), or
    /// <paramref name="centerLng"/> is outside valid longitude range (-180 to 180).
    /// </exception>
    public static async Task<IEnumerable<Location>> GetLocationsInTimeRangeAndRadiusAsync(
        this LocationRepository repository,
        int vehicleId,
        DateTime startTime,
        DateTime endTime,
        double centerLat,
        double centerLng,
        double radiusKm)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime, nameof(startTime));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(radiusKm, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(centerLat, -90);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(centerLat, 90);
        ArgumentOutOfRangeException.ThrowIfLessThan(centerLng, -180);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(centerLng, 180);

        var locations = await repository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);

        var distanceMethod = typeof(LocationRepository).GetMethod(
            "CalculateDistance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (distanceMethod is null)
        {
            throw new InvalidOperationException("CalculateDistance method not found in LocationRepository.");
        }

        return locations.Where(l =>
        {
            var distance = (double)distanceMethod.Invoke(
                repository,
                new object[] { centerLat, centerLng, l.Latitude, l.Longitude });
            return distance <= radiusKm;
        });
    }

    /// <summary>
    /// Gets the total distance traveled by a vehicle between two time points.
    /// </summary>
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleId">ID of the vehicle to query.</param>
    /// <param name="startTime">Start of time range (inclusive).</param>
    /// <param name="endTime">End of time range (inclusive).</param>
    /// <returns>Total distance traveled in kilometers.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="vehicleId"/> is less than or equal to 0, or
    /// <paramref name="startTime"/> is after <paramref name="endTime"/>.
    /// </exception>
    public static async Task<double> GetTotalDistanceTraveledAsync(
        this LocationRepository repository,
        int vehicleId,
        DateTime startTime,
        DateTime endTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime, nameof(startTime));

        var locations = await repository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);

        if (locations.Count() < 2)
        {
            return 0;
        }

        double totalDistance = 0;
        Location? previousLocation = null;

        var distanceMethod = typeof(LocationRepository).GetMethod(
            "CalculateDistance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (distanceMethod is null)
        {
            throw new InvalidOperationException("CalculateDistance method not found in LocationRepository.");
        }

        foreach (var location in locations.OrderBy(l => l.RecordedAt))
        {
            if (previousLocation != null && location.Speed.HasValue)
            {
                var distance = (double)distanceMethod.Invoke(
                    repository,
                    new object[] {
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
    /// <param name="repository">The location repository instance.</param>
    /// <param name="vehicleId">ID of the vehicle to query.</param>
    /// <param name="startTime">Start of time range (inclusive).</param>
    /// <param name="endTime">End of time range (inclusive).</param>
    /// <returns>Average speed in km/h.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="vehicleId"/> is less than or equal to 0, or
    /// <paramref name="startTime"/> is after <paramref name="endTime"/>.
    /// </exception>
    public static async Task<double> GetAverageSpeedAsync(
        this LocationRepository repository,
        int vehicleId,
        DateTime startTime,
        DateTime endTime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime, nameof(startTime));

        var stats = await repository.GetLocationStatsAsync(vehicleId, startTime, endTime);
        return stats.avgSpeed;
    }

    /// <summary>
    /// Gets locations filtered by type and time range.
    /// </summary>
    /// <param name="repository">The location repository instance.</param>
    /// <param name="locationType">Type of locations to filter by.</param>
    /// <param name="startTime">Start of time range (inclusive).</param>
    /// <param name="endTime">End of time range (inclusive).</param>
    /// <returns>Collection of locations matching the criteria.</returns>
    /// <exception cref="ArgumentException"><paramref name="startTime"/> is after <paramref name="endTime"/>.</exception>
    public static async Task<IEnumerable<Location>> GetLocationsByTypeAndTimeRangeAsync(
        this LocationRepository repository,
        LocationType locationType,
        DateTime startTime,
        DateTime endTime)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, endTime, nameof(startTime));

        var locations = await repository.GetLocationsByTypeAsync(locationType);
        return locations.Where(l => l.RecordedAt >= startTime && l.RecordedAt <= endTime);
    }
}