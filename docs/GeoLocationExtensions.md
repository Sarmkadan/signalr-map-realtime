# GeoLocationExtensions

Provides a collection of static extension methods for geographic coordinate calculations, validation, and formatting. Designed for use in real-time mapping applications, this class supports distance and bearing computations, coordinate validation, bounding box generation, and unit conversions without requiring any instance state.

## API

### `DistanceTo(double lat1, double lon1, double lat2, double lon2)`
Calculates the great-circle distance between two geographic coordinates using the Haversine formula.  
**Parameters:**  
- `lat1`, `lon1` – Latitude and longitude of the first point (in decimal degrees).  
- `lat2`, `lon2` – Latitude and longitude of the second point (in decimal degrees).  

**Returns:** `double` – Distance in kilometers.  
**Throws:** `ArgumentOutOfRangeException` if any coordinate is invalid (latitude outside [-90, 90] or longitude outside [-180, 180]).

### `DistanceBetween(double lat1, double lon1, double lat2, double lon2)`
Alias for `DistanceTo`; computes the great-circle distance between two points.  
**Parameters:** Same as `DistanceTo`.  
**Returns:** `double` – Distance in kilometers.  
**Throws:** Same as `DistanceTo`.

### `KilometersToMiles(double kilometers)`
Converts a distance from kilometers to miles.  
**Parameters:**  
- `kilometers` – Distance in kilometers.  

**Returns:** `double` – Equivalent distance in miles.  
**Throws:** None.

### `MilesToKilometers(double miles)`
Converts a distance from miles to kilometers.  
**Parameters:**  
- `miles` – Distance in miles.  

**Returns:** `double` – Equivalent distance in kilometers.  
**Throws:** None.

### `KilometersToMeters(double kilometers)`
Converts a distance from kilometers to meters.  
**Parameters:**  
- `kilometers` – Distance in kilometers.  

**Returns:** `double` – Equivalent distance in meters.  
**Throws:** None.

### `IsValidLatitude(double latitude)`
Validates whether a value is a plausible latitude.  
**Parameters:**  
- `latitude` – Value to test.  

**Returns:** `bool` – `true` if the value is between -90 and 90 (inclusive); otherwise `false`.  
**Throws:** None.

### `IsValidLongitude(double longitude)`
Validates whether a value is a plausible longitude.  
**Parameters:**  
- `longitude` – Value to test.  

**Returns:** `bool` – `true` if the value is between -180 and 180 (inclusive); otherwise `false`.  
**Throws:** None.

### `IsValidCoordinate(double latitude, double longitude)`
Checks whether both latitude and longitude are within their valid ranges.  
**Parameters:**  
- `latitude`, `longitude` – Coordinates to validate.  

**Returns:** `bool` – `true` if both are valid; otherwise `false`.  
**Throws:** None.

### `BearingTo(double lat1, double lon1, double lat2, double lon2)`
Computes the initial bearing (forward azimuth) from the first point to the second point.  
**Parameters:** Same as `DistanceTo`.  
**Returns:** `double` – Bearing in decimal degrees (0° = north, 90° = east, etc.), normalized to [0, 360).  
**Throws:** `ArgumentOutOfRangeException` if any coordinate is invalid.

### `GetCardinalDirection(double bearing)`
Converts a bearing in degrees to a human-readable cardinal or intercardinal direction.  
**Parameters:**  
- `bearing` – Bearing in decimal degrees (0–360).  

**Returns:** `string` – One of: `"N"`, `"NNE"`, `"NE"`, `"ENE"`, `"E"`, `"ESE"`, `"SE"`, `"SSE"`, `"S"`, `"SSW"`, `"SW"`, `"WSW"`, `"W"`, `"WNW"`, `"NW"`, `"NNW"`.  
**Throws:** `ArgumentOutOfRangeException` if `bearing` is negative or greater than 360.

### `IsWithinRadius(double centerLat, double centerLon, double targetLat, double targetLon, double radiusKm)`
Determines whether a target point lies within a specified radius (in kilometers) from a center point.  
**Parameters:**  
- `centerLat`, `centerLon` – Center coordinate.  
- `targetLat`, `targetLon` – Target coordinate.  
- `radiusKm` – Radius in kilometers.  

