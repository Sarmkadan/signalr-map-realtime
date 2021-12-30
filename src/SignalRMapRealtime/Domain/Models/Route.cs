// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Represents a planned route with waypoints for a vehicle or driver.
/// </summary>
public class Route
{
    /// <summary>Unique identifier for the route.</summary>
    public int Id { get; set; }

    /// <summary>Route name or identifier (e.g., "Route-001", "Downtown Delivery").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Detailed description of the route.</summary>
    public string? Description { get; set; }

    /// <summary>Vehicle assigned to this route.</summary>
    public int VehicleId { get; set; }

    /// <summary>Reference to the assigned vehicle.</summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>User/driver assigned to this route.</summary>
    public int? AssignedUserId { get; set; }

    /// <summary>Reference to the assigned user/driver.</summary>
    public User? AssignedUser { get; set; }

    /// <summary>Planned departure time from the origin.</summary>
    public DateTime PlannedDepartureTime { get; set; }

    /// <summary>Estimated arrival time at final destination.</summary>
    public DateTime EstimatedArrivalTime { get; set; }

    /// <summary>Actual departure time (recorded when route started).</summary>
    public DateTime? ActualDepartureTime { get; set; }

    /// <summary>Actual arrival time at final destination.</summary>
    public DateTime? ActualArrivalTime { get; set; }

    /// <summary>Origin location (starting point) of the route.</summary>
    public string? Origin { get; set; }

    /// <summary>Final destination location of the route.</summary>
    public string? Destination { get; set; }

    /// <summary>Total planned distance in kilometers.</summary>
    public double? TotalDistance { get; set; }

    /// <summary>Actual distance traveled in kilometers.</summary>
    public double? ActualDistance { get; set; }

    /// <summary>Indicates if the route is currently active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Indicates if the route has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Collection of waypoints on this route.</summary>
    public ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

    /// <summary>Tracking session associated with this route.</summary>
    public int? TrackingSessionId { get; set; }

    /// <summary>Reference to the associated tracking session.</summary>
    public TrackingSession? TrackingSession { get; set; }

    /// <summary>Timestamp when the route was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of last update to route information.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Starts the route and records the actual departure time.
    /// </summary>
    public void StartRoute()
    {
        IsActive = true;
        ActualDepartureTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes the route and records the actual arrival time.
    /// </summary>
    public void CompleteRoute(double actualDistance)
    {
        IsActive = false;
        IsCompleted = true;
        ActualArrivalTime = DateTime.UtcNow;
        ActualDistance = actualDistance;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the route and clears active status.
    /// </summary>
    public void CancelRoute()
    {
        IsActive = false;
        IsCompleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the completion percentage based on completed waypoints.
    /// </summary>
    public int CalculateCompletionPercentage()
    {
        if (Waypoints.Count == 0)
            return 0;
        int completedCount = Waypoints.Count(w => w.IsCompleted);
        return (completedCount * 100) / Waypoints.Count;
    }

    /// <summary>
    /// Gets the next waypoint in the route sequence.
    /// </summary>
    public Waypoint? GetNextWaypoint()
    {
        return Waypoints.OrderBy(w => w.Sequence).FirstOrDefault(w => !w.IsCompleted);
    }

    /// <summary>
    /// Adds a waypoint to the route with automatic sequence assignment.
    /// </summary>
    public void AddWaypoint(Waypoint waypoint)
    {
        ArgumentNullException.ThrowIfNull(waypoint);
        waypoint.Sequence = Waypoints.Count + 1;
        waypoint.Route = this;
        Waypoints.Add(waypoint);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if all waypoints in the route have been completed.
    /// </summary>
    public bool AllWaypointsCompleted()
    {
        return Waypoints.Count > 0 && Waypoints.All(w => w.IsCompleted);
    }
}
