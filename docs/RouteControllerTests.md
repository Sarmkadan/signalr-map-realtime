# RouteControllerTests

Test suite for the `RouteController` API endpoints, verifying CRUD operations, validation behavior, and HTTP response semantics for route management in the SignalR map realtime application.

## API

### `public RouteControllerTests()`
Initializes a new instance of the test class. Sets up the test server, HTTP client, and any required mock dependencies for controller integration testing.

### `public async Task GetRoutes_ReturnsSuccessAndCorrectContentType()`
Verifies that `GET /api/routes` returns HTTP 200 OK with a JSON content type.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 200 or the `Content-Type` header does not indicate JSON.

### `public async Task PostRoute_ReturnsCreatedRoute()`
Verifies that `POST /api/routes` with a valid route payload returns HTTP 201 Created and the created route entity with a generated identifier.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 201, the `Location` header is missing, or the response body does not match the submitted route with an assigned ID.

### `public async Task GetRouteById_ReturnsNotFound_ForNonExistentId()`
Verifies that `GET /api/routes/{id}` returns HTTP 404 Not Found when the requested route identifier does not exist.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 404.

### `public async Task GetRouteById_ReturnsRoute_ForExistingId()`
Verifies that `GET /api/routes/{id}` returns HTTP 200 OK with the correct route entity when the identifier exists.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 200 or the returned route does not match the expected entity.

### `public async Task PutRoute_UpdatesExistingRoute()`
Verifies that `PUT /api/routes/{id}` with a valid payload updates the existing route and returns HTTP 204 No Content.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 204, or a subsequent GET does not reflect the updated values.

### `public async Task DeleteRoute_RemovesRoute()`
Verifies that `DELETE /api/routes/{id}` removes the route and returns HTTP 204 No Content, and that a subsequent GET returns 404.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 204, or the route remains accessible after deletion.

### `public async Task PostRoute_ReturnsBadRequest_ForInvalidModel()`
Verifies that `POST /api/routes` with an invalid payload (missing required fields, malformed geometry) returns HTTP 400 Bad Request with validation error details.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 400 or the response does not contain validation problem details.

### `public async Task PutRoute_ReturnsBadRequest_ForMismatchedId()`
Verifies that `PUT /api/routes/{id}` returns HTTP 400 Bad Request when the route identifier in the URL does not match the identifier in the request body.
- **Parameters**: None.
- **Returns**: `Task` completing when the assertion passes.
- **Throws**: `Xunit.Sdk.XunitException` if the response status code is not 400.

## Usage

```csharp
// Example 1: Running the full test suite via xUnit CLI
// dotnet test --filter "FullyQualifiedName~RouteControllerTests"
// This executes all test methods in the class against the configured test server,
// validating the complete route lifecycle: create, read, update, delete, and error cases.

// Example 2: Debugging a single test with a custom WebApplicationFactory
var factory = new CustomWebApplicationFactory<Program>()
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureServices(services =>
        {
            // Replace database with in-memory provider for isolated test run
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    });

var client = factory.CreateClient();
var testInstance = new RouteControllerTests { Client = client };

// Execute a specific scenario manually
await testInstance.PostRoute_ReturnsCreatedRoute();
await testInstance.GetRouteById_ReturnsRoute_ForExistingId();
```

## Notes

- **Test Isolation**: Each test method should run against a clean database state. The test class relies on the `WebApplicationFactory` to reset or recreate the database between tests. If tests share state, `DeleteRoute_RemovesRoute` may leave residue affecting `GetRouteById_ReturnsNotFound_ForNonExistentId`.
- **Thread Safety**: The test class is not thread-safe. xUnit executes test methods in the same class sequentially by default, but parallel execution across classes may cause conflicts if the underlying test server or database is shared. Use `IClassFixture<WebApplicationFactory<Program>>` with a per-class factory instance to ensure isolation.
- **Async Disposal**: The `HttpClient` and `WebApplicationFactory` implement `IAsyncDisposable`. Ensure the test runner or fixture disposes them properly to avoid port exhaustion or connection leaks during large test runs.
- **Validation Details**: `PostRoute_ReturnsBadRequest_ForInvalidModel` and `PutRoute_ReturnsBadRequest_ForMismatchedId` depend on the model validation pipeline and `ApiBehaviorOptions`. If validation is suppressed globally, these tests will fail.
- **Idempotency**: `PutRoute_UpdatesExistingRoute` assumes PUT is idempotent. The test does not verify repeated PUT calls produce the same result; add a separate test if idempotency guarantees are required.
- **Concurrency**: No tests cover concurrent modifications (e.g., simultaneous PUT/DELETE on the same route). If the controller uses optimistic concurrency tokens, additional tests should verify `409 Conflict` responses.
