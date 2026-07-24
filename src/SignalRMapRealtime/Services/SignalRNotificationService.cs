#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.Events;
using SignalRMapRealtime.Hubs;

/// <summary>
/// Service implementation for broadcasting domain events to SignalR hubs.
/// Converts domain events into SignalR method calls that update the map UI in real-time.
/// </summary>
public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<LocationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the SignalR notification service.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for LocationHub.</param>
    /// <param name="logger">Logger for tracking service operations.</param>
    public SignalRNotificationService(IHubContext<LocationHub> hubContext, ILogger<SignalRNotificationService> logger)
    {
        ArgumentNullException.ThrowIfNull(hubContext);
        ArgumentNullException.ThrowIfNull(logger);

        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Broadcasts a vehicle stale event to all connected clients.
    /// Clients can use this to gray-out or hide the vehicle marker on the map.
    /// </summary>
    /// <param name="staleEvent">The vehicle stale event to broadcast.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when staleEvent is null.</exception>
    public async Task BroadcastVehicleStaleAsync(VehicleStaleEvent staleEvent)
    {
        ArgumentNullException.ThrowIfNull(staleEvent);

        try
        {
            _logger.LogInformation(
                "Broadcasting VehicleStaleEvent for vehicle {VehicleId} ({VehicleRegistration}) - Stale since {StaleSince}",
                staleEvent.VehicleId,
                staleEvent.VehicleRegistration,
                staleEvent.StaleSince);

            // Broadcast to the specific vehicle group
            await _hubContext
                .Clients.Group($"vehicle-{staleEvent.VehicleId}")
                .SendAsync("VehicleStale", staleEvent)
                .ConfigureAwait(false);

            // Also broadcast to all clients for global state management
            await _hubContext.Clients.All.SendAsync("VehicleStale", staleEvent).ConfigureAwait(false);

            _logger.LogInformation(
                "VehicleStaleEvent broadcast successfully for vehicle {VehicleId}",
                staleEvent.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error broadcasting VehicleStaleEvent for vehicle {VehicleId}: {Message}",
                staleEvent.VehicleId,
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Broadcasts a vehicle active event to all connected clients.
    /// Clients can use this to restore or show the vehicle marker on the map.
    /// </summary>
    /// <param name="activeEvent">The vehicle active event to broadcast.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when activeEvent is null.</exception>
    public async Task BroadcastVehicleActiveAsync(VehicleActiveEvent activeEvent)
    {
        ArgumentNullException.ThrowIfNull(activeEvent);

        try
        {
            _logger.LogInformation(
                "Broadcasting VehicleActiveEvent for vehicle {VehicleId} ({VehicleRegistration}) - Recovered at {RecoveryTime}",
                activeEvent.VehicleId,
                activeEvent.VehicleRegistration,
                activeEvent.RecoveryTime);

            // Broadcast to the specific vehicle group
            await _hubContext
                .Clients.Group($"vehicle-{activeEvent.VehicleId}")
                .SendAsync("VehicleActive", activeEvent)
                .ConfigureAwait(false);

            // Also broadcast to all clients for global state management
            await _hubContext.Clients.All.SendAsync("VehicleActive", activeEvent).ConfigureAwait(false);

            _logger.LogInformation(
                "VehicleActiveEvent broadcast successfully for vehicle {VehicleId}",
                activeEvent.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error broadcasting VehicleActiveEvent for vehicle {VehicleId}: {Message}",
                activeEvent.VehicleId,
                ex.Message);
            throw;
        }
    }
}