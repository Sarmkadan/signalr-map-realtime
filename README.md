// ... (rest of file remains unchanged)

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

// ... (rest of file remains unchanged)
