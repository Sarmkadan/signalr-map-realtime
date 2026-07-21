#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Event published when a vehicle has dwelled inside a geofence longer than the configured threshold.
/// Fired when a vehicle enters a geofence and remains inside for longer than the specified dwell time.
/// </summary>
public class GeofenceDwellEvent : DomainEvent
{
    /// <summary>
    /// ID of the geofence where the vehicle is dwelling.
    /// </summary>
    public Guid GeofenceId { get; set; }

    /// <summary>
    /// Name of the geofence where the vehicle is dwelling.
    /// </summary>
    public string GeofenceName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the vehicle that is dwelling inside the geofence.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// Vehicle registration number or identifier.
    /// </summary>
    public string VehicleRegistration { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle name or description.
    /// </summary>
    public string VehicleName { get; set; } = string.Empty;

    /// <summary>
    /// When the vehicle entered the geofence (UTC).
    /// </summary>
    public DateTime EnteredAt { get; set; }

    /// <summary>
    /// Duration of the dwell in minutes.
    /// </summary>
    public double DwellDurationMinutes { get; set; }

    /// <summary>
    /// Configured maximum dwell time threshold in minutes.
    /// </summary>
    public int MaxDwellMinutes { get; set; }

    /// <summary>
    /// Current location where the dwell is being detected.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Current longitude where the dwell is being detected.
    /// </summary>
    public double Longitude { get; set; }
}