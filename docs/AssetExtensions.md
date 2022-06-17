# AssetExtensions

Extension methods for working with asset models in real-time mapping scenarios. These utilities provide common checks and transformations for asset data, particularly useful in SignalR-based applications where asset states and locations are frequently updated.

## API

### `public static bool IsAssigned(Asset asset)`

Determines whether the given asset has been assigned to a specific location or entity.

- **Parameters**
  - `asset`: The asset instance to check. May be `null`.
- **Return value**
  - `true` if the asset is assigned (e.g., has a non-default location or assigned user); `false` otherwise.
- **Exceptions**
  - Throws `ArgumentNullException` if `asset` is `null`.

---

### `public static string GetAssetDetails(Asset asset)`

Generates a human-readable summary of the asset’s key properties for display or logging purposes.

- **Parameters**
  - `asset`: The asset instance to summarize. May be `null`.
- **Return value**
  - A formatted string containing asset ID, name, and current status. Returns `"Unassigned"` if `asset` is `null`.
- **Exceptions**
  - None.

---
### `public static bool NeedsSpecialHandling(Asset asset)`

Indicates whether the asset requires special processing logic, such as high-frequency updates or unique rendering behavior.

- **Parameters**
  - `asset`: The asset instance to evaluate. May be `null`.
- **Return value**
  - `true` if the asset has a flag or property indicating special handling is needed; `false` otherwise.
- **Exceptions**
  - None.

---
### `public static int GetLocationHistoryCount(Asset asset)`

Returns the number of historical location entries associated with the asset.

- **Parameters**
  - `asset`: The asset instance to inspect. May be `null`.
- **Return value**
  - The count of location history entries. Returns `0` if `asset` is `null` or has no history.
- **Exceptions**
  - None.

## Usage
