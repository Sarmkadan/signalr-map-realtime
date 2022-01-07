// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Represents a continuous tracking session for a vehicle with location history.
/// </summary>
public class TrackingSession
{
    /// <summary>Unique identifier for the tracking session.</summary>
    public int Id { get; set; }

    /// <summary>Session name or reference identifier.</summary>
    public string SessionName { get; set; } = string.Empty;

    /// <summary>Vehicle being tracked in this session.</summary>
    public int VehicleId { get; set; }

    /// <summary>Reference to the tracked vehicle.</summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>Route associated with this session, if any.</summary>
    public int? RouteId { get; set; }

    /// <summary>Reference to the associated route.</summary>
    public Route? Route { get; set; }

    /// <summary>Current status of the tracking session.</summary>
    public SessionStatus Status { get; set; } = SessionStatus.Pending;

    /// <summary>Time when tracking started.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Time when tracking ended.</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Total distance traveled during this session in kilometers.</summary>
    public double TotalDistance { get; set; }

    /// <summary>Average speed during the session in km/h.</summary>
    public double AverageSpeed { get; set; }

    /// <summary>Maximum speed reached during the session in km/h.</summary>
    public double MaxSpeed { get; set; }

    /// <summary>Total idle time during the session in seconds.</summary>
    public long TotalIdleSeconds { get; set; }

    /// <summary>Collection of all location points recorded in this session.</summary>
    public ICollection<Location> Locations { get; set; } = new List<Location>();

    /// <summary>Notes or observations about the session.</summary>
    public string? Notes { get; set; }

    /// <summary>Timestamp when the session record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of last update to session data.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Starts the tracking session and sets initial state.
    /// </summary>
    public void StartSession()
    {
        Status = SessionStatus.Active;
        StartTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Pauses the tracking session temporarily.
    /// </summary>
    public void PauseSession()
    {
        if (Status == SessionStatus.Active)
        {
            Status = SessionStatus.Paused;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Resumes a paused tracking session.
    /// </summary>
    public void ResumeSession()
    {
        if (Status == SessionStatus.Paused)
        {
            Status = SessionStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Completes the tracking session and calculates final statistics.
    /// </summary>
    public void CompleteSession()
    {
        Status = SessionStatus.Completed;
        EndTime = DateTime.UtcNow;
        CalculateSessionStatistics();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the tracking session abnormally.
    /// </summary>
    public void CancelSession(string reason = "")
    {
        Status = SessionStatus.Cancelled;
        EndTime = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
            Notes = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a location point for this tracking session.
    /// </summary>
    public void RecordLocation(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("Cannot record location on inactive session.");
        Locations.Add(location);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates session duration in hours.
    /// </summary>
    public double GetSessionDurationHours()
    {
        DateTime end = EndTime ?? DateTime.UtcNow;
        return (end - StartTime).TotalHours;
    }

    /// <summary>
    /// Calculates and updates session statistics based on recorded locations.
    /// </summary>
    private void CalculateSessionStatistics()
    {
        if (Locations.Count < 2)
            return;

        var sortedLocations = Locations.OrderBy(l => l.RecordedAt).ToList();
        TotalDistance = 0;
        double totalSpeed = 0;
        MaxSpeed = 0;

        for (int i = 1; i < sortedLocations.Count; i++)
        {
            TotalDistance += sortedLocations[i].CalculateDistanceTo(sortedLocations[i - 1]);
            if (sortedLocations[i].Speed.HasValue)
            {
                totalSpeed += sortedLocations[i].Speed.Value;
                MaxSpeed = Math.Max(MaxSpeed, sortedLocations[i].Speed.Value);
            }
        }

        AverageSpeed = Locations.Count > 0 ? totalSpeed / Locations.Count : 0;
    }

    /// <summary>
    /// Gets the duration of the session as a TimeSpan.
    /// </summary>
    public TimeSpan GetSessionDuration()
    {
        DateTime end = EndTime ?? DateTime.UtcNow;
        return end - StartTime;
    }
}
