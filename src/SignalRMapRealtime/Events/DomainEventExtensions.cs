#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace SignalRMapRealtime.Events;

/// <summary>
/// Extension methods for <see cref="DomainEvent"/> and its derived types.
/// Provides utility methods for working with domain events.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class DomainEventExtensions
{
    /// <summary>
    /// Creates a deep copy of the domain event.
    /// Useful for event processing pipelines where you need to modify events without
    /// affecting the original.
    /// </summary>
    /// <param name="domainEvent">The domain event to clone.</param>
    /// <returns>A new instance with the same property values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static DomainEvent Clone(this DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return domainEvent switch
        {
            LocationUpdatedEvent locationEvent => new LocationUpdatedEvent
            {
                EventId = locationEvent.EventId,
                OccurredAt = locationEvent.OccurredAt,
                TriggeredBy = locationEvent.TriggeredBy,
                CorrelationId = locationEvent.CorrelationId,
                VehicleId = locationEvent.VehicleId,
                Latitude = locationEvent.Latitude,
                Longitude = locationEvent.Longitude,
                Accuracy = locationEvent.Accuracy,
                PreviousLatitude = locationEvent.PreviousLatitude,
                PreviousLongitude = locationEvent.PreviousLongitude,
                Speed = locationEvent.Speed,
                Heading = locationEvent.Heading
            },

            VehicleStatusChangedEvent statusEvent => new VehicleStatusChangedEvent
            {
                EventId = statusEvent.EventId,
                OccurredAt = statusEvent.OccurredAt,
                TriggeredBy = statusEvent.TriggeredBy,
                CorrelationId = statusEvent.CorrelationId,
                VehicleId = statusEvent.VehicleId,
                VehiclePlate = statusEvent.VehiclePlate,
                PreviousStatus = statusEvent.PreviousStatus,
                NewStatus = statusEvent.NewStatus,
                Reason = statusEvent.Reason,
                Metadata = new Dictionary<string, object>(statusEvent.Metadata)
            },

            TrackingSessionCompletedEvent sessionEvent => new TrackingSessionCompletedEvent
            {
                EventId = sessionEvent.EventId,
                OccurredAt = sessionEvent.OccurredAt,
                TriggeredBy = sessionEvent.TriggeredBy,
                CorrelationId = sessionEvent.CorrelationId,
                SessionId = sessionEvent.SessionId,
                VehicleId = sessionEvent.VehicleId,
                StartedAt = sessionEvent.StartedAt,
                EndedAt = sessionEvent.EndedAt,
                TotalDistanceKm = sessionEvent.TotalDistanceKm,
                LocationCount = sessionEvent.LocationCount,
                AverageSpeedKmh = sessionEvent.AverageSpeedKmh
            },

            GeofenceViolationEvent geofenceEvent => new GeofenceViolationEvent
            {
                EventId = geofenceEvent.EventId,
                OccurredAt = geofenceEvent.OccurredAt,
                TriggeredBy = geofenceEvent.TriggeredBy,
                CorrelationId = geofenceEvent.CorrelationId,
                GeofenceId = geofenceEvent.GeofenceId,
                VehicleId = geofenceEvent.VehicleId,
                ViolationType = geofenceEvent.ViolationType,
                Latitude = geofenceEvent.Latitude,
                Longitude = geofenceEvent.Longitude,
                DistanceFromBoundaryMeters = geofenceEvent.DistanceFromBoundaryMeters
            },

            _ => throw new InvalidOperationException($"Unknown DomainEvent type: {domainEvent.GetType().FullName}")
        };
    }

    /// <summary>
    /// Serializes the domain event to JSON with proper type information.
    /// Useful for event persistence, messaging systems, or logging.
    /// </summary>
    /// <param name="domainEvent">The domain event to serialize.</param>
    /// <param name="options">Optional JSON serialization options.</param>
    /// <returns>JSON string representation of the event.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static string ToJson(this DomainEvent domainEvent, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        options ??= new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(domainEvent, options);
    }

    /// <summary>
    /// Determines if this event represents a location update.
    /// Useful for filtering events in event handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to check.</param>
    /// <returns>True if the event is a LocationUpdatedEvent; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static bool IsLocationUpdate(this DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return domainEvent is LocationUpdatedEvent;
    }

    /// <summary>
    /// Determines if this event represents a vehicle status change.
    /// Useful for filtering events in event handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to check.</param>
    /// <returns>True if the event is a VehicleStatusChangedEvent; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static bool IsStatusChange(this DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return domainEvent is VehicleStatusChangedEvent;
    }

    /// <summary>
    /// Gets the vehicle identifier from the domain event.
    /// Returns <see cref="Guid"/>.<see cref="Guid.Empty"/> if the event doesn't contain vehicle information.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <returns>The vehicle ID or <see cref="Guid"/>.<see cref="Guid.Empty"/> if not applicable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static Guid GetVehicleId(this DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return domainEvent switch
        {
            LocationUpdatedEvent locationEvent => locationEvent.VehicleId,
            VehicleStatusChangedEvent statusEvent => statusEvent.VehicleId,
            TrackingSessionCompletedEvent sessionEvent => sessionEvent.VehicleId,
            GeofenceViolationEvent geofenceEvent => geofenceEvent.VehicleId,
            _ => Guid.Empty
        };
    }

    /// <summary>
    /// Gets a human-readable description of the event.
    /// Useful for logging, debugging, and user notifications.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <returns>Human-readable description of the event.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is <see langword="null"/>.</exception>
    public static string GetDescription(this DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return domainEvent switch
        {
            LocationUpdatedEvent locationEvent =>
                $"Location updated for vehicle {locationEvent.VehicleId} at {locationEvent.OccurredAt:HH:mm:ss} " +
                $"(Lat: {locationEvent.Latitude:F6}, Lon: {locationEvent.Longitude:F6}) " +
                $"Speed: {locationEvent.Speed?.ToString("F1") ?? "N/A"} km/h, " +
                $"Accuracy: {locationEvent.Accuracy:F1}m",

            VehicleStatusChangedEvent statusEvent =>
                $"Vehicle status changed from '{statusEvent.PreviousStatus}' to '{statusEvent.NewStatus}' for vehicle {statusEvent.VehicleId} ({statusEvent.VehiclePlate}) " +
                $"Reason: {statusEvent.Reason}",

            TrackingSessionCompletedEvent sessionEvent =>
                $"Tracking session {sessionEvent.SessionId} completed for vehicle {sessionEvent.VehicleId} " +
                $"Duration: {(sessionEvent.EndedAt - sessionEvent.StartedAt).TotalMinutes:F0} minutes, " +
                $"Distance: {sessionEvent.TotalDistanceKm:F1} km, " +
                $"Avg Speed: {sessionEvent.AverageSpeedKmh:F1} km/h",

            GeofenceViolationEvent geofenceEvent =>
                $"Geofence violation ({geofenceEvent.ViolationType}) for vehicle {geofenceEvent.VehicleId} " +
                $"at ({geofenceEvent.Latitude:F6}, {geofenceEvent.Longitude:F6}) " +
                $"Distance from boundary: {geofenceEvent.DistanceFromBoundaryMeters:F0}m",

            _ => $"Event: {domainEvent.EventName} (ID: {domainEvent.EventId}) at {domainEvent.OccurredAt}"
        };
    }
}