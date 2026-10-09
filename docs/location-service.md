# LocationService

`LocationService` (`src/SignalRMapRealtime/Services/LocationService.cs`) implements `ILocationService`. It is the business-logic layer for vehicle location points: recording and validating new points, reading latest/historical/nearby data, computing distances and trip statistics, and cleaning up old records. It sits between the REST controllers and the `LocationRepository` / `VehicleRepository` data layer, and returns DTOs mapped with AutoMapper.

## Dependencies

Constructor parameters (all null-checked with `ArgumentNullException.ThrowIfNull`):

| Dependency | Purpose |
|---|---|
| `LocationRepository` | Persistence and queries for `Location` entities |
| `VehicleRepository` | Vehicle lookup and update (the vehicle keeps its latest location) |
| `IMapper` | Entity to DTO mapping |
| `ILogger<LocationService>` | Structured logging |

The service has no state of its own. It is registered as scoped in `src/SignalRMapRealtime/Configuration/DependencyInjection.cs`.

## Methods

### Write operations

**`RecordLocationAsync(CreateLocationDto, CancellationToken)`** returns `LocationDto`

1. Validates coordinates with `ValidateCoordinates`. Invalid values throw `InvalidLocationException(lat, lng)`.
2. Loads the vehicle by `VehicleId`. If missing, throws `VehicleNotFoundException`.
3. Creates a `Location` with `RecordedAt` and `CreatedAt` set to `DateTime.UtcNow`. The server clock is authoritative. Any client timestamp is not used.
4. Saves the location, then calls `vehicle.RecordLocation(location)` and saves the vehicle. These are two separate `SaveChangesAsync` calls, so a failure between them can leave the location saved without the vehicle update.

**`UpdateLocationAsync(Guid, UpdateLocationDto, CancellationToken)`** returns `LocationDto`

Partial update. Only non-null `Speed`, `Bearing`, `LocationType` and non-whitespace `Address` / `Notes` are applied. Coordinates and timestamps cannot be changed. If the location does not exist, throws `InvalidLocationException("Location not found.")`.

**`DeleteLocationAsync(Guid, CancellationToken)`** returns `bool`

Removes the location. Returns `false` if it does not exist (no exception).

**`CleanupOldLocationsAsync(int daysOld = 90, CancellationToken)`** returns `int`

Delegates to `LocationRepository.DeleteOldLocationsAsync(daysOld)` and returns the number of deleted rows. Intended to be called from a background job.

### Read operations

| Method | Behaviour |
|---|---|
| `GetLatestLocationAsync(int vehicleId, ct)` | Latest location of one vehicle, or `null`. |
| `GetLocationHistoryAsync(int vehicleId, DateTime start, DateTime end, ct)` | All points in a time range, via the repository. |
| `GetRecentLocationsAsync(int vehicleId, int hoursBack = 24, ct)` | Points from the last N hours. |
| `GetVehicleLocationHistoryAsync(int vehicleId, int page, int pageSize, DateTime? start, DateTime? end, ct)` | Paginated history, newest first. Page size is normalized with `PaginationExtensions.NormalizePaginationParameters(..., 100)`. Start and end are inclusive. |
| `GetLocationsAsync(int page, int pageSize, ct)` | Paginated list of all locations. |
| `GetLocationByIdAsync(Guid, ct)` | One location by ID, or `null`. |
| `GetLocationsByTypeAsync(LocationType, ct)` | All locations of one type. |
| `GetLocationsNearbyAsync(double lat, double lng, double radiusKm = 1.0, ct)` | Points within a radius, via the repository. |
| `GetNearestAssets(double lat, double lng, int count, ct)` | The `count` vehicles closest to a point, using only each vehicle's latest location (haversine distance). |
| `GetLocationStatsAsync(int vehicleId, DateTime start, DateTime end, ct)` | Returns `LocationStatsDto` with point count, min/max/average speed, and total distance. |

### Pure helpers

- **`ValidateCoordinates(double lat, double lng)`** checks the ranges in `LocationConstants` (`MinLatitude`..`MaxLatitude`, `MinLongitude`..`MaxLongitude`). Logs a warning on failure.
- **`CalculateDistance(lat1, lon1, lat2, lon2)`** returns the great-circle distance in km using the haversine formula and `LocationConstants.EarthRadiusKm`.

`GetLocationStatsAsync` sums the distance between consecutive points, ordered by `RecordedAt`. The total is therefore the path length, not the straight-line distance between the first and last point.

## Error handling

| Exception | Thrown by |
|---|---|
| `InvalidLocationException` | `RecordLocationAsync` (bad coordinates), `UpdateLocationAsync` (not found) |
| `VehicleNotFoundException` | `RecordLocationAsync` |

Not-found reads return `null`. Not-found deletes return `false`. Callers (controllers) translate these into HTTP responses.

## Logging

- `Information`: record, update, delete, cleanup start and result.
- `Debug`: read queries, with their parameters.
- `Warning`: invalid coordinates, missing vehicle or location.

Coordinates and vehicle IDs are logged as structured fields, not string concatenation.

## Known limitations

These are current behaviours of the code. Review them before relying on the service in a new path.

1. **`RecordLocationAsync` logs before the null check.** The first log statement reads `locationDto.VehicleId` and the other fields, so a `null` argument raises `NullReferenceException` instead of `ArgumentNullException`. The `ThrowIfNull` call comes after the log.
2. **`GetLocationsAsync` and `GetNearestAssets` load every location into memory.** Both call `GetAllAsync` and filter or paginate in C#. `GetNearestAssets` is a full table scan per request.
3. **`GetVehicleLocationHistoryAsync` also loads all locations** (`GetAllAsync`) and filters by vehicle in memory. Time filters and paging are applied after the load. Repository-level filtering would scale better.
4. **`GetLocationsAsync` ignores the page number in the query.** It fetches everything and then applies `PaginatedResponse.FromList`, so memory use grows with table size.
5. **Two writes are not atomic.** `RecordLocationAsync` saves the location and the vehicle in separate transactions.
6. **Some methods drop the `CancellationToken`.** `GetLatestLocationAsync`, `GetLocationHistoryAsync`, `GetRecentLocationsAsync`, `GetLocationsNearbyAsync`, `GetLocationsByTypeAsync`, `GetLocationStatsAsync` and `CleanupOldLocationsAsync` do not pass the token to the repository, so they cannot be cancelled mid-query.
7. **`GetNearestAssets` is not named `...Async`** but returns a `Task`. Its name differs from the rest of the interface.

## Related

- `ILocationService` (`Services/ILocationService.cs`) - the contract.
- `LocationController` - REST entry point.
- `LocationHub` - SignalR hub for live updates.
- `LocationServiceTests` (`tests/signalr-map-realtime.Tests/LocationServiceTests.cs`) - unit tests.
