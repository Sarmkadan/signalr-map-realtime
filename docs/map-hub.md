# MapHub (map real-time layer)

> There is no class named `MapHub` in the codebase. The real-time map endpoint is
> `LocationHub` (`src/SignalRMapRealtime/Hubs/LocationHub.cs`), mapped at `/hubs/location`.
> The related types are `MapHubService` (broadcast facade) and `LocationHubExtensions`.
> Some older docs still refer to `MapHub`; see [Known discrepancies](#known-discrepancies).

## Overview

| Item | Value |
|------|-------|
| Hub class | `SignalRMapRealtime.Hubs.LocationHub` (`Hub`) |
| Endpoint | `/hubs/location` (`Program.cs`, `app.MapHub<LocationHub>("/hubs/location")`) |
| Authentication | API key scheme `ApiKeyAuthenticationOptions.DefaultScheme` (class-level `[Authorize]`) |
| Ingest methods | Additionally require role `device` or `ingest` (method-level `[Authorize(Roles = "device,ingest")]`) |
| Facade | `MapHubService` (`Hubs/MapHubService.cs`), wraps `IHubContext<LocationHub>` |
| Other hub | `RoutePlaybackHub` at `/hubs/playback` (separate, not covered here) |

Clients connect to receive live vehicle positions, status changes, alerts and route progress for map
views. Devices and ingest services push positions in.

## Groups

Broadcasts from `LocationHub` target SignalR groups. Group names are built from the following patterns:

| Group | Pattern | Joined by |
|-------|---------|-----------|
| Vehicle | `vehicle-{vehicleId}` | `SubscribeToVehicle`, `SubscribeToVehicles` |
| Fleet | `fleet-{fleetName}` | `SubscribeToFleet` |

Connections are removed from groups with `UnsubscribeFromVehicle` / `UnsubscribeFromFleet`.
Group membership is per connection and is not persisted.

## Client -> server methods

| Method | Parameters | Auth | Behavior |
|--------|-----------|------|----------|
| `SendLocationUpdate` | `CreateLocationDto locationDto` | `device`, `ingest` | Looks up the vehicle, records the location via `ILocationService.RecordLocationAsync`, then adds it to `LocationUpdateThrottler`. The throttler decides whether it is buffered (throttling and coalescing by asset type). On unknown vehicle sends `Error` to the caller. |
| `NotifyAssetRemoved` | `int vehicleId` | `device`, `ingest` | Calls `_throttler.Remove(vehicleId)` and sends `AssetRemoved` to `vehicle-{id}`. |
| `BroadcastVehicleStatusChange` | `int vehicleId, string newStatus` | `device`, `ingest` | Sends `VehicleStatusChanged` to `vehicle-{id}`. |
| `NotifyVehicleStaleAsync` | `int vehicleId, string vehicleRegistration, DateTime staleSince, int staleWindowMinutes, double timeSinceLastUpdateMinutes` | `device`, `ingest` | Sends `VehicleStale` (a `VehicleStaleEvent`) to `vehicle-{id}`. Rethrows on error. |
| `NotifyVehicleActiveAsync` | `int vehicleId, string vehicleRegistration, DateTime recoveryTime, int staleWindowMinutes, double timeInStaleStateMinutes` | no role attribute (class-level auth only) | Sends `VehicleActive` (a `VehicleActiveEvent`) to `vehicle-{id}`. Rethrows on error. |
| `SendAlert` | `int vehicleId, string alertType, string message` | `device`, `ingest` | Sends `Alert` to `vehicle-{id}`. |
| `BroadcastFleetStatus` | `string fleetName, int onlineCount, int totalCount` | no role attribute (class-level auth only) | Sends `FleetStatusUpdated` to `fleet-{name}`. Percentage is computed server-side. |
| `BroadcastRouteProgress` | `int routeId, int completionPercentage, string status` | no role attribute (class-level auth only) | Resolves the route's vehicle through `RouteRepository`, sends `RouteProgressUpdated` to `vehicle-{id}`. Logs a warning if the route has no vehicle. |
| `RequestAllVehicleLocations` | none | class-level auth | Sends `PositionSnapshot` (list of last known locations of online vehicles) to the caller. Intended for resync after reconnect. |
| `RequestVehicleLocation` | `int vehicleId` | class-level auth | Sends `VehicleLocation` (latest location) to the caller. |
| `RequestOnlineVehicleCount` | none | class-level auth | Sends `OnlineVehicleCount` to the caller. |
| `SubscribeToVehicle` / `UnsubscribeFromVehicle` | `int vehicleId` | class-level auth | Joins or leaves `vehicle-{id}`. Caller receives `SubscribedToVehicle` / `UnsubscribedFromVehicle`. |
| `SubscribeToVehicles` | `int[] ids` | class-level auth | Joins `vehicle-{id}` for every id > 0 (others are skipped). Empty array sends `Error`. Caller receives `SubscribedToVehicles`. |
| `SubscribeToFleet` / `UnsubscribeFromFleet` | `string fleetName` | class-level auth | Joins or leaves `fleet-{name}`. Caller receives `SubscribedToFleet` / `UnsubscribedFromFleet`. |
| `Ping` | none | class-level auth | Replies with `Pong` to the caller. |

Errors that the hub handles are reported to the caller as the `Error` event with a short text message.

## Server -> client events

| Event | Payload | Sent to |
|-------|---------|---------|
| `Error` | `string` message | Caller |
| `PositionSnapshot` | list of last locations | Caller |
| `VehicleLocation` | latest location | Caller |
| `OnlineVehicleCount` | `int` | Caller |
| `SubscribedToVehicle`, `SubscribedToVehicles`, `UnsubscribedFromVehicle` | vehicle id(s) | Caller |
| `SubscribedToFleet`, `UnsubscribedFromFleet` | fleet name | Caller |
| `Pong` | none | Caller |
| `AssetRemoved` | `int` vehicle id | `vehicle-{id}` |
| `VehicleStatusChanged` | `{ vehicleId, newStatus, timestamp }` | `vehicle-{id}` |
| `VehicleStale` | `VehicleStaleEvent` | `vehicle-{id}` |
| `VehicleActive` | `VehicleActiveEvent` | `vehicle-{id}` |
| `Alert` | `{ vehicleId, alertType, message, timestamp }` | `vehicle-{id}` |
| `RouteProgressUpdated` | `{ routeId, completionPercentage, status, timestamp }` | `vehicle-{id}` |
| `FleetStatusUpdated` | `{ fleetName, onlineCount, totalCount, percentage, timestamp }` | `fleet-{name}` |

Location updates from the throttler are not described in this file. Check `LocationUpdateThrottler`
for the outgoing event name and payload.

## Connection lifecycle

- `OnConnectedAsync` / `OnDisconnectedAsync` only log the connection id (and exception message on disconnect).
- No group state is restored on reconnect. Clients must re-invoke `Subscribe*` and then
  `RequestAllVehicleLocations` to resync markers.

## MapHubService (facade)

`MapHubService` is a `sealed` class that sends messages through `IHubContext<LocationHub>` so that
services do not depend on the hub directly.

| Method | Behavior |
|--------|----------|
| `BroadcastLocationAsync(string groupName, LocationDto location, ct)` | Sends `ReceiveLocationUpdate` to an arbitrary group. |
| `SendGeofenceAlertAsync(string connectionId, GeofenceAlertDto alert, ct)` | Sends `GeofenceAlert` to one connection. |
| `BroadcastVehicleStatusAsync(string vehicleId, string status, ct)` | Sends `VehicleStatusChanged` to **all** clients with `{ VehicleId, Status, Timestamp }`. |
| `AddToGroupAsync(string connectionId, string groupName, ct)` | Adds a connection to any group. |

Constructor dependencies: `IHubContext<LocationHub>`, `IVehicleService`, `ILogger<MapHubService>`.
`IVehicleService` is stored but not used by the current methods.

**Registration:** no `AddScoped`/`AddSingleton` for `MapHubService` exists in the codebase
(checked with `grep -rn MapHubService`). It cannot be injected until it is registered.

## Known discrepancies

Code and docs that do not match. Confirm before relying on them.

1. **Name:** `MapHub` is referenced in `docs/RateLimitingMiddleware.md` (`app.MapHub<MapHub>("/hubs/map")`),
   `docs/LocationController.md`, `docs/UserRepository.md`, and `docs/IEventBus.md` (`RealTimeMapHub`).
   None of these types or endpoints exist. The live endpoint is `/hubs/location`.
   `docs/architecture.md` and `docs/troubleshooting.md` use `/locationHub`, which is also not the mapped path.
2. **Groups:** `MapHubService.BroadcastVehicleStatusAsync` broadcasts to all clients, while
   `LocationHub.BroadcastVehicleStatusChange` targets `vehicle-{id}`. The two produce different audiences.
3. **Event names:** `MapHubService` uses `ReceiveLocationUpdate` and `GeofenceAlert`. `LocationHub` does not
   send either name. Clients built for one path will not receive the other.
4. **Attribute placement:** in `LocationHub.cs` several `[Authorize(Roles = ...)]` attributes and
   `<exception>` tags sit between the XML summary and the method signature. The compiler reports CS1587
   for these comments. The attributes still apply to the methods.
5. **Auth scope:** `NotifyVehicleActiveAsync`, `BroadcastFleetStatus` and `BroadcastRouteProgress`
   have no `Roles` attribute, so any authenticated API-key client can call them. The XML comments on the
   role-restricted methods say they require roles, but the attributes are the actual gate.

## Build check

`dotnet build src/SignalRMapRealtime/SignalRMapRealtime.csproj` completes with 0 errors (warnings only).
