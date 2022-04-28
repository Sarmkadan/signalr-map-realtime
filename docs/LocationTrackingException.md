# LocationTrackingException

`LocationTrackingException` is a custom exception type used in the `signalr-map-realtime` project to encapsulate errors related to vehicle location tracking operations. It provides contextual information such as vehicle, asset, and session identifiers, along with specific sub-exceptions that detail the root cause of the failure (e.g., invalid location data or missing entities).

## API

### Constructors

#### `LocationTrackingException()`
Initializes a new instance of the `LocationTrackingException` class with default values.  
**Parameters:** None.  
**Returns:** A new `LocationTrackingException` instance.  
**Throws:** No exceptions are thrown by this constructor.

#### `LocationTrackingException(string message)`
Initializes a new instance with a specified error message.  
**Parameters:**  
- `message` (string): The error message explaining the exception.  
**Returns:** A new `LocationTrackingException` instance.  
**Throws:** No exceptions are thrown by this constructor.

#### `LocationTrackingException(string message, Exception innerException)`
Initializes a new instance with a specified error message and inner exception.  
**Parameters:**  
- `message` (string): The error message explaining the exception.  
- `innerException` (Exception): The underlying exception that caused this exception.  
**Returns:** A new `LocationTrackingException` instance.  
**Throws:** No exceptions are thrown by this constructor.

---

### Properties

#### `int VehicleId`
Gets the identifier of the vehicle associated with the location tracking error.  
**Purpose:** Identifies the vehicle involved in the failed operation.  
**Returns:** The vehicle's unique identifier.  
**Throws:** No exceptions.

#### `VehicleNotFoundException VehicleNotFoundException`
Gets the specific exception thrown when a vehicle could not be located.  
**Purpose:** Provides details about the missing vehicle.  
**Returns:** A `VehicleNotFoundException` instance if applicable; otherwise, `null`.  
**Throws:** No exceptions.

#### `double? Latitude`
Gets the latitude coordinate that caused the error, if applicable.  
**Purpose:** Indicates the invalid latitude value in location-related failures.  
**Returns:** The latitude as a nullable double, or `null` if not relevant.  
**Throws:** No exceptions.

#### `double? Longitude`
Gets the longitude coordinate that caused the error, if applicable.  
**Purpose:** Indicates the invalid longitude value in location-related failures.  
**Returns:** The longitude as a nullable double, or `null` if not relevant.  
**Throws:** No exceptions.

#### `InvalidLocationException InvalidLocationException`
Gets the specific exception thrown when location data is invalid.  
**Purpose:** Provides details about invalid coordinates or formatting.  
**Returns:** An `InvalidLocationException` instance if applicable; otherwise, `null`.  
**Throws:** No exceptions.

#### `int AssetId`
Gets the identifier of the asset associated with the location tracking error.  
**Purpose:** Identifies the asset involved in the failed operation.  
**Returns:** The asset's unique identifier.  
**Throws:** No exceptions.

#### `AssetNotFoundException AssetNotFoundException`
Gets the specific exception thrown when an asset could not be located.  
**Purpose:** Provides details about the missing asset.  
**Returns:** An `AssetNotFoundException` instance if applicable; otherwise, `null`.  
**Throws:** No exceptions.

#### `int SessionId`
Gets the identifier of the tracking session associated with the error.  
**Purpose:** Identifies the session involved in the failed operation.  
**Returns:** The session's unique identifier.  
**Throws:** No exceptions.

#### `TrackingSessionNotFoundException TrackingSessionNotFoundException`
Gets the specific exception thrown when a tracking session could not be located.  
**Purpose:** Provides details about the missing session.  
**Returns:** A `TrackingSessionNotFoundException` instance if applicable; otherwise, `null`.  
**Throws:** No exceptions.

---

## Usage

### Example 1: Vehicle Not Found
```csharp
try
{
    var vehicle = vehicleService.GetVehicle(vehicleId);
    if (vehicle == null)
    {
        throw new LocationTrackingException(
            $"Vehicle with ID {vehicleId} not found.",
            new VehicleNotFoundException($"Vehicle {vehicleId} does not exist.")
        )
        {
            VehicleId = vehicleId
        };
    }
}
catch (VehicleNotFoundException ex)
{
    // Handle or rethrow as LocationTrackingException
    throw new LocationTrackingException("Failed to track vehicle location.", ex)
    {
        VehicleId = vehicleId,
        VehicleNotFoundException = ex
    };
}
```

### Example 2: Invalid Location Data
```csharp
try
{
    ValidateLocation(latitude, longitude);
}
catch (InvalidLocationException ex)
{
    throw new LocationTrackingException(
        "Invalid location coordinates provided.",
        ex
    )
    {
        Latitude = latitude,
        Longitude = longitude,
        InvalidLocationException = ex
    };
}
```

---

## Notes

- **Immutability:** All properties are read-only after construction. Values are set during initialization and cannot be modified, ensuring thread-safety for read operations.  
- **Null Handling:** Nullable properties (`Latitude`, `Longitude`) may return `null` if the error context does not involve coordinate validation.  
- **Inner Exceptions:** Sub-exceptions (`VehicleNotFoundException`, `InvalidLocationException`, etc.) are optional and only populated when the error directly corresponds to their specific failure mode.  
- **Usage Context:** This exception is typically thrown in service layers where vehicle, asset, or session validation occurs prior to location tracking operations.