**Returns:** `bool` – `true` if the great-circle distance from center to target is ≤ `radiusKm`; otherwise `false`.  
**Throws:** `ArgumentOutOfRangeException` if any coordinate is invalid or `radiusKm` is negative.

### `GetBoundingBox(double centerLat, double centerLon, double radiusKm)`
Calculates a bounding box (minimum and maximum latitude/longitude) that encloses a circle of given radius around a center point.  
**Parameters:**  
- `centerLat`, `centerLon` – Center coordinate.  
- `radiusKm` – Radius in kilometers.  

**Returns:** `(double MinLat, double MinLon, double MaxLat, double MaxLon)` – Tuple representing the bounding box corners.  
**Throws:** `ArgumentOutOfRangeException` if the center coordinate is invalid or `radiusKm` is negative.

### `FormatCoordinates(double latitude, double longitude, int decimalPlaces = 6)`
Formats a coordinate pair into a human-readable string.  
**Parameters:**  
- `latitude`, `longitude` – Coordinates to format.  
- `decimalPlaces` – Number of decimal places for the output (default 6).  

**Returns:** `string` – Formatted as `"lat, lon"` (e.g., `"48.8566, 2.3522"`).  
**Throws:** `ArgumentOutOfRangeException` if `decimalPlaces` is negative.

## Usage

### Example 1: Distance and Bearing Between Two Cities
```csharp
using SignalrMapRealtime; // Namespace assumed

double latParis = 48.8566, lonParis = 2.3522;
double latTokyo = 35.6762, lonTokyo = 139.6503;

double distanceKm = GeoLocationExtensions.DistanceTo(latParis, lonParis, latTokyo, lonTokyo);
double distanceMi = GeoLocationExtensions.KilometersToMiles(distanceKm);
double bearing = GeoLocationExtensions.BearingTo(latParis, lonParis, latTokyo, lonTokyo);
string direction = GeoLocationExtensions.GetCardinalDirection(bearing);

Console.WriteLine($"Paris → Tokyo: {distanceKm:F1} km ({distanceMi:F1} mi), bearing {bearing:F1}° ({direction})");
```

### Example 2: Validating Coordinates and Checking Proximity
```csharp
double userLat = 40.7128, userLon = -74.0060; // New York
double venueLat = 40.7580, venueLon = -73.9855; // Times Square
double radiusKm = 5.0;

if (GeoLocationExtensions.IsValidCoordinate(userLat, userLon) &&
    GeoLocationExtensions.IsValidCoordinate(venueLat, venueLon))
{
    bool nearby = GeoLocationExtensions.IsWithinRadius(userLat, userLon, venueLat, venueLon, radiusKm);
    Console.WriteLine(nearby ? "Venue is within 5 km." : "Venue is farther than 5 km.");
    
    var bbox = GeoLocationExtensions.GetBoundingBox(userLat, userLon, radiusKm);
    Console.WriteLine($"Bounding box: {bbox.MinLat:F4} to {bbox.MaxLat:F4} lat, {bbox.MinLon:F4} to {bbox.MaxLon:F4} lon");
}
```

## Notes

- **Coordinate Validation:** All methods that accept latitude/longitude parameters will throw `ArgumentOutOfRangeException` if the values are outside the standard ranges. Use `IsValidCoordinate` to pre-validate input when necessary.
- **Edge Cases:**  
  - Distance calculations between antipodal points may produce results near 20,000 km (half the Earth’s circumference).  
  - `BearingTo` returns the initial bearing; for long paths the bearing changes along the great circle.  
  - `GetBoundingBox` approximates the bounding box using a spherical Earth model; near the poles the box may be distorted.  
  - `GetCardinalDirection` uses 16-point compass rose; bearings exactly on boundaries (e.g., 22.5°) are assigned to the next direction (e.g., NNE).
- **Thread Safety:** All members are static and operate only on their parameters; no shared mutable state exists. The class is inherently thread-safe.
- **Unit Conversions:** `KilometersToMiles`, `MilesToKilometers`, and `KilometersToMeters` use standard conversion factors and accept any non-negative input (negative values are not validated but produce mathematically correct negative results).
