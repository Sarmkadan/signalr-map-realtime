#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.DTOs;

/// <summary>
/// In-memory tracker that maintains the latest known position for each vehicle
/// and provides spatial queries over the current snapshot.
/// </summary>
public sealed class LocationTracker : IDisposable
{
    private readonly ConcurrentDictionary<string, TrackedPosition> _positions = new();
    private readonly ILogger<LocationTracker> _logger;
    private readonly Timer _staleCheckTimer;
    private readonly TimeSpan _staleThreshold = TimeSpan.FromMinutes(10);
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="LocationTracker"/> class.</summary>
    /// <param name="logger">Logger instance.</param>
    public LocationTracker(ILogger<LocationTracker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _staleCheckTimer = new Timer(EvictStaleEntries, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
    }

    /// <summary>
    /// Updates or inserts the latest position for a vehicle.
    /// </summary>
    /// <param name="vehicleId">Vehicle identifier.</param>
    /// <param name="latitude">WGS-84 latitude in degrees.</param>
    /// <param name="longitude">WGS-84 longitude in degrees.</param>
    /// <param name="heading">Optional heading in degrees (0-360).</param>
    /// <param name="speed">Optional speed in km/h.</param>
    public void UpdatePosition(string vehicleId, double latitude, double longitude, double? heading = null, double? speed = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vehicleId);

        var position = new TrackedPosition(latitude, longitude, heading, speed, DateTimeOffset.UtcNow);
        _positions.AddOrUpdate(vehicleId, position, (_, _) => position);

        _logger.LogTrace("Position updated for {VehicleId}: ({Lat}, {Lon})", vehicleId, latitude, longitude);
    }

    /// <summary>
    /// Returns the latest known position for a specific vehicle, or <c>null</c> if unknown.
    /// </summary>
    /// <param name="vehicleId">Vehicle identifier.</param>
    /// <returns>The tracked position or <c>null</c>.</returns>
    public TrackedPosition? GetPosition(string vehicleId)
    {
        _positions.TryGetValue(vehicleId, out var position);
        return position;
    }

    /// <summary>
    /// Returns all vehicles whose latest position falls within a bounding box.
    /// </summary>
    /// <param name="southWestLat">South-west corner latitude.</param>
    /// <param name="southWestLon">South-west corner longitude.</param>
    /// <param name="northEastLat">North-east corner latitude.</param>
    /// <param name="northEastLon">North-east corner longitude.</param>
    /// <returns>Dictionary of vehicle IDs to their positions within the bounds.</returns>
    public IReadOnlyDictionary<string, TrackedPosition> GetPositionsInBounds(
        double southWestLat, double southWestLon,
        double northEastLat, double northEastLon)
    {
        return _positions
            .Where(kv =>
                kv.Value.Latitude >= southWestLat && kv.Value.Latitude <= northEastLat &&
                kv.Value.Longitude >= southWestLon && kv.Value.Longitude <= northEastLon)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    /// <summary>
    /// Returns the total number of currently tracked vehicles.
    /// </summary>
    public int TrackedCount => _positions.Count;

    /// <summary>
    /// Calculates the Haversine distance in meters between two tracked vehicles.
    /// Returns <c>null</c> if either vehicle is not currently tracked.
    /// </summary>
    /// <param name="vehicleA">First vehicle identifier.</param>
    /// <param name="vehicleB">Second vehicle identifier.</param>
    /// <returns>Distance in meters, or <c>null</c>.</returns>
    public double? DistanceBetween(string vehicleA, string vehicleB)
    {
        if (!_positions.TryGetValue(vehicleA, out var a) || !_positions.TryGetValue(vehicleB, out var b))
            return null;

        const double R = 6_371_000; // Earth radius in meters
        var dLat = ToRadians(b.Latitude - a.Latitude);
        var dLon = ToRadians(b.Longitude - a.Longitude);
        var sinLat = Math.Sin(dLat / 2);
        var sinLon = Math.Sin(dLon / 2);
        var h = sinLat * sinLat + Math.Cos(ToRadians(a.Latitude)) * Math.Cos(ToRadians(b.Latitude)) * sinLon * sinLon;
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private void EvictStaleEntries(object? state)
    {
        var cutoff = DateTimeOffset.UtcNow - _staleThreshold;
        var stale = _positions.Where(kv => kv.Value.Timestamp < cutoff).Select(kv => kv.Key).ToList();

        foreach (var key in stale)
        {
            _positions.TryRemove(key, out _);
            _logger.LogInformation("Evicted stale position for vehicle {VehicleId}", key);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _staleCheckTimer.Dispose();
        _positions.Clear();
        _disposed = true;
    }
}

/// <summary>Immutable snapshot of a vehicle's tracked position.</summary>
/// <param name="Latitude">WGS-84 latitude in degrees.</param>
/// <param name="Longitude">WGS-84 longitude in degrees.</param>
/// <param name="Heading">Optional heading in degrees (0-360).</param>
/// <param name="Speed">Optional speed in km/h.</param>
/// <param name="Timestamp">UTC time when this position was recorded.</param>
public sealed record TrackedPosition(double Latitude, double Longitude, double? Heading, double? Speed, DateTimeOffset Timestamp);
