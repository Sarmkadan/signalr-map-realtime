#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Services;

using SignalRMapRealtime.Events;

/// <summary>
/// Service for broadcasting domain events to SignalR hubs for real-time client notifications.
/// Handles the conversion of domain events into SignalR method calls that update the map UI.
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>
    /// Broadcasts a vehicle stale event to all connected clients.
    /// Clients can use this to gray-out or hide the vehicle marker on the map.
    /// </summary>
    /// <param name="staleEvent">The vehicle stale event to broadcast.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BroadcastVehicleStaleAsync(VehicleStaleEvent staleEvent);

    /// <summary>
    /// Broadcasts a vehicle active event to all connected clients.
    /// Clients can use this to restore or show the vehicle marker on the map.
    /// </summary>
    /// <param name="activeEvent">The vehicle active event to broadcast.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BroadcastVehicleActiveAsync(VehicleActiveEvent activeEvent);
}