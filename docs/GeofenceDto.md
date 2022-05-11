# GeofenceDto

A data transfer object representing a geofence configuration for real-time map applications. Used to define geographic boundaries (circular or polygonal) for monitoring and event triggering in the `signalr-map-realtime` project.

## API

### Properties

- **`Id`** (Guid)
  A unique identifier for the geofence. Used as a primary key in storage and for reference in real-time events.

- **`Name`** (string)
  A human-readable name for the geofence. Required for display and identification in the UI.

- **`Description`** (string?)
  Optional descriptive text providing additional context about the geofence. May be null.

- **`Type`** (string)
  Indicates the geometric type of the geofence: `"Circle"` for circular boundaries defined by center and radius, or `"Polygon"` for boundaries defined by a series of coordinates. Must not be null or empty.

- **`IsActive`** (bool)
  Flag indicating whether the geofence is currently active and should be monitored. Defaults to `true` when creating new entries.

- **`CenterLatitude`** (double?)
  Latitude of the center point for circular geofences. Required when `Type` is `"Circle"`. Must be a valid geographic coordinate between -90 and 90. Null otherwise.

- **`CenterLongitude`** (double?)
  Longitude of the center point for circular geofences. Required when `Type` is `"Circle"`. Must be a valid geographic coordinate between -180 and 180. Null otherwise.

- **`RadiusKm`** (double?)
  Radius in kilometers for circular geofences. Must be a positive number when `Type` is `"Circle"`. Null otherwise.

- **`PolygonCoordinates`** (string?)
  A semicolon-separated list of latitude,longitude pairs defining the vertices of a polygonal geofence (e.g., `"40.7128,-74.0060;34.0522,-118.2437"`). Required when `Type` is `"Polygon"`. Must be a valid, non-empty string of coordinates in correct format. Null otherwise.

- **`CreatedAt`** (DateTime)
  Timestamp indicating when the geofence was first created. Set automatically by the system; not user-modifiable.

- **`UpdatedAt`** (DateTime)
  Timestamp indicating the last time the geofence was updated. Automatically updated by the system on modification.

- **`CreatedBy`** (string?)
  Identifier (e.g., username or system account) of the user or process that created the geofence. May be null if created by an automated system.

## Usage

### Creating a Circular Geofence
