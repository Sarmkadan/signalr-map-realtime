# AssetControllerTestsExtensions

Extension methods and helper utilities for writing unit tests against the `AssetController` in the `signalr-map-realtime` project. These methods simplify common test assertions and object creation for asset-related endpoints, reducing boilerplate in test suites.

## API

### `public static async Task<string> ShouldBeSuccessfulJsonResponse(this HttpResponseMessage response)`

Validates that an HTTP response is successful and contains a JSON payload. The method asserts the status code is 2xx and the response content is valid JSON.

- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Return value**
  - `Task<string>`: The deserialized JSON string content of the response.
- **Exceptions**
  - Throws if the response status code is not 2xx.
  - Throws if the response content is not valid JSON.

---

### `public static async Task<T> ShouldBeCreatedResource<T>(this HttpResponseMessage response)`

Validates that an HTTP response indicates successful creation of a resource and returns the deserialized resource. The status code must be 201 Created.

- **Type parameters**
  - `T`: The type of the resource to deserialize from the response.
- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Return value**
  - `Task<T>`: The deserialized resource of type `T`.
- **Exceptions**
  - Throws if the response status code is not 201.
  - Throws if the response content cannot be deserialized to `T`.

---

### `public static async Task<IReadOnlyList<AssetDto>> ShouldReturnAssetList(this HttpResponseMessage response)`

Validates that an HTTP response contains a list of `AssetDto` objects. The status code must be 200 OK and the content must be a valid JSON array of assets.

- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Return value**
  - `Task<IReadOnlyList<AssetDto>>`: The deserialized list of `AssetDto` objects.
- **Exceptions**
  - Throws if the response status code is not 200.
  - Throws if the response content is not a valid array of `AssetDto`.

---

### `public static async Task<AssetDto> ShouldReturnAsset(this HttpResponseMessage response)`

Validates that an HTTP response contains a single `AssetDto` object. The status code must be 200 OK and the content must be a valid JSON object representing an asset.

- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Return value**
  - `Task<AssetDto>`: The deserialized `AssetDto` object.
- **Exceptions**
  - Throws if the response status code is not 200.
  - Throws if the response content cannot be deserialized to `AssetDto`.

---
### `public static async Task ShouldBeNotFound(this HttpResponseMessage response)`

Validates that an HTTP response indicates a resource was not found. The status code must be 404 Not Found.

- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Exceptions**
  - Throws if the response status code is not 404.

---
### `public static async Task ShouldIndicateSuccessfulDeletion(this HttpResponseMessage response)`

Validates that an HTTP response indicates successful deletion of a resource. The status code must be 204 No Content.

- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Exceptions**
  - Throws if the response status code is not 204.

---
### `public static async Task ShouldBeBadRequest(this HttpResponseMessage response)`

Validates that an HTTP response indicates a client error. The status code must be 400 Bad Request.

- **Parameters**
  - `response` (HttpResponseMessage): The HTTP response to validate.
- **Exceptions**
  - Throws if the response status code is not 400.

---
### `public static AssetDto CreateTestAssetDto()`

Creates a new `AssetDto` instance populated with default test values suitable for use in test scenarios.

- **Return value**
  - `AssetDto`: A new `AssetDto` with non-null, non-default values for required properties.
- **Remarks**
  - The returned object is safe to mutate in tests without affecting other test cases.

---
### `public static AssetDto CreateTestAssetDto(Action<AssetDto>? configure = null)`

Creates a new `AssetDto` instance populated with default test values and optionally applies custom configuration.

- **Parameters**
  - `configure` (Action<AssetDto>?, optional): An optional delegate to further customize the returned `AssetDto`.
- **Return value**
  - `AssetDto`: A new `AssetDto` with non-null, non-default values for required properties, modified by `configure` if provided.
- **Remarks**
  - The returned object is safe to mutate in tests without affecting other test cases.

## Usage

### Example 1: Validating a successful asset creation
