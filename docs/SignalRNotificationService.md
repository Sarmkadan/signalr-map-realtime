# SignalRNotificationService

Source: `src/SignalRMapRealtime/Services/SignalRNotificationService.cs`

A service that turns vehicle domain events into SignalR client calls, so the map UI can gray out or restore vehicle markers in real time. It implements `ISignalRNotificationService` and sends everything through `IHubContext<LocationHub>`, so the messages reach clients connected to `LocationHub`.

The file exposes two public methods, both on the class. It has no other public members.

## Public API

```csharp
public class SignalRNotificationService : ISignalRNotificationService

public SignalRNotificationService(IHubContext<LocationHub> hubContext, ILogger<SignalRNotificationService> logger)

public Task BroadcastVehicleStaleAsync(VehicleStaleEvent staleEvent)
public Task BroadcastVehicleActiveAsync(VehicleActiveEvent activeEvent)
```

| Member | Purpose |
|--------|---------|
| Constructor | Stores the hub context and logger. Throws `ArgumentNullException` if either is null. |
| `BroadcastVehicleStaleAsync` | Sends a `VehicleStale` message for a vehicle that has stopped reporting. |
| `BroadcastVehicleActiveAsync` | Sends a `VehicleActive` message for a vehicle that has recovered. |

The interface `ISignalRNotificationService` is defined in another file. This document covers only the two methods implemented here.

## Constructor

| Parameter | Type | Notes |
|-----------|------|-------|
| `hubContext` | `IHubContext<LocationHub>` | Used for all client sends. |
| `logger` | `ILogger<SignalRNotificationService>` | Used for Information and Error messages. |

Both arguments are checked with `ArgumentNullException.ThrowIfNull`. The constructor does not create any group or connection.

## BroadcastVehicleStaleAsync

Notifies clients that a vehicle is stale. The UI can use this to gray out or hide the marker.

**Parameter:** `VehicleStaleEvent staleEvent`. The fields this method reads are `VehicleId`, `VehicleRegistration`, and `StaleSince`. The event itself is sent to clients unchanged.

**Behavior, in order:**

1. Throws `ArgumentNullException` if `staleEvent` is null. This check runs before the `try` block, so nothing is logged or sent.
2. Logs an Information message with the vehicle ID, registration, and `StaleSince`.
3. Sends `VehicleStale` with the event as the payload to the group `vehicle-{VehicleId}`.
4. Sends `VehicleStale` with the same payload to `Clients.All`.
5. Logs an Information message that the broadcast succeeded.

If any step throws, the method logs an Error message with the exception and the vehicle ID, then rethrows the original exception with `throw;`.

## BroadcastVehicleActiveAsync

Notifies clients that a vehicle has recovered. The UI can use this to show or restore the marker.

**Parameter:** `VehicleActiveEvent activeEvent`. The fields this method reads are `VehicleId`, `VehicleRegistration`, and `RecoveryTime`.

**Behavior** is the same as `BroadcastVehicleStaleAsync`, with these differences:

- The method name sent to clients is `VehicleActive`.
- Logging uses `RecoveryTime` instead of `StaleSince`.
- The argument check is on `activeEvent`.

## Delivery model

Each call makes two separate sends:

| Target | Method name | Who receives it |
|--------|-------------|-----------------|
| `Clients.Group("vehicle-{VehicleId}")` | `VehicleStale` / `VehicleActive` | Clients that have joined the group for that vehicle. Group membership is managed outside this file. |
| `Clients.All` | `VehicleStale` / `VehicleActive` | Every connected `LocationHub` client. |

A client that is both in the vehicle group and connected to all clients receives the message twice, once from each send. The code does not deduplicate. Clients should treat the message as idempotent. Setting a marker state to "stale" or "active" twice has the same result as doing it once.

The two sends are awaited one after the other with `ConfigureAwait(false)`. If the group send fails, the all-clients send is not attempted.

## Error handling

| Case | Result |
|------|--------|
| Null event argument | `ArgumentNullException`. Nothing is logged or sent. |
| Exception during either send, or in logging | Logged at Error level with the exception and `ex.Message`, then rethrown unchanged. |
| Success | Two Information messages are logged: one before the sends and one after. |

The service does not retry and does not swallow exceptions. Callers must handle failures from the returned task.

## Logging

All messages go through `ILogger<SignalRNotificationService>`.

| Level | Method | Message template | Placeholders |
|-------|--------|------------------|--------------|
| Information | Stale | `Broadcasting VehicleStaleEvent for vehicle {VehicleId} ({VehicleRegistration}) - Stale since {StaleSince}` | `VehicleId`, `VehicleRegistration`, `StaleSince` |
| Information | Stale | `VehicleStaleEvent broadcast successfully for vehicle {VehicleId}` | `VehicleId` |
| Error | Stale | `Error broadcasting VehicleStaleEvent for vehicle {VehicleId}: {Message}` | `VehicleId`, `Message` |
| Information | Active | `Broadcasting VehicleActiveEvent for vehicle {VehicleId} ({VehicleRegistration}) - Recovered at {RecoveryTime}` | `VehicleId`, `VehicleRegistration`, `RecoveryTime` |
| Information | Active | `VehicleActiveEvent broadcast successfully for vehicle {VehicleId}` | `VehicleId` |
| Error | Active | `Error broadcasting VehicleActiveEvent for vehicle {VehicleId}: {Message}` | `VehicleId`, `Message` |

The Error messages pass the exception to the logger and also include `ex.Message` in the text, so the message appears twice in the log output.

## Usage

Inject the interface and pass it an event that has already been created:

```csharp
public class VehicleMonitor(ISignalRNotificationService notifications)
{
    public Task NotifyStaleAsync(VehicleStaleEvent staleEvent) =>
        notifications.BroadcastVehicleStaleAsync(staleEvent);
}
```

The event type's constructor is not shown here. Create the event where the stale or recovery is detected.

The service is a plain class with no lifetime attributes in this file. Registration, including the lifetime, is set in the DI setup, which this file does not show.

## Related

- `LocationHub`: the hub whose clients receive these messages.
- `VehicleStaleEvent` and `VehicleActiveEvent`: the event types, in `SignalRMapRealtime.Events`.
- `VehicleStaleDetectionService`: the background service that detects stale vehicles. Whether it calls this service is not shown in this file.
