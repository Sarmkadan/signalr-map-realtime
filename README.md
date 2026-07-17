# SignalR Map Realtime

ASP.NET Core service for real-time vehicle/asset location tracking. Live updates are pushed
over SignalR hubs (`/hubs/location`, `/hubs/playback`), a REST API covers the same domain
(vehicles, locations, routes, assets, geofences, clustering, playback), and EF Core with
SQL Server provides persistence. See `docker-compose.yml` for a runnable setup.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the component breakdown, design
decisions (service lifetimes, in-memory event bus, playback engine), data flow, and known
limitations.

## DTO reference

Usage examples for selected DTOs follow.

## ApplicationDbContext

The `ApplicationDbContext` class represents the Entity Framework Core database context, serving as the central point for interacting with the database. It manages access to core system entities, including users, vehicles, locations, tracking sessions, routes, waypoints, and assets.

### Usage Example

```csharp
using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Data;

// Assuming options are configured in Startup/Program.cs
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Options;

using (var context = new ApplicationDbContext(options))
{
    // Accessing DbSets
    var vehicleCount = await context.Vehicles.CountAsync();
    
    // Adding a new entity
    context.Users.Add(new User { FullName = "John Doe", Email = "john@example.com" });
    await context.SaveChangesAsync();
    
    Console.WriteLine($"Database initialized. Total vehicles: {vehicleCount}");
}
```

## GeofenceDto

The `GeofenceDto` class represents a data transfer object for geofence information, providing essential details about configured zones. It is used for transmitting geofence data between system components.

### Usage Example

```csharp
// Create a new geofence DTO
var geofenceDto = new GeofenceDto
{
    Id = Guid.NewGuid(),
    Name = "Downtown Area",
    Description = "Geofence for monitoring downtown area",
    Type = "Circle",
    IsActive = true,
    CenterLatitude = 40.7128,
    CenterLongitude = -74.0060,
    RadiusKm = 5.0,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    CreatedBy = "Admin"
};

// Access geofence properties
Console.WriteLine($"Geofence ID: {geofenceDto.Id}");
Console.WriteLine($"Name: {geofenceDto.Name}, Type: {geofenceDto.Type}");
Console.WriteLine($"Is Active: {(geofenceDto.IsActive ? "Yes" : "No")}");
Console.WriteLine($"Center: {geofenceDto.CenterLatitude:F6}, {geofenceDto.CenterLongitude:F6}, Radius: {geofenceDto.RadiusKm:F1} km");

// Create a new geofence DTO for a polygon zone
var polygonGeofenceDto = new GeofenceDto
{
    Id = Guid.NewGuid(),
    Name = "Warehouse Perimeter",
    Description = "Geofence for warehouse perimeter",
    Type = "Polygon",
    IsActive = true,
    PolygonCoordinates = "40.7306,-73.9352;40.7189,-73.9512;40.7423,-73.9192",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    CreatedBy = "Admin"
};

// Access polygon geofence properties
Console.WriteLine($"Geofence ID: {polygonGeofenceDto.Id}");
Console.WriteLine($"Name: {polygonGeofenceDto.Name}, Type: {polygonGeofenceDto.Type}");
Console.WriteLine($"Is Active: {(polygonGeofenceDto.IsActive ? "Yes" : "No")}");
Console.WriteLine($"Polygon Coordinates: {polygonGeofenceDto.PolygonCoordinates}");
```

## UserDto

The `UserDto` class is a data transfer object that encapsulates user information used throughout the API. It includes identification, contact details, employment data, status flags, and timestamps.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.DTOs;

var user = new UserDto
{
    Id = 1,
    FullName = "Jane Doe",
    Email = "jane.doe@example.com",
    PhoneNumber = "+1-555-1234",
    EmployeeId = "E12345",
    JobTitle = "Software Engineer",
    Department = "Engineering",
    IsOnline = true,
    IsActive = true,
    LastLoginAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow
};

Console.WriteLine($"User: {user.FullName} ({user.Email}) - Active: {user.IsActive}");
```

## TrackingService

The `TrackingService` provides comprehensive functionality for managing the lifecycle of vehicle tracking sessions. It allows users to start, pause, resume, complete, and cancel sessions while also offering methods to retrieve session history, status information, and performance statistics such as distance and average speed.

### Usage Example

```csharp
using SignalRMapRealtime.Services;

