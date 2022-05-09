# LocationDto

A data transfer object representing a geographic location recorded by a vehicle, used primarily for real-time tracking and mapping in the SignalR-based system.

## API

### `Id`
- **Purpose**: Unique identifier for the location record.
- **Type**: `int`
- **Constraints**: Must be positive. Assigned by the system on creation.
- **Notes**: Used as a primary key in persistence layers.

### `Latitude`
- **Purpose**: Geographic coordinate indicating north–south position.
- **Type**: `double`
- **Constraints**: Must be between -90.0 and 90.0.
- **Notes**: Required field; invalid values may cause mapping or geofencing errors.

### `Longitude`
- **Purpose**: Geographic coordinate indicating east–west position.
- **Type**: `double`
- **Constraints**: Must be between -180.0 and 180.0.
- **Notes**: Required field; invalid values may cause mapping or geofencing errors.

### `Altitude`
- **Purpose**: Height above mean sea level in meters.
- **Type**: `double?`
- **Constraints**: Optional. If provided, must be a non-negative value.
- **Notes**: Used for 3D mapping and altitude-based filtering.

### `Accuracy`
- **Purpose**: Estimated horizontal accuracy of the location in meters.
- **Type**: `double?`
- **Constraints**: Optional. If provided, must be non-negative.
- **Notes**: Lower values indicate higher confidence in the location.

### `Speed`
- **Purpose**: Current speed of the vehicle in meters per second.
- **Type**: `double?`
- **Constraints**: Optional. If provided, must be non-negative.
- **Notes**: Derived from GPS or vehicle telemetry; used for motion analysis.

### `Bearing`
- **Purpose**: Direction of travel in degrees, relative to true north.
- **Type**: `double?`
- **Constraints**: Optional. If provided, must be between 0.0 and 360.0.
- **Notes**: Used for orientation in mapping UIs.

### `LocationType`
- **Purpose**: Categorizes the type of location (e.g., GPS, manual entry, inferred).
- **Type**: `LocationType` (enum)
- **Constraints**: Must be a valid enum value.
- **Notes**: Influences processing logic and display behavior.

### `Address`
- **Purpose**: Human-readable address derived from geocoding the coordinates.
- **Type**: `string?`
- **Constraints**: Optional. May be null if geocoding is unavailable.
- **Notes**: Typically populated asynchronously; not guaranteed to be current.

### `Notes`
- **Purpose**: Free-form text field for operator or system comments.
- **Type**: `string?`
- **Constraints**: Optional. No length restrictions.
- **Notes**: Used for logging context or anomalies.

### `VehicleId`
- **Purpose**: Foreign key referencing the vehicle associated with this location.
- **Type**: `int`
- **Constraints**: Must reference an existing vehicle.
- **Notes**: Used for filtering and aggregation by vehicle.

### `RecordedAt`
- **Purpose**: Timestamp when the location was captured by the vehicle or sensor.
- **Type**: `DateTime`
- **Constraints**: Must be in UTC or clearly timezone-aware.
- **Notes**: Critical for temporal analysis and replay.

### `CreatedAt`
- **Purpose**: Timestamp when the record was inserted into the system.
- **Type**: `DateTime`
- **Constraints**: Automatically set on creation; immutable.
- **Notes**: Used for auditing and data freshness tracking.

### `Timestamp`
- **Purpose**: Timestamp when the location data was transmitted or processed.
- **Type**: `DateTime`
- **Constraints**: Must be in UTC or clearly timezone-aware.
- **Notes**: May differ from `RecordedAt` due to transmission delays.

### `Heading`
- **Purpose**: Direction the vehicle is facing in degrees, relative to true north.
- **Type**: `double?`
- **Constraints**: Optional. If provided, must be between 0.0 and 360.0.
- **Notes**: Complementary to `Bearing`; used in navigation UIs.

## Usage

### Example 1: Creating and sending a location update
