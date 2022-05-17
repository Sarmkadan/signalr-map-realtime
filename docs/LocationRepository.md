# LocationRepository

`LocationRepository` provides data access operations for `Location` entities in the real-time signalR-based map application. It extends a generic base repository with `ApplicationDbContext`, offering specialized queries for retrieving vehicle locations by time, session, type, proximity, and statistical aggregation, as well as cleanup of stale records.

## API

### Constructor

```csharp
public LocationRepository(ApplicationDbContext context) : base
```

Initializes a new instance of the repository with the given `ApplicationDbContext`. The `context` is forwarded to the base repository class and must not be null.

### GetLatestLocationByVehicleAsync

```csharp
public async Task<Location?> GetLatestLocationByVehicleAsync(/* vehicle identifier */)
```

Returns the most recent `Location` recorded for a specific vehicle, or `null` if no locations exist. The vehicle identifier parameter is expected to match the entity's vehicle key.

### GetLocationsByTimeRangeAsync

```csharp
public async Task<IEnumerable<Location>> GetLocationsByTimeRangeAsync(DateTime from, DateTime to)
```

Retrieves all `Location` records whose timestamp falls within the inclusive `from` and exclusive `to` range. Returns an empty enumerable when no records match.

### GetLocationsBySessionAsync

```csharp
public async Task<IEnumerable<Location>> GetLocationsBySessionAsync(string sessionId)
```

Returns all `Location` entries belonging to the specified tracking session. Throws `ArgumentNullException` when `sessionId` is null or empty. Returns an empty enumerable for unknown sessions.

### GetLocationsByTypeAsync

```csharp
public async Task<IEnumerable<Location>> GetLocationsByTypeAsync(string type)
```

Filters `Location` records by a type discriminator (e.g., vehicle category or event type). Throws `ArgumentNullException` when `type` is null or empty. Returns an empty enumerable if the type has no associated records.

### GetRecentLocationsAsync

```csharp
public async Task<IEnumerable<Location>> GetRecentLocationsAsync(TimeSpan window)
```

Returns all `Location` entries recorded within the given `window` from the current UTC time. A zero or negative `window` yields an empty result set.

### GetLocationsNearbyAsync

```csharp
public async Task<IEnumerable<Location>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusMeters)
```

Retrieves `Location` records within `radiusMeters` of the specified geographic point. The distance calculation is performed on the database side where possible. Invalid coordinates (latitude outside ±90, longitude outside ±180) may cause an `ArgumentException`. A negative radius returns an empty enumerable.

### GetLocationStatsAsync

```csharp
public async Task<(int count, double minSpeed, double maxSpeed, double avgSpeed)> GetLocationStatsAsync(/* optional filter criteria */)
```

Computes aggregate statistics — total count, minimum speed, maximum speed, and average speed — over a subset of `Location` records defined by optional filter parameters. When no records match, returns `(0, 0, 0, 0)`. The average is calculated as the arithmetic mean of all matching speed values.

### DeleteOldLocationsAsync

```csharp
public async Task<int> DeleteOldLocationsAsync(DateTime cutoff)
```

Deletes all `Location` records with a timestamp older than the specified `cutoff`. Returns the number of rows removed. A `cutoff` in the future deletes no records and returns 0.

## Usage

### Retrieve recent locations and compute stats

```csharp
var repository = new LocationRepository(dbContext);

// Get locations from the last 5 minutes
var recent = await repository.GetRecentLocationsAsync(TimeSpan.FromMinutes(5));

// Compute speed stats for a specific vehicle session
var stats = await repository.GetLocationStatsAsync(sessionId: "session-abc123");
Console.WriteLine($"Count: {stats.count}, Avg speed: {stats.avgSpeed:F1} km/h");
```

### Proximity query with cleanup

```csharp
var repository = new LocationRepository(dbContext);

// Find all vehicles within 500 meters of a landmark
var nearby = await repository.GetLocationsNearbyAsync(48.8566, 2.3522, 500);

foreach (var loc in nearby)
{
    Console.WriteLine($"Vehicle {loc.VehicleId} at ({loc.Latitude}, {loc.Longitude})");
}

// Purge records older than 30 days
int removed = await repository.DeleteOldLocationsAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Cleaned up {removed} stale location records.");
```

## Notes

- All async methods internally rely on the `ApplicationDbContext` provided at construction. The context is not thread-safe; instances of `LocationRepository` should be scoped per unit of work (e.g., per request) and not shared across concurrent operations.
- Methods returning `IEnumerable<Location>` materialize the query results in memory. For large datasets, consider applying additional server-side filters before invocation.
- `GetLocationsNearbyAsync` may use database-specific spatial functions. If the underlying provider lacks spatial support, the implementation may fall back to an in-memory distance filter, which can degrade performance on large tables.
- `GetLocationStatsAsync` returns zero-initialized values when no records satisfy the filter criteria. Callers should check `count` before interpreting `avgSpeed` to avoid misleading zero averages.
- `DeleteOldLocationsAsync` executes a bulk delete. Depending on the database provider and volume, this may cause table locks or transaction log growth. Schedule cleanup during low-traffic windows for large datasets.
