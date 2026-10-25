# ApiErrorBase

Source: `src/SignalRMapRealtime/Exceptions/ApiErrorBase.cs`

`ApiErrorBase` is an abstract base class in the `SignalRMapRealtime.Exceptions` namespace. It wraps an `Exception` and produces a consistent error envelope (error code, message, HTTP status code, timestamp, trace ID, optional details, optional inner exception message) as a JSON string or as a dictionary. It implements `IApiErrorSerializable`.

Derived classes must implement `GetErrorCode()`. They may override `GetHttpStatusCode()`, `GetMessage()` and `GetDetails()`.

## Constructor

### `ApiErrorBase(Exception exception)`
Protected. Stores the wrapped exception.

**Parameters:**
- `exception` - the exception to serialize.

**Throws:** `ArgumentNullException` if `exception` is `null`.

## Properties

### `Exception`
`protected Exception { get; }`

The wrapped exception. Used by the base implementation to read the message, inner exception, and type for status code mapping. Derived classes can read it but cannot replace it.

## Methods

### `ToErrorResponse(bool indented = false)`
`public string`

Serializes the error envelope to a JSON string. The method:

1. Reads `GetErrorCode()`, `GetMessage()`, `GetDetails()` and `GetHttpStatusCode()`.
2. Builds a dictionary with these keys:
   - `errorCode` - value of `GetErrorCode()`.
   - `message` - value of `GetMessage()`.
   - `statusCode` - value of `GetHttpStatusCode()`.
   - `timestamp` - `DateTime.UtcNow` in round-trip ("o") format.
   - `traceId` - always `null` here. The comment in the source says the controller sets it.
3. Adds `details` only if `GetDetails()` returns a non-null value.
4. Adds `innerException` (the inner exception's `Message`) only if `Exception.InnerException` is not `null`.
5. Serializes the dictionary with the null-handling options. When `indented` is `true`, a copy of those options with `WriteIndented = true` is used.

**Parameters:**
- `indented` - `true` for indented output, `false` (default) for compact output.

**Returns:** JSON string with the error envelope.

Serialization options (static, private):
- Base: `JsonSerializerDefaults.Web`, camelCase property naming, case-insensitive property names.
- Null handling: `DefaultIgnoreCondition = WhenWritingNull`.
- Enums: `JsonStringEnumConverter` with camelCase naming.

### `GetHttpStatusCode()`
`public virtual int`

Returns the HTTP status code for the error. The default implementation maps `GetErrorCode()` as follows:

| Error code | Status code |
|---|---|
| `VALIDATION_ERROR` | 400 |
| `INVALID_INPUT` | 400 |
| `RESOURCE_NOT_FOUND` | 404 |
| `NOT_FOUND` | 404 |
| `UNAUTHORIZED` | 401 |
| `FORBIDDEN` | 403 |
| `CONFLICT` | 409 |
| `INTERNAL_SERVER_ERROR` | 500 |
| any other code | 400 if the wrapped exception is a `System.ComponentModel.DataAnnotations.ValidationException`; otherwise 500 |

Override this method to use a different mapping for a derived type.

### `GetErrorCode()`
`public abstract string`

Returns the error code used for programmatic handling. Every derived class must implement it. The base class does not define any error code values.

### `GetMessage()`
`public virtual string`

Returns `Exception.Message`. Override to change the message shown to clients.

### `GetDetails()`
`public virtual object?`

Returns `null` by default. Override to return type-specific details. Non-null values are added to the envelope under `details` by `ToErrorResponse`, and are always included by `CreateErrorResponse`.

### `CreateErrorResponse()`
`protected Dictionary<string, object?>`

Builds the same envelope as a dictionary for use by derived classes or controllers. It differs from `ToErrorResponse` in these ways:

- Returns a dictionary, not a JSON string.
- Always includes the `details` key, even when `GetDetails()` returns `null`.
- Does not include `innerException`.

Keys: `errorCode`, `message`, `statusCode`, `timestamp`, `traceId` (always `null`), `details`.

## Example

```csharp
public sealed class VehicleNotFoundApiError : ApiErrorBase
{
    private readonly Guid _vehicleId;

    public VehicleNotFoundApiError(Exception exception, Guid vehicleId)
        : base(exception)
    {
        _vehicleId = vehicleId;
    }

    public override string GetErrorCode() => "RESOURCE_NOT_FOUND";

    public override object? GetDetails() => new { vehicleId = _vehicleId };
}

// Compact JSON: {"errorCode":"RESOURCE_NOT_FOUND","message":"...","statusCode":404,"timestamp":"...","traceId":null,"details":{"vehicleId":"..."}}
string json = new VehicleNotFoundApiError(ex, id).ToErrorResponse();
```

## Notes

- The `_jsonOptions` field is declared but not used by any member in this file. Only `_jsonOptionsWithNullHandling` is used.
- `traceId` is always written as `null` by this class. Nothing in this file sets it.
