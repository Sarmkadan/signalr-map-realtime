# VehicleDtoExtensions

Provides extension methods for `VehicleDto` objects to determine online status, evaluate operational state, and format location and informational details for real-time mapping and monitoring scenarios.

## API

### `IsOnline`
Determines whether the vehicle is currently online based on its last reported timestamp and configured online threshold.

- **Parameters**: `VehicleDto vehicle` – The vehicle data transfer object to evaluate.
- **Return value**: `bool` – `true` if the vehicle is considered online; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `vehicle` is `null`.

### `IsInStatus`
Checks whether the vehicle is in a specific operational status.

- **Parameters**:
  - `VehicleDto vehicle` – The vehicle data transfer object to evaluate.
  - `VehicleStatus status` – The status to compare against.
- **Return value**: `bool` – `true` if the vehicle’s status matches the provided `status`; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `vehicle` is `null`.

### `GetStatusString`
Returns a human-readable string representation of the vehicle’s current status.

- **Parameters**: `VehicleDto vehicle` – The vehicle data transfer object.
- **Return value**: `string` – A localized or standardized status description (e.g., "Active", "Maintenance", "Offline").
- **Throws**: `ArgumentNullException` if `vehicle` is `null`.

### `GetLocationDetails`
Retrieves a formatted string containing the vehicle’s current location details, if available.

- **Parameters**: `VehicleDto vehicle` – The vehicle data transfer object.
- **Return value**: `string?` – A formatted location string (e.g., "Lat: 40.7128, Lon: -74.0060") or `null` if location data is missing or invalid.
- **Throws**: `ArgumentNullException` if `vehicle` is `null`.

### `GetInfoString`
Generates a concise informational string combining status and location details for display purposes.

- **Parameters**: `VehicleDto vehicle` – The vehicle data transfer object.
- **Return value**: `string` – A formatted string (e.g., "Active - Lat: 40.7128, Lon: -74.0060") summarizing the vehicle’s state.
- **Throws**: `ArgumentNullException` if `vehicle` is `null`.

### `RequiresAttention`
Indicates whether the vehicle requires operator or system attention based on its status and recent activity.

- **Parameters**: `VehicleDto vehicle` – The vehicle data transfer object to evaluate.
- **Return value**: `bool` – `true` if the vehicle’s status suggests attention is needed (e.g., maintenance required, prolonged offline); otherwise, `false`.
- **Throws**: `ArgumentNullException` if `vehicle` is `null`.

## Usage
