# LocationController

The `LocationController` is an ASP.NET Core API controller that exposes RESTful endpoints for managing geographic location records in the `signalr-map-realtime` application. It handles CRUD operations—retrieving, creating, updating, and deleting locations—and is designed to work in conjunction with SignalR hubs to push real-time updates to connected clients. The controller uses standard HTTP status codes to indicate success or failure of each operation.

## API

### `public LocationController(ILocationRepository repository, IHubContext<MapHub> hubContext)`

Initializes a new instance of the controller with the required dependencies.

- **Parameters**  
  - `repository` – An implementation of `ILocationRepository` that provides data access for location entities.  
  - `hubContext` – The `IHubContext` for `MapHub`, used to broadcast location changes to all connected clients.

- **Throws**  
  - `ArgumentNullException` if either dependency is `null`.

### `public async Task<IActionResult> GetLocations()`

Retrieves all locations stored in the system.

- **Parameters** – None.  
- **Returns** – `OkObjectResult` containing a collection of location DTOs.  
- **Throws** – No explicit exceptions; any underlying data access exceptions are propagated.

### `public async Task<IActionResult> GetLocationById(int id)`

Retrieves a single location by its unique identifier.

- **Parameters**  
  - `id` – The integer identifier of the location.  
- **Returns** – `OkObjectResult` with the location DTO if found; otherwise `NotFoundResult`.  
- **Throws** – No explicit exceptions; data access exceptions may propagate.

### `public async Task<IActionResult> CreateLocation([FromBody] LocationCreateDto dto)`

Creates a new location from the provided data.

- **Parameters**  
  - `dto` – A `LocationCreateDto` containing required fields (e.g., latitude, longitude, name).  
- **Returns** – `CreatedAtActionResult` with the newly created location DTO and a route to `GetLocationById`.  
- **Throws** – `BadRequestObjectResult` if the model state is invalid or the DTO is `null`. Data validation errors are returned as part of the response.

### `public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationUpdateDto dto)`

Updates an existing location identified by `id` with the supplied data.

- **Parameters**  
  - `id` – The integer identifier of the location to update.  
  - `dto` – A `LocationUpdateDto` containing the fields to modify.  
- **Returns** – `OkObjectResult` with the updated location DTO if successful; `NotFoundResult` if the location does not exist.  
- **Throws** – `BadRequestObjectResult` if the model state is invalid or the DTO is `null`.

### `public async Task<IActionResult> DeleteLocation(int id)`

Deletes a location by its identifier.

- **Parameters**  
  - `id` – The integer identifier of the location to delete.  
- **Returns** – `NoContentResult` on successful deletion; `NotFoundResult` if the location does not exist.  
- **Throws** – No explicit exceptions; data access exceptions may propagate.

## Usage

The following examples demonstrate typical usage of the controller within an ASP.NET Core application.

**Example 1: Retrieving all locations and creating a new one**

```csharp
// Assume controller is injected via constructor
public class LocationService
{
    private readonly LocationController _controller;

    public LocationService(LocationController controller)
    {
        _controller = controller;
    }

    public async Task<List<LocationDto>> GetAllLocationsAsync()
    {
        var result = await _controller.GetLocations();
        if (result is OkObjectResult okResult && okResult.Value is List<LocationDto> locations)
        {
            return locations;
        }
        return new List<LocationDto>();
    }

    public async Task<LocationDto?> CreateLocationAsync(double lat, double lng, string name)
    {
        var dto = new LocationCreateDto { Latitude = lat, Longitude = lng, Name = name };
        var result = await _controller.CreateLocation(dto);
        if (result is CreatedAtActionResult createdResult && createdResult.Value is LocationDto created)
        {
            return created;
        }
        return null;
    }
}
```

**Example 2: Updating and deleting a location**

```csharp
public async Task<bool> UpdateLocationNameAsync(int id, string newName)
{
    var dto = new LocationUpdateDto { Name = newName };
    var result = await _controller.UpdateLocation(id, dto);
    return result is OkObjectResult;
}

public async Task<bool> DeleteLocationAsync(int id)
{
    var result = await _controller.DeleteLocation(id);
    return result is NoContentResult;
}
```

## Notes

- **Edge Cases**  
  - Calling `GetLocationById`, `UpdateLocation`, or `DeleteLocation` with a non‑existent `id` returns `404 NotFound`; no exception is thrown.  
  - `CreateLocation` and `UpdateLocation` return `400 BadRequest` when the request body is `null` or fails model validation (e.g., missing required fields, out‑of‑range coordinates).  
  - The controller does not perform authorization checks; access control must be added via middleware or attributes if required.

- **Thread Safety**  
  - The controller itself is stateless and thread‑safe because it does not hold mutable instance state beyond its injected dependencies.  
  - The underlying `ILocationRepository` and `IHubContext` implementations are expected to be thread‑safe (e.g., registered as singletons or scoped appropriately).  
  - Concurrent calls to `CreateLocation` or `UpdateLocation` may lead to race conditions if the repository does not implement optimistic concurrency; the controller does not add any locking.  
  - SignalR broadcasts are asynchronous and non‑blocking; the controller does not wait for delivery confirmation.
