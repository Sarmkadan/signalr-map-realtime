// ... (rest of file remains unchanged)

## PaginatedResponse

The `PaginatedResponse<T>` class serves as a generic wrapper for paginated data, providing metadata about the current page and the total count of items. It includes properties for the list of items, page number, page size, total count, total pages, and indicators for the presence of next and previous pages.

### Usage Example

```csharp
var items = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };
var paginatedResponse = PaginatedResponse<string>.FromList(items, 1, 2);
Console.WriteLine($"Items: {string.Join(", ", paginatedResponse.Items)}, Page Number: {paginatedResponse.PageNumber}, Page Size: {paginatedResponse.PageSize}");
Console.WriteLine($"Total Count: {paginatedResponse.TotalCount}, Total Pages: {paginatedResponse.TotalPages}, Has Next Page: {paginatedResponse.HasNextPage}, Has Previous Page: {paginatedResponse.HasPreviousPage}");
```

## ErrorResponse

The `ErrorResponse` class represents a standardized error response structure for providing comprehensive error information to clients. It includes properties for a general error message, error code, field-level validation errors, HTTP status code, timestamp, and optional details like stack trace and inner exception.

### Usage Example

```csharp
try
{
    // Attempt to perform some operation that may throw an error
    var data = new Dictionary<string, string[]> { ["field1"] = new[] { "Error message 1" } };
    throw new ValidationException(data);
}
catch (Exception ex)
{
    var errorResponse = ErrorResponse.ValidationError(data, "Validation failed", "abc123");
    Console.WriteLine($"Error: {errorResponse.Message}, Code: {errorResponse.ErrorCode}, Status Code: {errorResponse.StatusCode}");
    foreach (var error in errorResponse.Errors)
    {
        Console.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value)}");
    }
}
```

## ApiResponse

The `ApiResponse<T>` and its non‑generic counterpart provide a consistent wrapper for API responses. They expose properties such as `Success`, `Data`, `Message`, `StatusCode`, `Timestamp`, and `TraceId`, and static factory methods `SuccessResponse` and `FailureResponse` for quick construction.

```csharp
// Successful response with data
var success = ApiResponse<string>.SuccessResponse("Hello, world!", traceId: "abc123");

// Failed response without data
var failure = ApiResponse<string>.FailureResponse("Something went wrong", statusCode: 500);

// Non‑generic success
var ok = ApiResponse.SuccessResponse("All good", traceId: "xyz789");

// Non‑generic failure
var err = ApiResponse.FailureResponse("Bad request", statusCode: 400);
```

The example demonstrates creating both generic and non‑generic responses, setting custom status codes and trace identifiers, and accessing the exposed properties.
