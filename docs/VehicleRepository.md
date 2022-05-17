# VehicleRepository

The `VehicleRepository` class serves as the primary data access layer for vehicle entities within the `signalr-map-realtime` project, inheriting from a base repository to leverage shared database context management. It provides specialized asynchronous query methods tailored for real-time mapping scenarios, enabling efficient retrieval of vehicles based on operational status, driver assignment, asset type, and specific telemetry conditions such as fuel levels and speeding violations.

## API

### `public VehicleRepository(ApplicationDbContext context)`
Initializes a new instance of the `VehicleRepository` class.
*   **Parameters**:
    *   `context`: The `ApplicationDbContext` instance used to interact with the underlying database.
*   **Remarks**: This constructor passes the provided context to the base class to establish the database session.

### `public async Task<IEnumerable<Vehicle>> GetVehiclesByStatusAsync`
Retrieves a collection of vehicles matching a specific operational status.
*   **Parameters**: Accepts arguments defining the target status (implementation dependent on base or overload signature).
*   **Returns**: A task representing the asynchronous operation, containing an enumerable collection of `Vehicle` objects.
*   **Exceptions**: Throws database-related exceptions if the connection fails or the query cannot be executed.

### `public async Task<IEnumerable<Vehicle>> GetOnlineVehiclesAsync`
Fetches all vehicles currently marked as online in the system.
*   **Returns**: A task containing an enumerable collection of `Vehicle` objects where the online flag is true.
*   **Exceptions**: Throws if the database context is unavailable or the query times out.

### `public async Task<Vehicle?> GetByRegistrationNumberAsync`
Locates a single vehicle entity using its unique registration number.
*   **Parameters**: Requires the registration number string as input.
*   **Returns**: A task containing the `Vehicle` object if found, or `null` if no match exists.
*   **Exceptions**: Throws on database connectivity issues.

### `public async Task<IEnumerable<Vehicle>> GetVehiclesByDriverAsync`
Returns all vehicles assigned to a specific driver.
*   **Parameters**: Accepts an identifier for the driver (e.g., Driver ID).
*   **Returns**: A task containing an enumerable collection of `Vehicle` objects associated with the specified driver.
*   **Exceptions**: Throws if the driver identifier is invalid in the context of the database schema or on connection failure.

### `public async Task<IEnumerable<Vehicle>> GetVehiclesByAssetTypeAsync`
Retrieves vehicles filtered by their specific asset type classification.
*   **Parameters**: Accepts the asset type enumeration or string identifier.
*   **Returns**: A task containing an enumerable collection of `Vehicle` objects matching the asset type.
*   **Exceptions**: Throws on database errors.

### `public async Task<IEnumerable<Vehicle>> GetLowFuelVehiclesAsync`
Identifies vehicles currently reporting fuel levels below a defined threshold.
*   **Returns**: A task containing an enumerable collection of `Vehicle` objects flagged for low fuel.
*   **Exceptions**: Throws if telemetry data cannot be read or the database is unreachable.

### `public async Task<IEnumerable<Vehicle>> GetSpeedingVehiclesAsync`
Fetches vehicles currently exceeding speed limits based on real-time tracking data.
*   **Returns**: A task containing an enumerable collection of `Vehicle` objects identified as speeding.
*   **Exceptions**: Throws on database query failures.

### `public async Task<int> GetOnlineVehicleCountAsync`
Calculates the total number of vehicles currently online without retrieving the full entity set.
*   **Returns**: A task containing an integer representing the count of online vehicles.
*   **Exceptions**: Throws if the count aggregation fails due to database errors.

### `public async Task<Vehicle?> GetVehicleWithTrackingDataAsync`
Retrieves a specific vehicle entity along with its related real-time tracking information.
*   **Parameters**: Accepts an identifier for the target vehicle.
*   **Returns**: A task containing the `Vehicle` object populated with tracking data, or `null` if not found.
*   **Exceptions**: Throws if the join operation fails or the database is unavailable.

## Usage

### Example 1: Retrieving Online Vehicles and Count
This example demonstrates fetching the list of online vehicles for map rendering and simultaneously retrieving the total count for dashboard statistics.

```csharp
public async Task UpdateRealTimeMapAsync(VehicleRepository repository)
{
    // Retrieve all vehicles currently online
    var onlineVehicles = await repository.GetOnlineVehiclesAsync();
    
    // Get the total count efficiently without loading all entities again
    int totalOnlineCount = await repository.GetOnlineVehicleCountAsync();

    Console.WriteLine($"Displaying {onlineVehicles.Count()} of {totalOnlineCount} online vehicles.");
    
    // Process vehicles for SignalR broadcast
    foreach (var vehicle in onlineVehicles)
    {
        // Broadcast position update
    }
}
```

### Example 2: Handling Alerts for Low Fuel and Speeding
This example illustrates querying vehicles based on specific telemetry alerts to trigger notifications.

```csharp
public async Task ProcessVehicleAlertsAsync(VehicleRepository repository, string registrationNumber)
{
    // Check specific vehicle details with tracking data
    var vehicle = await repository.GetVehicleWithTrackingDataAsync(registrationNumber);
    
    if (vehicle == null)
    {
        throw new InvalidOperationException("Vehicle not found.");
    }

    // Retrieve all vehicles currently speeding
    var speedingVehicles = await repository.GetSpeedingVehiclesAsync();
    
    // Retrieve all vehicles with low fuel
    var lowFuelVehicles = await repository.GetLowFuelVehiclesAsync();

    var alertSummary = new 
    {
        TargetVehicleStatus = vehicle.LicensePlate,
        TotalSpeedingIncidents = speedingVehicles.Count(),
        TotalLowFuelIncidents = lowFuelVehicles.Count()
    };

    // Send alert summary to monitoring service
    await SendAlertNotificationAsync(alertSummary);
}
```

## Notes

*   **Null Handling**: Methods returning a single entity (`GetByRegistrationNumberAsync`, `GetVehicleWithTrackingDataAsync`) return `null` if no record is found rather than throwing an exception. Callers must handle null checks appropriately.
*   **Thread Safety**: As with most Entity Framework Core `DbContext` derived implementations, the `VehicleRepository` instance is not thread-safe. A new instance should be scoped per request or per unit of work to avoid concurrency conflicts within the underlying `ApplicationDbContext`.
*   **Asynchronous Execution**: All data access methods are asynchronous. Blocking on these tasks (e.g., using `.Result` or `.Wait()`) in UI or ASP.NET Core contexts may lead to deadlocks; always use `await`.
*   **Data Consistency**: Methods like `GetSpeedingVehiclesAsync` and `GetLowFuelVehiclesAsync` rely on the current state of telemetry data in the database. There may be a slight latency between the actual vehicle state and the queried result depending on the frequency of telemetry updates.
