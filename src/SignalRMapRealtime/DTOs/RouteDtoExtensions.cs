namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Provides extension methods for <see cref="RouteDto"/>.
/// </summary>
public static class RouteDtoExtensions
{
    /// <summary>
    /// Determines if a route is currently active and not completed.
    /// </summary>
    /// <param name="route">The route to check.</param>
    /// <returns>True if the route is active and not completed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="route"/> is null.</exception>
    public static bool IsRouteActive(this RouteDto route)
    {
        ArgumentNullException.ThrowIfNull(route);
        return route.IsActive && !route.IsCompleted;
    }

    /// <summary>
    /// Calculates the duration of a route in minutes.
    /// </summary>
    /// <param name="route">The route to calculate the duration for.</param>
    /// <returns>The duration of the route in minutes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="route"/> is null.</exception>
    public static double CalculateDurationInMinutes(this RouteDto route)
    {
        ArgumentNullException.ThrowIfNull(route);
        var duration = route.EstimatedArrivalTime - route.PlannedDepartureTime;
        return duration.TotalMinutes;
    }

    /// <summary>
    /// Checks if a route has a valid vehicle assigned.
    /// </summary>
    /// <param name="route">The route to check.</param>
    /// <returns>True if the route has a valid vehicle assigned; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="route"/> is null.</exception>
    public static bool HasValidVehicle(this RouteDto route)
    {
        ArgumentNullException.ThrowIfNull(route);
        return route.VehicleId > 0 && route.Vehicle != null;
    }
}
