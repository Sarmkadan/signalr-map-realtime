#nullable enable

namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="Route"/> class to calculate route durations,
/// check delays, and determine route status.
/// </summary>
public static class RouteExtensions
{
    /// <summary>
    /// Calculates the estimated duration of the route in minutes based on planned times.
    /// </summary>
    /// <param name="route">The route to calculate duration for.</param>
    /// <returns>Estimated duration in minutes, or null if planned times are not set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null.</exception>
    public static int? GetEstimatedDurationMinutes(this Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (route.PlannedDepartureTime == default || route.EstimatedArrivalTime == default)
        {
            return null;
        }

        var duration = route.EstimatedArrivalTime - route.PlannedDepartureTime;
        return (int)duration.TotalMinutes;
    }

    /// <summary>
    /// Calculates the actual duration of the route in minutes based on actual times.
    /// </summary>
    /// <param name="route">The route to calculate duration for.</param>
    /// <returns>Actual duration in minutes, or null if actual times are not set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null.</exception>
    public static int? GetActualDurationMinutes(this Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (route.ActualDepartureTime is null || route.ActualArrivalTime is null)
        {
            return null;
        }

        var duration = route.ActualArrivalTime.Value - route.ActualDepartureTime.Value;
        return (int)duration.TotalMinutes;
    }

    /// <summary>
    /// Checks if the route is delayed compared to the planned schedule.
    /// </summary>
    /// <param name="route">The route to check for delays.</param>
    /// <returns>True if the route is delayed, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null.</exception>
    public static bool IsDelayed(this Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (route.ActualArrivalTime is null || route.EstimatedArrivalTime == default)
        {
            return false;
        }

        return route.ActualArrivalTime.Value > route.EstimatedArrivalTime;
    }

    /// <summary>
    /// Gets the delay duration in minutes if the route is delayed.
    /// </summary>
    /// <param name="route">The route to check for delay duration.</param>
    /// <returns>Delay duration in minutes, or null if not delayed or times not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null.</exception>
    public static int? GetDelayMinutes(this Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (!route.IsDelayed() || route.ActualArrivalTime is null || route.EstimatedArrivalTime == default)
        {
            return null;
        }

        var delay = route.ActualArrivalTime.Value - route.EstimatedArrivalTime;
        return (int)delay.TotalMinutes;
    }

    /// <summary>
    /// Calculates the total straight-line distance in kilometers across the route's waypoints,
    /// summing the Haversine distance between each consecutive pair ordered by sequence.
    /// </summary>
    /// <param name="route">The route to calculate waypoint distance for.</param>
    /// <returns>Total distance in kilometers, or 0 if the route has fewer than two waypoints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is null.</exception>
    public static double GetWaypointDistanceKm(this Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        var ordered = route.Waypoints.OrderBy(w => w.Sequence).ToList();
        var total = 0.0;

        for (var i = 1; i < ordered.Count; i++)
        {
            total += ordered[i - 1].CalculateDistanceTo(ordered[i]);
        }

        return total;
    }
}