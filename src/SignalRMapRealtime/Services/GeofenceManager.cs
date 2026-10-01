#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Events;

/// <summary>
/// High-level geofence lifecycle manager that coordinates zone registration,
/// bulk evaluation, and expiration across the system.
/// </summary>
public sealed class GeofenceManager : IDisposable
{
    private readonly IGeofenceService _geofenceService;
    private readonly ILogger<GeofenceManager> _logger;
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _zoneExpiry = new();
    private readonly Timer _expiryTimer;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="GeofenceManager"/> class.</summary>
    /// <param name="geofenceService">Underlying geofence service for zone operations.</param>
    /// <param name="logger">Logger instance.</param>
    public GeofenceManager(IGeofenceService geofenceService, ILogger<GeofenceManager> logger)
    {
        _geofenceService = geofenceService ?? throw new ArgumentNullException(nameof(geofenceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _expiryTimer = new Timer(PurgeExpiredZones, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Registers a geofence zone with an optional time-to-live.
    /// When <paramref name="ttl"/> is provided the zone is automatically removed after expiration.
    /// </summary>
    /// <param name="dto">Zone creation parameters.</param>
    /// <param name="ttl">Optional duration after which the zone expires.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered zone DTO.</returns>
    public async Task<GeofenceDto> RegisterWithExpiryAsync(
        CreateGeofenceDto dto,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var zone = await _geofenceService.RegisterZoneAsync(dto, cancellationToken);

        if (ttl.HasValue)
        {
            _zoneExpiry[zone.Id] = DateTimeOffset.UtcNow.Add(ttl.Value);
            _logger.LogInformation("Zone {ZoneId} registered with TTL {Ttl}", zone.Id, ttl.Value);
        }
        else
        {
            _logger.LogInformation("Zone {ZoneId} registered without expiry", zone.Id);
        }

        return zone;
    }

    /// <summary>
    /// Evaluates a batch of vehicle positions against all active geofence zones.
    /// </summary>
    /// <param name="positions">Collection of vehicle-id to coordinate pairs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping vehicle IDs to their geofence alert lists.</returns>
    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<GeofenceAlertDto>>> EvaluateBatchAsync(
        IEnumerable<(Guid VehicleId, double Latitude, double Longitude)> positions,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentDictionary<Guid, IReadOnlyList<GeofenceAlertDto>>();

        var tasks = positions.Select(async pos =>
        {
            var alerts = await _geofenceService.CheckLocationAsync(pos.VehicleId, pos.Latitude, pos.Longitude, cancellationToken);
            results[pos.VehicleId] = alerts;
        });

        await Task.WhenAll(tasks);

        _logger.LogDebug("Batch evaluation completed for {Count} vehicles", results.Count);
        return results;
    }

    /// <summary>
    /// Returns zones that are currently active and have not expired.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of non-expired active zones.</returns>
    public async Task<IReadOnlyList<GeofenceDto>> GetNonExpiredZonesAsync(CancellationToken cancellationToken = default)
    {
        var zones = await _geofenceService.GetActiveZonesAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        return zones
            .Where(z => !_zoneExpiry.TryGetValue(z.Id, out var expiry) || expiry > now)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Removes all zones and clears the expiry cache.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of zones removed.</returns>
    public async Task<int> RemoveAllZonesAsync(CancellationToken cancellationToken = default)
    {
        var zones = await _geofenceService.GetActiveZonesAsync(cancellationToken);
        var count = 0;

        foreach (var zone in zones)
        {
            if (await _geofenceService.RemoveZoneAsync(zone.Id, cancellationToken))
            {
                _zoneExpiry.TryRemove(zone.Id, out _);
                count++;
            }
        }

        _logger.LogInformation("Removed {Count} geofence zones", count);
        return count;
    }

    private void PurgeExpiredZones(object? state)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = _zoneExpiry.Where(kv => kv.Value <= now).Select(kv => kv.Key).ToList();

        foreach (var zoneId in expired)
        {
            _zoneExpiry.TryRemove(zoneId, out _);
            _ = _geofenceService.RemoveZoneAsync(zoneId);
            _logger.LogInformation("Auto-expired geofence zone {ZoneId}", zoneId);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _expiryTimer.Dispose();
        _zoneExpiry.Clear();
        _disposed = true;
    }
}
