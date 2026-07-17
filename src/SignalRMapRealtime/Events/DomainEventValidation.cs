#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SignalRMapRealtime.Events;

/// <summary>
/// Provides validation helpers for <see cref="DomainEvent"/> and its derived types.
/// Validates domain events to ensure they contain valid data before processing.
/// </summary>
public static class DomainEventValidation
{
    /// <summary>
    /// Validates a domain event and returns a list of validation problems.
    /// Each problem is a human-readable string describing what's wrong with the event.
    /// </summary>
    /// <param name="value">The domain event to validate.</param>
    /// <returns>List of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate base DomainEvent properties
        if (value.EventId == Guid.Empty)
        {
            problems.Add("EventId must not be empty");
        }

        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a valid DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }

        // Validate TriggeredBy
        if (value.TriggeredBy is { Length: 0 })
        {
            problems.Add("TriggeredBy must not be an empty string");
        }

        // Validate CorrelationId
        if (value.CorrelationId is { Length: 0 })
        {
            problems.Add("CorrelationId must not be an empty string");
        }

        // Validate event-specific properties based on concrete type
        switch (value)
        {
            case LocationUpdatedEvent locationEvent:
                ValidateLocationUpdatedEvent(locationEvent, problems);
                break;

            case VehicleStatusChangedEvent statusEvent:
                ValidateVehicleStatusChangedEvent(statusEvent, problems);
                break;

            case TrackingSessionCompletedEvent sessionEvent:
                ValidateTrackingSessionCompletedEvent(sessionEvent, problems);
                break;

            case GeofenceViolationEvent geofenceEvent:
                ValidateGeofenceViolationEvent(geofenceEvent, problems);
                break;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines if a domain event is valid (has no validation problems).
    /// </summary>
    /// <param name="value">The domain event to check.</param>
    /// <returns>True if the event is valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a domain event is valid, throwing an exception if it's not.
    /// The exception contains a list of all validation problems.
    /// </summary>
    /// <param name="value">The domain event to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the event is invalid, with a list of problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static void EnsureValid(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Domain event validation failed. Problems:\n" + string.Join("\n", problems),
                nameof(value));
        }
    }

    private static void ValidateLocationUpdatedEvent(
        LocationUpdatedEvent locationEvent,
        List<string> problems)
    {
        // Validate LocationUpdatedEvent specific properties
        if (locationEvent.VehicleId == Guid.Empty)
        {
            problems.Add("VehicleId must not be empty");
        }

        // Validate coordinates (latitude must be between -90 and 90)
        if (locationEvent.Latitude is < -90.0 or > 90.0)
        {
            problems.Add("Latitude must be between -90.0 and 90.0");
        }

        // Validate longitude (must be between -180 and 180)
        if (locationEvent.Longitude is < -180.0 or > 180.0)
        {
            problems.Add("Longitude must be between -180.0 and 180.0");
        }

        // Validate accuracy (should be non-negative)
        if (locationEvent.Accuracy < 0.0)
        {
            problems.Add("Accuracy must be non-negative");
        }

        // Validate previous coordinates if provided
        if (locationEvent.PreviousLatitude is not null)
        {
            if (locationEvent.PreviousLatitude is < -90.0 or > 90.0)
            {
                problems.Add("PreviousLatitude must be between -90.0 and 90.0");
            }
        }

        if (locationEvent.PreviousLongitude is not null)
        {
            if (locationEvent.PreviousLongitude is < -180.0 or > 180.0)
            {
                problems.Add("PreviousLongitude must be between -180.0 and 180.0");
            }
        }

        // Validate speed (should be non-negative if provided)
        if (locationEvent.Speed is not null && locationEvent.Speed < 0.0)
        {
            problems.Add("Speed must be non-negative");
        }

        // Validate heading (should be between 0 and 360 if provided)
        if (locationEvent.Heading is not null)
        {
            if (locationEvent.Heading is < 0.0 or > 360.0)
            {
                problems.Add("Heading must be between 0.0 and 360.0");
            }
        }
    }

    private static void ValidateVehicleStatusChangedEvent(
        VehicleStatusChangedEvent statusEvent,
        List<string> problems)
    {
        // Validate VehicleStatusChangedEvent specific properties
        if (statusEvent.VehicleId == Guid.Empty)
        {
            problems.Add("VehicleId must not be empty");
        }

        if (string.IsNullOrWhiteSpace(statusEvent.VehiclePlate))
        {
            problems.Add("VehiclePlate must not be null or whitespace");
        }
        else if (statusEvent.VehiclePlate.Length == 0)
        {
            problems.Add("VehiclePlate must not be an empty string");
        }

        if (string.IsNullOrWhiteSpace(statusEvent.PreviousStatus))
        {
            problems.Add("PreviousStatus must not be null or whitespace");
        }
        else if (statusEvent.PreviousStatus.Length == 0)
        {
            problems.Add("PreviousStatus must not be an empty string");
        }

        if (string.IsNullOrWhiteSpace(statusEvent.NewStatus))
        {
            problems.Add("NewStatus must not be null or whitespace");
        }
        else if (statusEvent.NewStatus.Length == 0)
        {
            problems.Add("NewStatus must not be an empty string");
        }

        if (string.IsNullOrWhiteSpace(statusEvent.Reason))
        {
            problems.Add("Reason must not be null or whitespace");
        }
        else if (statusEvent.Reason.Length == 0)
        {
            problems.Add("Reason must not be an empty string");
        }

        // Validate Metadata is not null
        if (statusEvent.Metadata is null)
        {
            problems.Add("Metadata must not be null");
        }
    }

    private static void ValidateTrackingSessionCompletedEvent(
        TrackingSessionCompletedEvent sessionEvent,
        List<string> problems)
    {
        // Validate TrackingSessionCompletedEvent specific properties
        if (sessionEvent.SessionId == Guid.Empty)
        {
            problems.Add("SessionId must not be empty");
        }

        if (sessionEvent.VehicleId == Guid.Empty)
        {
            problems.Add("VehicleId must not be empty");
        }

        if (sessionEvent.StartedAt == default)
        {
            problems.Add("StartedAt must be set to a valid DateTime");
        }
        else if (sessionEvent.StartedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("StartedAt cannot be in the future");
        }

        if (sessionEvent.EndedAt == default)
        {
            problems.Add("EndedAt must be set to a valid DateTime");
        }
        else if (sessionEvent.EndedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("EndedAt cannot be in the future");
        }
        else if (sessionEvent.EndedAt < sessionEvent.StartedAt)
        {
            problems.Add("EndedAt must be after StartedAt");
        }

        // Validate numeric properties
        if (sessionEvent.TotalDistanceKm < 0.0)
        {
            problems.Add("TotalDistanceKm must be non-negative");
        }

        if (sessionEvent.LocationCount < 0)
        {
            problems.Add("LocationCount must be non-negative");
        }

        if (sessionEvent.AverageSpeedKmh < 0.0)
        {
            problems.Add("AverageSpeedKmh must be non-negative");
        }
    }

    private static void ValidateGeofenceViolationEvent(
        GeofenceViolationEvent geofenceEvent,
        List<string> problems)
    {
        // Validate GeofenceViolationEvent specific properties
        if (geofenceEvent.GeofenceId == Guid.Empty)
        {
            problems.Add("GeofenceId must not be empty");
        }

        if (geofenceEvent.VehicleId == Guid.Empty)
        {
            problems.Add("VehicleId must not be empty");
        }

        if (string.IsNullOrWhiteSpace(geofenceEvent.ViolationType))
        {
            problems.Add("ViolationType must not be null or whitespace");
        }
        else if (geofenceEvent.ViolationType.Length == 0)
        {
            problems.Add("ViolationType must not be an empty string");
        }
        else if (geofenceEvent.ViolationType != "Entered" && geofenceEvent.ViolationType != "Exited")
        {
            problems.Add("ViolationType must be either 'Entered' or 'Exited'");
        }

        // Validate coordinates (latitude must be between -90 and 90)
        if (geofenceEvent.Latitude is < -90.0 or > 90.0)
        {
            problems.Add("Latitude must be between -90.0 and 90.0");
        }

        // Validate longitude (must be between -180 and 180)
        if (geofenceEvent.Longitude is < -180.0 or > 180.0)
        {
            problems.Add("Longitude must be between -180.0 and 180.0");
        }

        // Validate DistanceFromBoundaryMeters
        if (geofenceEvent.DistanceFromBoundaryMeters is < -10000.0 or > 10000.0)
        {
            problems.Add("DistanceFromBoundaryMeters must be between -10000.0 and 10000.0");
        }
    }
}