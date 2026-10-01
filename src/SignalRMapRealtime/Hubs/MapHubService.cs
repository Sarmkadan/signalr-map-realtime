#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;

/// <summary>
/// Facade service for broadcasting map-related events through the <see cref="LocationHub"/>.
/// Decouples business logic from direct SignalR hub context usage.
/// </summary>
public sealed class MapHubService
{
    private readonly IHubContext<LocationHub> _hubContext;
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<MapHubService> _logger;

    /// <summary>Initializes a new instance of the <see cref="MapHubService"/> class.</summary>
    /// <param name="hubContext">SignalR hub context for <see cref="LocationHub"/>.</param>
    /// <param name="vehicleService">Vehicle service for status lookups.</param>
    /// <param name="logger">Logger instance.</param>
    public MapHubService(
        IHubContext<LocationHub> hubContext,
        IVehicleService vehicleService,
        ILogger<MapHubService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Broadcasts a location update to all connected clients in a specific group.
    /// </summary>
    /// <param name="groupName">SignalR group name (typically a fleet or region identifier).</param>
    /// <param name="location">Location data to broadcast.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task BroadcastLocationAsync(string groupName, LocationDto location, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(location);

        await _hubContext.Clients.Group(groupName)
            .SendAsync("ReceiveLocationUpdate", location, cancellationToken);

        _logger.LogDebug("Broadcasted location for vehicle {VehicleId} to group {Group}",
            location.VehicleId, groupName);
    }

    /// <summary>
    /// Sends a geofence alert notification to a specific connected client.
    /// </summary>
    /// <param name="connectionId">Target client connection identifier.</param>
    /// <param name="alert">Geofence alert details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendGeofenceAlertAsync(string connectionId, GeofenceAlertDto alert, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentNullException.ThrowIfNull(alert);

        await _hubContext.Clients.Client(connectionId)
            .SendAsync("GeofenceAlert", alert, cancellationToken);

        _logger.LogInformation("Sent geofence alert {AlertType} to connection {ConnectionId}",
            alert.ViolationType, connectionId);
    }

    /// <summary>
    /// Broadcasts a vehicle status change to all connected clients.
    /// </summary>
    /// <param name="vehicleId">Vehicle identifier.</param>
    /// <param name="status">New vehicle status string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task BroadcastVehicleStatusAsync(string vehicleId, string status, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vehicleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);

        await _hubContext.Clients.All
            .SendAsync("VehicleStatusChanged", new { VehicleId = vehicleId, Status = status, Timestamp = DateTimeOffset.UtcNow }, cancellationToken);

        _logger.LogInformation("Vehicle {VehicleId} status changed to {Status}", vehicleId, status);
    }

    /// <summary>
    /// Adds a connection to a named group for scoped broadcasting.
    /// </summary>
    /// <param name="connectionId">Client connection identifier.</param>
    /// <param name="groupName">Group to join.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        await _hubContext.Groups.AddToGroupAsync(connectionId, groupName, cancellationToken);
        _logger.LogDebug("Connection {ConnectionId} added to group {Group}", connectionId, groupName);
    }
}
