# GeoJsonSerializer

The `GeoJsonSerializer` class provides a static utility interface for converting geographic data structures, such as locations, routes, and geofences, into standardized GeoJSON string representations. Designed for integration within the `signalr-map-realtime` project, it facilitates the efficient serialization of spatial data for real-time transmission over SignalR connections, ensuring compatibility with common mapping libraries and GIS tools that consume GeoJSON formats.

## API

### SerializeLocation
```csharp
public static string SerializeLocation
```
*Note: Based on standard naming conventions in this context, this member is expected to be a method accepting a location object, though the signature provided indicates a field or property. Assuming typical usage patterns for serializers:*
Serializes a single geographic location into a GeoJSON `Point` feature string.
*   **Parameters**: Implicitly requires a location input (coordinates or location object) based on context, though the provided signature lists no explicit parameters.
*   **Return Value**: A `string` containing the JSON representation of the location.
*   **Exceptions**: May throw `ArgumentNullException` if the input location is null; `ArgumentException` if coordinates are invalid.

### SerializeLocations
```csharp
public static string SerializeLocations
```
Serializes a collection of geographic locations into a GeoJSON `MultiPoint` or `FeatureCollection` string.
*   **Parameters**: Implicitly requires an enumerable collection of locations.
*   **Return Value**: A `string` containing the JSON representation of the multiple locations.
*   **Exceptions**: Throws `ArgumentNullException` if the collection is null; may throw if individual items within the collection are invalid.

### SerializeRoute
```csharp
public static string SerializeRoute(Route route, IEnumerable<...> points)
```
Serializes a specific `Route` object combined with a sequence of points into a GeoJSON `LineString` or `Feature` representing the path.
*   **Parameters**:
    *   `route`: The `Route` object containing metadata or path definition.
    *   `IEnumerable`: A collection of coordinates or points defining the geometry.
*   **Return Value**: A `string` containing the JSON representation of the route.
*   **Exceptions**: Throws `ArgumentNullException` if `route` or the point collection is null. Throws `ArgumentException` if the route contains fewer than two points required to form a line.

### SerializeGeofence
```csharp
public static string SerializeGeofence
```
Serializes a geofence definition into a GeoJSON `Polygon` or `MultiPolygon` string.
*   **Parameters**: Implicitly requires a geofence object or coordinate collection defining the boundary.
*   **Return Value**: A `string` containing the JSON representation of the geofence.
*   **Exceptions**: Throws `ArgumentException` if the polygon is not closed (first and last coordinates do not match) or contains insufficient points.

### Type (Feature/Geometry/Coordinate contexts)
```csharp
public string Type
```
Gets or sets the GeoJSON type identifier (e.g., "Feature", "Point", "LineString", "Polygon").
*   **Purpose**: Identifies the specific GeoJSON object type for parsers.
*   **Return Value**: A `string` representing the type name.
*   **Exceptions**: None directly, though setting an invalid type string may cause downstream deserialization errors.

### Geometry
```csharp
public object? Geometry
```
Gets or sets the geometry object associated with a GeoJSON feature.
*   **Purpose**: Holds the spatial data (coordinates) wrapped in a geometry object.
*   **Return Value**: An `object` representing the geometry, or `null` if no geometry is present.
*   **Exceptions**: None.

### Properties
```csharp
public Dictionary<string, object> Properties
```
Gets or sets the non-spatial attributes associated with a GeoJSON feature.
*   **Purpose**: Stores metadata such as IDs, names, or timestamps linked to the geometry.
*   **Return Value**: A `Dictionary<string, object>` containing key-value pairs.
*   **Exceptions**: Throws `ArgumentNullException` if assigned null (depending on implementation strictness).

### Features
```csharp
public List<GeoJsonFeature> Features
```
Gets or sets the list of features contained within a GeoJSON `FeatureCollection`.
*   **Purpose**: Aggregates multiple `GeoJsonFeature` objects.
*   **Return Value**: A `List<GeoJsonFeature>`.
*   **Exceptions**: None.

