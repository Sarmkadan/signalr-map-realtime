#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Hubs;

using Microsoft.AspNetCore.SignalR;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Constants;
using SignalRMapRealtime.Events;
using Microsoft.AspNetCore.Authorization; // Hotfix: Added for [Authorize] attribute
using SignalRMapRealtime.Authentication; // Hotfix: Added for ApiKeyAuthenticationOptions

/// <summary>
/// SignalR hub for real-time location tracking and vehicle updates.
/// Clients connect to receive live location updates, vehicle status changes, and route progress.
/// </summary>
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme)] // Hotfix: Require API Key authentication for this hub
public class LocationHub : Hub
{
    private readonly ILocationService _locationService;
    private readonly IVehicleService _vehicleService;
    private readonly ITrackingService _trackingService;
    private readonly RouteRepository _routeRepository;
    private readonly LocationUpdateThrottler _throttler;
    private readonly ILogger<LocationHub> _logger;

    /// <summary>
    /// Initializes a new instance of LocationHub.
    /// </summary>
    public LocationHub(ILocationService locationService, IVehicleService vehicleService, ITrackingService trackingService, RouteRepository routeRepository, LocationUpdateThrottler throttler, ILogger<LocationHub> logger)
    {
        ArgumentNullException.ThrowIfNull(locationService);
        ArgumentNullException.ThrowIfNull(vehicleService);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(routeRepository);
        ArgumentNullException.ThrowIfNull(throttler);
        ArgumentNullException.ThrowIfNull(logger);
        _locationService = locationService;
        _vehicleService = vehicleService;
        _trackingService = trackingService;
        _routeRepository = routeRepository;
        _throttler = throttler;
        _logger = logger;
    }

