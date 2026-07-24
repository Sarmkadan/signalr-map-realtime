#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

using Microsoft.Extensions.Logging;
using SignalRMapRealtime.Services;

/// <summary>
/// Handles VehicleActiveEvent and broadcasts it to SignalR clients for real-time UI updates.
/// This ensures that when a previously stale vehicle becomes active again, all connected map clients
/// are notified so they can restore or show the vehicle marker on the map.
/// </summary>
public class VehicleActiveEventHandler
{
    private readonly ISignalRNotificationService _notificationService;
    private readonly ILogger<VehicleActiveEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the VehicleActiveEventHandler.
    /// </summary>
    /// <param name="notificationService">Service for broadcasting events to SignalR clients.</param>
    /// <param name="logger">Logger for tracking handler operations.</param>
    public VehicleActiveEventHandler(ISignalRNotificationService notificationService, ILogger<VehicleActiveEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(logger);

        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the VehicleActiveEvent by broadcasting it to all connected SignalR clients.
    /// </summary>
    /// <param name="@event">The vehicle active event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(VehicleActiveEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation(
            "Handling VehicleActiveEvent for vehicle {VehicleId} ({VehicleRegistration}) - Recovered at {RecoveryTime}",
            @event.VehicleId,
            @event.VehicleRegistration,
            @event.RecoveryTime);

        try
        {
            await _notificationService.BroadcastVehicleActiveAsync(@event).ConfigureAwait(false);
            _logger.LogInformation("VehicleActiveEvent handled successfully for vehicle {VehicleId}", @event.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling VehicleActiveEvent for vehicle {VehicleId}: {Message}",
                @event.VehicleId,
                ex.Message);
            throw;
        }
    }
}