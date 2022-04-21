#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Singleton service that enforces per-asset-type location update rate limits.
/// Maintains the last-accepted timestamp for each vehicle and gates new updates
/// against the configured minimum interval for the asset's type.
/// </summary>
public class LocationUpdateThrottler
{
    private readonly ConcurrentDictionary<int, DateTime> _lastUpdateTimes = new();
    private readonly ThrottleOptions _options;
    private readonly ILogger<LocationUpdateThrottler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LocationUpdateThrottler"/>.
    /// </summary>
    public LocationUpdateThrottler(IOptions<ThrottleOptions> options, ILogger<LocationUpdateThrottler> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Returns <c>true</c> when the update for <paramref name="vehicleId"/> should be
    /// suppressed because the configured minimum interval for <paramref name="assetType"/>
    /// has not yet elapsed since the last accepted update.
    /// Returns <c>false</c> and records the current timestamp when the update is accepted.
    /// </summary>
    public bool ShouldThrottle(int vehicleId, AssetType assetType)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Throttling disabled, accepting location update for vehicle {VehicleId}", vehicleId);
            return false;
        }

        var interval = _options.GetIntervalForAssetType(assetType);
        var now = DateTime.UtcNow;

        if (_lastUpdateTimes.TryGetValue(vehicleId, out var lastUpdate) && now - lastUpdate < interval)
        {
            _logger.LogDebug(
                "Location update throttled for vehicle {VehicleId} (asset type: {AssetType}). Last update was {LastUpdateSeconds} seconds ago, minimum interval is {IntervalSeconds} seconds",
                vehicleId,
                assetType,
                (now - lastUpdate).TotalSeconds,
                interval.TotalSeconds);
            return true;
        }

        _lastUpdateTimes[vehicleId] = now;
        _logger.LogDebug("Accepted location update for vehicle {VehicleId} (asset type: {AssetType})", vehicleId, assetType);
        return false;
    }

    /// <summary>
    /// Removes the throttle entry for <paramref name="vehicleId"/>, typically called
    /// when an asset is removed from tracking so stale state is not retained.
    /// </summary>
    public void Remove(int vehicleId)
    {
        _logger.LogDebug("Removing throttle entry for vehicle {VehicleId}", vehicleId);
        _lastUpdateTimes.TryRemove(vehicleId, out _);
    }
}
