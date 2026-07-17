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

