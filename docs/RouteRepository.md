# RouteRepository

`RouteRepository` is a data-access component in the `signalr-map-realtime` project that encapsulates database operations for `Route` entities. It inherits from a base repository class and uses `ApplicationDbContext` to interact with the underlying relational store. The repository provides a set of asynchronous query methods tailored to common route‑related scenarios such as filtering by vehicle, user, completion status, date range, and performance metrics like longest routes or average completion time.

## API

### `RouteRepository(ApplicationDbContext context)`

Initializes a new instance of the repository with the given database context.

- **Parameters**  
  `context` – An instance of `ApplicationDbContext` that provides access to the database.
- **Exceptions**  
  `ArgumentNullException` – if `context` is `null`.

### `Task<IEnumerable<Route>> GetActiveRoutesByVehicleAsync(…)`

Retrieves all routes that are currently active (i.e., not completed or cancelled) for a specified vehicle.

- **Parameters**  
  `vehicleId` – The unique identifier of the vehicle (type depends on the application, typically `int`, `Guid`, or `string`).
- **Returns**  
  A collection of `Route` objects representing active routes for the given vehicle.
- **Exceptions**  
  `InvalidOperationException` – if the underlying query fails due to a data integrity issue.

### `Task<IEnumerable<Route>> GetRoutesByUserAsync(…)`

Returns all routes assigned to or created by a specific user.

- **Parameters**  
  `userId` – The identifier of the user (type depends on the application, typically `string` or `Guid`).
- **Returns**  
  A collection of `Route` objects associated with the user.
- **Exceptions**  
  None documented.

### `Task<IEnumerable<Route>> GetRoutesByCompletionAsync(…)`

Filters routes based on their completion status.

- **Parameters**  
  `isCompleted` – A boolean indicating whether to return completed (`true`) or incomplete (`false`) routes.
- **Returns**  
  A collection of `Route` objects matching the specified completion state.
- **Exceptions**  
  None documented.

### `Task<Route?> GetRouteWithDetailsAsync(…)`

Retrieves a single route together with its related details (e.g., waypoints, stops, or associated data).

- **Parameters**  
  `routeId` – The unique identifier of the route.
- **Returns**  
  A `Route` object with eagerly loaded navigation properties, or `null` if no route with the given identifier exists.
- **Exceptions**  
  None documented.

### `Task<IEnumerable<Route>> GetRoutesByDateRangeAsync(…)`

Returns all routes whose creation or scheduled date falls within a specified date range.

- **Parameters**  
  `startDate` – The inclusive start of the date range (type `DateTime` or `DateTimeOffset`).  
  `endDate` – The inclusive end of the date range.
- **Returns**  
  A collection of `Route` objects whose relevant date property lies between `startDate` and `endDate`.
- **Exceptions**  
  `ArgumentException` – if `startDate` is later than `endDate`.

### `Task<IEnumerable<Route>> GetLongestRoutesAsync(…)`

Retrieves the top‑N routes ordered by distance or duration (the exact metric is implementation‑defined).

- **Parameters**  
  `count` – The maximum number of routes to return (type `int`).
- **Returns**  
  A collection of up to `count` `Route` objects, sorted by length in descending order.
- **Exceptions**  
  `ArgumentOutOfRangeException` – if `count` is less than 1.

### `Task<double?> GetAverageCompletionTimeAsync(…)`

Calculates the average time taken to complete routes, typically measured in minutes or hours.

- **Parameters**  
  (Optional) A filter parameter such as a date range or vehicle identifier; the exact signature is not exposed in the public API.
- **Returns**  
  A `double?` representing the average completion time, or `null` if no completed routes exist.
- **Exceptions**  
  None documented.

### `Task<IEnumerable<Route>> GetPendingRoutesAsync(…)`

Returns all routes that have not yet been started or are awaiting assignment.

- **Parameters**  
  None.
- **Returns**  
  A collection of `Route` objects in a pending state.
- **Exceptions**  
  None documented.

## Usage

### Example 1: Retrieve active routes for a vehicle and display their identifiers

```csharp
public async Task ShowActiveRoutesForVehicleAsync(RouteRepository repository, int vehicleId)
{
    var activeRoutes = await repository.GetActiveRoutesByVehicleAsync(vehicleId);
    foreach (var route in activeRoutes)
    {
        Console.WriteLine($"Active route ID: {route.Id}");
    }
}
```

### Example 2: Calculate average completion time and log a warning if no data exists

```csharp
public async Task LogAverageCompletionTimeAsync(RouteRepository repository)
{
    double? avgTime = await repository.GetAverageCompletionTimeAsync();
    if (avgTime.HasValue)
    {
        Console.WriteLine($"Average completion time: {avgTime.Value:F2} minutes");
    }
    else
    {
        Console.WriteLine("Warning: No completed routes found to calculate average.");
    }
}
```

## Notes

- **Thread safety** – `RouteRepository` is not inherently thread‑safe. Each instance relies on a single `ApplicationDbContext`, which is not designed for concurrent access. Callers should ensure that repository methods are not invoked simultaneously from multiple threads without external synchronization (e.g., using a lock or by scoping a new repository instance per operation).
- **Null handling** – Methods that accept optional filter parameters may throw `ArgumentNullException` if a required identifier is `null`. Always validate input before calling repository methods.
- **Lazy vs. eager loading** – `GetRouteWithDetailsAsync` explicitly loads related data. Other methods return only the `Route` entity itself; navigation properties will be `null` unless explicitly included via the query or the context’s change tracker.
- **Performance** – Queries such as `GetLongestRoutesAsync` and `GetAverageCompletionTimeAsync` may involve aggregation or sorting over large datasets. Consider indexing the relevant columns (e.g., distance, completion time) in the database to avoid full table scans.
- **Disposal** – The repository does not own the `ApplicationDbContext`; the caller is responsible for disposing the context when it is no longer needed. The repository itself does not implement `IDisposable`.
