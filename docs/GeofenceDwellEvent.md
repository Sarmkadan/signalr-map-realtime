# GeofenceDwellEvent

The `GeofenceDwellEvent` represents a domain event triggered when a vehicle remains within a specific geofence boundary for a duration that meets or exceeds a defined threshold. This event captures the precise moment the dwell condition was satisfied, providing contextual data about the vehicle, the location, and the duration of the stay to facilitate real-time alerts, reporting, or automated workflow triggers within the `signalr-map-realtime` system.

## API

The following public members expose the state of the dwell event. As this type functions as a data transfer object for domain events, these members are properties exposing immutable state captured at the time of the event.

### `GeofenceId`
*   **Type:** `Guid`
*   **Purpose:** Uniquely identifies the geofence zone where the dwell event occurred.
*   **Parameters:** None (Property getter).
*   **Return Value:** The GUID associated with the geofence.
*   **Throws:** Never.

### `GeofenceName`
*   **Type:** `string`
*   **Purpose:** Provides the human-readable name of the geofence zone for logging and display purposes.
*   **Parameters:** None (Property getter).
*   **Return Value:** The name of the geofence. May be null if the name was unavailable at the time of event creation.
*   **Throws:** Never.

### `VehicleId`
*   **Type:** `Guid`
*   **Purpose:** Uniquely identifies the vehicle that triggered the dwell event.
*   **Parameters:** None (Property getter).
*   **Return Value:** The GUID associated with the vehicle.
*   **Throws:** Never.

### `VehicleRegistration`
*   **Type:** `string`
*   **Purpose:** Contains the license plate or registration number of the vehicle.
*   **Parameters:** None (Property getter).
*   **Return Value:** The registration string. May be null if not assigned.
*   **Throws:** Never.

### `VehicleName`
*   **Type:** `string`
*   **Purpose:** Provides the friendly name or alias assigned to the vehicle.
*   **Parameters:** None (Property getter).
*   **Return Value:** The vehicle name. May be null if not assigned.
*   **Throws:** Never.

### `EnteredAt`
*   **Type:** `DateTime`
*   **Purpose:** Records the exact timestamp when the vehicle initially entered the geofence boundary.
*   **Parameters:** None (Property getter).
*   **Return Value:** The UTC date and time of entry.
*   **Throws:** Never.

### `DwellDurationMinutes`
*   **Type:** `double`
*   **Purpose:** Indicates the total time, in minutes, the vehicle has remained inside the geofence up to the moment the event was raised.
*   **Parameters:** None (Property getter).
*   **Return Value:** A double-precision floating-point number representing minutes.
*   **Throws:** Never.

### `MaxDwellMinutes`
*   **Type:** `int`
*   **Purpose:** Defines the configured threshold limit (in minutes) that, when exceeded or met, triggers this event.
*   **Parameters:** None (Property getter).
*   **Return Value:** The integer limit in minutes.
*   **Throws:** Never.

### `Latitude`
*   **Type:** `double`
*   **Purpose:** Specifies the latitude coordinate of the vehicle's last known position used to validate the dwell condition.
*   **Parameters:** None (Property getter).
*   **Return Value:** The latitude value in decimal degrees.
*   **Throws:** Never.

### `Longitude`
*   **Type:** `double`
*   **Purpose:** Specifies the longitude coordinate of the vehicle's last known position used to validate the dwell condition.
*   **Parameters:** None (Property getter).
*   **Return Value:** The longitude value in decimal degrees.
*   **Throws:** Never.

## Usage

### Example 1: Filtering High-Priority Dwell Events
This example demonstrates how to process a stream of events and isolate instances where a vehicle has exceeded the maximum allowed dwell time significantly, useful for generating critical alerts.

```csharp
public void ProcessDwellEvents(IEnumerable<GeofenceDwellEvent> events)
{
    foreach (var evt in events)
    {
        // Check if the vehicle has dwelled longer than the configured maximum
        if (evt.DwellDurationMinutes > evt.MaxDwellMinutes)
        {
            var excessTime = evt.DwellDurationMinutes - evt.MaxDwellMinutes;
            
            Console.WriteLine(
                $"ALERT: Vehicle '{evt.VehicleRegistration}' ({evt.VehicleName}) " +
                $"has exceeded dwell limit in '{evt.GeofenceName}' by {excessTime:F1} minutes."
            );
            
            // Trigger notification logic here
            SendNotification(evt.VehicleId, $"Excessive dwell detected at {evt.GeofenceName}");
        }
    }
}
```

### Example 2: Logging Entry Details for Audit
This example illustrates extracting specific spatial and temporal data from the event for audit trail storage, focusing on when the vehicle entered and its precise location.

```csharp
public async Task LogDwellAuditAsync(GeofenceDwellEvent dwellEvent, IAuditRepository repository)
{
    var auditRecord = new DwellAuditRecord
    {
        EventId = Guid.NewGuid(),
        TargetId = dwellEvent.VehicleId,
        TargetName = dwellEvent.VehicleRegistration,
        ZoneId = dwellEvent.GeofenceId,
        ZoneName = dwellEvent.GeofenceName,
        EntryTimestamp = dwellEvent.EnteredAt,
        CurrentDuration = dwellEvent.DwellDurationMinutes,
        LocationLat = dwellEvent.Latitude,
        LocationLon = dwellEvent.Longitude,
        RecordedAt = DateTime.UtcNow
    };

    await repository.SaveAsync(auditRecord);
}
```

## Notes

*   **Immutability:** The `GeofenceDwellEvent` is designed as an immutable data snapshot. All public members are read-only properties intended to reflect the state of the system at the exact moment the event was raised. Modifying these values after instantiation is not supported and may lead to data inconsistency in downstream consumers.
*   **Time Precision:** The `EnteredAt` property typically stores UTC time. Consumers should ensure consistent time zone handling when calculating relative times or displaying this value to end-users. The `DwellDurationMinutes` is calculated based on the difference between the event generation time and `EnteredAt`.
*   **Nullability:** String properties (`GeofenceName`, `VehicleRegistration`, `VehicleName`) may return `null` if the associated metadata was missing or deleted at the time the event was constructed. Callers should perform null checks before accessing members like `Length` or performing string concatenation.
*   **Thread Safety:** As this type exposes only immutable state (get-only properties wrapping primitive types or immutable structs like `Guid` and `DateTime`), instances of `GeofenceDwellEvent` are inherently thread-safe for read operations across multiple threads without requiring external synchronization.
*   **Coordinate Validity:** The `Latitude` and `Longitude` properties are raw doubles. While they generally conform to WGS84 standards, the system does not enforce range validation (-90 to 90 for Lat, -180 to 180 for Lon) within the property getters. Invalid coordinates, if present, indicate upstream data ingestion issues rather than property access errors.
