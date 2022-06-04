# VehicleController

The `VehicleController` class is an ASP.NET Core API controller that provides CRUD operations and status queries for vehicle entities in the signalr‑map‑realtime application. It exposes HTTP endpoints that interact with the underlying service layer to retrieve, create, update, delete, and check the status of vehicles.

## API

### GetVehicles
- **Purpose:** Retrieves a collection of all vehicles.
- **Parameters:** None.
- **Return Value:** `Task<IActionResult>` yielding `200 OK` with a JSON array of `VehicleDto` objects, or `204 No Content` when no vehicles exist.
- **When it throws:** May propagate exceptions from the service layer (e.g., `InvalidOperationException`) which result in a `500 Internal Server Error` response.

### GetVehicleById
- **Purpose:** Retrieves a single vehicle by its identifier.
- **Parameters:** `int id` (the unique identifier of the vehicle).
- **Return Value:** `Task<IActionResult>` yielding `200 OK` with the matching `VehicleDto`, or `404 Not Found` if no vehicle with the given ID exists.
- **When it throws:** Throws `ArgumentException` if `id` is less than or equal to zero, resulting in a `400 Bad Request`. Service‑layer faults produce `500 Internal Server Error`.

### CreateVehicle
- **Purpose:** Creates a new vehicle record.
- **Parameters:** `[FromBody] VehicleDto vehicle` containing the data for the new vehicle.
- **Return Value:** `Task<IActionResult>` yielding `201 Created` with the newly created `VehicleDto` and a location header, or `400 Bad Request` if the payload is invalid.
- **When it throws:** Throws `ArgumentNullException` if `vehicle` is null (`400 Bad Request`). Validation errors from model binding produce `400 Bad Request`. Unexpected service errors yield `500 Internal Server Error`.

### UpdateVehicle
- **Purpose:** Updates an existing vehicle record.
- **Parameters:** `int id` (identifier of the vehicle to update) and `[FromBody] VehicleDto vehicle` containing the updated data.
- **Return Value:** `Task<IActionResult>` yielding `204 No Content` on successful update, `400 Bad Request` if the request data is invalid, or `404 Not Found` if no vehicle matches the supplied `id`.
- **When it throws:** Throws `ArgumentNullException` if `vehicle` is null (`400 Bad Request`). Throws `ArgumentException` if `id` does not match the identifier within `vehicle` (if such validation is enforced) resulting in `400 Bad Request`. Service‑layer exceptions lead to `500 Internal Server Error`.

### DeleteVehicle
- **Purpose:** Deletes a vehicle by its identifier.
- **Parameters:** `int id` (the identifier of the vehicle to remove).
- **Return Value:** `Task<IActionResult>` yielding `204 No Content` when the deletion succeeds, or `404 Not Found` if no vehicle with the given ID exists.
- **When it throws:** Throws `ArgumentException` for non‑positive `id` values (`400 Bad Request`). Unexpected errors from the service layer cause `500 Internal Server Error`.

### GetVehicleStatus
- **Purpose:** Retrieves the current operational status of a specified vehicle.
- **Parameters:** `int id` (the identifier of the vehicle whose status is requested).
- **Return Value:** `Task<IActionResult>` yielding `200 OK` with a `VehicleStatusDto` (or similar) describing the status, or `404 Not Found` if the vehicle does not exist.
- **When it throws:** Throws `ArgumentException` for invalid `id` values (`400 Bad Request`). Service faults produce `500 Internal Server Error`.

## Usage

```csharp
// Example 1: Retrieve all vehicles using HttpClient
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
HttpResponseMessage response = await client.GetAsync("api/vehicle");
response.EnsureSuccessStatusCode();
var vehicles = await response.Content.ReadFromJsonAsync<IEnumerable<VehicleDto>>();
```

```csharp
// Example 2: Create a new vehicle
var newVehicle = new VehicleDto
{
    Make = "Contoso",
    Model = "EcoDrive",
    Year = 2024,
    VIN = "1HGCM82633A004352"
};

using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
var content = JsonContent.Create(newVehicle);
HttpResponseMessage response = await client.PostAsync("api/vehicle", content);
if (response.IsSuccessStatusCode)
{
    var created = await response.Content.ReadFromJsonAsync<VehicleDto>();
    Console.WriteLine($"Vehicle created with ID {created.Id}");
}
else
{
    Console.Error.WriteLine($"Failed to create vehicle: {response.StatusCode}");
}
```

## Notes

- The controller is instantiated per request; therefore it is stateless and thread‑safe with respect to concurrent HTTP requests.
- All methods rely on injected services for data access; any thread‑safety concerns reside in those services, not in the controller itself.
- Passing a null body to `CreateVehicle` or `UpdateVehicle` results in a `400 Bad Request` due to model binding validation.
- Negative or zero identifiers are treated as invalid input and produce a `400 Bad Request` before reaching the service layer.
- If the underlying service throws an exception, the controller does not catch it; the ASP.NET Core exception handling middleware will convert it to a `500 Internal Server Error` response.
- Consumers should check for `404 Not Found` when operating on a specific vehicle ID to handle missing resources gracefully.
- The `GetVehicles` endpoint may return an empty array (`200 OK`) or `204 No Content` depending on implementation; callers should be prepared to handle both as indicating no data.
