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
