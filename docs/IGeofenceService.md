# IGeofenceService
The `IGeofenceService` interface provides a set of methods for managing geofences, which are virtual boundaries around a physical location. It allows for registering and removing zones, retrieving active zones, and checking if a location is within any of the registered zones. This interface is designed to be used in applications that require location-based services, such as mapping and tracking systems.

## API
* `GeofenceService`: The constructor for the `GeofenceService` class, which implements the `IGeofenceService` interface.
* `RegisterZoneAsync`: Registers a new geofence zone. Returns a `Task<GeofenceDto>` containing the registered zone's details. Throws if the zone cannot be registered.
* `RemoveZoneAsync`: Removes a previously registered geofence zone. Returns a `Task<bool>` indicating whether the removal was successful. Throws if the zone does not exist or cannot be removed.
* `GetActiveZonesAsync`: Retrieves a list of all active geofence zones. Returns a `Task<IReadOnlyList<GeofenceDto>>` containing the list of active zones.
* `CheckLocationAsync`: Checks if a given location is within any of the registered geofence zones. Returns a `Task<IReadOnlyList<GeofenceAlertDto>>` containing a list of alerts for the location.
* `AddGeofencing`: A static method that adds geofencing services to an `IServiceCollection`. Returns the modified `IServiceCollection`.

## Usage
```csharp
// Example 1: Registering a new geofence zone
var geofenceService = new GeofenceService();
var zone = new GeofenceDto { Id = 1, Latitude = 37.7749, Longitude = -122.4194, Radius = 100 };
var registeredZone = await geofenceService.RegisterZoneAsync(zone);
Console.WriteLine($"Registered zone: {registeredZone.Id}");

// Example 2: Checking if a location is within any registered zones
var location = new Location { Latitude = 37.7859, Longitude = -122.4364 };
var alerts = await geofenceService.CheckLocationAsync(location);
foreach (var alert in alerts)
{
    Console.WriteLine($"Alert: {alert.Message}");
}
```

## Notes
The `IGeofenceService` interface is designed to be thread-safe, allowing multiple concurrent calls to its methods. However, the implementation of the `GeofenceService` class may have specific requirements or limitations regarding thread-safety. It is recommended to review the implementation details to ensure correct usage in multi-threaded environments. Additionally, the `CheckLocationAsync` method may return multiple alerts if the location is within multiple registered zones. The `AddGeofencing` method should be used to add geofencing services to an `IServiceCollection` during application startup.
