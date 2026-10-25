# ControllerExtensions

`ControllerExtensions` is a static class in the `SignalRMapRealtime.Controllers` namespace that provides extension methods for building standardized error responses from controllers. It converts an `Exception` into the unified error envelope through the `IApiErrorSerializable` pipeline (`ToApiError()`, `ToErrorResponse(...)`, `GetHttpStatusCode()`), so every controller reports errors in the same shape.

The class itself does not catch or log exceptions. Callers pass in an exception they already hold, and the methods only build the response.

## API

### ErrorResponse(Exception, bool)
**Signature** – `public static IActionResult ErrorResponse(this ControllerBase controller, Exception exception, bool indented = false)`

**Purpose** – Returns an `IActionResult` carrying the standardized error envelope for any exception.

**Parameters**
- `controller` (ControllerBase): The controller instance the method extends.
- `exception` (Exception): The exception to convert.
- `indented` (bool, optional): When `true`, the JSON body is indented for readability. Defaults to `false`.

**Return Value** – An `IActionResult` created by `controller.StatusCode(...)`. The HTTP status code is taken from the converted API error (`GetHttpStatusCode()`), and the body is the serialized error JSON (`ToErrorResponse(indented)`).

**Exceptions**
- `ArgumentNullException` if `controller` is null.
- `ArgumentNullException` if `exception` is null.

### ErrorResponse<TException>(TException, bool)
**Signature** – `public static IActionResult ErrorResponse<TException>(this ControllerBase controller, TException exception, bool indented = false) where TException : Exception`

**Purpose** – Generic overload of `ErrorResponse` for a specific exception type. The behavior is the same as the non-generic overload.

**Parameters**
- `controller` (ControllerBase): The controller instance the method extends.
- `exception` (`TException`, constrained to `Exception`): The exception to convert.
- `indented` (bool, optional): When `true`, the JSON body is indented. Defaults to `false`.

**Return Value** – The same `IActionResult` as the non-generic overload: a status-coded response whose body is the serialized error envelope.

**Exceptions**
- `ArgumentNullException` if `controller` is null.
- `ArgumentNullException` if `exception` is null.

**Overload note** – When the argument's static type is a derived exception type, the compiler selects this generic overload. When the static type is `Exception`, the non-generic overload is selected. Both produce identical output.

### CreateErrorResponse(string?)
**Signature** – `public static object CreateErrorResponse(this Exception exception, string? traceId = null)`

**Purpose** – Builds the error envelope as a plain object instead of an `IActionResult`. Use it when a caller needs the payload itself, for example to embed it in a different response or to log it.

**Parameters**
- `exception` (Exception): The exception to convert. This method extends `Exception`, not `ControllerBase`.
- `traceId` (string?, optional): An optional trace identifier to include in the envelope. Defaults to `null`.

**Return Value** – An anonymous object with these properties:

| Property     | Source                                  |
|--------------|-----------------------------------------|
| `errorCode`  | `error.GetErrorCode()`                  |
| `message`    | `error.GetMessage()`                    |
| `statusCode` | `error.GetHttpStatusCode()`             |
| `timestamp`  | `DateTime.UtcNow` in ISO 8601 (`"o"`)   |
| `traceId`    | The `traceId` argument, or `null`       |
| `details`    | `error.GetDetails()`                    |

The method's XML comment describes the result as a dictionary. The actual return value is an anonymous object, so its properties are accessed by name, not by key.

**Exceptions**
- `ArgumentNullException` if `exception` is null.

## Usage

```csharp
using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        try
        {
            // ... load the vehicle ...
            return Ok();
        }
        catch (Exception ex)
        {
            // Returns the status code and error envelope chosen by the error pipeline.
            return this.ErrorResponse(ex);
        }
    }
}
```

```csharp
using System;
using SignalRMapRealtime.Controllers;

// Build the envelope without an HTTP response, e.g. to log it with a trace identifier.
object payload = exception.CreateErrorResponse(traceId: HttpContext.TraceIdentifier);
```

## Notes
- All three methods validate their arguments with `ArgumentNullException.ThrowIfNull` before doing any work, so a null controller or exception fails fast.
- `ErrorResponse` and `ErrorResponse<TException>` return the status code from the converted error, not a fixed `500`. The status code depends on how the exception is mapped by `ToApiError()`.
- `CreateErrorResponse` has no `indented` parameter; its result is an object, not serialized JSON. Serialization happens only in the `ErrorResponse` overloads.
- The `timestamp` in `CreateErrorResponse` is generated per call in UTC, so two calls for the same exception produce different timestamps.
- The helper methods used here (`ToApiError`, `ToErrorResponse`, `GetHttpStatusCode`, `GetErrorCode`, `GetMessage`, `GetDetails`) are not defined in this file. Their behavior is documented with the `IApiErrorSerializable` and error types.
