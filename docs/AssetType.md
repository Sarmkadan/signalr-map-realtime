# AssetType

Source: `src/SignalRMapRealtime/Domain/Enums/AssetType.cs`

The file lives in the `SignalRMapRealtime.Domain.Enums` namespace and defines one public type:

- `AssetType` - a public enum that classifies the tracked assets in the system. Its summary says it defines "the classification of tracked assets in the system."

The enum has no attributes and no methods. Its underlying type is the default `int`. The values are numbered from `0` to `8` in declaration order, and each value is set explicitly.

## Values

| Name | Value | Meaning (from the XML doc comment) |
|------|-------|------------------------------------|
| `DeliveryVan` | `0` | Delivery van or cargo vehicle. |
| `Courier` | `1` | Courier or personal transport. |
| `Bicycle` | `2` | Bicycle for urban delivery. |
| `Motorcycle` | `3` | Motorcycle or scooter. |
| `Portable` | `4` | Portable device or equipment. |
| `FixedAsset` | `5` | Fixed asset or container at location. |
| `Drone` | `6` | Drone or aerial vehicle. |
| `Equipment` | `7` | General equipment asset. |
| `Vehicle` | `8` | Vehicle asset classification. |

## Notes on the values

- The values are stored as their integer numbers. Any code or stored data that uses the numbers depends on this order. Changing a number or reordering values changes what it means.
- Some values overlap in meaning. `DeliveryVan`, `Courier`, `Bicycle`, `Motorcycle`, `Drone`, and `Vehicle` can all describe something that moves. `Vehicle` is a general "vehicle asset" classification, and the other vehicle-like values are more specific. The file does not say how to choose between them.
- `Portable` and `Equipment` both describe equipment. `Portable` refers to a portable device, and `Equipment` is a general equipment asset. The file does not say how they differ in practice.
- `FixedAsset` is the only value that describes an asset that does not move. Its comment says it is a fixed asset or container at a location.
- The comments describe each value in a few words. They do not define rules, limits, or behavior for any value.
