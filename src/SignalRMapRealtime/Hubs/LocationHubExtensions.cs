#nullable enable

namespace SignalRMapRealtime.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="LocationHub"/> that provide additional convenience functionality
/// for managing SignalR connections, broadcasting to vehicle groups, and handling common hub operations.
/// </summary>
public static class LocationHubExtensions
{
    /// <summary>
    /// Broadcasts a message to all clients subscribed to a specific vehicle.
    /// </summary>
    /// <param name="hub">The location hub instance.</param>
    /// <param name="vehicleId">The vehicle ID to broadcast to.</param>
    /// <param name="methodName">The SignalR method name to invoke.</param>
    /// <param name="args">Optional arguments to pass to the method.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hub"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="methodName"/> is null or empty.</exception>
    public static async Task BroadcastToVehicleGroupAsync(this LocationHub hub, int vehicleId, string methodName, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(hub);
        ArgumentException.ThrowIfNullOrEmpty(methodName);

        await hub.Clients
            .Group($"vehicle-{vehicleId}")
            .SendAsync(methodName, args)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the current connection ID of the calling client.
    /// </summary>
    /// <param name="hub">The location hub instance.</param>
    /// <returns>The connection ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hub"/> is null.</exception>
    public static string GetConnectionId(this LocationHub hub)
    {
        ArgumentNullException.ThrowIfNull(hub);
        return hub.Context.ConnectionId;
    }

    /// <summary>
    /// Broadcasts a message to all clients in a specific fleet group.
    /// </summary>
    /// <param name="hub">The location hub instance.</param>
    /// <param name="fleetName">The fleet name.</param>
    /// <param name="methodName">The SignalR method name to invoke.</param>
    /// <param name="args">Optional arguments to pass to the method.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hub"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fleetName"/> or <paramref name="methodName"/> is null or empty.</exception>
    public static async Task BroadcastToFleetAsync(this LocationHub hub, string fleetName, string methodName, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(hub);
        ArgumentException.ThrowIfNullOrEmpty(fleetName);
        ArgumentException.ThrowIfNullOrEmpty(methodName);

        await hub.Clients
            .Group($"fleet-{fleetName}")
            .SendAsync(methodName, args)
            .ConfigureAwait(false);
    }
}
