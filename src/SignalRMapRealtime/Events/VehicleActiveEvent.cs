#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Event published when a previously stale vehicle becomes active again after receiving location updates.
/// This event is fired when a vehicle that was marked as stale regains connectivity and sends new location data.
/// </summary>
public class VehicleActiveEvent : DomainEvent
{
    /// <summary>
    /// ID of the vehicle that became active.
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
    /// When the vehicle sent its first location update after being stale (UTC).
    /// </summary>
    public DateTime RecoveryTime { get; set; }

    /// <summary>
    /// Time window configuration used for stale detection (in minutes).
    /// </summary>
    public int StaleWindowMinutes { get; set; }

    /// <summary>
    /// Time elapsed since the vehicle was marked stale when it recovered (in minutes).
    /// </summary>
    public double TimeInStaleStateMinutes { get; set; }
}