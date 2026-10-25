# VehicleStaleDetectionService

Source: `src/SignalRMapRealtime/Services/VehicleStaleDetectionService.cs`

A `BackgroundService` that watches vehicle location freshness. It publishes a `VehicleStaleEvent` when an online vehicle's last location is older than the stale window, and a `VehicleActiveEvent` when a previously stale vehicle sends a recent location again. The file contains only this class.

## Public API

```csharp
public class VehicleStaleDetectionService : BackgroundService
```

| Member | Purpose |
|--------|---------|
| Constructor `(ILogger<VehicleStaleDetectionService>, IServiceScopeFactory, IOptions<SignalrMapRealtimeOptions>)` | Stores the logger, scope factory, and `options.Value`. Throws `ArgumentNullException` for any null argument. |
| `ExecuteAsync(CancellationToken)` (protected override) | The detection loop. See [Execution loop](#execution-loop). |
| `ResetVehicleStaleTracking(int vehicleId)` | Forgets a vehicle's stale state. Does not publish any event. See [Manual reset](#manual-reset). |

The class is not sealed. The file does not show how the service is registered in DI. The constructor's doc comment says it takes `IServiceScopeFactory` so that scoped services (repositories, the domain event dispatcher) are resolved from a valid lifetime. A `BackgroundService` is a singleton, so it cannot inject scoped services directly.

## Constants

| Constant | Value | Meaning |
|----------|-------|---------|
| `StaleWindowIntervalMultiplier` | `2` | Number of location update intervals that may pass before a vehicle is stale. |
| `RecoveryWindowMinutes` | `5` | A stale vehicle recovers when its latest location is less than this many minutes old. |

The loop delay is a literal `30` seconds in `ExecuteAsync`. The error back-off is a literal `10` seconds. Neither is a named constant.

## Configuration

The stale window is a computed property:

```csharp
StaleWindowMinutes = _options.AppInfo.LocationUpdateIntervalSeconds / 60 * StaleWindowIntervalMultiplier;
```

Read it with these rules in mind:

- The division is integer division. `LocationUpdateIntervalSeconds` is truncated to whole minutes before the multiplier is applied. With an interval of 90 seconds, the window is `1 * 2 = 2` minutes. With an interval under 60 seconds, the window is `0` minutes.
- A zero-minute window makes every vehicle with a located position look stale on the first check. Any positive age is greater than zero. The file does not validate the setting.
- The stale window is recomputed on every access. It is not cached.

The recovery window is fixed at 5 minutes and does not depend on the configuration.

## Execution loop

`ExecuteAsync` runs until the stopping token is cancelled:

1. Logs an Information message that the service started.
2. In a loop, creates one DI scope per cycle with `_scopeFactory.CreateScope()`, then runs:
   - `DetectStaleVehiclesAsync(scope.ServiceProvider, stoppingToken)`
   - `DetectRecoveredVehiclesAsync(scope.ServiceProvider, stoppingToken)`
3. Waits 30 seconds with `Task.Delay`, using the stopping token.
4. On `OperationCanceledException`, logs that the service is stopping and exits the loop.
5. On any other exception, logs an Error and waits 10 seconds before the next cycle.
6. After the loop, logs that the service stopped.

Because the two detection methods run in sequence inside one `try`, an exception from `DetectStaleVehiclesAsync` skips `DetectRecoveredVehiclesAsync` for that cycle. The 10-second back-off applies to both.

## Stale detection

`DetectStaleVehiclesAsync` resolves `VehicleRepository` and `IDomainEventDispatcher` from the scope. Then it:

1. Computes `now = DateTime.UtcNow`, `staleWindow`, and `cutoffTime = now - staleWindow`. The cutoff is only logged at Debug level. The check below uses the window directly.
2. Calls `GetOnlineVehiclesAsync()` on the vehicle repository. The file passes no cancellation token to this call.
3. For each vehicle:
   - If `LastLocation` is null, logs Debug and skips it.
   - If the vehicle ID is already in the stale set, skips it.
   - If `now - LastLocation.RecordedAt > staleWindow`, marks the vehicle stale and dispatches a `VehicleStaleEvent`.

When a vehicle becomes stale, the service:

- Adds its ID to `_currentlyStaleVehicles`.
- Stores its last update time in `_lastActiveTimes`.
- Dispatches a `VehicleStaleEvent` through `IDomainEventDispatcher.DispatchAsync`, with these values:

| Property | Value |
|----------|-------|
| `VehicleId` | `vehicle.Id` |
| `VehicleRegistration` | `vehicle.RegistrationNumber` |
| `VehicleName` | `vehicle.Name`, or `vehicle.RegistrationNumber` if `Name` is null |
| `LastUpdateTime` | `LastLocation.RecordedAt` |
| `StaleSince` | `now` |
| `StaleWindowMinutes` | The computed stale window |
| `TimeSinceLastUpdateMinutes` | `now - LastUpdateTime`, in minutes |
| `IsRecovery` | `false` |
| `WasPreviouslyStale` | `false` |

Any exception in this method is logged as an Error and then rethrown.

## Recovery detection

`DetectRecoveredVehiclesAsync` resolves `VehicleRepository`, `LocationRepository`, and `IDomainEventDispatcher`. It iterates over a snapshot (`_currentlyStaleVehicles.ToList()`) of the stale set. For each stale vehicle:

1. Calls `LocationRepository.GetLatestLocationByVehicleAsync(vehicleId)`. This reads the latest location from the location repository, not from `Vehicle.LastLocation`.
2. If a location exists and `now - RecordedAt < 5 minutes`, the vehicle has recovered:
   - Calls `VehicleRepository.GetByIdAsync(vehicleId, cancellationToken)`.
   - If the vehicle exists, removes it from `_currentlyStaleVehicles` and `_lastActiveTimes`, then dispatches a `VehicleActiveEvent` with:
     - `VehicleId`, `VehicleRegistration`, and `VehicleName` (same fallback as the stale event)
     - `RecoveryTime = now`
     - `StaleWindowMinutes` (the computed stale window)
     - `TimeInStaleStateMinutes = now - _lastActiveTimes[vehicleId]`, in minutes

If the vehicle is missing from `GetByIdAsync`, or no location is found, the vehicle stays in the stale set. The file has no other path that removes it.

Errors are handled per vehicle. A failure for one vehicle is logged as an Error, and that vehicle is removed from both tracking collections. The loop then continues with the next vehicle. An outer `catch` logs the error and rethrows.

## Manual reset

`ResetVehicleStaleTracking(int vehicleId)` removes the vehicle from `_currentlyStaleVehicles`. If it was present, it also removes the entry from `_lastActiveTimes` and logs Debug. It publishes no event, so a reset vehicle receives neither a `VehicleActiveEvent` nor another `VehicleStaleEvent` from this method. The file's comment gives one use: a vehicle removed from tracking.

## Lifecycle of a vehicle

| Situation | Result |
|-----------|--------|
| Online, location older than the window | `VehicleStaleEvent` dispatched. Vehicle is tracked as stale. |
| Stale, next location within 5 minutes | `VehicleActiveEvent` dispatched. Vehicle is no longer tracked. |
| Stale, no recent location, vehicle still online | No event. Stays tracked. |
| Stale, then no longer returned by `GetOnlineVehiclesAsync` | The file does not remove it. Stays tracked until recovery or `ResetVehicleStaleTracking`. |
| Stale, recovery check threw an error | Removed from tracking without an event. If the location is still old on a later cycle, the stale check re-adds it and dispatches a new `VehicleStaleEvent`. |
| Stale, `GetByIdAsync` returned null | Stays tracked. No event. |

## Thread safety and state

State lives in two non-thread-safe collections: `Dictionary<int, DateTime> _lastActiveTimes` and `HashSet<int> _currentlyStaleVehicles`. The loop is the only code path that writes them during normal operation, and the loop runs on one thread. `ResetVehicleStaleTracking` is public and can be called from other threads, but it does not lock. A reset that overlaps with a detection cycle is a data race. The file does not guard against it.

## Cancellation

- `ExecuteAsync` passes the stopping token to both detection methods and to `Task.Delay`.
- `GetOnlineVehiclesAsync` and `GetLatestLocationByVehicleAsync` do not receive the token in this file.
- `GetByIdAsync` receives it.
- The per-vehicle `catch` in recovery catches `Exception`, which includes `OperationCanceledException`. During shutdown, a cancelled call inside that loop is logged as an Error, and the vehicle is removed from tracking. The loop does not stop there. The shutdown exits at the `Task.Delay` call.

## Logging

| Level | Event |
|-------|-------|
| Information | Service started, service stopping, service stopped. |
| Debug | Stale check cutoff. Recovery check with the tracked stale count. Vehicle has no location data. Stale tracking reset. |
| Information | Vehicle became stale, with minutes since last update and the cutoff. Published `VehicleStaleEvent`. Vehicle recovered, with time spent stale. Published `VehicleActiveEvent`. |
| Error | Loop failure (with 10-second back-off). Stale detection failure. Recovery detection failure. Per-vehicle recovery failure. |
