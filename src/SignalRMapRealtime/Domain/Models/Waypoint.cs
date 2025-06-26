// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Represents a waypoint or stop along a planned route.
/// </summary>
public class Waypoint
{
    /// <summary>Unique identifier for the waypoint.</summary>
    public int Id { get; set; }

    /// <summary>Order of this waypoint in the route sequence.</summary>
    public int Sequence { get; set; }

    /// <summary>Name or description of the waypoint destination.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Latitude coordinate of the waypoint.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude coordinate of the waypoint.</summary>
    public double Longitude { get; set; }

    /// <summary>Full address of the waypoint location.</summary>
    public string? Address { get; set; }

    /// <summary>Time window start for arrival at this waypoint (ISO 8601 time format).</summary>
    public string? ArrivalTimeStart { get; set; }

    /// <summary>Time window end for arrival at this waypoint (ISO 8601 time format).</summary>
    public string? ArrivalTimeEnd { get; set; }

    /// <summary>Estimated duration to spend at this waypoint in minutes.</summary>
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>Special instructions for this stop.</summary>
    public string? Instructions { get; set; }

    /// <summary>Contact person name at this waypoint.</summary>
    public string? ContactName { get; set; }

    /// <summary>Contact phone number at this waypoint.</summary>
    public string? ContactPhone { get; set; }

    /// <summary>Indicates if this waypoint has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Actual arrival time at this waypoint.</summary>
    public DateTime? ActualArrivalTime { get; set; }

    /// <summary>Actual departure time from this waypoint.</summary>
    public DateTime? ActualDepartureTime { get; set; }

    /// <summary>Route this waypoint belongs to.</summary>
    public int RouteId { get; set; }

    /// <summary>Reference to the associated route.</summary>
    public Route? Route { get; set; }

    /// <summary>
    /// Marks this waypoint as completed with actual arrival and departure times.
    /// </summary>
    public void CompleteWaypoint(DateTime arrivalTime, DateTime? departureTime = null)
    {
        IsCompleted = true;
        ActualArrivalTime = arrivalTime;
        ActualDepartureTime = departureTime ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Resets the waypoint to incomplete state.
    /// </summary>
    public void Reset()
    {
        IsCompleted = false;
        ActualArrivalTime = null;
        ActualDepartureTime = null;
    }

    /// <summary>
    /// Validates that the waypoint coordinates are geographically valid.
    /// </summary>
    public bool HasValidCoordinates()
    {
        return Latitude >= -90 && Latitude <= 90 && Longitude >= -180 && Longitude <= 180;
    }

    /// <summary>
    /// Checks if the waypoint has a valid time window defined.
    /// </summary>
    public bool HasTimeWindow()
    {
        return !string.IsNullOrWhiteSpace(ArrivalTimeStart) && !string.IsNullOrWhiteSpace(ArrivalTimeEnd);
    }
}
