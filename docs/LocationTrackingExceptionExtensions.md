# LocationTrackingExceptionExtensions

Provides static extension methods that extract structured information from exceptions thrown during real-time location tracking operations. These helpers convert raw exception data into typed identifiers, coordinate snapshots, human-readable error messages, and boolean error-type checks, enabling callers to handle location-specific failures without parsing exception internals directly.

## API

### GetVehicleId

```csharp
public static int? GetVehicleId(this Exception exception)
```

Extracts the vehicle identifier associated with the faulting location operation, if one was recorded in the exception data dictionary.

**Parameters:**
- `exception` — The exception instance to inspect. Must not be null.

**Returns:** The vehicle ID as a nullable integer, or `null` when no vehicle ID was attached to the exception.

**Throws:** `ArgumentNullException` when `exception` is null.

---

### GetAssetId

```csharp
public static int? GetAssetId(this Exception exception)
```

Extracts the asset identifier associated with the faulting location operation, if one was recorded in the exception data dictionary.

**Parameters:**
- `exception` — The exception instance to inspect. Must not be null.

**Returns:** The asset ID as a nullable integer, or `null` when no asset ID was attached to the exception.

**Throws:** `ArgumentNullException` when `exception` is null.

---

### GetSessionId

```csharp
public static int? GetSessionId(this Exception exception)
```

Extracts the tracking session identifier associated with the faulting location operation, if one was recorded in the exception data dictionary.

**Parameters:**
- `exception` — The exception instance to inspect. Must not be null.

**Returns:** The session ID as a nullable integer, or `null` when no session ID was attached to the exception.

**Throws:** `ArgumentNullException` when `exception` is null.

---

### GetCoordinates

```csharp
public static void GetCoordinates(this Exception exception, out double latitude, out double longitude)
```

Retrieves the latitude and longitude that were in flight when the location tracking exception occurred. Values are extracted from the exception data dictionary and returned via output parameters.

**Parameters:**
- `exception` — The exception instance to inspect. Must not be null.
- `latitude` — Out parameter receiving the latitude value, or `0.0` if no coordinates were recorded.
- `longitude` — Out parameter receiving the longitude value, or `0.0` if no coordinates were recorded.

**Returns:** Void. Results are delivered through the `out` parameters.

**Throws:** `ArgumentNullException` when `exception` is null.

---

### ToErrorMessage

```csharp
public static string ToErrorMessage(this Exception exception)
```

Produces a consolidated, human-readable error message by combining the exception's own message with any vehicle, asset, session, and coordinate metadata found in its data dictionary. The resulting string is suitable for logging, diagnostics, or user-facing status updates.

**Parameters:**
- `exception` — The exception instance to format. Must not be null.

**Returns:** A formatted string containing the exception message and any available contextual identifiers and coordinates.

**Throws:** `ArgumentNullException` when `exception` is null.

---

### IsNotFoundError

```csharp
public static bool IsNotFoundError(this Exception exception)
```

Determines whether the exception represents a "not found" condition — typically indicating that a requested vehicle, asset, or session does not exist or is no longer active in the tracking system.

**Parameters:**
- `exception` — The exception instance to test. Must not be null.

**Returns:** `true` if the exception is classified as a not-found error; otherwise `false`.

**Throws:** `ArgumentNullException` when `exception` is null.

---

### IsInvalidLocationError

```csharp
public static bool IsInvalidLocationError(this Exception exception)
```

Determines whether the exception represents an invalid location condition — typically indicating that the supplied coordinates are out of range, malformed, or otherwise unacceptable for processing.

**Parameters:**
- `exception` — The exception instance to test. Must not be null.

**Returns:** `true` if the exception is classified as an invalid-location error; otherwise `false`.

**Throws:** `ArgumentNullException` when `exception` is null.

## Usage

### Example 1: Logging a structured warning on location failure

```csharp
try
{
    await locationService.PublishLocationAsync(vehicleId, assetId, sessionId, lat, lon);
}
catch (Exception ex) when (ex.IsNotFoundError())
{
    int? vid = ex.GetVehicleId();
    int? aid = ex.GetAssetId();
    int? sid = ex.GetSessionId();
    ex.GetCoordinates(out double lat, out double lon);

    logger.Warning(
        "Location publish failed — entity not found. " +
        "Vehicle={VehicleId}, Asset={AssetId}, Session={SessionId}, " +
        "LastKnownLat={Lat}, LastKnownLon={Lon}, Detail={Detail}",
        vid, aid, sid, lat, lon, ex.ToErrorMessage());
}
```

### Example 2: Translating an exception into an API error response

```csharp
catch (Exception ex) when (ex.IsInvalidLocationError())
{
    ex.GetCoordinates(out double lat, out double lon);
    string message = ex.ToErrorMessage();

    return Results.BadRequest(new
    {
        Code = "INVALID_LOCATION",
        Message = message,
        SubmittedLatitude = lat,
        SubmittedLongitude = lon
    });
}
```

## Notes

- All methods throw `ArgumentNullException` when passed a null exception reference. Callers should guard with null checks if the exception source is uncertain.
- The `GetVehicleId`, `GetAssetId`, and `GetSessionId` methods return `null` when the corresponding key is absent from the exception's data dictionary. Callers must handle the null case to avoid unintended default-value assumptions.
- `GetCoordinates` sets both output parameters to `0.0` when no coordinate data is present. A coordinate of `(0.0, 0.0)` is a valid geographic point; consumers should distinguish "no data" by also checking whether `IsInvalidLocationError` or `IsNotFoundError` returns true, or by testing for the presence of vehicle/session identifiers.
- The boolean error-classification methods (`IsNotFoundError`, `IsInvalidLocationError`) rely on internal exception markers set by the location tracking layer. Exceptions originating from unrelated subsystems will return `false` for both checks.
- These methods are static extension methods and carry no mutable state. They are safe to call concurrently from multiple threads provided each thread supplies its own exception instance. No shared static state is modified during execution.
