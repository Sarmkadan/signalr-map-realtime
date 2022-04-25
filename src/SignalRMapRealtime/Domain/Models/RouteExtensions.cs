#nullable enable

namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Extension methods for the Route class providing additional functionality for route management,
/// time calculations, and status checks.
/// </summary>
public static class RouteExtensions
{
    /// <summary>
    /// Calculates the estimated duration of the route in minutes based on planned times.
    /// </summary>
    /// <param name="route">The route to calculate duration for.</param>
    /// <returns>Estimated duration in minutes, or null if planned times are not set.</returns>
    public static int? GetEstimatedDurationMinutes(this Route route)
    {
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
    public static int? GetActualDurationMinutes(this Route route)
    {
        if (route.ActualDepartureTime == null || route.ActualArrivalTime == null)
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
    public static bool IsDelayed(this Route route)
    {
        if (route.ActualArrivalTime == null || route.EstimatedArrivalTime == default)
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
    public static int? GetDelayMinutes(this Route route)
    {
        if (!route.IsDelayed() || route.ActualArrivalTime == null || route.EstimatedArrivalTime == default)
        {
            return null;
        }

        var delay = route.ActualArrivalTime.Value - route.EstimatedArrivalTime;
        return (int)delay.TotalMinutes;
    }
}