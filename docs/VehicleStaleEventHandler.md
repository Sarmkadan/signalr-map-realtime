# VehicleStaleEventHandler

Source: `src/SignalRMapRealtime/Events/VehicleStaleEventHandler.cs`

`VehicleStaleEventHandler` is a public class in the `SignalRMapRealtime.Events` namespace that implements `IEventHandler<VehicleStaleEvent>`. It handles `VehicleStaleEvent`, which signals that a vehicle has become stale. The handler forwards the event to connected SignalR clients through `ISignalRNotificationService`, so map clients can gray out or hide the vehicle marker.

## Constructor

```csharp
public VehicleStaleEventHandler(ISignalRNotificationService notificationService, ILogger<VehicleStaleEventHandler> logger)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `notificationService` | `ISignalRNotificationService` | Service used to broadcast the event to SignalR clients. |
| `logger` | `ILogger<VehicleStaleEventHandler>` | Logger for handler operations. |

Both parameters are checked with `ArgumentNullException.ThrowIfNull`. A `null` value throws `ArgumentNullException` at construction time. The constructor stores the two values in private readonly fields.

## Methods

### HandleAsync

```csharp
public async Task HandleAsync(VehicleStaleEvent @event)
```

Handles the event by broadcasting it to all connected SignalR clients. The method does the following, in order:

1. Checks that `@event` is not `null`. A `null` value throws `ArgumentNullException`.
2. Writes an `Information` log entry with the vehicle's `VehicleId`, `VehicleRegistration`, and `StaleSince`.
3. Calls `_notificationService.BroadcastVehicleStaleAsync(@event)` and awaits it with `ConfigureAwait(false)`.
4. On success, writes an `Information` log entry that the event was handled for the vehicle.
5. If the broadcast throws, writes an `Error` log entry that includes the exception and its message, then rethrows the original exception with `throw;`.

| Parameter | Type | Description |
|-----------|------|-------------|
| `event` | `VehicleStaleEvent` | The event to handle. |

**Returns:** `Task`, which completes when the broadcast completes.

**Throws:**
- `ArgumentNullException` when `event` is `null`.
- Any exception thrown by `ISignalRNotificationService.BroadcastVehicleStaleAsync`. The handler logs it and rethrows it, so the caller receives the original exception.

## Logging

| Level | Message template | When |
|-------|------------------|------|
| Information | `Handling VehicleStaleEvent for vehicle {VehicleId} ({VehicleRegistration}) - Stale since {StaleSince}` | Before the broadcast starts. |
| Information | `VehicleStaleEvent handled successfully for vehicle {VehicleId}` | After the broadcast completes without an exception. |
| Error | `Error handling VehicleStaleEvent for vehicle {VehicleId}: {Message}` | When the broadcast throws. The exception is logged and then rethrown. |

## Related

- `VehicleStaleEvent`: the event payload.
- `ISignalRNotificationService`: the service that performs the broadcast.
- `VehicleActiveEventHandler`: the counterpart that handles the vehicle becoming active again. See `docs/VehicleActiveEventHandler.md`.
