# AssetControllerTests

`AssetControllerTests` is the unit test suite for the `AssetController` in the `signalr-map-realtime` project. It validates the behavior of the controller’s HTTP endpoints—GET, POST, PUT, and DELETE—for managing asset resources. The class ensures that the controller returns correct HTTP status codes, content types, and response bodies under both successful and failure scenarios, including invalid model states and mismatched identifiers.

## API

### public AssetControllerTests

The default constructor for the test class. It is invoked by the test runner to instantiate the test suite. No parameters, no return value, and it does not throw exceptions under normal test infrastructure conditions.

### public async Task GetAssets_ReturnsSuccessAndCorrectContentType

Verifies that a GET request to the assets endpoint returns a successful status code and a response with the expected content type (e.g., `application/json`). This test ensures the controller correctly serializes the collection of assets and sets the appropriate media type header.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Relies on assertion failures if the status code or content type does not match expectations; does not throw directly.

### public async Task PostAsset_ReturnsCreatedAsset

Validates that a POST request with a valid asset payload results in a `201 Created` response containing the created asset resource. It confirms the controller’s ability to deserialize input, persist the asset, and return the newly created object.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the response status is not `Created` or the returned asset does not match the submitted data.

### public async Task GetAssetById_ReturnsNotFound_ForNonExistentId

Ensures that requesting an asset by an identifier that does not exist in the data store yields a `404 Not Found` response. This test confirms proper error handling for missing resources.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the status code is not `NotFound`.

### public async Task GetAssetById_ReturnsAsset_ForExistingId

Confirms that a GET request for a specific, existing asset identifier returns a successful response with the correct asset data. It verifies the controller’s lookup logic and response serialization for single resources.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the status code is not successful or the returned asset does not match the expected entity.

### public async Task PutAsset_UpdatesExistingAsset

Tests that a PUT request targeting an existing asset with a complete, valid payload updates the resource and returns the appropriate success response (typically `200 OK` or `204 No Content`). It validates the full update path.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the response status does not indicate a successful update or the persisted state does not reflect the changes.

### public async Task DeleteAsset_RemovesAsset

Verifies that a DELETE request for an existing asset removes it from the data store and returns a success status (commonly `204 No Content`). A subsequent retrieval of the same identifier should yield a `404 Not Found`.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the deletion response is not successful or the asset remains retrievable afterward.

### public async Task PostAsset_ReturnsBadRequest_ForInvalidModel

Ensures that submitting a POST request with an invalid asset model (e.g., missing required fields or malformed data) results in a `400 Bad Request` response. This test confirms that model validation is enforced on creation.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the status code is not `BadRequest`.

### public async Task PutAsset_ReturnsBadRequest_ForMismatchedId

Validates that a PUT request where the route identifier does not match the identifier in the request body produces a `400 Bad Request` response. This test ensures the controller guards against inconsistent update requests.

- **Parameters:** None (test method).
- **Returns:** A `Task` representing the asynchronous test operation.
- **Throws:** Assertion failures if the status code is not `BadRequest`.

## Usage

```csharp
// Example 1: Running all AssetControllerTests using xUnit
public class TestRunner
{
    [Fact]
    public async Task RunAllAssetControllerTests()
    {
        var tests = new AssetControllerTests();
        
        await tests.GetAssets_ReturnsSuccessAndCorrectContentType();
        await tests.PostAsset_ReturnsCreatedAsset();
        await tests.GetAssetById_ReturnsNotFound_ForNonExistentId();
        await tests.GetAssetById_ReturnsAsset_ForExistingId();
        await tests.PutAsset_UpdatesExistingAsset();
        await tests.DeleteAsset_RemovesAsset();
        await tests.PostAsset_ReturnsBadRequest_ForInvalidModel();
        await tests.PutAsset_ReturnsBadRequest_ForMismatchedId();
    }
}
```

```csharp
// Example 2: Integrating with a CI pipeline via dotnet test
// In a build script or CI configuration:
// dotnet test --filter "FullyQualifiedName~AssetControllerTests"
//
// This executes all tests in AssetControllerTests, ensuring controller
// behavior is validated before deployment.
```

## Notes

- All test methods are asynchronous and return `Task`; they should be awaited to capture assertion failures properly. Running them without awaiting may result in unobserved task exceptions.
- The tests assume a controlled, isolated test environment (e.g., an in-memory database or mocked dependencies). They do not test thread safety directly; concurrent access scenarios are not covered by these signatures.
- Edge cases such as empty request bodies, null fields within otherwise valid models, or duplicate identifiers are not explicitly represented in the listed members and may be covered by additional tests elsewhere in the suite.
- `PutAsset_ReturnsBadRequest_ForMismatchedId` specifically guards against route/body ID mismatches. A PUT with a valid payload but a non-existent ID may behave differently and is not part of this documented set.
- The test class itself is stateless; each test method sets up its own context. No shared mutable state exists between tests, eliminating thread-safety concerns within the suite.
