#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Hubs;

using Microsoft.AspNetCore.SignalR;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Constants;
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
    private readonly LocationUpdateThrottler _throttler;
    private readonly ILogger<LocationHub> _logger;

    /// <summary>
    /// Initializes a new instance of LocationHub.
    /// </summary>
    public LocationHub(ILocationService locationService, IVehicleService vehicleService, ITrackingService trackingService, LocationUpdateThrottler throttler, ILogger<LocationHub> logger)
    {
        ArgumentNullException.ThrowIfNull(locationService);
        ArgumentNullException.ThrowIfNull(vehicleService);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(throttler);
        ArgumentNullException.ThrowIfNull(logger);
        _locationService = locationService;
        _vehicleService = vehicleService;
        _trackingService = trackingService;
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
    /// Receives and broadcasts a location update for a vehicle in real-time.
    /// Updates are subject to per-asset-type throttling configured in LocationThrottle settings.
    /// </summary>
    public async Task SendLocationUpdate(CreateLocationDto locationDto)
    {
        try
        {
            // Apply per-asset-type throttle to reduce unnecessary SignalR traffic.
            var vehicle = await _vehicleService.GetVehicleAsync(locationDto.VehicleId).ConfigureAwait(false);
            if (vehicle is not null && _throttler.ShouldThrottle(locationDto.VehicleId, vehicle.AssetType))
            {
                _logger.LogDebug("Location update throttled for vehicle {VehicleId} (asset type: {AssetType})", locationDto.VehicleId, vehicle.AssetType);
                return;
            }

            var location = await _locationService.RecordLocationAsync(locationDto).ConfigureAwait(false);

            // Broadcast to all connected clients
            await Clients.All.SendAsync("LocationUpdated", location).ConfigureAwait(false);

            // Notify vehicle-specific listeners
            await Clients.Group($"vehicle-{locationDto.VehicleId}").SendAsync("VehicleLocationUpdated", location).ConfigureAwait(false);

            _logger.LogInformation("Location updated for vehicle {VehicleId}", locationDto.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error sending location update: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Failed to update location").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Notifies all clients that a vehicle's status has changed.
    /// </summary>
    public async Task BroadcastVehicleStatusChange(int vehicleId, string newStatus)
    {
        try
        {
            await Clients.All.SendAsync("VehicleStatusChanged", new { vehicleId, newStatus, timestamp = DateTime.UtcNow }).ConfigureAwait(false);
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
            await Clients.All.SendAsync("RouteProgressUpdated", new { routeId, completionPercentage, status, timestamp = DateTime.UtcNow }).ConfigureAwait(false);
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
            await Clients.All.SendAsync("Alert", alert).ConfigureAwait(false);
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
            var status = new { fleetName, onlineCount, totalCount, percentage = (onlineCount * 100) / totalCount, timestamp = DateTime.UtcNow };
            await Clients.All.SendAsync("FleetStatusUpdated", status).ConfigureAwait(false);
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
}
