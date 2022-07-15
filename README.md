// =============================================================================
// ... (rest of file remains unchanged)

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