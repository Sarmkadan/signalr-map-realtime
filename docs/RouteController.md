# RouteController
The `RouteController` provides HTTP endpoints for managing and calculating routes within the signalr‑map‑realtime application. It follows REST conventions, returning appropriate status codes and JSON payloads.

## API
### GetRoutes
- **Purpose:** Retrieve a collection of all stored routes.  
- **Parameters:** None.  
- **Return Value:** `Task<IActionResult>` yielding `200 OK` with an `IEnumerable<RouteDto>` when routes exist, or `204 No Content` when the collection is empty.  
- **When it throws:** If the underlying data source is unavailable, an `InvalidOperationException` may be thrown, resulting in a `500 Internal Server Error`.

### GetRouteById
- **Purpose:** Obtain a single route by its unique identifier.  
- **Parameters:** `id` (`Guid`) – the identifier of the route to fetch.  
- **Return Value:** `Task<IActionResult>` yielding `200 OK` with a `RouteDto` if found, or `404 Not Found` if no route matches the id.  
- **When it throws:** Supplying `Guid.Empty` raises an `ArgumentException`, which is translated to a `400 Bad Request`. Unexpected repository errors cause a `500 Internal Server Error`.

### CreateRoute
- **Purpose:** Insert a new route into the system.  
- **Parameters:** `route` (`RouteDto`) supplied in the request body.  
- **Return Value:** `Task<IActionResult>` yielding `201 Created` with a `Location` header pointing to the new resource and the created `RouteDto` in the body, or `400 Bad Request` if validation fails.  
- **When it throws:** Validation errors produce a `400 Bad Request`. Persistence failures (e.g., `DbUpdateException`) lead to a `500 Internal Server Error`.

### UpdateRoute
- **Purpose:** Modify an existing route’s data.  
- **Parameters:** `id` (`Guid`) – the route to update; `route` (`RouteDto`) containing the updated values.  
- **Return Value:** `Task<IActionResult>` yielding `204 No Content` on successful update, `400 Bad Request` if the id in the path does not match the id in the payload, or `404 Not Found` when the route does not exist.  
- **When it throws:** An empty `Guid` for `id` triggers an `ArgumentException` (`400 Bad Request`). Concurrency conflicts raise a `DbUpdateConcurrencyException` resulting in a `409 Conflict`. Other unexpected errors produce `500 Internal Server Error`.

### DeleteRoute
- **Purpose:** Remove a route from the system.  
- **Parameters:** `id` (`Guid`) – the identifier of the route to delete.  
- **Return Value:** `Task<IActionResult>` yielding `204 No Content` on successful deletion, or `404 Not Found` if the route cannot be located.  
- **When it throws:** Providing `Guid.Empty` results in an `ArgumentException` (`400 Bad Request`). Errors during deletion (e.g., foreign‑key constraints) cause a `500 Internal Server Error`.

### CalculateRoute
- **Purpose:** Compute an optimal path between two points using the routing engine.  
- **Parameters:** `request` (`CalculateRouteRequest`) containing start and end coordinates and optional travel mode.  
- **Return Value:** `Task<IActionResult>` yielding `200 OK` with a `RouteResultDto` describing the calculated route, `400 Bad Request` for invalid input, or `500 Internal Server Error` if the calculation service fails.  
- **When it throws:** A `null` request throws `ArgumentNullException` (`400 Bad Request`). Invalid coordinate ranges or unsupported travel modes produce validation errors (`400`). Service‑unavailability leads to `500`.

## Usage
```csharp
// Example 1: Fetch all routes
using System.Net.Http.Json;
using System.Threading.Tasks;

public async Task<IEnumerable<RouteDto>> GetAllRoutesAsync()
{
    using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
    var response = await client.GetAsync("/api/route");
    response.EnsureSuccessStatusCode();
    return await response.ReadFromJsonAsync<IEnumerable<RouteDto>>();
}
```
```csharp
// Example 2: Create a new route
using System.Net.Http.Json;
using System.Threading.Tasks;

public async Task<RouteDto> AddRouteAsync(RouteDto newRoute)
{
    using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
    var response = await client.PostAsJsonAsync("/api/route", newRoute);
    response.EnsureSuccessStatusCode();
    return await response.ReadFromJsonAsync<RouteDto>();
}
```
## Notes
- The controller is stateless; each request is processed independently, making it safe for concurrent invocation.
- Thread‑safety of the operations depends on the injected services (e.g., `IRouteRepository`, `IRouteCalculator`). Consumers should ensure those services are thread‑safe or scoped appropriately.
- Supplying `Guid.Empty` to any endpoint that expects an identifier results in a `400 Bad Request`.
- `CalculateRoute` validates that start and end coordinates fall within the supported geographic bounds; out‑of‑range values trigger a `400` response.
- Unexpected exceptions are caught by the ASP.NET Core pipeline and returned as `500 Internal Server Error`; callers should not rely on specific exception types leaking from the controller.
- No session or cached state is retained within the controller, so horizontal scaling behind a load balancer requires no affinity considerations.
