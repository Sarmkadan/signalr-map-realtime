# GeofenceServiceTests
The `GeofenceServiceTests` class is designed to test the functionality of the `GeofenceService` class, which is responsible for managing geofences and detecting when vehicles enter or exit these zones. This test class ensures that the service behaves correctly under various scenarios, including registering and removing zones, and checking vehicle locations against these zones.

## API
The `GeofenceServiceTests` class contains several test methods that verify the correctness of the `GeofenceService` class:
* `public async Task RegisterZone_WithValidDto_ReturnsZoneDtoWithMatchingProperties`: Tests that registering a zone with a valid DTO returns a `ZoneDto` with matching properties.
* `public async Task GetActiveZones_AfterRegisteringZone_ContainsRegisteredZone`: Tests that getting active zones after registering a zone contains the registered zone.
* `public async Task RemoveZone_ExistingZone_ReturnsTrueAndZoneIsGone`: Tests that removing an existing zone returns `true` and the zone is removed.
* `public async Task RemoveZone_NonExistentId_ReturnsFalse`: Tests that removing a non-existent zone returns `false`.
* `public async Task CheckLocation_WhenVehicleEntersCircleZone_EmitsEnteredAlert`: Tests that checking a vehicle's location when it enters a circle zone emits an entered alert.
* `public async Task CheckLocation_VehicleExitsZone_EmitsExitedAlert`: Tests that checking a vehicle's location when it exits a zone emits an exited alert.
* `public async Task CheckLocation_VehicleRemainsInsideZone_ProducesNoDuplicateAlerts`: Tests that checking a vehicle's location when it remains inside a zone produces no duplicate alerts.

## Usage
Here are two examples of using the `GeofenceServiceTests` class:
```csharp
// Example 1: Registering a zone and checking its properties
var geofenceService = new GeofenceService();
var zoneDto = new ZoneDto { Id = 1, Name = "Test Zone", Latitude = 37.7749, Longitude = -122.4194, Radius = 1000 };
await geofenceService.RegisterZone(zoneDto);
var registeredZone = await geofenceService.GetActiveZones();
Assert.IsTrue(registeredZone.Any(z => z.Id == zoneDto.Id && z.Name == zoneDto.Name));

// Example 2: Checking a vehicle's location against a zone
var vehicleLocation = new Location { Latitude = 37.7859, Longitude = -122.4364 };
var zone = new Zone { Id = 1, Name = "Test Zone", Latitude = 37.7749, Longitude = -122.4194, Radius = 1000 };
await geofenceService.CheckLocation(vehicleLocation, zone);
// Verify that an entered alert is emitted
```

## Notes
The `GeofenceServiceTests` class assumes that the `GeofenceService` class is thread-safe and can handle concurrent requests. However, in a real-world scenario, you may need to consider synchronization mechanisms to ensure that zone registrations and removals are properly synchronized. Additionally, the `GeofenceService` class may throw exceptions if the input data is invalid or if there are errors during zone registration or removal. You should handle these exceptions accordingly in your production code.
