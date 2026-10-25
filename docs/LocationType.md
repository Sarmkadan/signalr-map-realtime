# LocationType

Source: `src/SignalRMapRealtime/Domain/Enums/LocationType.cs`

The file lives in the `SignalRMapRealtime.Domain.Enums` namespace and defines one public type:

- `LocationType` - a public enum that categorizes the different types of locations in the tracking system. Its summary says it "categorizes different types of locations in the tracking system."

The enum has no attributes and no methods. Its underlying type is the default `int`. The values are numbered from `0` to `5` in declaration order, and each value is set explicitly.

## Values

| Name | Value | Meaning (from the XML doc comment) |
|------|-------|------------------------------------|
| `TrackPoint` | `0` | Location point recorded during vehicle transit. |
| `DeliveryPoint` | `1` | Delivery or service location. |
| `Depot` | `2` | Vehicle depot or base station. |
| `CustomerLocation` | `3` | Customer or client location. |
| `Waypoint` | `4` | Waypoint on the planned route. |
| `Geofence` | `5` | Geofence boundary or area. |

## Notes on the values

- The values are stored as their integer numbers. Any code or stored data that uses the numbers depends on this order. Changing a number or reordering values changes what it means.
- `TrackPoint` is the only value whose comment ties it to motion. It describes a point recorded while a vehicle is in transit.
- `DeliveryPoint`, `Depot`, and `CustomerLocation` describe places in the business process: where a delivery or service happens, where vehicles are based, and where customers are. The file does not say how these differ in practice.
- `Waypoint` refers to a point on a planned route. `TrackPoint` refers to a recorded point. The file does not say whether a route waypoint can also be stored as a track point.
- `Geofence` is named after the geofence feature, but its comment describes it as a "boundary or area". The file does not say how it relates to the separate geofence types in the domain model.
- The comments describe each value in a few words. They do not define rules, limits, or behavior for any value.
