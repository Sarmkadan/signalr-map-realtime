#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Event published when a vehicle is marked as stale due to lack of location updates.
/// A vehicle becomes stale when it hasn't sent location updates within the configured stale detection window.
/// </summary>
public class VehicleStaleEvent : DomainEvent
{
    /// <summary>
    /// ID of the vehicle that became stale.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Vehicle registration number or identifier.
    /// </summary>
    public string VehicleRegistration { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle name or description.
    /// </summary>
    public string VehicleName { get; set; } = string.Empty;

    /// <summary>
    /// When the vehicle was last updated (UTC).
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// Timestamp when the vehicle became stale (UTC).
    /// </summary>
    public DateTime StaleSince { get; set; }

    /// <summary>
    /// Time window configuration used for stale detection (in minutes).
    /// </summary>
    public int StaleWindowMinutes { get; set; }

    /// <summary>
    /// Time elapsed since last update when vehicle was marked stale (in minutes).
    /// </summary>
    public double TimeSinceLastUpdateMinutes { get; set; }

    /// <summary>
    /// Indicates if this is a recovery event (vehicle was previously stale and is now active again).
    /// </summary>
    public bool IsRecovery { get; set; }

    /// <summary>
    /// Previous stale status before this event.
    /// </summary>
    public bool WasPreviouslyStale { get; set; }
}