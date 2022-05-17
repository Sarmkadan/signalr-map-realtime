# LocationRepositoryExtensions

`LocationRepositoryExtensions` provides a set of static extension methods for querying and aggregating location data from a repository. These methods encapsulate common patterns for retrieving vehicle locations by time range, session, type, proximity, and statistical summaries, reducing boilerplate in services that consume location data.

## API

### GetLatestLocationsByVehiclesAsync

```csharp
public static async Task<Dictionary<int, Location?>> GetLatestLocationsByVehiclesAsync(
    this ILocationRepository repository,
    IEnumerable<int> vehicleIds)
```

Returns the most recent known location for each specified vehicle. Vehicles for which no location exists are represented by a `null` value in the dictionary.

**Parameters:**
- `repository` — the location repository instance.
- `vehicleIds` — a collection of vehicle identifiers to look up.

**Returns:** a dictionary mapping each vehicle ID to its latest `Location`, or `null` if none is found.

**Throws:** `ArgumentNullException` when `repository` or `vehicleIds` is `null`.

---

### GetLatestLocationsWithVehicleDetailsAsync

```csharp
public static async Task<IEnumerable<(int VehicleId, Location? Location)>> GetLatestLocationsWithVehicleDetailsAsync(
    this ILocationRepository repository)
```

Retrieves the latest location for every vehicle that has at least one recorded location, returning the results as a sequence of tuples containing the vehicle ID and its most recent location.

**Parameters:**
- `repository` — the location repository instance.

**Returns:** an enumerable of tuples, each pairing a vehicle ID with its latest `Location`. Vehicles without any location data are omitted.

**Throws:** `ArgumentNullException` when `repository` is `null`.

---

### GetLocationsByTimeRangeAsync

```csharp
public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsByTimeRangeAsync(
    this ILocationRepository repository,
    IEnumerable<int> vehicleIds,
    DateTime startTime,
    DateTime endTime)
```

Groups all locations recorded within a given time window by vehicle.

**Parameters:**
- `repository` — the location repository instance.
- `vehicleIds` — the vehicles to include.
- `startTime` — inclusive start of the time window.
- `endTime` — inclusive end of the time window.

**Returns:** a dictionary mapping each vehicle ID to an ordered sequence of its `Location` records within the window. Vehicles with no matching locations are omitted.

**Throws:**
- `ArgumentNullException` when `repository` or `vehicleIds` is `null`.
- `ArgumentException` when `startTime` is later than `endTime`.

---

### GetLocationsBySessionsAsync

```csharp
public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsBySessionsAsync(
    this ILocationRepository repository,
    IEnumerable<int> sessionIds)
```

Retrieves all locations belonging to the specified sessions, grouped by vehicle ID.

**Parameters:**
- `repository` — the location repository instance.
- `sessionIds` — a collection of session identifiers.

**Returns:** a dictionary mapping each vehicle ID to its locations recorded during the given sessions.

**Throws:** `ArgumentNullException` when `repository` or `sessionIds` is `null`.

---

### GetLocationsByTypeAndVehicleAsync

```csharp
public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsByTypeAndVehicleAsync(
    this ILocationRepository repository,
    string locationType,
    IEnumerable<int> vehicleIds)
```

Filters locations by a specific type and groups them by vehicle.

**Parameters:**
- `repository` — the location repository instance.
- `locationType` — the type string to filter on (e.g., `"GPS"`, `"Manual"`).
- `vehicleIds` — the vehicles to include.

**Returns:** a dictionary mapping each vehicle ID to its locations of the specified type.

**Throws:**
- `ArgumentNullException` when `repository`, `locationType`, or `vehicleIds` is `null`.
- `ArgumentException` when `locationType` is empty or whitespace.

---

### GetRecentLocationsAsync

```csharp
public static async Task<Dictionary<int, IEnumerable<Location>>> GetRecentLocationsAsync(
    this ILocationRepository repository,
    IEnumerable<int> vehicleIds,
    TimeSpan duration)
```

Returns locations recorded within a recent time window ending at the current UTC time, grouped by vehicle.

**Parameters:**
- `repository` — the location repository instance.
- `vehicleIds` — the vehicles to include.
- `duration` — the lookback period from now.