// Assuming trackingService is injected
// Start a new session for vehicle 1
int sessionId = await trackingService.StartTrackingSessionAsync(1, "Morning Route");

// Pause the session
await trackingService.PauseSessionAsync(sessionId);

// Get session stats
double distance = await trackingService.GetSessionDistanceAsync(sessionId);
double avgSpeed = await trackingService.GetSessionAverageSpeedAsync(sessionId);

Console.WriteLine($"Session {sessionId} stats: Distance={distance}km, AvgSpeed={avgSpeed}km/h");

// Complete the session
await trackingService.CompleteSessionAsync(sessionId);
```

## INotificationService

The `INotificationService` provides a unified interface for sending notifications across multiple channels, including email, SMS, and push notifications. It supports queuing notifications for asynchronous processing, allowing applications to remain responsive while handling background delivery tasks.

### Usage Example

```csharp
using SignalRMapRealtime.Services;

// Assuming notificationService is injected (e.g., as InMemoryNotificationService)
await notificationService.SendEmailAsync("admin@example.com", "Alert", "Vehicle tracker triggered.");
await notificationService.SendPushAsync("user123", "Traffic Alert", "Heavy traffic on route.");

// Multi-channel notification
await notificationService.SendMultiChannelAsync("user123@example.com", "Update", "Session complete.", includeEmail: true, includePush: true);

// Access pending notifications (e.g., for background processing)
var pending = InMemoryNotificationService.GetPendingNotifications(maxCount: 5);
foreach (var item in pending)
{
    Console.WriteLine($"Notification {item.Id}: {item.Type} to {item.Recipient} - Status: {item.Status}");
}
```

## ICacheService

The `ICacheService` interface provides a unified, abstract interface for caching operations, supporting both local memory caching and distributed caching to improve application performance. It allows business logic to store, retrieve, check for, and remove cached data without needing to know the underlying cache provider implementation.

### Usage Example

```csharp
using SignalRMapRealtime.Services;
using SignalRMapRealtime.DTOs;

// Assuming cacheService is injected
string cacheKey = "vehicle_1_details";

// 1. Get or Create
// Retrieves from cache if exists, otherwise calls the factory to fetch and cache
var vehicle = await cacheService.GetOrCreateAsync(
    cacheKey,
    async () => {
        // Simulate database or API call
        return await Task.FromResult(new VehicleDto { Id = 1, Name = "Vehicle 1" });
    },
    TimeSpan.FromMinutes(5)
);

// 2. Check Exists
if (await cacheService.ExistsAsync(cacheKey))
{
    // 3. Get
    var cachedVehicle = await cacheService.GetAsync<VehicleDto>(cacheKey);
    Console.WriteLine($"Retrieved: {cachedVehicle?.Name}");
}

// 4. Set
await cacheService.SetAsync("vehicle_1_status", "Active", TimeSpan.FromMinutes(1));

// 5. Remove
await cacheService.RemoveAsync("vehicle_1_status");

// 6. Remove By Pattern
await cacheService.RemoveByPatternAsync("vehicle_*");

// 7. Clear
await cacheService.ClearAsync();
```

## ILocationService

The `ILocationService` interface provides comprehensive functionality for managing location-based operations, including recording location updates, querying location history, and performing geofencing calculations. It also facilitates retrieving aggregated performance statistics for vehicles over specific time intervals.

### Usage Example

```csharp
using SignalRMapRealtime.Services;
using SignalRMapRealtime.DTOs;

// Assuming locationService is injected
var vehicleId = 1;
var startTime = DateTime.UtcNow.AddHours(-24);
var endTime = DateTime.UtcNow;

// Retrieve location statistics for the vehicle
LocationStatsDto stats = await locationService.GetLocationStatsAsync(vehicleId, startTime, endTime);

// Access the statistics
Console.WriteLine($"Points recorded: {stats.PointCount}");
Console.WriteLine($"Speed range: {stats.MinSpeed} - {stats.MaxSpeed} km/h");
Console.WriteLine($"Average speed: {stats.AverageSpeed} km/h");
Console.WriteLine($"Total distance: {stats.TotalDistance} km");
```

## LocationService

The `LocationService` class is the concrete implementation of the `ILocationService` interface, providing the core business logic for tracking vehicle locations, managing history, and performing geographic calculations. It handles coordinate validation, persistence of location updates, and the aggregation of performance statistics for fleet monitoring.

### Usage Example

```csharp
using SignalRMapRealtime.Services;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Domain.Enums;

// Assuming locationService (LocationService) is injected
// Record a new location update
var location = await locationService.RecordLocationAsync(new CreateLocationDto {
    VehicleId = 1,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 45.5,
    LocationType = LocationType.Gps
});

// Check coordinate validity
bool isValid = locationService.ValidateCoordinates(40.7128, -74.0060);

// Calculate distance between points
double distance = locationService.CalculateDistance(40.7128, -74.0060, 40.7130, -74.0062);
Console.WriteLine($"Location {location.Id} recorded, isValid: {isValid}, distance: {distance:F4} km");
```

## IGeofenceService

The `IGeofenceService` manages configurable geofence zones and evaluates location-based boundary alerts for vehicles. It tracks vehicle presence within defined areas and publishes violation events when vehicles enter or exit these zones.

### Usage Example

```csharp
using SignalRMapRealtime.Services;
using SignalRMapRealtime.DTOs;

// Register the service in DI
// services.AddGeofencing();

// Assuming geofenceService is injected
var zoneDto = await geofenceService.RegisterZoneAsync(new CreateGeofenceDto {
    Name = "Warehouse",
    IsActive = true,
    CenterLatitude = 40.7128,
    CenterLongitude = -74.0060,
    RadiusKm = 1.0
});

// Check a vehicle's position
var alerts = await geofenceService.CheckLocationAsync(Guid.NewGuid(), 40.7129, -74.0061);
foreach (var alert in alerts)
{
    Console.WriteLine($"Alert: {alert.ViolationType} {alert.GeofenceName}");
}

// Get active zones
var activeZones = await geofenceService.GetActiveZonesAsync();
Console.WriteLine($"Active zones count: {activeZones.Count}");

// Remove a zone
await geofenceService.RemoveZoneAsync(zoneDto.Id);
```

## TrackingSessionRepository

The `TrackingSessionRepository` class is the data access layer for tracking sessions. It provides methods to query tracking sessions, including active sessions, history by vehicle, status, date ranges, and performance statistics.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var repository = new TrackingSessionRepository(dbContext);

// 1. Get active session for a vehicle
var activeSession = await repository.GetActiveSessionByVehicleAsync(1);
if (activeSession != null)
{
    Console.WriteLine($"Active session found: {activeSession.Id}, Start: {activeSession.StartTime}");
}

// 2. Get session history by vehicle
var vehicleSessions = await repository.GetSessionsByVehicleAsync(1);
Console.WriteLine($"Found {vehicleSessions.Count()} sessions for vehicle 1.");

// 3. Get high speed sessions
var highSpeedSessions = await repository.GetHighSpeedSessionsAsync(speedThreshold: 90.0);
Console.WriteLine($"Found {highSpeedSessions.Count()} sessions exceeding 90 km/h.");

// 4. Get active session count
int activeCount = await repository.GetActiveSessionCountAsync();
Console.WriteLine($"Total active sessions: {activeCount}");

// 5. Get total distance for a vehicle
double totalDistance = await repository.GetTotalDistanceTraveledAsync(1);
Console.WriteLine($"Total distance traveled by vehicle 1: {totalDistance:F2} km");
```

## BaseRepository

The `BaseRepository` class is an abstract generic repository that provides common data access operations for all entity types in the system. It serves as the foundation for concrete repository implementations, offering CRUD operations, batch operations, and query capabilities that work with any entity type inheriting from `BaseEntity`.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var repository = new ConcreteRepository<Vehicle>(dbContext);

// 1. Add a new entity
var newVehicle = new Vehicle { Name = "Truck 1", RegistrationNumber = "ABC123" };
await repository.AddAsync(newVehicle);
await repository.SaveChangesAsync();

// 2. Get entity by ID
var vehicle = await repository.GetByIdAsync(1);
if (vehicle != null)
{
    Console.WriteLine($"Found vehicle: {vehicle.Name}");
}

// 3. Get all entities
var allVehicles = await repository.GetAllAsync();
Console.WriteLine($"Total vehicles: {allVehicles.Count()}");

// 4. Find entities matching criteria
var activeVehicles = await repository.FindAsync(v => v.IsActive);
Console.WriteLine($"Active vehicles: {activeVehicles.Count()}");

// 5. Update an entity
if (vehicle != null)
{
    vehicle.Name = "Updated Truck 1";
    await repository.UpdateAsync(vehicle);
    await repository.SaveChangesAsync();
}

// 6. Remove by ID
await repository.RemoveByIdAsync(2);
await repository.SaveChangesAsync();

// 7. Check if entity exists
bool exists = await repository.ExistsAsync(v => v.RegistrationNumber == "ABC123");
Console.WriteLine($"Vehicle with ABC123 exists: {exists}");

// 8. Get count
int vehicleCount = await repository.CountAsync();
Console.WriteLine($"Total vehicles in database: {vehicleCount}");
```

## LocationRepository

The `LocationRepository` class provides specialized data access operations for location entities, enabling efficient querying of vehicle location history, spatial queries, and location statistics. It extends the `BaseRepository<Location>` with location-specific methods for retrieving the latest positions, filtering by time ranges, session tracking, and geographic proximity searches.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var locationRepository = new LocationRepository(dbContext);

// 1. Get the latest location for a vehicle
var latestLocation = await locationRepository.GetLatestLocationByVehicleAsync(1);
if (latestLocation != null)
{
    Console.WriteLine($"Latest location for vehicle 1: Lat={latestLocation.Latitude:F6}, Lng={latestLocation.Longitude:F6}, Speed={latestLocation.Speed} km/h");
}

// 2. Get locations recorded in the last 24 hours
var recentLocations = await locationRepository.GetRecentLocationsAsync(1);
Console.WriteLine($"Found {recentLocations.Count()} recent locations for vehicle 1");

// 3. Get locations within a time range
var timeRangeLocations = await locationRepository.GetLocationsByTimeRangeAsync(
    1, 
    DateTime.UtcNow.AddHours(-24),
    DateTime.UtcNow
);
Console.WriteLine($"Found {timeRangeLocations.Count()} locations in time range");

// 4. Get locations for a tracking session
var sessionLocations = await locationRepository.GetLocationsBySessionAsync(101);
Console.WriteLine($"Found {sessionLocations.Count()} locations in session 101");

// 5. Get locations by type
var gpsLocations = await locationRepository.GetLocationsByTypeAsync(LocationType.Gps);
Console.WriteLine($"Found {gpsLocations.Count()} GPS locations");

// 6. Find locations within 5km of a point
var nearbyLocations = await locationRepository.GetLocationsNearbyAsync(40.7128, -74.0060, 5.0);
Console.WriteLine($"Found {nearbyLocations.Count()} locations within 5km radius");

// 7. Get location statistics for a vehicle
var stats = await locationRepository.GetLocationStatsAsync(
    1,
    DateTime.UtcNow.AddDays(-7),
    DateTime.UtcNow
);
Console.WriteLine($"Location stats: {stats.count} points, " +
    $"Speed range: {stats.minSpeed:F1}-{stats.maxSpeed:F1} km/h, " +
    $"Average: {stats.avgSpeed:F1} km/h");

// 8. Delete old locations (older than 90 days)
int deletedCount = await locationRepository.DeleteOldLocationsAsync(90);
Console.WriteLine($"Deleted {deletedCount} old location records");
```

## AssetRepository

The `AssetRepository` class provides data access operations for asset entities in the system. It extends the `BaseRepository<Asset>` with asset-specific methods for querying assets by serial number, type, vehicle assignment, tracking status, and special handling requirements. The repository includes methods for retrieving asset values, tracking history, and filtering by various conditions.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var assetRepository = new AssetRepository(dbContext);

// 1. Get asset by serial number
var assetBySerial = await assetRepository.GetBySerialNumberAsync("SN123456789");
if (assetBySerial != null)
{
    Console.WriteLine($"Found asset: {assetBySerial.Name} (Serial: {assetBySerial.SerialNumber})");
}

// 2. Get all GPS tracking devices
var gpsAssets = await assetRepository.GetAssetsByTypeAsync(AssetType.GpsTrackingDevice);
Console.WriteLine($"Found {gpsAssets.Count()} GPS tracking devices");

// 3. Get assets assigned to vehicle 1
var vehicleAssets = await assetRepository.GetAssetsByVehicleAsync(1);
Console.WriteLine($"Found {vehicleAssets.Count()} assets assigned to vehicle 1");

// 4. Get unassigned assets
var unassignedAssets = await assetRepository.GetUnassignedAssetsAsync();
Console.WriteLine($"Found {unassignedAssets.Count()} unassigned assets");

// 5. Get assets requiring special handling
var specialAssets = await assetRepository.GetSpecialHandlingAssetsAsync();
Console.WriteLine($"Found {specialAssets.Count()} assets requiring special handling");

// 6. Get asset with full tracking history
var assetWithHistory = await assetRepository.GetAssetWithHistoryAsync(1);
if (assetWithHistory != null)
{
    Console.WriteLine($"Asset {assetWithHistory.Name} has {assetWithHistory.LocationHistory?.Count ?? 0} location history records");
}

// 7. Get recently tracked assets (last 30 minutes)
var recentAssets = await assetRepository.GetRecentlyTrackedAssetsAsync(30);
Console.WriteLine($"Found {recentAssets.Count()} assets tracked in last 30 minutes");

// 8. Get total value of all trailers
var totalTrailerValue = await assetRepository.GetTotalValueByTypeAsync(AssetType.Trailer);
Console.WriteLine($"Total trailer value: {totalTrailerValue:C}");

// 9. Get assets not tracked in last 24 hours
var staleAssets = await assetRepository.GetNotRecentlyTrackedAsync(24);
Console.WriteLine($"Found {staleAssets.Count()} assets not tracked in last 24 hours");

// 10. Count assets in "Excellent" condition
var excellentCount = await assetRepository.CountByConditionAsync("Excellent");
Console.WriteLine($"Found {excellentCount} assets in Excellent condition");
```

## VehicleService

The `VehicleService` provides a comprehensive API for managing vehicle entities, including creation, retrieval, updates, and status tracking. It facilitates fleet operations by allowing management of vehicle assignments, operational status, and monitoring of performance metrics such as fuel levels and speed violations.

### Usage Example

```csharp
using SignalRMapRealtime.Services;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Domain.Enums;

// Assuming vehicleService is injected
// Retrieve a vehicle by ID
var vehicle = await vehicleService.GetVehicleAsync(1);
if (vehicle != null)
{
    Console.WriteLine($"Vehicle: {vehicle.Name} (Registration: {vehicle.RegistrationNumber})");
    
    // Update vehicle status
    await vehicleService.UpdateVehicleStatusAsync(vehicle.Id, VehicleStatus.Active);
    
    // Set vehicle online
    await vehicleService.SetVehicleOnlineStatusAsync(vehicle.Id, true);
}

// Get fleet statistics
int onlineCount = await vehicleService.GetOnlineVehicleCountAsync();
var speedingVehicles = await vehicleService.GetSpeedingVehiclesAsync();

Console.WriteLine($"Fleet status: {onlineCount} online, {speedingVehicles.Count()} speeding.");
```

## RoutePlaybackService

The `RoutePlaybackService` provides functionality to manage and simulate the playback of recorded vehicle routes. It supports starting, pausing, resuming, and stopping playback sessions, while also allowing seeking to specific timestamps, adjusting playback speed, and retrieving playback statistics and snapshots.

### Usage Example

```csharp
using SignalRMapRealtime.Services;
using SignalRMapRealtime.DTOs;

// Assuming routePlaybackService is injected
// Start a playback session
Guid playbackId = await routePlaybackService.StartPlaybackAsync(trackingSessionId: 101, loop: true);

// Get current state
var state = await routePlaybackService.GetPlaybackStateAsync(playbackId);
Console.WriteLine($"Playback {playbackId} active: {state?.IsPlaying}");

// Adjust playback settings
await routePlaybackService.SetPlaybackSpeedAsync(playbackId, 2.0);
await routePlaybackService.SeekToTimestampAsync(playbackId, DateTime.UtcNow.AddMinutes(-5));

// Retrieve statistics
var stats = await routePlaybackService.GetPlaybackStatisticsAsync(playbackId);
Console.WriteLine($"Playback covered {stats?.TotalDistance} km");

// Cleanup
await routePlaybackService.StopPlaybackAsync(playbackId);
```

## VehicleRepository

The `VehicleRepository` class provides specialized data access operations for vehicle entities in the system. It extends the `BaseRepository<Vehicle>` with vehicle-specific methods for querying vehicles by status, online state, registration number, driver assignment, asset type, and operational conditions like fuel levels and speed violations. The repository includes methods for retrieving vehicles with complete tracking data and counting online vehicles.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;

// Assuming dbContext is injected
var vehicleRepository = new VehicleRepository(dbContext);

// 1. Get vehicles by status
var activeVehicles = await vehicleRepository.GetVehiclesByStatusAsync(VehicleStatus.Active);
Console.WriteLine($"Found {activeVehicles.Count()} active vehicles");

// 2. Get online vehicles
var onlineVehicles = await vehicleRepository.GetOnlineVehiclesAsync();
Console.WriteLine($"Found {onlineVehicles.Count()} online vehicles");
```

## UserRepository

The `UserRepository` class provides data access operations for user entities in the system. It extends the `BaseRepository<User>` with user-specific methods for querying users by email, employee ID, department, job title, online status, and activity state. The repository includes methods for retrieving users with their assigned vehicles/routes, counting online users, and managing user deactivation.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var userRepository = new UserRepository(dbContext);

// 1. Get user by email
var userByEmail = await userRepository.GetByEmailAsync("john.doe@example.com");
if (userByEmail != null)
{
    Console.WriteLine($"Found user: {userByEmail.FullName} (Email: {userByEmail.Email})");
}

// 2. Get user by employee ID
var userByEmployeeId = await userRepository.GetByEmployeeIdAsync("E12345");
if (userByEmployeeId != null)
{
    Console.WriteLine($"Found user: {userByEmployeeId.FullName} (Employee ID: {userByEmployeeId.EmployeeId})");
}

// 3. Get online users
var onlineUsers = await userRepository.GetOnlineUsersAsync();
Console.WriteLine($"Found {onlineUsers.Count()} online users");

// 4. Get active users
var activeUsers = await userRepository.GetActiveUsersAsync();
Console.WriteLine($"Found {activeUsers.Count()} active users");

// 5. Get users by department
var engineeringUsers = await userRepository.GetUsersByDepartmentAsync("Engineering");
Console.WriteLine($"Found {engineeringUsers.Count()} users in Engineering department");

// 6. Get drivers with vehicles
var driversWithVehicles = await userRepository.GetDriversWithVehiclesAsync();
Console.WriteLine($"Found {driversWithVehicles.Count()} drivers with assigned vehicles");

// 7. Get recently logged in users (last 7 days)
var recentUsers = await userRepository.GetRecentlyLoggedInUsersAsync();
Console.WriteLine($"Found {recentUsers.Count()} users logged in recently");

// 8. Get users by job title
var managers = await userRepository.GetUsersByJobTitleAsync("Manager");
Console.WriteLine($"Found {managers.Count()} managers");

// 9. Get online user count
int onlineCount = await userRepository.GetOnlineUserCountAsync();
Console.WriteLine($"Total online users: {onlineCount}");

