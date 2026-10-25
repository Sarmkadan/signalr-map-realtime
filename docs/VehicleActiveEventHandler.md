# VehicleActiveEventHandler

Source: `src/SignalRMapRealtime/Events/VehicleActiveEventHandler.cs`

`VehicleActiveEventHandler` is a public class in the `SignalRMapRealtime.Events` namespace that implements `IEventHandler<VehicleActiveEvent>`. It handles `VehicleActiveEvent`, which is raised when a previously stale vehicle becomes active again. The handler forwards the event to connected SignalR clients through `ISignalRNotificationService`, so map clients can restore or show the vehicle marker.

## Constructor

```csharp
public VehicleActiveEventHandler(ISignalRNotificationService notificationService, ILogger<VehicleActiveEventHandler> logger)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `notificationService` | `ISignalRNotificationService` | Service used to broadcast the event to SignalR clients. |
| `logger` | `ILogger<VehicleActiveEventHandler>` | Logger for handler operations. |

Both parameters are checked with `ArgumentNullException.ThrowIfNull`. A `null` value throws `ArgumentNullException` at construction time. The constructor stores the two values in private readonly fields.

## Methods

### HandleAsync

```csharp
public async Task HandleAsync(VehicleActiveEvent @event)
```

Handles the event by broadcasting it to all connected SignalR clients. The method does the following, in order:

1. Checks that `@event` is not `null`. A `null` value throws `ArgumentNullException`.
2. Writes an `Information` log entry with the vehicle's `VehicleId`, `VehicleRegistration`, and `RecoveryTime`.
3. Calls `_notificationService.BroadcastVehicleActiveAsync(@event)` and awaits it with `ConfigureAwait(false)`.
4. On success, writes an `Information` log entry that the event was handled for the vehicle.
5. If the broadcast throws, writes an `Error` log entry that includes the exception and its message, then rethrows the original exception with `throw;`.

| Parameter | Type | Description |
|-----------|------|-------------|
| `event` | `VehicleActiveEvent` | The event to handle. |

**Returns:** `Task`, which completes when the broadcast completes.

**Throws:**
- `ArgumentNullException` when `event` is `null`.
- Any exception thrown by `ISignalRNotificationService.BroadcastVehicleActiveAsync`. The handler logs it and rethrows it, so the caller receives the original exception.

## Logging

| Level | Message template | When |
|-------|------------------|------|
| Information | `Handling VehicleActiveEvent for vehicle {VehicleId} ({VehicleRegistration}) - Recovered at {RecoveryTime}` | Before the broadcast starts. |
| Information | `VehicleActiveEvent handled successfully for vehicle {VehicleId}` | After the broadcast completes without an exception. |
| Error | `Error handling VehicleActiveEvent for vehicle {VehicleId}: {Message}` | When the broadcast throws. The exception is logged and then rethrown. |

## Related

- `VehicleActiveEvent`: the event payload. See `docs/VehicleActiveEvent.md`.
- `ISignalRNotificationService`: the service that performs the broadcast.
