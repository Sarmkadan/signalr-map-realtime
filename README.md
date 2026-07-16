// ... (rest of file remains unchanged)

## LocationDto

The `LocationDto` class represents a data transfer object for location information, providing essential details such as coordinates, speed, bearing, and metadata. It is commonly used for transmitting location data between layers of the application. 

### Usage Example

```csharp
// Create a new location DTO
var locationDto = new LocationDto
{
    Latitude = 40.7128,
    Longitude = -74.0060,
    Altitude = 10.5,
    Accuracy = 3.2,
    Speed = 45.6,
    Bearing = 125.3,
    LocationType = LocationType.TrackingPoint,
    Address = "123 Main St, New York, NY",
    Notes = "Sample location note",
    VehicleId = 5,
    RecordedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    Timestamp = DateTime.UtcNow,
    Heading = 125.3
};

// Access location properties
Console.WriteLine($"Latitude: {locationDto.Latitude}, Longitude: {locationDto.Longitude}");
Console.WriteLine($"Speed: {locationDto.Speed} km/h, Bearing: {locationDto.Bearing} degrees");
```

// ... (rest of file remains unchanged)
