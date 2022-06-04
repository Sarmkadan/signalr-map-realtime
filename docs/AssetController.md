# AssetController

The `AssetController` serves as the primary HTTP interface for managing asset entities within the `signalr-map-realtime` project. It exposes a standard set of RESTful endpoints to retrieve, create, update, and delete asset records, facilitating synchronization between the server-side data store and real-time clients connected via SignalR.

## API

### `public AssetController`
Initializes a new instance of the `AssetController` class. This constructor typically resolves dependencies required for data access and real-time notification services through dependency injection.

### `public async Task<IActionResult> GetAssets`
Retrieves a collection of all available assets.
*   **Parameters**: None.
*   **Return Value**: Returns an `IActionResult` containing an `OkObjectResult` with the list of assets if successful, or an appropriate error status code if the retrieval fails.
*   **Exceptions**: May throw exceptions related to database connectivity or serialization errors during the asynchronous operation.

### `public async Task<IActionResult> GetAssetById`
Retrieves a specific asset based on its unique identifier.
*   **Parameters**: Accepts an identifier (typically routed or queried as `id`) corresponding to the target asset.
*   **Return Value**: Returns an `IActionResult`. If the asset is found, it returns `OkObjectResult`; if not found, it returns `NotFoundResult`.
*   **Exceptions**: May throw format exceptions if the provided ID is invalid or data access exceptions if the underlying store is unavailable.

### `public async Task<IActionResult> CreateAsset`
Creates a new asset record in the system.
*   **Parameters**: Accepts the asset data payload (usually via `[FromBody]`) required to instantiate the new entity.
*   **Return Value**: Returns an `IActionResult`, typically `CreatedAtActionResult` containing the newly created asset and its location header, or `BadRequestResult` if validation fails.
*   **Exceptions**: May throw exceptions if the data violates uniqueness constraints or if the payload is malformed.

### `public async Task<IActionResult> UpdateAsset`
Updates an existing asset with new data.
*   **Parameters**: Accepts the unique identifier of the asset to update and the new data payload.
*   **Return Value**: Returns an `IActionResult`. Returns `OkObjectResult` with the updated asset on success, `NotFoundResult` if the ID does not exist, or `BadRequestResult` if validation fails.
*   **Exceptions**: May throw concurrency exceptions if the resource has been modified since retrieval, or data access exceptions on failure.

### `public async Task<IActionResult> DeleteAsset`
Removes an asset from the system based on its identifier.
*   **Parameters**: Accepts the unique identifier of the asset to delete.
*   **Return Value**: Returns an `IActionResult`. Returns `NoContentResult` on successful deletion, or `NotFoundResult` if the asset does not exist.
*   **Exceptions**: May throw exceptions if foreign key constraints prevent deletion or if the data store is unreachable.

## Usage

### Example 1: Retrieving and Filtering Assets
This example demonstrates how to inject the controller (or call it via an HTTP client) to fetch the full list of assets and process the result.

```csharp
public class AssetService
{
    private readonly AssetController _controller;

    public AssetService(AssetController controller)
    {
        _controller = controller;
    }

    public async Task<IEnumerable<Asset>> GetActiveAssetsAsync()
    {
        var result = await _controller.GetAssets();
        
        if (result is OkObjectResult okResult && okResult.Value is IEnumerable<Asset> assets)
        {
            return assets.Where(a => a.IsActive);
        }

        throw new InvalidOperationException("Failed to retrieve assets.");
    }
}
```

### Example 2: Creating a New Asset
This example shows the instantiation of an asset model and passing it to the creation endpoint.

```csharp
public async Task InitializeNewAssetAsync(AssetController controller, string name, decimal value)
{
    var newAsset = new AssetCreateDto
    {
        Name = name,
        Value = value,
        Timestamp = DateTime.UtcNow
    };

    // Simulating the HTTP context required for model binding if calling directly
    // In standard usage, this is handled by the MVC framework pipeline
    var result = await controller.CreateAsset(newAsset);

    if (result is CreatedAtActionResult created && created.Value is Asset asset)
    {
        Console.WriteLine($"Asset created with ID: {asset.Id}");
    }
    else
    {
        Console.WriteLine("Asset creation failed.");
    }
}
```

## Notes

*   **Thread Safety**: As an ASP.NET Core controller, `AssetController` is instantiated per request. While the instance itself is not shared across threads, the underlying services injected into it must be thread-safe if they are registered as singletons. The `async` methods ensure non-blocking I/O, allowing the server to handle concurrent requests efficiently without blocking threads.
*   **Concurrency**: When using `UpdateAsset` or `DeleteAsset`, race conditions may occur if multiple clients attempt to modify the same asset simultaneously. Implementations should utilize optimistic concurrency control (e.g., ETags or row versions) where applicable to prevent data loss.
*   **Null Handling**: The `GetAssetById` method explicitly distinguishes between a missing resource (`404 Not Found`) and a server error. Callers should verify the result type before casting the `Value` property to avoid null reference exceptions.
*   **Validation**: `CreateAsset` and `UpdateAsset` rely on model state validation. If the incoming payload fails data annotations or custom validation logic, the controller returns a `400 Bad Request` without attempting database operations.
