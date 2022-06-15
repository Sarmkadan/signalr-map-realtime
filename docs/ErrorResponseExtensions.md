# ErrorResponseExtensions

Utility extension methods for the `ErrorResponse` type that simplify creation, serialization, and user-facing error formatting. These static helpers allow callers to construct `ErrorResponse` instances with fluent modifications, convert them to JSON strings, and extract human-readable messages without directly manipulating the underlying object.

## API

### WithMessage

```csharp
public static ErrorResponse WithMessage(this ErrorResponse error, string message)
```

Creates a new `ErrorResponse` instance with the specified message applied, leaving all other properties unchanged from the original.

**Parameters:**
- `error` — The source `ErrorResponse` to copy from. Must not be null.
- `message` — The error message string to set on the returned instance.

**Returns:** A new `ErrorResponse` with the given message.

**Throws:** `ArgumentNullException` when `error` is null.

---

### WithStatusCode

```csharp
public static ErrorResponse WithStatusCode(this ErrorResponse error, int statusCode)
```

Creates a new `ErrorResponse` instance with the specified HTTP-style status code applied, preserving all other properties from the original.

**Parameters:**
- `error` — The source `ErrorResponse` to copy from. Must not be null.
- `statusCode` — The integer status code to set on the returned instance.

**Returns:** A new `ErrorResponse` with the given status code.

**Throws:** `ArgumentNullException` when `error` is null.

---

### ToJson

```csharp
public static string ToJson(this ErrorResponse error)
```

Serializes the `ErrorResponse` instance to its JSON string representation.

**Parameters:**
- `error` — The `ErrorResponse` to serialize. Must not be null.

**Returns:** A JSON string representing the error object.

**Throws:** `ArgumentNullException` when `error` is null. May throw `JsonSerializationException` or similar if the object graph contains non-serializable data.

---

### ToFriendlyMessage

```csharp
public static string ToFriendlyMessage(this ErrorResponse error)
```

Produces a human-readable, user-facing message string derived from the `ErrorResponse`. This is typically a formatted combination of the status code and message, suitable for display in client applications or logs.

**Parameters:**
- `error` — The `ErrorResponse` to format. Must not be null.

**Returns:** A formatted string intended for end-user consumption.

**Throws:** `ArgumentNullException` when `error` is null.

## Usage

### Example 1: Building and returning an error from a service

```csharp
public ErrorResponse CreateNotFoundError(string resourceName)
{
    var baseError = new ErrorResponse();
    return baseError
        .WithStatusCode(404)
        .WithMessage($"Resource '{resourceName}' was not found.");
}

// Later, serializing for an HTTP response:
var error = CreateNotFoundError("UserProfile");
string jsonPayload = error.ToJson();
// jsonPayload: {"statusCode":404,"message":"Resource 'UserProfile' was not found."}
```

### Example 2: Logging a friendly message from a caught error

```csharp
try
{
    // Some operation that produces an ErrorResponse
}
catch (OperationException ex)
{
    var error = new ErrorResponse()
        .WithStatusCode(500)
        .WithMessage(ex.Message);

    // Display to user:
    Console.WriteLine(error.ToFriendlyMessage());
    // Output: "Error 500: An unexpected failure occurred."

    // Log raw JSON for diagnostics:
    logger.Error(error.ToJson());
}
```

## Notes

- All methods are pure extension methods that treat the source `ErrorResponse` as immutable; each returns a new instance rather than mutating the original. This makes them safe for chaining in fluent-style expressions.
- `WithMessage` and `WithStatusCode` perform shallow copies of the source object. If `ErrorResponse` gains reference-type properties in the future, callers should be aware that those references are shared between the original and the copy.
- `ToJson` relies on the configured JSON serializer (typically `System.Text.Json` or Newtonsoft.Json). Serialization behavior—such as property naming policy, null handling, and indentation—depends on the serializer defaults or any attributes applied to the `ErrorResponse` class.
- `ToFriendlyMessage` may return a string that includes internal technical details if the `ErrorResponse` message was not sanitized upstream. Callers exposing this output to untrusted clients should ensure messages are curated.
- None of these methods are thread-safe by design; they operate on local instances. Concurrent access to the same `ErrorResponse` instance across threads while calling these methods is safe only because the instance is not mutated—each call reads and produces a new object independently.
