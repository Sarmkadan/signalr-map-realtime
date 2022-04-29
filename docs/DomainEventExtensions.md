# DomainEventExtensions

Provides extension methods for inspecting, serializing, and cloning `DomainEvent` instances in the real-time vehicle tracking domain. These methods centralize common event-handling logic used by SignalR hubs, background processors, and API controllers.

## API

### Clone
```csharp
public static DomainEvent Clone(this DomainEvent source)
```
Creates a deep copy of the specified domain event.

**Parameters**  
- `source`: The event to clone. Must not be null.

**Returns**  
A new `DomainEvent` instance with identical property values.

**Exceptions**  
- `ArgumentNullException`: Thrown if `source` is null.

---

### ToJson
```csharp
public static string ToJson(this DomainEvent source)
```
Serializes the domain event to a JSON string using the configured serializer options.

**Parameters**  
- `source`: The event to serialize. Must not be null.

**Returns**  
A UTF-8 JSON representation of the event.

**Exceptions**  
- `ArgumentNullException`: Thrown if `source` is null.
- `JsonException`: Thrown if serialization fails due to circular references or unsupported types.

---

### IsLocationUpdate
```csharp
public static bool IsLocationUpdate(this DomainEvent source)
```
Determines whether the event represents a vehicle location update.

**Parameters**  
- `source`: The event to inspect. Must not be null.

**Returns**  
`true` if the event type indicates a location update; otherwise `false`.

**Exceptions**  
- `ArgumentNullException`: Thrown if `source` is null.

---

### IsStatusChange
```csharp
public static bool IsStatusChange(this DomainEvent source)
```
Determines whether the event represents a vehicle status change.

**Parameters**  
- `source`: The event to inspect. Must not be null.

**Returns**  
`true` if the event type indicates a status change; otherwise `false`.

**Exceptions**  
- `ArgumentNullException`: Thrown if `source` is null.

---

### GetVehicleId
```csharp
public static Guid GetVehicleId(this DomainEvent source)
```
Extracts the vehicle identifier associated with the event.

**Parameters**  
- `source`: The event containing the vehicle ID. Must not be null.

**Returns**  
The `Guid` identifying the vehicle.

**Exceptions**  
- `ArgumentNullException`: Thrown if `source` is null.
- `InvalidOperationException`: Thrown if the event does not contain a vehicle ID.

---

### GetDescription
```csharp
public static string GetDescription(this DomainEvent source)
```
Generates a human-readable summary of the event for logging or debugging.

**Parameters**  
- `source`: The event to describe. Must not be null.

**Returns**  
A non-empty string summarizing the event type and key properties.

**Exceptions**  
- `ArgumentNullException`: Thrown if `source` is null.

## Usage

### Filtering and routing events in a SignalR hub
```csharp
public async Task HandleEventAsync(DomainEvent domainEvent)
{
    if (domainEvent.IsLocationUpdate())
    {
        var vehicleId = domainEvent.GetVehicleId();
        await Clients.Group($"vehicle-{vehicleId}").SendAsync("LocationUpdated", domainEvent.ToJson());
    }
    else if (domainEvent.IsStatusChange())
    {
        var description = domainEvent.GetDescription();
        _logger.LogInformation("Status change: {Description}", description);
        await Clients.Group("dispatchers").SendAsync("StatusChanged", domainEvent.ToJson());
    }
}
```

### Cloning events for audit logging before mutation
```csharp
public DomainEvent EnrichWithMetadata(DomainEvent original, string correlationId)
{
    var clone = original.Clone();
    clone.Metadata["correlationId"] = correlationId;
    clone.Metadata["processedAt"] = DateTimeOffset.UtcNow.ToString("O");
    return clone;
}
```

## Notes

- All methods validate the `source` parameter and throw `ArgumentNullException` if null; callers should not rely on null-safe behavior.
- `Clone` performs a deep copy; modifications to the returned instance do not affect the original. The implementation uses serialization round-tripping, so custom types in the event must be serializable.
- `ToJson` uses the application's shared `JsonSerializerOptions` (camelCase, ignore nulls). The output is not indented.
- `IsLocationUpdate` and `IsStatusChange` are mutually exclusive for current event types but are implemented as independent checks; future event types may satisfy both.
- `GetVehicleId` throws `InvalidOperationException` for system-level events that lack a vehicle association (e.g., `MapResetEvent`). Callers should check event type first or catch the exception.
- `GetDescription` is intended for diagnostics only; its format is not guaranteed stable across versions.
- All methods are pure and thread-safe. They hold no shared state and can be called concurrently without synchronization.
