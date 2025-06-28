// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Base class for domain events in the application.
/// Domain events represent significant things that happen within the business domain.
/// Examples: LocationUpdated, VehicleStatusChanged, RouteCompleted.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// Helps track event processing and correlate with logs.
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User or system that triggered the event.
    /// Null for system-triggered events.
    /// </summary>
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// Request ID for correlating this event with HTTP requests/logs.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets the event name for logging and identification.
    /// </summary>
    public virtual string EventName => GetType().Name;
}

/// <summary>
/// Event published when a vehicle's location is updated.
/// Fired when a new location record is created for a vehicle.
/// </summary>
public class LocationUpdatedEvent : DomainEvent
{
    /// <summary>
    /// ID of the vehicle whose location was updated.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// The new location data.
    /// </summary>
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }

    /// <summary>
    /// Previous location (if available).
    /// Allows calculating movement direction and distance.
    /// </summary>
    public double? PreviousLatitude { get; set; }
    public double? PreviousLongitude { get; set; }

    /// <summary>
    /// Speed of the vehicle at this location (km/h).
    /// </summary>
    public double? Speed { get; set; }

    /// <summary>
    /// Heading direction (0-360 degrees).
    /// 0 = North, 90 = East, 180 = South, 270 = West.
    /// </summary>
    public double? Heading { get; set; }
}

/// <summary>
/// Event published when a vehicle's operational status changes.
/// Examples: Active->Inactive, Available->InRoute, InRoute->Completed.
/// </summary>
public class VehicleStatusChangedEvent : DomainEvent
{
    /// <summary>
    /// ID of the vehicle whose status changed.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// The vehicle's plate/identifier.
    /// </summary>
    public string VehiclePlate { get; set; } = string.Empty;

    /// <summary>
    /// Previous status before the change.
    /// </summary>
    public string PreviousStatus { get; set; } = string.Empty;

    /// <summary>
    /// New status after the change.
    /// </summary>
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the status change.
    /// Examples: "Manual Update", "Route Completed", "Maintenance Required".
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Any additional metadata about the status change.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Event published when a tracking session ends.
/// Contains summary information about the session.
/// </summary>
public class TrackingSessionCompletedEvent : DomainEvent
{
    /// <summary>
    /// ID of the completed tracking session.
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// ID of the vehicle that was tracked.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// When the session started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the session ended.
    /// </summary>
    public DateTime EndedAt { get; set; }

    /// <summary>
    /// Total distance covered during the session (km).
    /// </summary>
    public double TotalDistanceKm { get; set; }

    /// <summary>
    /// Number of location updates recorded.
    /// </summary>
    public int LocationCount { get; set; }

    /// <summary>
    /// Average speed during the session (km/h).
    /// </summary>
    public double AverageSpeedKmh { get; set; }
}

/// <summary>
/// Event published when geofence boundaries are violated.
/// Fired when a vehicle enters or exits a defined zone.
/// </summary>
public class GeofenceViolationEvent : DomainEvent
{
    /// <summary>
    /// ID of the geofence that was violated.
    /// </summary>
    public Guid GeofenceId { get; set; }

    /// <summary>
    /// ID of the vehicle that violated the geofence.
    /// </summary>
    public Guid VehicleId { get; set; }

    /// <summary>
    /// Type of violation: "Entered" or "Exited".
    /// </summary>
    public string ViolationType { get; set; } = string.Empty;

    /// <summary>
    /// Location where the violation occurred.
    /// </summary>
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Distance from geofence boundary (negative = inside, positive = outside).
    /// </summary>
    public double DistanceFromBoundaryMeters { get; set; }
}
