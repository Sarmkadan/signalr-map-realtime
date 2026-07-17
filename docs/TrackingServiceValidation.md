# TrackingServiceValidation

`TrackingServiceValidation` is a centralized, static utility class within the `signalr-map-realtime` framework designed to enforce data integrity for vehicle identifiers, session parameters, and tracking metadata. By providing a unified set of diagnostic, boolean, and imperative validation methods, it enables consistent handling of input validation across service layers, ensuring that all session operations—such as starting, pausing, or completing sessions—proceed with validated and compliant configuration data.

## API

### Validation Rule Methods
These methods return an `IReadOnlyList<string>` containing validation error messages. If the input is valid, the returned list is empty.

*   `ValidateVehicleId`: Validates the format and requirements of a vehicle identifier.
*   `ValidateSessionId`: Validates the format and requirements of a session identifier.
*   `ValidateSessionName`: Validates the requirements of a session name string.
*   `ValidateRouteId`: Validates the format and requirements of a route identifier.
*   `ValidateCancellationReason`: Validates the format and requirements of a cancellation reason.
*   `ValidateStartSessionParameters`: Validates parameters for starting a session.
*   `ValidatePauseSessionParameters`: Validates parameters for pausing a session.
*   `ValidateResumeSessionParameters`: Validates parameters for resuming a session.
*   `ValidateCompleteSessionParameters`: Validates parameters for completing a session.
*   `ValidateCancelSessionParameters`: Validates parameters for canceling a session.
*   `ValidateSessionStatusParameters`: Validates parameters for session status operations.
*   `ValidateSessionDistanceParameters`: Validates parameters for session distance tracking.
*   `ValidateSessionSpeedParameters`: Validates parameters for session speed tracking.

### Validation Check Methods
These methods return a `bool` indicating whether the provided input is valid according to the defined rules.

*   `IsValidVehicleId`: Returns `true` if the vehicle ID is valid.
*   `IsValidSessionId`: Returns `true` if the session ID is valid.
*   `IsValidSessionName`: Returns `true` if the session name is valid.
*   `IsValidRouteId`: Returns `true` if the route ID is valid.
*   `IsValidCancellationReason`: Returns `true` if the cancellation reason is valid.

### Enforcement Methods
These methods perform validation and throw an exception if the provided input is invalid.

*   `EnsureValidVehicleId`: Ensures the vehicle ID is valid; throws an exception if invalid.
*   `EnsureValidSessionId`: Ensures the session ID is valid; throws an exception if invalid.

## Usage

### Example 1: Conditional Logic using `IsValid`
Using the boolean check methods to perform conditional logic without throwing exceptions.

```csharp
public void ProcessVehicleUpdate(string vehicleId)
{
    if (TrackingServiceValidation.IsValidVehicleId(vehicleId))
    {
        // Proceed with processing
    }
    else
    {
        // Log error and handle invalid input gracefully
        _logger.LogWarning("Invalid vehicle ID provided: {VehicleId}", vehicleId);
    }
}
```

### Example 2: Input Enforcement using `EnsureValid`
Using the `Ensure` methods to enforce strict data contracts at method entry points.

```csharp
public void StartTrackingSession(string sessionId)
{
    // Will throw if sessionId does not meet requirements
    TrackingServiceValidation.EnsureValidSessionId(sessionId);

    // Proceed with logic, knowing sessionId is guaranteed valid
    _sessionRepository.Activate(sessionId);
}
```

## Notes

*   **Thread Safety**: The `TrackingServiceValidation` class is static and stateless, as its methods perform validation logic based solely on the provided input arguments. Therefore, all methods are thread-safe and can be safely invoked concurrently from multiple threads.
*   **Edge Cases**: Input values that are `null`, empty, or consist entirely of whitespace will generally fail validation in the `Validate` and `IsValid` methods. Users of the `EnsureValid` methods should expect exceptions to be thrown for these inputs.
*   **Diagnostic Usage**: The `Validate` methods are intended for diagnostic scenarios, such as gathering comprehensive error feedback to present to a user or to log detailed validation failures, as opposed to the `IsValid` methods which are intended for control flow.
