#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Hubs;

/// <summary>
/// Singleton service that enforces per-asset-type location update rate limits and coalesces
/// high-frequency location broadcasts into batched updates.
///
/// <para>
/// Maintains the last-accepted timestamp for each vehicle and gates new updates against the
/// configured minimum interval for the asset's type. Additionally, buffers incoming location updates
/// and flushes them periodically (every 250-500ms) to reduce SignalR traffic while maintaining
/// real-time responsiveness.
/// </para>
/// </summary>
public class LocationUpdateThrottler : IDisposable
{
    private readonly ConcurrentDictionary<int, DateTime> _lastUpdateTimes = new();
    private readonly ConcurrentDictionary<int, Channel<LocationDto>> _pendingUpdates = new();
    private readonly ConcurrentDictionary<int, Task> _flushTasks = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly ThrottleOptions _options;
    private readonly ILogger<LocationUpdateThrottler> _logger;
    private readonly IHubContext<LocationHub> _hubContext;
    private readonly TimeSpan _flushInterval;

    /// <summary>
    /// Initializes a new instance of <see cref="LocationUpdateThrottler"/>.
    /// </summary>
    /// <param name="options">Throttle configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="hubContext">SignalR hub context for broadcasting coalesced updates.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public LocationUpdateThrottler(IOptions<ThrottleOptions> options, ILogger<LocationUpdateThrottler> logger, IHubContext<LocationHub> hubContext)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(hubContext);
        _options = options.Value;
        _logger = logger;
        _hubContext = hubContext;
        _flushInterval = TimeSpan.FromMilliseconds(_options.CoalesceFlushIntervalMilliseconds);

        // Start background flushing task
        _ = FlushPendingUpdatesLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Returns <c>true</c> when the update for <paramref name="vehicleId"/> should be
    /// suppressed because the configured minimum interval for <paramref name="assetType"/>
    /// has not yet elapsed since the last accepted update.
    /// Returns <c>false</c> and records the current timestamp when the update is accepted.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to check.</param>
    /// <param name="assetType">The asset type for interval configuration.</param>
    /// <returns><c>true</c> if throttled; <c>false</c> if accepted.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when vehicleId is invalid.</exception>
    public bool ShouldThrottle(int vehicleId, AssetType assetType)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);

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
    /// Adds a location update to the coalescing buffer for the specified vehicle.
    /// The update will be held until either:
    /// <list type="bullet">
    /// <item>The flush interval elapses (250-500ms)</item>
    /// <item>The buffer reaches its maximum capacity (100 updates)</item>
    /// <item>A higher priority immediate flush is requested</item>
    /// </list>
    /// </summary>
    /// <param name="locationDto">The location update to buffer.</param>
    /// <param name="assetType">The type of asset for throttling configuration.</param>
    /// <returns><c>true</c> if the update was buffered; <c>false</c> if throttled or rejected.</returns>
    /// <exception cref="ArgumentNullException">Thrown when locationDto is null.</exception>
    public bool AddToBuffer(LocationDto locationDto, AssetType assetType)
    {
        ArgumentNullException.ThrowIfNull(locationDto);

        if (!_options.Enabled)
        {
            return false;
        }

        // Check if this update should be throttled based on minimum interval
        if (ShouldThrottle(locationDto.VehicleId, assetType))
        {
            return false;
        }

        try
        {
            var channel = _pendingUpdates.GetOrAdd(
                locationDto.VehicleId,
                _ => Channel.CreateBounded<LocationDto>(new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = true
                })
            );

            // Try to write to the channel, drop oldest if full
            if (channel.Writer.TryWrite(locationDto))
            {
                _logger.LogDebug("Buffered location update for vehicle {VehicleId} (queue size: {QueueSize})",
                    locationDto.VehicleId, channel.Reader.Count + 1);
                return true;
            }

            _logger.LogWarning("Failed to buffer location update for vehicle {VehicleId} - channel full", locationDto.VehicleId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buffering location update for vehicle {VehicleId}", locationDto.VehicleId);
            return false;
        }
    }

    /// <summary>
    /// Flushes all pending updates for a specific vehicle immediately.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to flush.</param>
    /// <returns>The number of flushed updates, or 0 if none pending.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when vehicleId is invalid.</exception>
    public async Task<int> FlushImmediatelyAsync(int vehicleId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);

        if (!_pendingUpdates.TryGetValue(vehicleId, out var channel) || channel.Reader.Count == 0)
        {
            return 0;
        }

        return await FlushChannelAsync(channel, vehicleId, forceFlush: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the throttle entry for <paramref name="vehicleId"/>, typically called
    /// when an asset is removed from tracking so stale state is not retained.
    /// Also cancels any pending flush operations for this vehicle.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when vehicleId is invalid.</exception>
    public void Remove(int vehicleId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);

        _logger.LogDebug("Removing throttle entry for vehicle {VehicleId}", vehicleId);
        _lastUpdateTimes.TryRemove(vehicleId, out _);
        _pendingUpdates.TryRemove(vehicleId, out _);
        _flushTasks.TryRemove(vehicleId, out _);
    }

    /// <summary>
    /// Background loop that periodically flushes pending location updates.
    /// </summary>
    private async Task FlushPendingUpdatesLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_flushInterval, cancellationToken).ConfigureAwait(false);

                if (_pendingUpdates.IsEmpty)
                {
                    continue;
                }

                var tasks = new List<Task<int>>();
                foreach (var kvp in _pendingUpdates)
                {
                    tasks.Add(FlushChannelAsync(kvp.Value, kvp.Key, forceFlush: false));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Flush loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in flush loop");
        }
    }

    /// <summary>
    /// Flushes updates from a single channel and returns the count of flushed updates.
    /// </summary>
    private async Task<int> FlushChannelAsync(Channel<LocationDto> channel, int vehicleId, bool forceFlush)
    {
        if (channel.Reader.Count == 0)
        {
            return 0;
        }

        var updates = new List<LocationDto>();
        var count = 0;

        try
        {
            // Read all available updates from the channel
            await foreach (var update in channel.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                updates.Add(update);
                count++;
            }

            if (count == 0)
            {
                return 0;
            }

            // If we have multiple updates, only keep the latest one (coalesce)
            var locationToSend = updates.Count > 1
                ? updates[^1] // Get the last (most recent) update
                : updates[0];

            _logger.LogDebug("Flushing {Count} buffered location updates for vehicle {VehicleId} (sending latest)", count, vehicleId);

            // Broadcast the coalesced update to all subscribers of this vehicle
            await _hubContext.Clients.Group($"vehicle-{vehicleId}")
                .SendAsync("LocationUpdated", locationToSend)
                .ConfigureAwait(false);

            // Also notify for real-time vehicle location updates
            await _hubContext.Clients.Group($"vehicle-{vehicleId}")
                .SendAsync("VehicleLocationUpdated", locationToSend)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing channel for vehicle {VehicleId}", vehicleId);
        }
        finally
        {
            // Clear the channel after flushing
            // Note: Channel doesn't have a clear method, so we just remove it
            // New updates will create a new channel
            _pendingUpdates.TryRemove(vehicleId, out _);
        }

        return count;
    }

    /// <summary>
    /// Disposes the throttler and cancels all background operations.
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}