**Returns:** a dictionary mapping each vehicle ID to its locations recorded within the last `duration`.

**Throws:**
- `ArgumentNullException` when `repository` or `vehicleIds` is `null`.
- `ArgumentOutOfRangeException` when `duration` is negative or zero.

---

### GetLocationsNearbyAsync

```csharp
public static async Task<Dictionary<int, IEnumerable<Location>>> GetLocationsNearbyAsync(
    this ILocationRepository repository,
    double latitude,
    double longitude,
    double radiusMeters,
    DateTime? since = null)
```

Finds locations within a circular geographic area, optionally restricted to a minimum timestamp.

**Parameters:**
- `repository` — the location repository instance.
- `latitude` — center point latitude.
- `longitude` — center point longitude.
- `radiusMeters` — search radius in meters.
- `since` — optional earliest timestamp; when `null`, no lower time bound is applied.

**Returns:** a dictionary mapping each vehicle ID to its locations that fall within the specified radius.

**Throws:**
- `ArgumentNullException` when `repository` is `null`.
- `ArgumentOutOfRangeException` when `radiusMeters` is negative or zero.
- `ArgumentException` when `latitude` or `longitude` is outside valid ranges.

---

### GetLocationStatsAsync

```csharp
public static async Task<Dictionary<int, (int count, double minSpeed, double maxSpeed, double avgSpeed)>> GetLocationStatsAsync(
    this ILocationRepository repository,
    IEnumerable<int> vehicleIds,
    DateTime? since = null)
```

Computes per-vehicle speed statistics across all locations, optionally filtered to those recorded after a given time.

**Parameters:**
- `repository` — the location repository instance.
- `vehicleIds` — the vehicles to include.
- `since` — optional earliest timestamp; when `null`, all locations are considered.

**Returns:** a dictionary mapping each vehicle ID to a tuple containing the count of locations, minimum speed, maximum speed, and average speed. Vehicles with no qualifying locations are omitted.

**Throws:** `ArgumentNullException` when `repository` or `vehicleIds` is `null`.

---

### GetLocationsInTimeRangeAndRadiusAsync

```csharp
public static async Task<IEnumerable<Location>> GetLocationsInTimeRangeAndRadiusAsync(
    this ILocationRepository repository,
    double latitude,
    double longitude,
    double radiusMeters,
    DateTime startTime,
    DateTime endTime)
```

Returns a flat sequence of locations that satisfy both a time window and a geographic radius constraint.

**Parameters:**
- `repository` — the location repository instance.
- `latitude` — center point latitude.
- `longitude` — center point longitude.
- `radiusMeters` — search radius in meters.
- `startTime` — inclusive start of the time window.
- `endTime` — inclusive end of the time window.

**Returns:** an enumerable of `Location` objects matching both the spatial and temporal filters.

**Throws:**
- `ArgumentNullException` when `repository` is `null`.
- `ArgumentOutOfRangeException` when `radiusMeters` is negative or zero.
- `ArgumentException` when `latitude` or `longitude` is outside valid ranges, or `startTime` is later than `endTime`.

---

### GetTotalDistanceTraveledAsync

```csharp
public static async Task<double> GetTotalDistanceTraveledAsync(
    this ILocationRepository repository,
    int vehicleId,
    DateTime? since = null)
```

Calculates the cumulative distance traveled by a single vehicle by summing the haversine distances between consecutive locations ordered by timestamp.

**Parameters:**
- `repository` — the location repository instance.
- `vehicleId` — the vehicle to calculate distance for.
- `since` — optional earliest timestamp; when `null`, all locations are used.

**Returns:** the total distance in meters. Returns `0.0` if the vehicle has fewer than two qualifying locations.

**Throws:** `ArgumentNullException` when `repository` is `null`.

---

### GetAverageSpeedAsync

```csharp
public static async Task<double> GetAverageSpeedAsync(
    this ILocationRepository repository,
    int vehicleId,
    DateTime? since = null)
```

Computes the arithmetic mean of the speed values recorded for a single vehicle.

**Parameters:**
- `repository` — the location repository instance.
- `vehicleId` — the vehicle to calculate for.
- `since` — optional earliest timestamp; when `null`, all locations are used.

