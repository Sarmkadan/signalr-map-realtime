# UserRepository

The `UserRepository` class serves as the data access layer for user-related operations within the `signalr-map-realtime` project. Inheriting from a base repository pattern, it provides specialized asynchronous methods to query, filter, and manage `User` entities via an `ApplicationDbContext`. This repository is specifically designed to support real-time features by exposing endpoints for tracking online status, activity states, and specific user roles such as drivers, while also handling administrative tasks like user deactivation.

## API

### `public UserRepository(ApplicationDbContext context)`
Initializes a new instance of the `UserRepository` class.
*   **Parameters**:
    *   `context`: The `ApplicationDbContext` instance used to interact with the database.
*   **Remarks**: Calls the base constructor to establish the database connection context.

### `public async Task<User?> GetByEmailAsync`
Retrieves a single user entity based on their email address.
*   **Parameters**: Expects a string argument representing the email address (inferred from method name convention).
*   **Returns**: A `Task` containing the `User` object if found; otherwise, `null`.
*   **Throws**: May throw database-related exceptions if the context is unavailable or the query fails.

### `public async Task<User?> GetByEmployeeIdAsync`
Retrieves a single user entity based on their unique employee identifier.
*   **Parameters**: Expects a string or integer argument representing the employee ID (inferred from method name convention).
*   **Returns**: A `Task` containing the `User` object if found; otherwise, `null`.
*   **Throws**: May throw database-related exceptions if the context is unavailable or the query fails.

### `public async Task<IEnumerable<User>> GetOnlineUsersAsync`
Retrieves a collection of users currently marked as online in the system.
*   **Parameters**: None.
*   **Returns**: A `Task` containing an enumerable collection of `User` objects. Returns an empty collection if no users are online.
*   **Throws**: May throw database-related exceptions if the query execution fails.

### `public async Task<IEnumerable<User>> GetActiveUsersAsync`
Retrieves a collection of users who are currently active (distinct from merely online, potentially implying non-deactivated or recently engaged status).
*   **Parameters**: None.
*   **Returns**: A `Task` containing an enumerable collection of `User` objects. Returns an empty collection if no active users exist.
*   **Throws**: May throw database-related exceptions if the query execution fails.

### `public async Task<IEnumerable<User>> GetUsersByDepartmentAsync`
Retrieves all users belonging to a specific department.
*   **Parameters**: Expects a string or department identifier argument (inferred from method name convention).
*   **Returns**: A `Task` containing an enumerable collection of `User` objects. Returns an empty collection if no matches are found.
*   **Throws**: May throw database-related exceptions if the query execution fails.

### `public async Task<IEnumerable<User>> GetDriversWithVehiclesAsync`
Retrieves a collection of users with the role of "Driver," including their associated vehicle data.
*   **Parameters**: None.
*   **Returns**: A `Task` containing an enumerable collection of `User` objects, eagerly loaded with vehicle information.
*   **Throws**: May throw database-related exceptions if the query or join operation fails.

### `public async Task<IEnumerable<User>> GetRecentlyLoggedInUsersAsync`
Retrieves a collection of users sorted by their most recent login timestamp.
*   **Parameters**: None (may implicitly filter by a time window based on implementation).
*   **Returns**: A `Task` containing an enumerable collection of `User` objects.
*   **Throws**: May throw database-related exceptions if the query execution fails.

### `public async Task<bool> DeactivateUserAsync`
Deactivates a specific user account in the database.
*   **Parameters**: Expects a user identifier (e.g., ID or User object) to target the deactivation (inferred from method name convention).
*   **Returns**: A `Task` containing a `bool`. Returns `true` if the deactivation was successful and changes were saved; `false` if the user was not found or no changes were made.
*   **Throws**: May throw database-related exceptions if the update or save operation fails.

### `public async Task<IEnumerable<User>> GetUsersByJobTitleAsync`
Retrieves all users matching a specific job title.
*   **Parameters**: Expects a string argument representing the job title (inferred from method name convention).
*   **Returns**: A `Task` containing an enumerable collection of `User` objects. Returns an empty collection if no matches are found.
*   **Throws**: May throw database-related exceptions if the query execution fails.

### `public async Task<int> GetOnlineUserCountAsync`
Returns the total count of users currently marked as online.
*   **Parameters**: None.
*   **Returns**: A `Task` containing an `int` representing the number of online users.
*   **Throws**: May throw database-related exceptions if the count query fails.

## Usage

### Example 1: Retrieving Drivers for Real-Time Mapping
This example demonstrates how to fetch all drivers with their associated vehicle data to populate a real-time map interface.

```csharp
public async Task BroadcastDriverLocations(UserRepository userRepository, IHubContext<MapHub> hubContext)
{
    // Retrieve all drivers along with their vehicle details
    var drivers = await userRepository.GetDriversWithVehiclesAsync();
    
    var driverData = drivers.Select(d => new 
    {
        Id = d.Id,
        Name = d.FullName,
        VehiclePlate = d.Vehicle?.LicensePlate,
        LastKnownLocation = d.CurrentLocation
    });

    // Send the data to connected clients via SignalR
    await hubContext.Clients.All.SendAsync("UpdateDrivers", driverData);
}
```

### Example 2: Administrative User Deactivation
This example illustrates checking for a user by email and deactivating their account if they are found.

```csharp
public async Task<bool> HandleUserOffboarding(UserRepository userRepository, string email)
{
    // Locate the user
    var user = await userRepository.GetByEmailAsync(email);
    
    if (user == null)
    {
        return false; // User does not exist
    }

    // Attempt to deactivate the user
    bool success = await userRepository.DeactivateUserAsync(user.Id);
    
    if (success)
    {
        // Trigger downstream cleanup logic if necessary
        await AuditLogService.LogAction($"User {email} deactivated");
    }

    return success;
}
```

## Notes

*   **Null Handling**: Methods returning a single `User` (`GetByEmailAsync`, `GetByEmployeeIdAsync`) return `null` if no record is found rather than throwing an exception. Callers must perform null checks before accessing properties.
*   **Empty Collections**: List retrieval methods (`GetOnlineUsersAsync`, `GetUsersByDepartmentAsync`, etc.) return an empty `IEnumerable<User>` rather than `null` when no results match the criteria.
*   **Thread Safety**: As with most Entity Framework `DbContext` derived repositories, `UserRepository` instances are not thread-safe. A new instance should be resolved per request or unit of work to avoid concurrency conflicts within the database context.
*   **Async Execution**: All public members are asynchronous. Blocking on these tasks (e.g., using `.Result` or `.Wait()`) in ASP.NET Core environments may lead to thread pool starvation or deadlocks; always use `await`.
*   **Data Consistency**: The `GetOnlineUserCountAsync` and `GetOnlineUsersAsync` methods rely on the current state of the database. In high-concurrency real-time scenarios, the count returned may differ slightly from the actual number of items enumerated if the dataset changes between calls.