    /// <summary>
    /// Called when a new client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client {Context.ConnectionId} disconnected. Exception: {exception?.Message}");
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    /// <summary>
    /// Receives and processes a location update for a vehicle in real-time.
    /// Updates are subject to per-asset-type throttling and coalescing configured in LocationThrottle settings.
    /// </summary>
    /// <param name="locationDto">The location data to record and broadcast.</param>
    /// <exception cref="ArgumentNullException">Thrown when locationDto is null.</exception>
    public async Task SendLocationUpdate(CreateLocationDto locationDto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(locationDto);

            // Get vehicle to determine asset type for throttling configuration
            var vehicle = await _vehicleService.GetVehicleAsync(locationDto.VehicleId).ConfigureAwait(false);
            if (vehicle is null)
            {
                _logger.LogWarning("Vehicle {VehicleId} not found, cannot process location update", locationDto.VehicleId);
                await Clients.Caller.SendAsync("Error", "Vehicle not found").ConfigureAwait(false);
                return;
            }

            // Record the location in the database
            var location = await _locationService.RecordLocationAsync(locationDto).ConfigureAwait(false);

            // Add to coalescing buffer - this will handle throttling and batching
            var buffered = _throttler.AddToBuffer(location, vehicle.AssetType);

            if (!buffered)
            {
                _logger.LogDebug("Location update not buffered for vehicle {VehicleId} (throttled or disabled)", locationDto.VehicleId);
                return;
            }

            _logger.LogInformation("Location buffered for vehicle {VehicleId} (asset type: {AssetType})", locationDto.VehicleId, vehicle.AssetType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing location update for vehicle {VehicleId}: {Message}", locationDto.VehicleId, ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to process location update").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Broadcasts an <c>AssetRemoved</c> event to all clients and cleans up associated state
    /// so that clients can remove the corresponding Leaflet marker and its event listeners.
    /// </summary>
    /// <param name="vehicleId">ID of the asset/vehicle being removed from tracking.</param>
    public async Task NotifyAssetRemoved(int vehicleId)
    {
        try
        {
            _throttler.Remove(vehicleId);
            await Clients.Group($"vehicle-{vehicleId}").SendAsync("AssetRemoved", vehicleId).ConfigureAwait(false);
            _logger.LogInformation("Asset removed notification sent for vehicle {VehicleId}", vehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error notifying asset removal for vehicle {VehicleId}: {Message}", vehicleId, ex.Message);
        }
    }

    /// <summary>
    /// Sends a full snapshot of the latest known positions for all online vehicles to the calling client.
    /// Clients should invoke this method on reconnect to resync stale Leaflet map markers.
    /// </summary>
    public async Task RequestAllVehicleLocations()
    {
        try
        {
            var vehicles = await _vehicleService.GetOnlineVehiclesAsync().ConfigureAwait(false);
            var snapshot = vehicles
                .Where(v => v.LastLocation is not null)
                .Select(v => v.LastLocation!)
                .ToList();

            await Clients.Caller.SendAsync("PositionSnapshot", snapshot).ConfigureAwait(false);
            _logger.LogInformation("Position snapshot ({Count} vehicles) sent to reconnected client {ConnectionId}", snapshot.Count, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error sending position snapshot: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to retrieve position snapshot").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Notifies all clients that a vehicle's status has changed.
    /// </summary>
    public async Task BroadcastVehicleStatusChange(int vehicleId, string newStatus)
    {
        try
        {
            await Clients.Group($"vehicle-{vehicleId}").SendAsync("VehicleStatusChanged", new { vehicleId, newStatus, timestamp = DateTime.UtcNow }).ConfigureAwait(false);
            _logger.LogInformation("Vehicle {VehicleId} status changed to {NewStatus}", vehicleId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error broadcasting vehicle status: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Subscribes a client to real-time updates for a specific vehicle.
    /// </summary>
    public async Task SubscribeToVehicle(int vehicleId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId}").ConfigureAwait(false);
            await Clients.Caller.SendAsync("SubscribedToVehicle", vehicleId).ConfigureAwait(false);
            _logger.LogInformation("Client {ConnectionId} subscribed to vehicle {VehicleId}", Context.ConnectionId, vehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error subscribing to vehicle: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to vehicle").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Unsubscribes a client from real-time updates for a specific vehicle.
    /// </summary>
    public async Task UnsubscribeFromVehicle(int vehicleId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId}").ConfigureAwait(false);
            await Clients.Caller.SendAsync("UnsubscribedFromVehicle", vehicleId).ConfigureAwait(false);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from vehicle {VehicleId}", Context.ConnectionId, vehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error unsubscribing from vehicle: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to unsubscribe from vehicle").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Subscribes a client to real-time updates for multiple vehicles at once.
    /// This is useful for map viewports showing multiple vehicles or fleet dashboards.
    /// </summary>
    /// <param name="ids">Array of vehicle IDs to subscribe to.</param>
    /// <exception cref="ArgumentNullException">Thrown when ids is null.</exception>
    /// <exception cref="ArgumentException">Thrown when ids array is empty or contains invalid vehicle IDs.</exception>
    public async Task SubscribeToVehicles(int[] ids)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(ids);

            if (ids.Length == 0)
            {
                _logger.LogWarning("Empty vehicle IDs array provided for subscription");
                await Clients.Caller.SendAsync("Error", "No vehicle IDs provided").ConfigureAwait(false);
                return;
            }

            foreach (var vehicleId in ids)
            {
                if (vehicleId <= 0)
                {
                    _logger.LogWarning("Invalid vehicle ID {VehicleId} in subscription request", vehicleId);
                    continue;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId}").ConfigureAwait(false);
                _logger.LogInformation("Client {ConnectionId} subscribed to vehicle {VehicleId}", Context.ConnectionId, vehicleId);
            }

            await Clients.Caller.SendAsync("SubscribedToVehicles", ids).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to vehicles");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to vehicles").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Requests the latest location for a vehicle from the client.
    /// </summary>
    public async Task RequestVehicleLocation(int vehicleId)
    {
        try
        {
            var location = await _locationService.GetLatestLocationAsync(vehicleId).ConfigureAwait(false);
            await Clients.Caller.SendAsync("VehicleLocation", location).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error requesting vehicle location: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to retrieve vehicle location").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Broadcasts route progress updates to subscribed clients.
    /// </summary>
    public async Task BroadcastRouteProgress(int routeId, int completionPercentage, string status)
    {
        try
        {
            // Get the route to determine which vehicle to notify
            var route = await _routeRepository.GetRouteWithDetailsAsync(routeId).ConfigureAwait(false);
            var vehicleId = route?.VehicleId ?? 0;

            if (vehicleId > 0)
            {
                await Clients.Group($"vehicle-{vehicleId}").SendAsync("RouteProgressUpdated", new { routeId, completionPercentage, status, timestamp = DateTime.UtcNow }).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("Route {RouteId} has no associated vehicle, cannot send progress update", routeId);
            }

            _logger.LogInformation("Route {RouteId} progress: {CompletionPercentage}%", routeId, completionPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error broadcasting route progress: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Notifies clients of an alert (speed violation, fuel low, etc.).
    /// </summary>
    public async Task SendAlert(int vehicleId, string alertType, string message)
    {
        try
        {
            var alert = new { vehicleId, alertType, message, timestamp = DateTime.UtcNow };
            await Clients.Group($"vehicle-{vehicleId}").SendAsync("Alert", alert).ConfigureAwait(false);
            _logger.LogWarning("Alert for vehicle {VehicleId}: {AlertType} - {Message}", vehicleId, alertType, message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error sending alert: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Requests online vehicle count from the service.
    /// </summary>
    public async Task RequestOnlineVehicleCount()
    {
        try
        {
            var count = await _vehicleService.GetOnlineVehicleCountAsync().ConfigureAwait(false);
            await Clients.Caller.SendAsync("OnlineVehicleCount", count).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error requesting online vehicle count: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to retrieve vehicle count").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Broadcasts when all vehicles in a group come online.
    /// </summary>
    public async Task BroadcastFleetStatus(string fleetName, int onlineCount, int totalCount)
    {
        try
        {
            var status = new { fleetName, onlineCount, totalCount, percentage = totalCount > 0 ? (onlineCount * 100) / totalCount : 0, timestamp = DateTime.UtcNow };
            await Clients.Group($"fleet-{fleetName}").SendAsync("FleetStatusUpdated", status).ConfigureAwait(false);
            _logger.LogInformation("Fleet {FleetName} status: {OnlineCount}/{TotalCount} online", fleetName, onlineCount, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error broadcasting fleet status: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Handles a heartbeat ping to maintain connection.
    /// </summary>
    public async Task Ping()
    {
        try
        {
            await Clients.Caller.SendAsync("Pong").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error handling ping: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Subscribes a client to real-time updates for a specific fleet.
    /// </summary>
    public async Task SubscribeToFleet(string fleetName)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"fleet-{fleetName}").ConfigureAwait(false);
            await Clients.Caller.SendAsync("SubscribedToFleet", fleetName).ConfigureAwait(false);
            _logger.LogInformation("Client {ConnectionId} subscribed to fleet {FleetName}", Context.ConnectionId, fleetName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error subscribing to fleet: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to fleet").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Unsubscribes a client from real-time updates for a specific fleet.
    /// </summary>
    public async Task UnsubscribeFromFleet(string fleetName)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"fleet-{fleetName}").ConfigureAwait(false);
            await Clients.Caller.SendAsync("UnsubscribedFromFleet", fleetName).ConfigureAwait(false);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from fleet {FleetName}", Context.ConnectionId, fleetName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error unsubscribing from fleet: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Broadcasts a vehicle stale event to all clients subscribed to a specific vehicle.
    /// Clients can use this to gray-out or hide the vehicle marker on the map.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to broadcast to.</param>
    /// <param name="vehicleRegistration">The vehicle registration number.</param>
    /// <param name="staleSince">When the vehicle became stale (UTC).</param>
    /// <param name="staleWindowMinutes">The stale detection window in minutes.</param>
    /// <param name="timeSinceLastUpdateMinutes">Time elapsed since last update when vehicle was marked stale (in minutes).</param>
    /// <exception cref="ArgumentException">Thrown when vehicleId is less than or equal to 0.</exception>
    public async Task NotifyVehicleStaleAsync(int vehicleId, string vehicleRegistration, DateTime staleSince, int staleWindowMinutes, double timeSinceLastUpdateMinutes)
    {
        try
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);
            ArgumentException.ThrowIfNullOrEmpty(vehicleRegistration);

            var @event = new VehicleStaleEvent
            {
                VehicleId = vehicleId,
                VehicleRegistration = vehicleRegistration,
                VehicleName = vehicleRegistration, // Use registration as name for simplicity
                LastUpdateTime = staleSince.AddMinutes(-timeSinceLastUpdateMinutes),
                StaleSince = staleSince,
                StaleWindowMinutes = staleWindowMinutes,
                TimeSinceLastUpdateMinutes = timeSinceLastUpdateMinutes,
                IsRecovery = false,
                WasPreviouslyStale = false
            };

            await Clients.Group($"vehicle-{vehicleId}").SendAsync("VehicleStale", @event).ConfigureAwait(false);
            _logger.LogInformation("Vehicle stale notification sent for vehicle {VehicleId}", vehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying vehicle stale for vehicle {VehicleId}: {Message}", vehicleId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Broadcasts a vehicle active event to all clients subscribed to a specific vehicle.
    /// Clients can use this to restore or show the vehicle marker on the map.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to broadcast to.</param>
    /// <param name="vehicleRegistration">The vehicle registration number.</param>
    /// <param name="recoveryTime">When the vehicle became active again (UTC).</param>
    /// <param name="staleWindowMinutes">The stale detection window in minutes.</param>
    /// <param name="timeInStaleStateMinutes">Time the vehicle spent in stale state (in minutes).</param>
    /// <exception cref="ArgumentException">Thrown when vehicleId is less than or equal to 0.</exception>
    public async Task NotifyVehicleActiveAsync(int vehicleId, string vehicleRegistration, DateTime recoveryTime, int staleWindowMinutes, double timeInStaleStateMinutes)
    {
        try
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vehicleId, 0);
            ArgumentException.ThrowIfNullOrEmpty(vehicleRegistration);

            var @event = new VehicleActiveEvent
            {
                VehicleId = vehicleId,
                VehicleRegistration = vehicleRegistration,
                VehicleName = vehicleRegistration, // Use registration as name for simplicity
                RecoveryTime = recoveryTime,
                StaleWindowMinutes = staleWindowMinutes,
                TimeInStaleStateMinutes = timeInStaleStateMinutes
            };

            await Clients.Group($"vehicle-{vehicleId}").SendAsync("VehicleActive", @event).ConfigureAwait(false);
            _logger.LogInformation("Vehicle active notification sent for vehicle {VehicleId}", vehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying vehicle active for vehicle {VehicleId}: {Message}", vehicleId, ex.Message);
            throw;
        }
    }
}
