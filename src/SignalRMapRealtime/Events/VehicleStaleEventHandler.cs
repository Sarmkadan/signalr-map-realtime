#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

using Microsoft.Extensions.Logging;
using SignalRMapRealtime.Services;

/// <summary>
/// Handles VehicleStaleEvent and broadcasts it to SignalR clients for real-time UI updates.
/// This ensures that when a vehicle becomes stale, all connected map clients are notified
/// so they can gray-out or hide the vehicle marker.
/// </summary>
public class VehicleStaleEventHandler : IEventHandler<VehicleStaleEvent>
{
    private readonly ISignalRNotificationService _notificationService;
    private readonly ILogger<VehicleStaleEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the VehicleStaleEventHandler.
    /// </summary>
    /// <param name="notificationService">Service for broadcasting events to SignalR clients.</param>
    /// <param name="logger">Logger for tracking handler operations.</param>
    public VehicleStaleEventHandler(ISignalRNotificationService notificationService, ILogger<VehicleStaleEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(logger);

        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the VehicleStaleEvent by broadcasting it to all connected SignalR clients.
    /// </summary>
    /// <param name="@event">The vehicle stale event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the event is null.</exception>
    public async Task HandleAsync(VehicleStaleEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation(
            "Handling VehicleStaleEvent for vehicle {VehicleId} ({VehicleRegistration}) - Stale since {StaleSince}",
            @event.VehicleId,
            @event.VehicleRegistration,
            @event.StaleSince);

        try
        {
            await _notificationService.BroadcastVehicleStaleAsync(@event).ConfigureAwait(false);
            _logger.LogInformation("VehicleStaleEvent handled successfully for vehicle {VehicleId}", @event.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling VehicleStaleEvent for vehicle {VehicleId}: {Message}",
                @event.VehicleId,
                ex.Message);
            throw;
        }
    }
}