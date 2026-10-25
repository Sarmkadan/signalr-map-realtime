# VehicleStatus

Source: `src/SignalRMapRealtime/Domain/Enums/VehicleStatus.cs`

The file lives in the `SignalRMapRealtime.Domain.Enums` namespace and defines one public type:

- `VehicleStatus` - a public enum for the operational status of a vehicle in the tracking system. Its summary says it "represents the operational status of a vehicle in the tracking system."

The enum has no attributes and no methods. Its underlying type is the default `int`. Each value is set explicitly, numbered `0` to `7` in declaration order.

## Values

| Name | Value | Meaning (from the XML doc comment) |
|------|-------|------------------------------------|
| `Idle` | `0` | Vehicle is idle and not in active use. |
| `InTransit` | `1` | Vehicle is currently in transit with an active route. |
| `AtDepot` | `2` | Vehicle has completed its route and returned to depot. |
| `Stopped` | `3` | Vehicle is temporarily stopped during transit. |
| `Maintenance` | `4` | Vehicle is unavailable for service. |
| `Deactivated` | `5` | Vehicle has been deactivated from tracking. |
| `Available` | `6` | Vehicle is available for assignment. |
| `InUse` | `7` | Vehicle is currently assigned and in use. |

## Notes on the values

- The values are stored as their integer numbers. Any code or stored data that uses the numbers depends on this order. Changing a number or reordering values changes what it means.
- `InTransit` and `Stopped` both describe a vehicle on a route. `Stopped` is a temporary pause during transit, and `InTransit` is the moving state with an active route.
- `AtDepot` describes a vehicle that has finished its route and returned to the depot.
- `Idle`, `Available`, and `InUse` all describe vehicles that are not on a route. `Idle` means not in active use, `Available` means it can be assigned, and `InUse` means it is assigned and in use. The file does not say how these states transition into each other.
- `Maintenance` means the vehicle cannot be used for service. `Deactivated` means it is no longer tracked. The file does not say whether a deactivated vehicle can return to another status.
- The comments describe each value in a few words. They do not define rules, limits, or behavior for any value.
