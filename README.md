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