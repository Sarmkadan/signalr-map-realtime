// ... existing content ...

## DomainEventExtensions

The `DomainEventExtensions` class provides utility methods for working with domain events in the application. These extensions enable easy cloning, serialization, and type checking of domain events.

### Usage Example

```csharp
var originalEvent = new VehicleStatusChangedEvent
{
    VehicleId = Guid.NewGuid(),
    Status = VehicleStatus.InTransit,
    Timestamp = DateTime.UtcNow
};

var clonedEvent = DomainEventExtensions.Clone(originalEvent);

Console.WriteLine(clonedEvent.ToJson());
// Output: {"VehicleId":"<guid>","Status":"InTransit","Timestamp":"<timestamp>"}

Console.WriteLine(DomainEventExtensions.IsLocationUpdate(originalEvent));
// Output: False

Console.WriteLine(DomainEventExtensions.IsStatusChange(originalEvent));
// Output: True

Console.WriteLine(DomainEventExtensions.GetVehicleId(originalEvent));
// Output: <guid>

Console.WriteLine(DomainEventExtensions.GetDescription(originalEvent));
// Output: A human-readable description of the event
```

## LocationRepositoryExtensions

The `LocationRepositoryExtensions` class provides utility methods for querying and retrieving location data from the repository, grouped by vehicle ID and filtered by time ranges, types, or proximity. These methods are optimized for common location analysis patterns like tracking, statistics, and spatial queries.

### Usage Example

```csharp
// Example: Get locations for vehicle ID 123 within a 10km radius of (40.7128, -74.0060) between two dates
var vehicleId = 123;
var centerLat = 40.7128;
var centerLon = -74.0060;
var radiusKm = 10.0;
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 1, 31);

var locations = await LocationRepositoryExtensions.GetLocationsInTimeRangeAndRadiusAsync(
    repository, 
    vehicleId, 
    centerLat, 
    centerLon, 
    radiusKm, 
    startDate, 
    endDate
);

Console.WriteLine($"Found {locations.Count()} locations for vehicle {vehicleId} in the specified area and time range.");
```

// ... existing content ...
