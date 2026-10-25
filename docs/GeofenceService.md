# GeofenceService

Source: `src/SignalRMapRealtime/Services/GeofenceService.cs`

An in-memory implementation of `IGeofenceService`. It holds geofence zones, tracks which zones each vehicle is currently inside, and emits an alert plus a `GeofenceViolationEvent` each time a vehicle enters or exits a zone. The file also contains the `GeofencingExtensions` class, which registers the service in DI.

Register it as a singleton. Zone definitions and per-vehicle presence live in instance dictionaries, so a new instance starts empty and a scoped or transient registration would lose state between requests.

## Public API

```csharp
public interface IGeofenceService
public sealed class GeofenceService : IGeofenceService
public static class GeofencingExtensions
```

### IGeofenceService

| Member | Purpose |
|--------|---------|
| `RegisterZoneAsync(CreateGeofenceDto, CancellationToken)` | Creates a zone from the DTO and stores it. Returns a `GeofenceDto`. |
| `RemoveZoneAsync(Guid, CancellationToken)` | Removes a zone by ID. Returns `true` if it existed. |
| `GetActiveZonesAsync(CancellationToken)` | Returns the zones whose `IsActive` is `true`, as a read-only list. |
| `CheckLocationAsync(Guid, double, double, CancellationToken)` | Compares a vehicle's position with its previous state and returns entry and exit alerts. |

### GeofenceService

| Member | Purpose |
|--------|---------|
| Constructor `(IEventBus, ILogger<GeofenceService>)` | Stores the event bus and logger. Throws `ArgumentNullException` if either is null. |
| Four `IGeofenceService` methods | See the sections below. |

### GeofencingExtensions

| Member | Purpose |
|--------|---------|
| `AddGeofencing(this IServiceCollection)` | Calls `AddSingleton<IGeofenceService, GeofenceService>()` and returns the same collection. Throws `ArgumentNullException` if `services` is null. |

## Internal state

