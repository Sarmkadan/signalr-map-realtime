# ThrottleOptions

Configuration options for controlling the frequency of real-time updates delivered to different types of assets in the SignalR real-time mapping system. These options allow fine-grained throttling of update rates based on asset type to balance performance and data freshness.

## API

### `Enabled`
Gets or sets a value indicating whether throttling is active. When `false`, all asset types receive updates at the maximum rate without throttling.

- **Type**: `bool`
- **Default**: `true`
- **Remarks**: Disabling throttling overrides all interval settings and delivers updates as soon as they are available.

---

### `DeliveryVanIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for delivery van assets.

- **Type**: `int`
- **Default**: `30`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---

### `CourierIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for courier assets.

- **Type**: `int`
- **Default**: `20`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---
### `BicycleIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for bicycle assets.

- **Type**: `int`
- **Default**: `15`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---
### `MotorcycleIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for motorcycle assets.

- **Type**: `int`
- **Default**: `10`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---
### `PortableIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for portable assets.

- **Type**: `int`
- **Default**: `5`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---
### `FixedAssetIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for fixed asset types.

- **Type**: `int`
- **Default**: `60`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---
### `DroneIntervalSeconds`
Gets or sets the minimum interval, in seconds, between updates for drone assets.

- **Type**: `int`
- **Default**: `5`
- **Remarks**:
  - Must be a positive integer.
  - A value of `0` or negative values are treated as `1` (updates every second).

---
### `GetIntervalForAssetType(AssetType assetType)`
Returns the throttling interval for the specified asset type.

- **Parameters**:
  - `assetType`: The type of asset for which to retrieve the interval.
- **Returns**: `TimeSpan` representing the minimum interval between updates for the given asset type.
- **Throws**: `ArgumentOutOfRangeException` if `assetType` is not a defined value in the `AssetType` enum.

## Usage

### Example 1: Configuring ThrottleOptions in Startup
