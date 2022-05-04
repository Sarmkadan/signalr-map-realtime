# Geofence

The `Geofence` type represents a geographic boundary used for location-based monitoring within the `signalr-map-realtime` project. It can define either a circular area (using a center point and radius) or a polygonal area (using a set of coordinates), enabling real-time tracking of whether a given point lies within the defined boundary.

## API

### `public Guid Id`
A unique identifier for the geofence. Generated automatically upon creation and immutable thereafter.

### `public string Name`
A human-readable name for the geofence. Must not be null or empty when persisted.

### `public string? Description`
An optional description providing additional context about the geofence. May be null.

### `public GeofenceType Type`
Specifies the shape of the geofence. Valid values are:
- `GeofenceType.Circular` – Defined by `CenterLatitude`, `CenterLongitude`, and `RadiusKm`.
- `GeofenceType.Polygonal` – Defined by `PolygonCoordinates`.

### `public bool IsActive`
Indicates whether the geofence is currently active for monitoring. Inactive geofences are ignored during point-inclusion checks.

### `public double? CenterLatitude`
The latitude of the center point for a circular geofence. Must be between -90.0 and 90.0 if provided. Null for polygonal geofences.

### `public double? CenterLongitude`
The longitude of the center point for a circular geofence. Must be between -180.0 and 180.0 if provided. Null for polygonal geofences.

### `public double? RadiusKm`
The radius (in kilometers) for a circular geofence. Must be a positive value if provided. Null for polygonal geofences.

### `public string? PolygonCoordinates`
A JSON-formatted string representing the vertices of a polygonal geofence. Expected format:
