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

## Vehicle

The `Vehicle` class represents a vehicle being tracked in the real-time location system. It provides comprehensive vehicle tracking capabilities including status monitoring, location history, driver assignments, fuel level tracking, and speed limit enforcement. Vehicles can be configured with manufacturer details, model information, and operational constraints like maximum speed and fuel capacity.

### Usage Example

```csharp
// Create a new delivery vehicle with manufacturer details
var vehicle = new Vehicle
{
    Id = 5,
    Name = "Delivery Van-001",
    RegistrationNumber = "ABC123",
    Manufacturer = "Ford",
    Model = "Transit",
    ModelYear = 2023,
    Make = "Ford",
    Year = 2023,
    VIN = "1FTEW1E83NKD00001",
    AssetType = AssetType.Van,
    MaxSpeed = 120.0,
    FuelLevel = 85.5,
    IsOnline = true,
    Status = VehicleStatus.Active,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Assign a driver to the vehicle
var driver = new User { Id = 10, Name = "John Doe", Email = "john.doe@company.com" };
vehicle.DriverId = driver.Id;
vehicle.Driver = driver;

// Record the vehicle's current location
var currentLocation = new Location
{
    Id = 1,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 45.6,
    Accuracy = 3.2,
    RecordedAt = DateTime.UtcNow,
    LocationType = LocationType.TrackingPoint,
    Address = "123 Main St, New York, NY",
    VehicleId = vehicle.Id
};

vehicle.RecordLocation(currentLocation);

// Update vehicle status
vehicle.UpdateStatus(VehicleStatus.OnRoute);

// Check if vehicle has exceeded speed limit
bool exceededLimit = vehicle.HasExceededSpeedLimit();
Console.WriteLine($"Vehicle exceeded speed limit: {exceededLimit}");

// Check if vehicle is idle
bool isIdle = vehicle.IsIdle();
Console.WriteLine($"Vehicle is idle: {isIdle}");

// Toggle online status
vehicle.SetOnlineStatus(false);

// Access vehicle's tracking sessions
var trackingSession = new TrackingSession
{
    Id = 1,
    SessionName = "Morning Delivery Run",
    VehicleId = vehicle.Id,
    Status = SessionStatus.Active,
    TotalDistance = 0,
    AverageSpeed = 0,
    MaxSpeed = 0,
    TotalIdleSeconds = 0,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

vehicle.TrackingSessions.Add(trackingSession);

// Access vehicle's routes
var route = new Route
{
    Id = 10,
    Name = "Downtown Delivery Route",
    Description = "Morning delivery route for downtown area"
};

vehicle.Routes.Add(route);

// Access vehicle's location history
foreach (var location in vehicle.Locations)
{
    Console.WriteLine($"Location recorded at {location.RecordedAt}: ({location.Latitude}, {location.Longitude})");
}
```

## TrackingSession

The `TrackingSession` class represents a continuous tracking session for a vehicle with comprehensive location history and statistics. It manages vehicle tracking state, session status, and calculates real-time metrics like distance traveled, average speed, and maximum speed. Sessions can be started, paused, resumed, completed, or cancelled, with automatic statistics calculation upon completion.

### Usage Example

```csharp
// Create a new tracking session for a delivery vehicle
var vehicle = new Vehicle { Id = 5, Name = "Truck-001", LicensePlate = "ABC123" };
var route = new Route { Id = 10, Name = "Downtown Delivery Route" };

var trackingSession = new TrackingSession
{
    Id = 1,
    SessionName = "Delivery Run #2024-001",
    VehicleId = vehicle.Id,
    Vehicle = vehicle,
    RouteId = route.Id,
    Route = route,
    Status = SessionStatus.Pending,
    TotalDistance = 0,
    AverageSpeed = 0,
    MaxSpeed = 0,
    TotalIdleSeconds = 0,
    Notes = "Morning delivery route",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Start the tracking session
trackingSession.StartSession();

// Record location points during the journey
var location1 = new Location
{
    Id = 1,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 45.6,
    RecordedAt = DateTime.UtcNow,
    LocationType = LocationType.TrackingPoint
};

trackingSession.RecordLocation(location1);

// Pause the session when vehicle stops
// trackingSession.PauseSession();

// Resume the session when vehicle continues
// trackingSession.ResumeSession();

// Record more locations
var location2 = new Location
{
    Id = 2,
    Latitude = 40.7306,
    Longitude = -73.9352,
    Speed = 55.2,
    RecordedAt = DateTime.UtcNow.AddMinutes(5),
    LocationType = LocationType.TrackingPoint
};

trackingSession.RecordLocation(location2);

// Complete the session and calculate statistics
trackingSession.CompleteSession();

// Access calculated statistics
Console.WriteLine($"Session completed in {trackingSession.GetSessionDurationHours():F2} hours");
Console.WriteLine($"Total distance: {trackingSession.TotalDistance:F2} km");
Console.WriteLine($"Average speed: {trackingSession.AverageSpeed:F2} km/h");
Console.WriteLine($"Max speed: {trackingSession.MaxSpeed:F2} km/h");
Console.WriteLine($"Total idle time: {trackingSession.TotalIdleSeconds} seconds");

// Access related entities
var completedVehicle = trackingSession.Vehicle;
var completedRoute = trackingSession.Route;
var allLocations = trackingSession.Locations;
```

## User

The `User` class represents a system user with comprehensive profile information, authentication details, and real-time presence tracking. It stores essential user data including contact information, employment details, profile images, and online/offline status. Users can be assigned to vehicles and routes, and their location history can be tracked for operational purposes.

### Usage Example

```csharp
// Create a new user representing a delivery driver
var user = new User
{
    Id = 10,
    FullName = "John Doe",
    Email = "john.doe@company.com",
    PhoneNumber = "+1-555-123-4567",
    EmployeeId = "EMP-2024-001",
    JobTitle = "Delivery Driver",
    Department = "Operations",
    ProfileImageUrl = "/images/users/john-doe.jpg",
    IsActive = true,
    IsOnline = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    LastLoginAt = DateTime.UtcNow.AddHours(-1)
};

// Set online status based on device activity
user.SetOnlineStatus(true);

// Validate user email format
bool isEmailValid = user.IsEmailValid();
Console.WriteLine($"Email is valid: {isEmailValid}");

// Update user's current location
var currentLocation = new Location
{
    Id = 20,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Address = "123 Main St, New York, NY"
};
user.UpdateLocation(currentLocation);

// Deactivate user account when needed
user.Deactivate();

// Access user's assigned vehicles
foreach (var vehicle in user.AssignedVehicles)
{
    Console.WriteLine($"Assigned vehicle: {vehicle.Name} (License: {vehicle.RegistrationNumber})");
}

// Access user's assigned routes
foreach (var route in user.AssignedRoutes)
{
    Console.WriteLine($"Assigned route: {route.Name}");
}
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

## Geofence

The `Geofence` class represents a geographic boundary zone used for proximity-based alerts and event triggers. It supports both circular zones (defined by a center point and radius) and polygonal zones (defined by an ordered series of coordinate vertices). Geofences can be used to monitor when tracked assets or vehicles enter or exit specific geographic areas, enabling automated notifications and workflow automation.

### Usage Example

```csharp
// Create a circular geofence for a warehouse delivery zone
var warehouseGeofence = new Geofence
{
  Name = "Warehouse Delivery Zone",
  Description = "Primary delivery and pickup area for warehouse operations",
  Type = GeofenceType.Circle,
  IsActive = true,
  CenterLatitude = 40.7128,
  CenterLongitude = -74.0060,
  RadiusKm = 0.5,
  CreatedBy = "system"
};

// Check if a delivery truck is within the warehouse zone
var truckLocation = new Location
{
  Latitude = 40.7130,
  Longitude = -74.0062
};

bool isInZone = warehouseGeofence.ContainsPoint(
  truckLocation.Latitude,
  truckLocation.Longitude
);

Console.WriteLine($"Truck is in warehouse zone: {isInZone}");

// Create a polygonal geofence for a restricted construction area
var restrictedArea = new Geofence
{
  Name = "Construction Zone Alpha",
  Description = "Restricted area during active construction work",
  Type = GeofenceType.Polygon,
  IsActive = true,
  PolygonCoordinates = "40.7128,-74.0060;40.7132,-74.0055;40.7135,-74.0060;40.7132,-74.0065",
  CreatedBy = "safety-team"
};

// Get polygon points for validation
var polygonPoints = restrictedArea.GetPolygonPoints();
Console.WriteLine($"Restricted area has {polygonPoints.Count} vertices");

// Check if a vehicle is entering the restricted construction zone
var vehicleLocation = new Location
{
  Latitude = 40.7131,
  Longitude = -74.0058
};

bool isInRestrictedZone = restrictedArea.ContainsPoint(
  vehicleLocation.Latitude,
  vehicleLocation.Longitude
);

Console.WriteLine($"Vehicle is in restricted zone: {isInRestrictedZone}");

// Access geofence properties
Console.WriteLine($"Geofence ID: {restrictedArea.Id}");
Console.WriteLine($"Created at: {restrictedArea.CreatedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Last updated: {restrictedArea.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
```
