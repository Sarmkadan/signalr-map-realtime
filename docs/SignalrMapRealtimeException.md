# SignalRMapRealtimeException

Source: `src/SignalRMapRealtime/Exceptions/SignalrMapRealtimeException.cs`

`SignalRMapRealtimeException` is the base exception for all `SignalRMapRealtime` specific errors. It lives in the `SignalRMapRealtime.Exceptions` namespace and derives from `System.Exception`.

The same file also contains `SignalrMapRealtimeException`, an obsolete shim kept for backward compatibility. See [Obsolete shim](#obsolete-shim-signalrmaprealtimeexception) below.

## Constructors

### `SignalRMapRealtimeException()`
Initializes a new instance with no message. The base `Exception` message is used.

### `SignalRMapRealtimeException(string message)`
Initializes a new instance with the specified error message.

**Parameters:**
- `message` - the error message that explains the reason for the exception.

### `SignalRMapRealtimeException(string message, Exception innerException)`
Initializes a new instance with the specified error message and a reference to the inner exception that caused it.

**Parameters:**
- `message` - the error message that explains the reason for the exception.
- `innerException` - the exception that is the cause of the current exception, or `null` if no inner exception is specified.

All three constructors delegate to the corresponding `System.Exception` constructor. The file does not add any properties, fields, or methods.

## Usage

```csharp
using SignalRMapRealtime.Exceptions;

throw new SignalRMapRealtimeException("Geofence boundary is invalid.");

try
{
    // ... call into a service ...
}
catch (InvalidOperationException ex)
{
    throw new SignalRMapRealtimeException("Failed to process location update.", ex);
}
```

## Obsolete shim: `SignalrMapRealtimeException`

`SignalrMapRealtimeException` (lowercase `r` in `Signalr`) is a deprecated type with the same three constructors. It derives from `SignalRMapRealtimeException`, so anything caught as the base type also catches instances of the shim.

- It is marked `[Obsolete("SignalrMapRealtimeException is obsolete. Use SignalRMapRealtimeException instead.", false)]`. The `false` argument means the compiler emits a warning, not an error, on use.
- It is retained for backward compatibility and will be removed in a future major version.
- New code should use `SignalRMapRealtimeException` instead.

The shim adds no behavior of its own. Each constructor only forwards its arguments to the base type.

## Notes

- Catch `SignalRMapRealtimeException` to handle all project errors at once. Catching the obsolete shim is only needed for code that still throws or handles it directly.
- Because the shim derives from the base type, a `catch (SignalRMapRealtimeException)` block also handles shim instances.
