# GeoLocationBenchmarks

`GeoLocationBenchmarks` is a benchmarking utility for evaluating the performance of geospatial calculation methods. It provides a console-based harness that measures execution time and throughput for operations such as distance calculation, cardinal direction determination, bounding box generation, and coordinate validation. The class is designed to run as a standalone application via its `Main` entry point, producing benchmark reports for comparative analysis of geolocation logic.

## API

### `public double CalculateDistance`

Calculates the great-circle distance between two geographic coordinates using the Haversine formula.

**Parameters:**  
Implicitly operates on two coordinate pairs (latitude and longitude in decimal degrees) provided as input arguments or internal fields during benchmark execution.

**Return Value:**  
`double` — the distance in kilometers between the two points, rounded to a precision consistent with the benchmark’s configuration.

**Throws:**  
No exceptions are thrown directly by this member. Invalid or out-of-range coordinate values may produce `double.NaN` or physically meaningless results rather than raising errors.

---

### `public string GetCardinalDirection`

Determines the compass bearing direction from an origin coordinate to a destination coordinate.

**Parameters:**  
Implicitly receives origin and destination latitude/longitude pairs during benchmark iteration.

**Return Value:**  
`string` — one of the eight primary or intercardinal directions (`"N"`, `"NE"`, `"E"`, `"SE"`, `"S"`, `"SW"`, `"W"`, `"NW"`) representing the bearing sector.

**Throws:**  
No exceptions are thrown. For identical origin and destination points, the method returns a defined default direction (typically `"N"`).

---

### `public (double, double, double, double) GetBoundingBox`

Computes a bounding box around a central coordinate given a radius in kilometers.

**Parameters:**  
Implicitly receives a center latitude, center longitude, and radius value during benchmark execution.

**Return Value:**  
`(double, double, double, double)` — a tuple containing `(minLatitude, maxLatitude, minLongitude, maxLongitude)` in decimal degrees. The box encloses all points within the specified radius of the center.

**Throws:**  
No exceptions are thrown. Extreme radius values near or exceeding Earth’s circumference may produce bounding boxes that wrap around the antimeridian or extend beyond valid latitude ranges; these are returned as computed without clamping.

---

### `public bool IsValidCoordinate`

Validates whether a given latitude and longitude pair represents a legitimate geographic coordinate.

**Parameters:**  
Implicitly receives a latitude and longitude value during benchmark iteration.

**Return Value:**  
`bool` — `true` if latitude is in the range `[-90, 90]` and longitude is in the range `[-180, 180]`; `false` otherwise.

**Throws:**  
No exceptions are thrown. Boundary values (exactly ±90° latitude, ±180° longitude) are considered valid.

---

### `public static void Main`

The application entry point that orchestrates benchmark execution.

**Parameters:**  
`string[] args` — command-line arguments, if any, passed to the benchmark runner.

**Return Value:**  
`void` — outputs benchmark results to the console, including iteration counts, total elapsed time, and per-operation averages.

**Throws:**  
No exceptions are intentionally thrown. Unhandled exceptions from underlying benchmark infrastructure may propagate and terminate the process.

## Usage

### Example 1: Running the Full Benchmark Suite

```csharp
// Execute all geolocation benchmarks with default settings
GeoLocationBenchmarks.Main(args: Array.Empty<string>());

// Typical console output:
// | Method              | Mean      | Error     | StdDev    |
// |-------------------- |----------:|----------:|----------:|
// | CalculateDistance   | 12.34 ns  | 0.15 ns   | 0.13 ns   |
// | GetCardinalDirection| 45.67 ns  | 0.89 ns   | 0.79 ns   |
// | GetBoundingBox      | 89.01 ns  | 1.23 ns   | 1.09 ns   |
// | IsValidCoordinate   | 3.45 ns   | 0.05 ns   | 0.04 ns   |
```

### Example 2: Integrating Benchmark Methods in a Diagnostic Context

```csharp
// Validate and measure individual operations outside the harness
var benchmarks = new GeoLocationBenchmarks();

bool valid = benchmarks.IsValidCoordinate(latitude: 51.5074, longitude: -0.1278);
if (valid)
{
    double distance = benchmarks.CalculateDistance(
        lat1: 51.5074, lon1: -0.1278,
        lat2: 48.8566, lon2: 2.3522);
    
    string direction = benchmarks.GetCardinalDirection(
        lat1: 51.5074, lon1: -0.1278,
        lat2: 48.8566, lon2: 2.3522);
    
    var bbox = benchmarks.GetBoundingBox(
        centerLat: 51.5074, centerLon: -0.1278, radiusKm: 50.0);
    
    Console.WriteLine($"Distance: {distance:F2} km");
    Console.WriteLine($"Direction: {direction}");
    Console.WriteLine($"BoundingBox: {bbox}");
}
```

## Notes

- **Edge Cases — Coordinates:** `IsValidCoordinate` treats exactly ±90° latitude and ±180° longitude as valid. Values infinitesimally outside these bounds return `false`. `CalculateDistance` and `GetCardinalDirection` do not internally validate inputs; passing invalid coordinates may yield `double.NaN` distances or undefined direction strings.
- **Edge Cases — Bounding Box:** `GetBoundingBox` does not clamp results to valid coordinate ranges. A sufficiently large radius can produce latitude bounds exceeding ±90° or longitude bounds exceeding ±180°, which callers must handle if geographic clipping is required.
- **Edge Cases — Cardinal Direction:** When origin and destination are identical, `GetCardinalDirection` returns `"N"` by convention. Bearings exactly on sector boundaries (e.g., 22.5°) are assigned to a consistent sector based on the implementation’s rounding rule.
- **Thread Safety:** The class is designed for single-threaded benchmark execution. Instance methods are not guaranteed to be thread-safe; concurrent invocation from multiple threads without external synchronization may produce corrupted state or race conditions in internal benchmark tracking counters.
- **Performance Measurement:** `Main` relies on BenchmarkDotNet or similar infrastructure. Results are indicative of the specific runtime and hardware configuration at execution time. Microbenchmark timings in the nanosecond range are subject to noise from CPU frequency scaling, cache effects, and JIT compilation.
