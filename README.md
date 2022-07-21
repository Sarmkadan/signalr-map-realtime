// ... (rest of file remains unchanged)

## Waypoint

The `Waypoint` class represents a specific location within a route that needs to be visited during navigation. It tracks sequencing information, timing constraints, estimated duration, completion status, and contact details for each waypoint in a route. Waypoints can be marked as completed and reset when needed.

### Usage Example

```csharp
// Create a new waypoint for a delivery route
var waypoint = new Waypoint
{
  Id = 1,
  Sequence = 1,
  Name = "Warehouse Pickup",
  Latitude = 40.7128,
  Longitude = -74.0060,
  Address = "123 Main St, New York, NY",
  ArrivalTimeStart = "09:00",
  ArrivalTimeEnd = "10:00",
  EstimatedDurationMinutes = 30,
  Instructions = "Pick up package from loading dock B",
  ContactName = "John Smith",
  ContactPhone = "+1-555-123-4567",
  RouteId = 101
};

// Mark the waypoint as completed
waypoint.CompleteWaypoint();

// Check if coordinates are valid
if (waypoint.HasValidCoordinates)
{
  Console.WriteLine($"Waypoint {waypoint.Name} at ({waypoint.Latitude}, {waypoint.Longitude})");
}

// Reset the waypoint for a new attempt
waypoint.Reset();

// Access route navigation
var route = waypoint.Route;
```

## Asset

The `Asset` class represents a trackable asset (equipment, container, package) in the system. It provides comprehensive tracking capabilities including location history, vehicle assignments, condition monitoring, and special handling requirements. Assets can be assigned to vehicles, have their location updated, and track various metadata such as value, serial numbers, and status conditions.

### Usage Example

```csharp
// Create a new asset representing a delivery container
var asset = new Asset
{
    Id = 101,
    Name = "Container-2024-001",
    SerialNumber = "CON-2024-001-ABC",
    AssetType = AssetType.Container,
    Value = 15000.00m,
    Description = "High-value medical equipment container",
    Condition = "Excellent",
    RequiresSpecialHandling = true,
    SpecialHandlingInstructions = "Keep upright, avoid extreme temperatures",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    LastTrackedAt = DateTime.UtcNow
};

// Assign the asset to a vehicle for transport
var vehicle = new Vehicle { Id = 5, Name = "Truck-001", LicensePlate = "ABC123" };
asset.AssignToVehicle(vehicle);

// Update the asset's current location
var currentLocation = new Location { Id = 20, Name = "Warehouse Dock 3B", Latitude = 40.7128, Longitude = -74.0060 };
asset.UpdateLocation(currentLocation);

// Add to location history
asset.LocationHistory.Add(new Location { Id = 15, Name = "Distribution Center", Latitude = 40.7306, Longitude = -73.9352 });

// Update condition status
asset.UpdateCondition("Good");

// Disable special handling when no longer needed
asset.DisableSpecialHandling();

// Unassign from vehicle when delivery is complete
asset.UnassignFromVehicle();
```

## Location

The `Location` class represents a geographic location point with comprehensive tracking metadata for real-time mapping and navigation systems. It stores essential GPS coordinates along with optional movement data (speed, bearing, accuracy), timing information, and contextual details like address and notes. The class includes utility methods for distance calculation and coordinate validation.

### Usage Example

```csharp
// Create a location record for a vehicle tracking system
var location = new Location
{
    Id = 1,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Altitude = 15.5,
    Accuracy = 3.2,
    Speed = 45.6,
    Bearing = 125.3,
    LocationType = LocationType.TrackingPoint,
    Address = "123 Main St, New York, NY 10001",
    Notes = "Vehicle on delivery route",
    VehicleId = 5,
    TrackingSessionId = 101,
    RecordedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    Timestamp = DateTime.UtcNow,
    Heading = 125.3
};

// Validate coordinates
if (location.IsValidCoordinate())
{
    Console.WriteLine($"Valid location: {location.Latitude}, {location.Longitude}");
}

// Calculate distance to another location (in kilometers)
var otherLocation = new Location
{
    Latitude = 40.7306,
    Longitude = -73.9352
};
double distanceKm = location.CalculateDistanceTo(otherLocation);
Console.WriteLine($"Distance to other location: {distanceKm:F2} km");

// Check if location is significantly different from another
bool isDifferent = location.IsDifferentFrom(otherLocation);
Console.WriteLine($"Location is different: {isDifferent}");

// Access related entities
var vehicle = location.Vehicle;
var trackingSession = location.TrackingSession;
```