// 10. Deactivate a user
bool deactivated = await userRepository.DeactivateUserAsync(1);
Console.WriteLine($"User deactivation {(deactivated ? "succeeded" : "failed")}");
```

## VehicleRepository

The `VehicleRepository` class provides specialized data access operations for vehicle entities in the system. It extends the `BaseRepository<Vehicle>` with vehicle-specific methods for querying vehicles by status, online state, registration number, driver assignment, asset type, and operational conditions like fuel levels and speed violations. The repository includes methods for retrieving vehicles with complete tracking data and counting online vehicles.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var vehicleRepository = new VehicleRepository(dbContext);

// 1. Get vehicles by status
var activeVehicles = await vehicleRepository.GetVehiclesByStatusAsync(VehicleStatus.Active);
Console.WriteLine($"Found {activeVehicles.Count()} active vehicles");

// 2. Get online vehicles
var onlineVehicles = await vehicleRepository.GetOnlineVehiclesAsync();
Console.WriteLine($"Found {onlineVehicles.Count()} online vehicles");

// 3. Get vehicle by registration number
var vehicleByReg = await vehicleRepository.GetByRegistrationNumberAsync("ABC123");
if (vehicleByReg != null)
{
    Console.WriteLine($"Found vehicle: {vehicleByReg.Name} (Reg: {vehicleByReg.RegistrationNumber})");
}

// 4. Get vehicles by driver
var driverVehicles = await vehicleRepository.GetVehiclesByDriverAsync(1);
Console.WriteLine($"Found {driverVehicles.Count()} vehicles assigned to driver 1");

// 5. Get vehicles by asset type
var truckVehicles = await vehicleRepository.GetVehiclesByAssetTypeAsync(AssetType.Truck);
Console.WriteLine($"Found {truckVehicles.Count()} trucks");

// 6. Get low fuel vehicles (fuel < 20%)
var lowFuelVehicles = await vehicleRepository.GetLowFuelVehiclesAsync(20.0);
Console.WriteLine($"Found {lowFuelVehicles.Count()} vehicles with low fuel");

// 7. Get speeding vehicles
var speedingVehicles = await vehicleRepository.GetSpeedingVehiclesAsync();
Console.WriteLine($"Found {speedingVehicles.Count()} speeding vehicles");

// 8. Get online vehicle count
int onlineCount = await vehicleRepository.GetOnlineVehicleCountAsync();
Console.WriteLine($"Total online vehicles: {onlineCount}");

// 9. Get vehicle with complete tracking data
var vehicleWithTracking = await vehicleRepository.GetVehicleWithTrackingDataAsync(1);
if (vehicleWithTracking != null)
{
    Console.WriteLine($"Vehicle {vehicleWithTracking.Name} has {vehicleWithTracking.Locations?.Count ?? 0} location records");
}
```

## RouteRepository

The `RouteRepository` class provides specialized data access operations for route entities in the system. It extends the `BaseRepository<Route>` with route-specific methods for querying routes by vehicle assignment, user assignment, completion status, date ranges, and performance metrics. The repository includes methods for retrieving routes with detailed waypoint information, finding pending routes, and calculating route statistics such as average completion time.

### Usage Example

```csharp
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Domain.Models;
using Microsoft.EntityFrameworkCore;

// Assuming dbContext is injected
var routeRepository = new RouteRepository(dbContext);

// 1. Get active routes for a specific vehicle
var activeRoutes = await routeRepository.GetActiveRoutesByVehicleAsync(1);
Console.WriteLine($"Found {activeRoutes.Count()} active routes for vehicle 1");

// 2. Get all routes assigned to a user
var userRoutes = await routeRepository.GetRoutesByUserAsync(1);
Console.WriteLine($"Found {userRoutes.Count()} routes assigned to user 1");

// 3. Get completed routes
var completedRoutes = await routeRepository.GetRoutesByCompletionAsync(isCompleted: true);
Console.WriteLine($"Found {completedRoutes.Count()} completed routes");

// 4. Get route with full details including waypoints
var routeWithDetails = await routeRepository.GetRouteWithDetailsAsync(1);
if (routeWithDetails != null)
{
    Console.WriteLine($"Route {routeWithDetails.Id} has {routeWithDetails.Waypoints?.Count ?? 0} waypoints");
}

// 5. Get routes within a specific date range
var dateRangeRoutes = await routeRepository.GetRoutesByDateRangeAsync(
    DateTime.UtcNow.AddMonths(-1),
    DateTime.UtcNow
);
Console.WriteLine($"Found {dateRangeRoutes.Count()} routes in the last month");

// 6. Get the longest routes by distance
var longestRoutes = await routeRepository.GetLongestRoutesAsync(10);
Console.WriteLine($"Top 10 longest routes:");
foreach (var route in longestRoutes)
{
    Console.WriteLine($"  - Route {route.Id}: {route.Distance} km");
}

// 7. Get average completion time for all routes
var avgCompletionTime = await routeRepository.GetAverageCompletionTimeAsync();
if (avgCompletionTime.HasValue)
{
    Console.WriteLine($"Average completion time: {avgCompletionTime.Value:F1} minutes");
}

// 8. Get pending routes (not yet completed)
var pendingRoutes = await routeRepository.GetPendingRoutesAsync();
Console.WriteLine($"Found {pendingRoutes.Count()} pending routes");
```

