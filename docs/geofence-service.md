# GeofenceService

`GeofenceService` is the in-memory implementation of `IGeofenceService`. It manages geofence zones (circles and polygons) and turns a stream of vehicle positions into discrete **entry** and **exit** alerts. Each alert is also published as a `GeofenceViolationEvent` on the event bus.

Source: `src/SignalRMapRealtime/Services/GeofenceService.cs`

## Registration

```csharp
services.AddGeofencing();   // registers IGeofenceService -> GeofenceService as a singleton
```

`AddGeofencing` is called from `Configuration/DependencyInjection.cs`.

The service must be a **singleton**. Zone definitions and per-vehicle presence state live in fields of the instance. A transient or scoped registration would lose that state between requests.

## Constructor

```csharp
public GeofenceService(IEventBus eventBus, ILogger<GeofenceService> logger)
```

Both arguments are null-checked with `ArgumentNullException.ThrowIfNull`.

## Public API

| Method | Returns | Behaviour |
|---|---|---|
| `RegisterZoneAsync(CreateGeofenceDto dto, ct)` | `GeofenceDto` | Creates a `Geofence` from the DTO, stores it under its new `Id`, and logs at Information level. An existing zone with the same name is not checked. |
| `RemoveZoneAsync(Guid id, ct)` | `bool` | Removes the zone. Returns `false` if the id is unknown. Does not clear presence records for vehicles that were inside the zone (see Known limitations). |
| `GetActiveZonesAsync(ct)` | `IReadOnlyList<GeofenceDto>` | Returns zones with `IsActive == true`. Inactive zones are still stored but never returned here and never evaluated. |
| `CheckLocationAsync(vehicleId, latitude, longitude, ct)` | `IReadOnlyList<GeofenceAlertDto>` | Evaluates one position against all active zones and returns the transitions since the previous call for that vehicle. |

The first three methods complete synchronously and wrap their result in `Task.FromResult`. `CheckLocationAsync` is the only method that awaits. None of the methods observe `ct`.

## Evaluation: `CheckLocationAsync`

1. Build the set of active zone ids that contain the point. Containment is delegated to `Geofence.ContainsPoint`, which handles `Circle` and `Polygon` zones.
2. Read the vehicle's previous set from `_presence`. A vehicle seen for the first time gets an empty set, so every zone that contains it produces an **Entered** alert.
3. For each zone in `current - previous`, emit **Entered**.
4. For each zone in `previous - current`, emit **Exited**.
5. Store `current` as the vehicle's new presence set.

Repeated positions inside the same zone produce no alerts. Only transitions do.

Each alert is a `GeofenceAlertDto` with `GeofenceId`, `GeofenceName`, `VehicleId`, `Latitude`, `Longitude`, and `ViolationType` (`"Entered"` or `"Exited"`). For every alert the service also:

- writes a `LogWarning` entry, and
- awaits `IEventBus.PublishAsync(new GeofenceViolationEvent { ... })`.

Publishing is awaited inside the loop. A slow or failing event bus therefore delays the caller and can stop later alerts from being emitted.

## Internal state

| Field | Type | Purpose |
|---|---|---|
| `_zones` | `ConcurrentDictionary<Guid, Geofence>` | All registered zones, active or not. |
| `_presence` | `ConcurrentDictionary<Guid, HashSet<Guid>>` | Per vehicle: the zone ids it is currently inside. |
| `_dwellStartTimes` | `ConcurrentDictionary<(Guid, Guid), DateTime>` | Declared for dwell tracking. **Not used yet** (see below). |

## Mapping

`Geofence` entities are converted to `GeofenceDto` by a private `MapToDto`. `Type` is written as `Type.ToString()`. `MaxDwellMinutes` is copied over, with a default of 60.

## Dwell time (not implemented)

The class summary and `GeofenceDwellEvent` describe dwell-time alerts for vehicles that stay inside a zone longer than `Geofence.MaxDwellMinutes`. In the current code this is **not implemented**:

- `_dwellStartTimes` is declared but never read or written.
- `CheckLocationAsync` never compares elapsed time to `MaxDwellMinutes`.
- Nothing publishes `GeofenceDwellEvent` from this class.

Treat `MaxDwellMinutes` as stored configuration only.

## Known limitations

- **No locking per vehicle.** `CheckLocationAsync` reads `_presence`, computes the diff, and then writes `_presence[vehicleId]`. Two concurrent calls for the same vehicle can both see the same previous state and emit duplicate Entered or Exited alerts. Calls for different vehicles do not interfere.
- **Removed zones are skipped silently.** If a zone is removed between the containment check and alert emission, `EmitAlertAsync` returns without an alert. The vehicle's presence set still drops that zone id.
- **Stale presence after removal.** `RemoveZoneAsync` does not touch `_presence`. Vehicles that were inside the removed zone keep its id until their next `CheckLocationAsync` call, which then sees no matching zone.
- **No duplicate-name check** on `RegisterZoneAsync`.
- **Memory grows with the vehicle count.** `_presence` gets one entry per vehicle ever checked and is never pruned. Vehicles that stop reporting stay in memory.
- **Cancellation is ignored.** `ct` is passed in but not checked.
- **Single process only.** State is not shared between instances. With multiple app instances, each one has its own view of presence, so alerts can be duplicated or missed.

## Example

```csharp
var zone = await geofence.RegisterZoneAsync(new CreateGeofenceDto
{
    Name = "Depot",
    Type = GeofenceType.Circle,
    CenterLatitude = 53.5511,
    CenterLongitude = 9.9937,
    RadiusKm = 0.5,
    IsActive = true,
});

// First call inside the zone: one Entered alert.
var a1 = await geofence.CheckLocationAsync(vehicleId, 53.5510, 9.9936);

// Still inside: no alerts.
var a2 = await geofence.CheckLocationAsync(vehicleId, 53.5512, 9.9938);

// Outside: one Exited alert.
var a3 = await geofence.CheckLocationAsync(vehicleId, 53.6000, 10.0500);
```

## Related

- `IGeofenceService`: the interface and its XML documentation.
- `Domain/Models/Geofence.cs`: zone entity and `ContainsPoint`.
- `Events/GeofenceViolationEvent.cs`, `Events/GeofenceDwellEvent.cs`: published events.
- `Controllers/GeofenceController.cs`: REST entry point for zone management.
- `tests/signalr-map-realtime.Tests/GeofenceServiceTests.cs`: unit tests.