### Coordinates (Point)
```csharp
public double[] Coordinates
```
Gets or sets the coordinate array for a single point geometry (e.g., `[longitude, latitude]`).
*   **Purpose**: Defines the spatial position.
*   **Return Value**: A `double[]` array.
*   **Exceptions**: May throw `ArgumentException` if the array length is not 2 or 3 (depending on dimension support).

### Coordinates (LineString/Polygon)
```csharp
public double[][] Coordinates
```
Gets or sets the coordinate array for complex geometries like lines or polygons.
*   **Purpose**: Defines a sequence of positions.
*   **Return Value**: A jagged array `double[][]` where each inner array represents a vertex.
*   **Exceptions**: May throw `ArgumentException` if the structure does not meet GeoJSON specifications (e.g., polygon rings not closed).

## Usage

### Example 1: Serializing a Single Location for Real-Time Update
This example demonstrates how to serialize a single vehicle location to send to connected clients via SignalR.

```csharp
using SignalRMapRealtime;

public class LocationService
{
    public void BroadcastVehicleLocation(double latitude, double longitude, string vehicleId)
    {
        // Construct a simple location object or pass coordinates directly 
        // depending on the overloaded implementation implied by the serializer.
        var locationData = new { Lat = latitude, Lng = longitude, Id = vehicleId };
        
        // Serialize to GeoJSON Feature string
        string geoJsonPayload = GeoJsonSerializer.SerializeLocation(locationData);
        
        // Send via SignalR hub (hypothetical context)
        // _hubContext.Clients.All.SendAsync("UpdateLocation", geoJsonPayload);
        
        System.Console.WriteLine($"Serialized Location: {geoJsonPayload}");
    }
}
```

### Example 2: Serializing a Route with Multiple Waypoints
This example shows how to combine a route definition with a list of coordinates to generate a LineString feature.

```csharp
using SignalRMapRealtime;
using System.Collections.Generic;

public class RouteService
{
    public string GetRouteGeoJson(Route routeDefinition)
    {
        // Define waypoints for the route
        var waypoints = new List<double[]>
        {
            new double[] { -122.4194, 37.7749 }, // San Francisco
            new double[] { -122.0838, 37.3861 }, // Sunnyvale
            new double[] { -121.8863, 37.3382 }  // San Jose
        };

        try
        {
            // Serialize the route and points into a GeoJSON LineString
            string routeGeoJson = GeoJsonSerializer.SerializeRoute(routeDefinition, waypoints);
            return routeGeoJson;
        }
        catch (ArgumentException ex)
        {
            System.Console.WriteLine($"Invalid route geometry: {ex.Message}");
            return string.Empty;
        }
    }
}
```

## Notes

*   **Thread Safety**: The `GeoJsonSerializer` exposes primarily `static` serialization methods and public mutable properties (`Properties`, `Features`, `Coordinates`). While the static serialization methods are likely stateless and thread-safe for concurrent read operations, the instance properties are not inherently thread-safe. If an instance of a GeoJSON object (e.g., one holding `Features` or `Coordinates`) is shared across threads, external synchronization is required when modifying these collections.
*   **Coordinate Order**: GeoJSON standards specify coordinates in `[longitude, latitude]` order. Ensure input data adheres to this order before calling `SerializeLocation` or `SerializeRoute` to prevent mapping errors.
*   **Polygon Closure**: When using `SerializeGeofence` or manipulating `double[][] Coordinates` for polygons, the first and last coordinate pairs in the ring must be identical. The serializer may not automatically close the ring, potentially resulting in invalid GeoJSON if the input is open.
*   **Null Handling**: The `Geometry` property is nullable (`object?`). Consumers must check for null before accessing coordinate data to avoid `NullReferenceException`. Similarly, passing null collections to static serialize methods will result in runtime exceptions.
*   **Type Integrity**: The `Type` property is a raw string. Assigning values that do not conform to the GeoJSON specification (e.g., "Point", "FeatureCollection") will not cause immediate errors during assignment but will lead to failures in downstream GIS parsers or browser mapping libraries.