**Returns:** the average speed in the unit stored on `Location`. Returns `0.0` if no qualifying locations exist.

**Throws:** `ArgumentNullException` when `repository` is `null`.

---

### GetLocationsByTypeAndTimeRangeAsync

```csharp
public static async Task<IEnumerable<Location>> GetLocationsByTypeAndTimeRangeAsync(
    this ILocationRepository repository,
    string locationType,
    DateTime startTime,
    DateTime endTime)
```

Returns locations of a specific type recorded within a time window, across all vehicles.

**Parameters:**
- `repository` — the location repository instance.
- `locationType` — the type string to filter on.
- `startTime` — inclusive start of the time window.
- `endTime` — inclusive end of the time window.

**Returns:** an enumerable of `Location` objects matching both the type and time constraints.

**Throws:**
- `ArgumentNullException` when `repository` or `locationType` is `null`.
- `ArgumentException` when `locationType` is empty or whitespace, or `startTime` is later than `endTime`.

## Usage

### Example 1: Dashboard — latest positions and recent activity

```csharp
var repo = serviceProvider.GetRequiredService<ILocationRepository>();
var activeVehicles = new[] { 101, 102, 103, 104 };

// Get current positions for all active vehicles
var latest = await repo.GetLatestLocationsByVehiclesAsync(activeVehicles);

// Get all locations from the last 15 minutes for these vehicles
var recent = await repo.GetRecentLocationsAsync(activeVehicles, TimeSpan.FromMinutes(15));

foreach (var vehicleId in activeVehicles)
{
    var current = latest[vehicleId];
    var trail = recent.TryGetValue(vehicleId, out var locations) ? locations : Enumerable.Empty<Location>();

    Console.WriteLine($"Vehicle {vehicleId}: current=({current?.Latitude},{current?.Longitude}), trail points={trail.Count()}");
}
```

### Example 2: Geofence monitoring with statistics

```csharp
var repo = serviceProvider.GetRequiredService<ILocationRepository>();
var fleetIds = Enumerable.Range(200, 50);
var depotLat = 52.5200;
var depotLon = 13.4050;
var fenceRadius = 5000; // 5 km

// Find all vehicles currently near the depot
var nearby = await repo.GetLocationsNearbyAsync(depotLat, depotLon, fenceRadius);

// Compute speed stats for the entire fleet over the last hour
var stats = await repo.GetLocationStatsAsync(fleetIds, DateTime.UtcNow.AddHours(-1));

foreach (var (vehicleId, (count, min, max, avg)) in stats)
{
    var isNearDepot = nearby.ContainsKey(vehicleId);
    Console.WriteLine($"Vehicle {vehicleId}: count={count}, avgSpeed={avg:F1}, nearDepot={isNearDepot}");
}
```

## Notes

- **Null handling:** methods returning `Dictionary<int, Location?>` use `null` values to distinguish vehicles with no recorded locations from those with a valid location. Callers should perform null checks before accessing location properties.
- **Empty collections:** methods that accept `IEnumerable<int>` for vehicle or session IDs return empty dictionaries or sequences when the input collection is empty, rather than throwing.
- **Ordering:** locations within returned `IEnumerable<Location>` sequences are ordered by timestamp ascending unless the underlying repository specifies otherwise.
- **Distance calculation:** `GetTotalDistanceTraveledAsync` uses haversine geometry and assumes `Location` exposes `Latitude` and `Longitude` in decimal degrees. The result is in meters.
- **Speed unit:** `GetAverageSpeedAsync` and `GetLocationStatsAsync` return speed values in whatever unit is stored on the `Location` object; no unit conversion is performed.
- **Thread safety:** these methods are static extension methods that delegate to the underlying `ILocationRepository`. Thread safety depends entirely on the implementation of that repository. The methods themselves hold no mutable state.
- **Time boundaries:** `startTime` and `endTime` parameters are inclusive on both ends. Overlapping queries that share boundary timestamps will include the same locations in both result sets.
- **Geographic constraints:** latitude must be in [-90, 90] and longitude in [-180, 180]. Values outside these ranges cause an `ArgumentException`.
