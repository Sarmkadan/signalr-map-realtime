# ErrorResponse

The `ErrorResponse` class provides a standardized structure for encapsulating and serializing error details within the `signalr-map-realtime` application. It is used to ensure consistent error reporting across both HTTP API endpoints and SignalR hub communications, facilitating easier debugging and client-side error handling by providing both human-readable messages and machine-readable error codes.

## API

### Properties

*   **`Message` (string)**: The primary, human-readable error message describing the issue.
*   **`ErrorCode` (string)**: A machine-readable string identifier used to categorize the error type, allowing clients to implement programmatic error handling logic.
*   **`Errors` (Dictionary<string, string[]>)**: A collection of field-specific validation errors. The key represents the field or property name, and the value is an array of messages corresponding to that field.
*   **`StatusCode` (int)**: The HTTP status code that best represents the nature of the error (e.g., 400, 404, 500).
*   **`Timestamp` (DateTime)**: The precise date and time when the error instance was created.
*   **`TraceId` (string?)**: An optional correlation ID used for tracking the request across service boundaries to assist in distributed tracing and debugging.
*   **`StackTrace` (string?)**: An optional field containing the stack trace of the error. This should be handled carefully to avoid leaking sensitive internal system information in production environments.
*   **`InnerException` (string?)**: An optional string representation of the underlying exception that caused this error response to be generated.

### Constructor

*   **`ErrorResponse()`**: Initializes a new instance of the `ErrorResponse` class.

### Static Factory Methods

These members provide pre-configured instances of `ErrorResponse` for common error scenarios:

*   **`ValidationError`**: Returns an `ErrorResponse` pre-configured for data validation failures (typically HTTP 400).
*   **`NotFoundError`**: Returns an `ErrorResponse` pre-configured for requested resources that do not exist (typically HTTP 404).
*   **`UnauthorizedError`**: Returns an `ErrorResponse` pre-configured for authentication failures (typically HTTP 401).
*   **`ForbiddenError`**: Returns an `ErrorResponse` pre-configured for authorization failures (typically HTTP 403).
*   **`ServerError`**: Returns an `ErrorResponse` pre-configured for unexpected internal application exceptions (typically HTTP 500).
*   **`ConflictError`**: Returns an `ErrorResponse` pre-configured for resource conflicts (typically HTTP 409).

## Usage

### Example 1: Returning an ErrorResponse from a Controller Action

```csharp
public IActionResult GetMapData(string mapId)
{
    var map = _mapService.GetById(mapId);
    
    if (map == null)
    {
        var error = ErrorResponse.NotFoundError;
        error.Message = $"Map with ID {mapId} was not found.";
        error.TraceId = HttpContext.TraceIdentifier;
        
        return NotFound(error);
    }
    
    return Ok(map);
}
```

### Example 2: Creating a Validation Error Response

```csharp
public ErrorResponse CreateValidationFailure(ModelStateDictionary modelState)
{
    var response = ErrorResponse.ValidationError;
    response.Message = "One or more validation errors occurred.";
    
    response.Errors = modelState
        .Where(kvp => kvp.Value.Errors.Count > 0)
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        );
        
    return response;
}
```

## Notes

*   **Sensitive Information**: Properties such as `StackTrace` and `InnerException` may contain sensitive infrastructure details. Ensure that these properties are only populated or exposed in development or staging environments, and sanitized in production configurations.
*   **Thread Safety**: `ErrorResponse` is designed as a Data Transfer Object (DTO). Instances are generally intended to be created and immediately serialized or returned. It is not inherently thread-safe; therefore, if an instance is shared across multiple threads, it should be treated as read-only.
*   **Serialization**: This class is intended to be serialized to JSON. Ensure that the JSON serializer used in the application is configured to handle the `Dictionary<string, string[]>` property and the nullable `string?` types appropriately.
