# VehicleActiveEvent

Source: `src/SignalRMapRealtime/Events/VehicleActiveEvent.cs`

Domain event published when a vehicle that was previously marked stale becomes active again. The event fires when a stale vehicle regains connectivity and sends new location data. The type lives in the `SignalRMapRealtime.Events` namespace and derives from `DomainEvent`.

## Properties

### VehicleId
**Type:** `int`  
ID of the vehicle that became active.

### VehicleRegistration
**Type:** `string`  
Vehicle registration number or identifier. Defaults to `string.Empty`.

### VehicleName
**Type:** `string`  
Vehicle name or description. Defaults to `string.Empty`.

### RecoveryTime
**Type:** `DateTime`  
When the vehicle sent its first location update after being stale (UTC).

### StaleWindowMinutes
**Type:** `int`  
Time window configuration used for stale detection, in minutes.

### TimeInStaleStateMinutes
**Type:** `double`  
Time elapsed since the vehicle was marked stale when it recovered, in minutes.

## Handler flow

This file only declares the event payload. It contains no publishing or handling logic, so the flow from publication to handling is not described here. The handler for this event is defined in `src/SignalRMapRealtime/Events/VehicleActiveEventHandler.cs`, which is outside the scope of this document.