| Field | Type | Contents |
|-------|------|----------|
| `_zones` | `ConcurrentDictionary<Guid, Geofence>` | All registered zones, active or not, keyed by zone ID. |
| `_presence` | `ConcurrentDictionary<Guid, HashSet<Guid>>` | For each vehicle, the set of zone IDs it was inside at its last check. |
| `_dwellStartTimes` | `ConcurrentDictionary<(Guid VehicleId, Guid ZoneId), DateTime>` | Declared but never read or written in this file. See [Dwell time](#dwell-time). |

No locks are taken. The dictionaries are thread-safe individually, but the logic in `CheckLocationAsync` is a read followed by a write (see [Concurrency](#concurrency)).

## RegisterZoneAsync

Builds a `Geofence` entity from the DTO and stores it under `zone.Id`. The DTO fields copied across are `Name`, `Description`, `Type`, `IsActive`, `CenterLatitude`, `CenterLongitude`, `RadiusKm`, `PolygonCoordinates`, `CreatedBy`, and `MaxDwellMinutes`.

1. Throws `ArgumentNullException` if `dto` is null.
2. Creates the entity and stores it with `_zones[zone.Id] = zone`. If the key already exists, the old zone is replaced.
3. Logs an Information message with the zone name and ID.
4. Returns the mapped `GeofenceDto`.

The method does not validate coordinates, radius, or polygon data. The file does not show how `Geofence` assigns `Id`, `CreatedAt`, or `UpdatedAt`.

The method is `async` in signature only. It returns `Task.FromResult`, so no I/O happens.

## RemoveZoneAsync

Calls `_zones.TryRemove(id, out var zone)`. If the zone was found, it logs an Information message and returns `true`. Otherwise it returns `false` and logs nothing.

Removing a zone does not clear its entries in `_presence`. A vehicle that was inside the zone keeps that zone ID in its presence set. The next `CheckLocationAsync` call compares against that stale set. Because `EmitAlertAsync` returns early when the zone is no longer in `_zones`, no alert is emitted for the removed zone. Also, `_dwellStartTimes` is not touched.

## GetActiveZonesAsync

Returns a snapshot of `_zones.Values` filtered by `IsActive` and mapped to `GeofenceDto`. The result is a new `List<GeofenceDto>` typed as `IReadOnlyList<GeofenceDto>`. The order is whatever the dictionary enumerates. The method does not take a `CancellationToken` into account.

## CheckLocationAsync

This is the core method. It takes one position for one vehicle and returns the alerts produced by the change from the previous position.

**Steps:**

1. **Find current zones.** It collects the IDs of all zones where `IsActive` is `true` and `z.ContainsPoint(latitude, longitude)` returns `true`. `ContainsPoint` is defined on `Geofence`, not in this file.
2. **Load previous zones.** It reads the vehicle's presence set from `_presence`, creating an empty set if the vehicle has no entry.
3. **Entries.** For each zone ID in `current − previous`, it calls `EmitAlertAsync` with `"Entered"`.
4. **Exits.** For each zone ID in `previous − current`, it calls `EmitAlertAsync` with `"Exited"`.
5. **Save state.** It sets `_presence[vehicleId] = currentIds`.
6. Returns the list of alerts. The list is empty when nothing changed.

Entry and exit alerts are edge-triggered. Repeated positions inside the same zone produce no alerts. Note that the zone set uses the same `IsActive` check as step 1. A zone that is deactivated while a vehicle is inside it is not in `currentIds` on the next check, so the vehicle gets an `"Exited"` alert for that zone.

The first check for a new vehicle compares against an empty set. Every zone the vehicle is inside produces an `"Entered"` alert.

Zone membership is computed with the vehicle's exact coordinates. There is no hysteresis or buffer. A vehicle that moves back and forth across a boundary produces an alert on every crossing.

### EmitAlertAsync (private)

For each transition, `EmitAlertAsync` does the following:

1. Looks up the zone in `_zones`. If it is missing, returns without doing anything.
2. Adds a `GeofenceAlertDto` to the alerts list with:
   - `GeofenceId`, `GeofenceName`: from the zone.
   - `VehicleId`, `Latitude`, `Longitude`: from the call.
   - `ViolationType`: `"Entered"` or `"Exited"`.
3. Logs a Warning message: `vehicle {VehicleId} entered|exited zone '{ZoneName}' ({ZoneId})`.
4. Publishes `GeofenceViolationEvent` on `IEventBus` with `GeofenceId`, `VehicleId`, `ViolationType`, `Latitude`, and `Longitude`. It awaits the publish.

If `PublishAsync` throws, the exception propagates out of `CheckLocationAsync`. The presence update in step 5 has not run yet, so the next check will try the same transitions again. The alerts from earlier transitions in that call are lost, and some events may have been published already.

## Dwell time

The class summary says the service tracks dwell time for vehicles that stay in a zone longer than `MaxDwellMinutes`. This file does not implement that:

- `_dwellStartTimes` is declared but never read or written.
- `GeofenceDwellEvent` is not referenced.
- `MaxDwellMinutes` is copied into the entity on register and copied back in `GeofenceDto`. No code compares it with elapsed time.

Dwell-based alerts are not produced by this file. Any dwell logic must be implemented elsewhere, or the summary and this field must be updated.

## Concurrency

`CheckLocationAsync` is not atomic per vehicle. It reads `_presence`, computes the transitions, publishes events, and then writes the new set. Two concurrent calls for the same vehicle can both see the same previous set and both emit the same `Entered` or `Exited` alerts. Calls for different vehicles do not interact.

`RegisterZoneAsync`, `RemoveZoneAsync`, and `GetActiveZonesAsync` read or modify `_zones` directly. Each one is safe on its own, but a check that runs during a zone change can see the change partway through.

## Mapping to DTOs

`MapToDto` copies all fields of `Geofence` into `GeofenceDto`. `Type` is converted with `Type.ToString()`, so the DTO carries the enum name as a string. The `Id`, `CreatedAt`, `UpdatedAt`, and `CreatedBy` fields are included.

## Logging

| Event | Level | Source |
|-------|-------|--------|
| Zone registered | Information | `RegisterZoneAsync` |
| Zone removed | Information | `RemoveZoneAsync` (only when found) |
| Boundary crossed | Warning | `EmitAlertAsync` |

## Related types

- `IGeofenceService`, `CreateGeofenceDto`, `GeofenceDto`, `GeofenceAlertDto`: request, response, and alert shapes used by this file.
- `Geofence`: the entity. Provides `ContainsPoint`, `IsActive`, and the other properties.
- `GeofenceViolationEvent`: the domain event published on each transition.
- `IEventBus`: the bus the events are published to.
