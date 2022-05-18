# AssetRepository

The `AssetRepository` class serves as the primary data access layer for managing `Asset` entities within the `signalr-map-realtime` application. Inheriting from a base repository pattern, it encapsulates complex querying logic related to asset tracking, valuation, condition monitoring, and assignment status against an `ApplicationDbContext`. This repository is designed to support real-time map updates by providing efficient, asynchronous retrieval methods for specific asset subsets, such as those requiring special handling, those recently tracked, or those currently unassigned to vehicles.

## API

### `public AssetRepository(ApplicationDbContext context)`
Initializes a new instance of the `AssetRepository` class.
*   **Parameters**:
    *   `context`: The `ApplicationDbContext` instance used to interact with the database.
*   **Remarks**: This constructor passes the context to the base class implementation to establish the database session.

### `public async Task<Asset?> GetBySerialNumberAsync(string serialNumber)`
Retrieves a single asset entity matching the provided serial number.
*   **Parameters**:
    *   `serialNumber`: The unique serial identifier of the asset.
*   **Returns**: An `Asset` object if found; otherwise, `null`.
*   **Exceptions**: Throws if the database connection is unavailable or the query fails.

### `public async Task<IEnumerable<Asset>> GetAssetsByTypeAsync(string assetType)`
Fetches a collection of assets filtered by their specific type classification.
*   **Parameters**:
    *   `assetType`: The string identifier representing the category or type of asset.
*   **Returns**: An enumerable collection of `Asset` objects matching the type. Returns an empty collection if no matches are found.
*   **Exceptions**: Throws on database connectivity issues or invalid query execution.

### `public async Task<IEnumerable<Asset>> GetAssetsByVehicleAsync(int vehicleId)`
Retrieves all assets currently assigned to a specific vehicle.
*   **Parameters**:
    *   `vehicleId`: The unique identifier of the vehicle.
*   **Returns**: An enumerable collection of `Asset` objects linked to the specified vehicle.
*   **Exceptions**: Throws if the underlying data store cannot be accessed.

### `public async Task<IEnumerable<Asset>> GetUnassignedAssetsAsync()`
Returns a list of all assets that are not currently assigned to any vehicle.
*   **Parameters**: None.
*   **Returns**: An enumerable collection of unassigned `Asset` objects.
*   **Exceptions**: Throws on database errors.

### `public async Task<IEnumerable<Asset>> GetSpecialHandlingAssetsAsync()`
Identifies and retrieves assets flagged for special handling requirements (e.g., hazardous materials, fragile items).
*   **Parameters**: None.
*   **Returns**: An enumerable collection of `Asset` objects requiring special handling.
*   **Exceptions**: Throws if the query execution fails.

### `public async Task<Asset?> GetAssetWithHistoryAsync(int assetId)`
Retrieves a specific asset along with its related tracking or maintenance history data, eagerly loaded.
*   **Parameters**:
    *   `assetId`: The unique identifier of the asset.
*   **Returns**: The `Asset` object including its history navigation property if found; otherwise, `null`.
*   **Exceptions**: Throws on database access failures.

### `public async Task<IEnumerable<Asset>> GetRecentlyTrackedAssetsAsync(TimeSpan timeWindow)`
Fetches assets that have reported a tracking event within the specified time duration.
*   **Parameters**:
    *   `timeWindow`: The `TimeSpan` defining how far back to look for tracking events (e.g., `TimeSpan.FromMinutes(30)`).
*   **Returns**: An enumerable collection of `Asset` objects with recent activity.
*   **Exceptions**: Throws if the database is unreachable.

### `public async Task<decimal> GetTotalValueByTypeAsync(string assetType)`
Calculates the aggregate monetary value of all assets belonging to a specific type.
*   **Parameters**:
    *   `assetType`: The category of assets to sum.
*   **Returns**: A `decimal` representing the total value. Returns `0` if no assets match the type.
*   **Exceptions**: Throws on query failure.

### `public async Task<IEnumerable<Asset>> GetNotRecentlyTrackedAsync(TimeSpan timeWindow)`
Identifies assets that have *not* reported a tracking event within the specified time duration, useful for detecting offline or lost units.
*   **Parameters**:
    *   `timeWindow`: The `TimeSpan` defining the inactivity threshold.
*   **Returns**: An enumerable collection of `Asset` objects considered inactive based on the time window.
*   **Exceptions**: Throws if the database connection fails.

### `public async Task<int> CountByConditionAsync(string condition)`
Counts the number of assets matching a specific condition status (e.g., "Good", "Damaged", "Maintenance").
*   **Parameters**:
    *   `condition`: The string representation of the asset condition.
*   **Returns**: An integer representing the count of matching assets.
*   **Exceptions**: Throws on database errors.

## Usage

### Example 1: Retrieving Unassigned Assets for Dispatch
This example demonstrates how to fetch all available assets that are not currently assigned to a vehicle, suitable for a dispatch dashboard.

```csharp
using var context = new ApplicationDbContext();
var repository = new AssetRepository(context);

// Retrieve all assets ready for assignment
var availableAssets = await repository.GetUnassignedAssetsAsync();

foreach (var asset in availableAssets)
{
    Console.WriteLine($"Asset ID: {asset.Id}, Type: {asset.Type}, Serial: {asset.SerialNumber}");
}
```

### Example 2: Monitoring Inactive Assets
This example identifies assets that have not transmitted location data in the last 4 hours, triggering an alert workflow.

```csharp
using var context = new ApplicationDbContext();
var repository = new AssetRepository(context);

var inactivityThreshold = TimeSpan.FromHours(4);
var staleAssets = await repository.GetNotRecentlyTrackedAsync(inactivityThreshold);

if (staleAssets.Any())
{
    Console.WriteLine($"Alert: {staleAssets.Count()} assets have not tracked recently.");
    // Trigger notification logic here
}
```

## Notes

*   **Null Handling**: Methods returning a single entity (`GetBySerialNumberAsync`, `GetAssetWithHistoryAsync`) return `null` if no record is found rather than throwing an exception. Callers must handle null checks appropriately.
*   **Empty Collections**: Methods returning lists (`IEnumerable<Asset>`) will return an empty collection rather than `null` if no records match the criteria.
*   **Thread Safety**: This repository is not inherently thread-safe for concurrent state modification if the underlying `ApplicationDbContext` is shared across threads without proper scoping. It is recommended to instantiate a new `AssetRepository` (and consequently a new `DbContext`) per request or unit of work.
*   **Async Execution**: All members are asynchronous and non-blocking. They should be awaited directly; do not wrap them in `Task.Run` or access `.Result` synchronously, as this may lead to deadlocks in ASP.NET environments.
*   **Database Dependencies**: All methods depend on an active database connection. Transient network failures will result in exceptions being thrown to the caller; implementing retry policies at the service layer is advised.
