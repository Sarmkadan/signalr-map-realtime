# VehicleStaleEvent
The `VehicleStaleEvent` type represents an event that occurs when a vehicle's data becomes stale, indicating that the vehicle has not sent an update within a specified time window. This type is used to track and manage vehicle data freshness in real-time mapping applications.

## API
The `VehicleStaleEvent` type has the following public members:
* `VehicleId`: An integer identifier for the vehicle that triggered the stale event.
* `VehicleRegistration`: A string representing the vehicle's registration number.
* `VehicleName`: A string representing the vehicle's name.
* `LastUpdateTime`: A `DateTime` object representing the last time the vehicle sent an update.
* `StaleSince`: A `DateTime` object representing the time when the vehicle's data became stale.
* `StaleWindowMinutes`: An integer representing the time window (in minutes) within which the vehicle's data is considered fresh.
* `TimeSinceLastUpdateMinutes`: A double representing the time (in minutes) since the vehicle's last update.
* `IsRecovery`: A boolean indicating whether the vehicle's data is recovering from a stale state.
* `WasPreviouslyStale`: A boolean indicating whether the vehicle's data was previously stale.

## Usage
Here are two examples of using the `VehicleStaleEvent` type in C#:
```csharp
// Example 1: Creating a new VehicleStaleEvent
VehicleStaleEvent staleEvent = new VehicleStaleEvent
{
    VehicleId = 123,
    VehicleRegistration = "ABC123",
    VehicleName = "Vehicle 1",
    LastUpdateTime = DateTime.Now.AddMinutes(-10),
    StaleSince = DateTime.Now.AddMinutes(-5),
    StaleWindowMinutes = 5,
    TimeSinceLastUpdateMinutes = 10,
    IsRecovery = false,
    WasPreviouslyStale = true
};

// Example 2: Handling a VehicleStaleEvent in a real-time mapping application
public void HandleVehicleStaleEvent(VehicleStaleEvent staleEvent)
{
    if (staleEvent.IsRecovery)
    {
        Console.WriteLine($"Vehicle {staleEvent.VehicleName} is recovering from a stale state.");
    }
    else
    {
        Console.WriteLine($"Vehicle {staleEvent.VehicleName} has become stale.");
    }
}
```

## Notes
When working with `VehicleStaleEvent`, consider the following edge cases:
* If `StaleWindowMinutes` is set to 0, the vehicle's data will be considered stale immediately after the last update.
* If `LastUpdateTime` is later than `StaleSince`, the vehicle's data is not considered stale.
* The `TimeSinceLastUpdateMinutes` property may not be exactly equal to the difference between `StaleSince` and `LastUpdateTime`, due to potential clock skew or other timing issues.
* The `VehicleStaleEvent` type is not thread-safe, as its properties can be modified concurrently. If multiple threads need to access or modify a `VehicleStaleEvent` instance, proper synchronization mechanisms should be used to ensure data integrity.
