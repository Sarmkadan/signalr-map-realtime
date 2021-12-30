// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Hubs;

using Microsoft.AspNetCore.SignalR;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Constants;

/// <summary>
/// SignalR hub for real-time location tracking and vehicle updates.
/// Clients connect to receive live location updates, vehicle status changes, and route progress.
/// </summary>
public class LocationHub : Hub
{
    private readonly ILocationService _locationService;
    private readonly IVehicleService _vehicleService;
    private readonly ITrackingService _trackingService;
    private readonly ILogger<LocationHub> _logger;

    /// <summary>
    /// Initializes a new instance of LocationHub.
    /// </summary>
    public LocationHub(ILocationService locationService, IVehicleService vehicleService, ITrackingService trackingService, ILogger<LocationHub> logger)
    {
        ArgumentNullException.ThrowIfNull(locationService);
        ArgumentNullException.ThrowIfNull(vehicleService);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(logger);
        _locationService = locationService;
        _vehicleService = vehicleService;
        _trackingService = trackingService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a new client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client {Context.ConnectionId} connected");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client {Context.ConnectionId} disconnected. Exception: {exception?.Message}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Receives and broadcasts a location update for a vehicle in real-time.
    /// </summary>
    public async Task SendLocationUpdate(CreateLocationDto locationDto)
    {
        try
        {
            var location = await _locationService.RecordLocationAsync(locationDto);

            // Broadcast to all connected clients
            await Clients.All.SendAsync("LocationUpdated", location);

            // Notify vehicle-specific listeners
            await Clients.Group($"vehicle-{locationDto.VehicleId}").SendAsync("VehicleLocationUpdated", location);

            _logger.LogInformation($"Location updated for vehicle {locationDto.VehicleId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending location update: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to update location");
        }
    }

    /// <summary>
    /// Notifies all clients that a vehicle's status has changed.
    /// </summary>
    public async Task BroadcastVehicleStatusChange(int vehicleId, string newStatus)
    {
        try
        {
            await Clients.All.SendAsync("VehicleStatusChanged", new { vehicleId, newStatus, timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Vehicle {vehicleId} status changed to {newStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error broadcasting vehicle status: {ex.Message}");
        }
    }

    /// <summary>
    /// Subscribes a client to real-time updates for a specific vehicle.
    /// </summary>
    public async Task SubscribeToVehicle(int vehicleId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId}");
            await Clients.Caller.SendAsync("SubscribedToVehicle", vehicleId);
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to vehicle {vehicleId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error subscribing to vehicle: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to vehicle");
        }
    }

    /// <summary>
    /// Unsubscribes a client from real-time updates for a specific vehicle.
    /// </summary>
    public async Task UnsubscribeFromVehicle(int vehicleId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId}");
            await Clients.Caller.SendAsync("UnsubscribedFromVehicle", vehicleId);
            _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from vehicle {vehicleId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unsubscribing from vehicle: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to unsubscribe from vehicle");
        }
    }

    /// <summary>
    /// Requests the latest location for a vehicle from the client.
    /// </summary>
    public async Task RequestVehicleLocation(int vehicleId)
    {
        try
        {
            var location = await _locationService.GetLatestLocationAsync(vehicleId);
            await Clients.Caller.SendAsync("VehicleLocation", location);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error requesting vehicle location: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to retrieve vehicle location");
        }
    }

    /// <summary>
    /// Broadcasts route progress updates to subscribed clients.
    /// </summary>
    public async Task BroadcastRouteProgress(int routeId, int completionPercentage, string status)
    {
        try
        {
            await Clients.All.SendAsync("RouteProgressUpdated", new { routeId, completionPercentage, status, timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Route {routeId} progress: {completionPercentage}%");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error broadcasting route progress: {ex.Message}");
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
            await Clients.All.SendAsync("Alert", alert);
            _logger.LogWarning($"Alert for vehicle {vehicleId}: {alertType} - {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending alert: {ex.Message}");
        }
    }

    /// <summary>
    /// Requests online vehicle count from the service.
    /// </summary>
    public async Task RequestOnlineVehicleCount()
    {
        try
        {
            var count = await _vehicleService.GetOnlineVehicleCountAsync();
            await Clients.Caller.SendAsync("OnlineVehicleCount", count);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error requesting online vehicle count: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to retrieve vehicle count");
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
            await Clients.All.SendAsync("FleetStatusUpdated", status);
            _logger.LogInformation($"Fleet {fleetName} status: {onlineCount}/{totalCount} online");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error broadcasting fleet status: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a heartbeat ping to maintain connection.
    /// </summary>
    public async Task Ping()
    {
        try
        {
            await Clients.Caller.SendAsync("Pong");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling ping: {ex.Message}");
        }
    }
}
