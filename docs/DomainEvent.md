# DomainEvent

Represents a domain event emitted by the real-time vehicle tracking system, capturing vehicle location updates, status transitions, and session lifecycle events with associated metadata for audit and replay purposes.

## API

### EventId
**Type:** `Guid`  
Unique identifier for this event instance, generated at emission time. Used for idempotency checks and event store indexing.

### OccurredAt
**Type:** `DateTime`  
Timestamp (UTC) when the event occurred in the domain. Set by the event source; not overwritten by infrastructure.

### TriggeredBy
**Type:** `string?`  
Identifier of the user, system, or device that initiated the action causing this event. Null when the trigger is ambient (e.g., GPS poll).

### CorrelationId
**Type:** `string?`  
Correlation identifier linking this event to a parent command or external request. Enables distributed tracing across service boundaries.

### VehicleId
**Type:** `Guid`  
Identifier of the vehicle this event pertains to. Present on all event variants.

### Latitude
**Type:** `double`  
Current latitude in decimal degrees (WGS84). Valid range: [-90, 90]. Present on location events.

### Longitude
**Type:** `double`  
Current longitude in decimal degrees (WGS84). Valid range: [-180, 180]. Present on location events.

### Accuracy
**Type:** `double`  
Horizontal accuracy radius in meters as reported by the positioning source. Non-negative. Present on location events.

### PreviousLatitude
**Type:** `double?`  
Latitude of the previous known position. Null when no prior position exists (first fix). Present on location events.

### PreviousLongitude
**Type:** `double?`  
Longitude of the previous known position. Null when no prior position exists. Present on location events.

### Speed
**Type:** `double?`  
Current speed in meters per second. Null when unavailable or stationary. Present on location events.

### Heading
**Type:** `double?`  
Current heading in degrees clockwise from true north (0–360). Null when unavailable. Present on location events.

### VehiclePlate
**Type:** `string`  
License plate of the vehicle. Included for human-readable identification in logs and notifications.

### PreviousStatus
**Type:** `string`  
Vehicle status before the transition (e.g., "Idle", "EnRoute", "Offline"). Present on status change events.

### NewStatus
**Type:** `string`  
Vehicle status after the transition. Present on status change events.

### Reason
**Type:** `string`  
Human-readable reason for the status change (e.g., "Driver logged in", "Geofence exit", "Heartbeat timeout").

### Metadata
**Type:** `Dictionary<string, object>`  
Extensible key-value payload for event-specific data not covered by fixed properties. Keys are case-sensitive. Values must be JSON-serializable. Never null; initialized to empty dictionary.

### SessionId
**Type:** `Guid`  
Identifier of the active tracking session. Groups location and status events belonging to a single operational period.

## Usage

### Publishing a location update event
```csharp
var locationEvent = new DomainEvent
{
    EventId = Guid.NewGuid(),
    OccurredAt = DateTime.UtcNow,
    TriggeredBy = "gps-device-42",
    CorrelationId = "cmd-7a3f9b1e",
    VehicleId = vehicle.Id,
    VehiclePlate = vehicle.Plate,
    Latitude = 59.3293,
    Longitude = 18.0686,
    Accuracy = 4.2,
    PreviousLatitude = 59.3290,
    PreviousLongitude = 18.0680,
    Speed = 12.5,
    Heading = 45.0,
    SessionId = activeSession.Id,
    Metadata = new Dictionary<string, object>
    {
        ["satellites"] = 8,
        ["fixType"] = "3D",
        ["hdop"] = 1.2
    }
};

await eventBus.PublishAsync(locationEvent, cancellationToken);
```

### Handling a status transition event
```csharp
public async Task HandleAsync(DomainEvent domainEvent, CancellationToken ct)
{
    if (string.IsNullOrEmpty(domainEvent.NewStatus))
        return; // Not a status change event

    var transition = new StatusTransition
    {
        VehicleId = domainEvent.VehicleId,
        VehiclePlate = domainEvent.VehiclePlate,
        FromStatus = domainEvent.PreviousStatus,
        ToStatus = domainEvent.NewStatus,
        Reason = domainEvent.Reason,
        OccurredAt = domainEvent.OccurredAt,
        CorrelationId = domainEvent.CorrelationId
    };

    await _transitionStore.AppendAsync(transition, ct);

    if (domainEvent.NewStatus == "Offline" && domainEvent.Metadata.TryGetValue("timeout", out var timeoutObj))
    {
        var timeoutMinutes = Convert.ToInt32(timeoutObj);
        _logger.LogWarning("Vehicle {Plate} went offline after {Timeout} min inactivity", domainEvent.VehiclePlate, timeoutMinutes);
    }
}
```

## Notes

- **Property overlap**: This type uses a flat property bag covering multiple event kinds (location, status, session). Consumers must inspect `NewStatus`/`PreviousStatus` presence or `Latitude`/`Longitude` validity to discriminate event semantics.
- **Nullability**: `TriggeredBy`, `CorrelationId`, `PreviousLatitude`, `PreviousLongitude`, `Speed`, and `Heading` are nullable. All other reference-type properties (`VehiclePlate`, `PreviousStatus`, `NewStatus`, `Reason`, `Metadata`) are non-null but may be empty strings.
- **Metadata safety**: The `Metadata` dictionary is mutable and not thread-safe. Do not share a single `DomainEvent` instance across threads while mutating `Metadata`. For concurrent access, copy the dictionary or use `ConcurrentDictionary` before assignment.
- **Value ranges**: `Latitude`/`Longitude` are not validated at the type level. Producers should enforce WGS84 bounds; consumers should guard against out-of-range values when projecting to maps or spatial indexes.
- **Time semantics**: `OccurredAt` reflects domain time, not ingestion time. Events may arrive out of order; handlers requiring ordering must sort by `OccurredAt` (and `EventId` as tiebreaker).
- **Serialization**: `Metadata` values of type `object` require a serializer capable of polymorphic deserialization (e.g., System.Text.Json with `JsonSerializerOptions.Default` or Newtonsoft.Json with `TypeNameHandling.Auto`). Avoid non-serializable types (delegates, open generics, `IQueryable`).
- **Immutability expectation**: Although properties are settable, treat instances as immutable after publication. Modifying a published event breaks event-sourcing guarantees and audit integrity.
