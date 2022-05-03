# ApiResponse

The `ApiResponse` class, including its generic counterpart `ApiResponse<T>`, provides a standardized envelope for API responses within the `signalr-map-realtime` project, ensuring consistent data structures for both successful operations and error reporting. By encapsulating status, timing, and diagnostic information, this structure facilitates predictable communication between service layers and client applications.

## API

### ApiResponse<T>

*   **`bool Success`**
    Indicates whether the operation completed successfully.
*   **`T? Data`**
    The payload returned by the operation if successful; otherwise, null.
*   **`string? Message`**
    An optional descriptive message regarding the outcome of the request.
*   **`int StatusCode`**
    An integer representing the status of the response, typically following HTTP status code conventions.
*   **`DateTime Timestamp`**
    The exact time at which the response instance was generated.
*   **`string? TraceId`**
    A unique identifier used for tracking and correlating the request through logs and diagnostic systems.
*   **`ApiResponse()`**
    Initializes a new instance of the `ApiResponse<T>` class.
*   **`static ApiResponse<T> SuccessResponse`**
    A static factory method to generate a pre-configured successful response instance.
*   **`static ApiResponse<T> FailureResponse`**
    A static factory method to generate a pre-configured failed response instance.

### ApiResponse (Non-Generic)

*   **`bool Success`**
    Indicates whether the operation completed successfully.
*   **`string? Message`**
    An optional descriptive message regarding the outcome of the request.
*   **`int StatusCode`**
    An integer representing the status of the response.
*   **`DateTime Timestamp`**
    The exact time at which the response instance was generated.
*   **`string? TraceId`**
    A unique identifier used for tracking and correlating the request.
*   **`ApiResponse()`**
    Initializes a new instance of the `ApiResponse` class.
*   **`static ApiResponse SuccessResponse`**
    A static factory method to generate a pre-configured successful response instance.
*   **`static ApiResponse FailureResponse`**
    A static factory method to generate a pre-configured failed response instance.

## Usage

### Example 1: Returning a successful response with data
```csharp
public ApiResponse<UserDto> GetUser(int userId)
{
    var user = _userService.Find(userId);
    if (user == null)
    {
        return ApiResponse<UserDto>.FailureResponse;
    }
    
    var response = ApiResponse<UserDto>.SuccessResponse;
    response.Data = new UserDto(user);
    response.StatusCode = 200;
    response.Timestamp = DateTime.UtcNow;
    return response;
}
```

### Example 2: Returning a failure response without data
```csharp
public ApiResponse DeleteItem(int itemId)
{
    var result = _itemService.Remove(itemId);
    if (!result)
    {
        var response = ApiResponse.FailureResponse;
        response.Message = "Item not found or could not be deleted.";
        response.StatusCode = 404;
        response.Timestamp = DateTime.UtcNow;
        return response;
    }
    
    return ApiResponse.SuccessResponse;
}
```

## Notes

*   **Serialization:** `ApiResponse` is designed to be easily serialized into JSON for transmission over WebSockets or HTTP. Ensure that property names are correctly mapped if specific casing conventions (e.g., camelCase) are required by client consumers.
*   **Thread Safety:** The `ApiResponse` class is intended to be used as a Data Transfer Object (DTO). Instances should generally be treated as immutable once populated to ensure thread safety when passed through asynchronous service pipelines.
*   **TraceId:** When implementing handlers, it is recommended to populate the `TraceId` from the incoming request context to maintain observability across the system.
*   **Factory Methods:** The static `SuccessResponse` and `FailureResponse` properties should be treated as factory methods. Depending on the implementation, they may return a new instance or a default configured instance; do not rely on object identity comparisons.
