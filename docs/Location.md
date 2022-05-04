# Location

Represents a geographical point recorded in the `signalr-map-realtime` system, typically associated with a vehicle's position during a tracking session. This type stores coordinate data (latitude, longitude, optional altitude), accuracy metrics, movement attributes (speed, bearing, heading), and contextual information such as address, notes, and timestamps. It also provides utility methods for distance calculation and coordinate validation.

## API

### `public int Id`
Unique identifier for the location record in the database.

### `public double Latitude`
The latitude coordinate in decimal degrees (WGS 84). Valid range: -90.0 to 90.0. Values outside this range indicate invalid data.

### `public double Longitude`
The longitude coordinate in decimal degrees (WGS 84). Valid range: -180.0 to 180.0. Values outside this range indicate invalid data.

### `public double? Altitude`
Optional altitude in meters above sea level (WGS 84). `null` if not provided or unavailable.

### `public double? Accuracy`
Optional horizontal accuracy of the coordinate in meters. Lower values indicate higher precision. `null` if not provided or unavailable.

### `public double? Speed`
Optional ground speed in meters per second at the time of recording. `null` if not provided or unavailable.

### `public double? Bearing`
Optional direction of travel in decimal degrees (0-360, where 0 is true north). `null` if not provided or unavailable.

### `public LocationType LocationType`
Categorizes the location record (e.g., GPS, manual entry, or estimated). See `LocationType` enum for possible values.

### `public string? Address`
Optional human-readable address derived from reverse geocoding. `null` if not resolved or unavailable.

### `public string? Notes`
Optional user-provided annotations or context for the location. `null` if not provided.

### `public int VehicleId`
Foreign key referencing the associated `Vehicle` record. Zero or negative values indicate no association.

### `public Vehicle? Vehicle`
Navigation property to the associated `Vehicle` entity. `null` if not loaded or if `VehicleId` is invalid.

### `public int TrackingSessionId`
Foreign key referencing the associated `TrackingSession` record. Zero or negative values indicate no association.

### `public TrackingSession? TrackingSession`
Navigation property to the associated `TrackingSession` entity. `null` if not loaded or if `TrackingSessionId` is invalid.

### `public DateTime RecordedAt`
Timestamp when the location was recorded by the device or system. May differ from `CreatedAt` in imported or historical data.

### `public DateTime CreatedAt`
Timestamp when the location record was created in the database. Automatically set on insertion.

### `public DateTime Timestamp`
Alias for `RecordedAt`. Provides backward compatibility for legacy systems.

### `public double? Heading`
Optional compass direction in decimal degrees (0-360, where 0 is true north). Similar to `Bearing` but may represent static orientation rather than movement direction. `null` if not provided or unavailable.

### `public double CalculateDistanceTo(Location other)`
Calculates the great-circle distance in meters between this location and another `Location` using the Haversine formula.

**Parameters:**
- `other`: The target `Location` to measure distance to. Must not be `null`.

**Returns:**
Distance in meters. Returns `double.NaN` if either location's coordinates are invalid.

**Throws:**
- `ArgumentNullException`: If `other` is `null`.

### `public bool IsValidCoordinate()`
Validates whether the `Latitude` and `Longitude` values are within their respective valid ranges (-90 to 90 and -180 to 180).

**Returns:**
`true` if coordinates are valid; otherwise, `false`.

## Usage

### Example 1: Recording a Vehicle Location
