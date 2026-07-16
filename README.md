// ... (rest of file remains unchanged)

## LocationClusterDto

The `LocationClusterDto` record represents a cluster of geographic coordinates that have been grouped together based on spatial proximity. It contains the centroid coordinates of the cluster along with the bounding box that encompasses all points in the cluster, making it ideal for visualizing dense location data on maps.

### Usage Example

```csharp
// Create a new location cluster
var cluster = new LocationClusterDto(
    CenterLatitude: 40.7128,
    CenterLongitude: -74.0060,
    Count: 42,
    MinLatitude: 40.6984,
    MaxLatitude: 40.7272,
    MinLongitude: -74.0201,
    MaxLongitude: -73.9819
);

// Access cluster properties
Console.WriteLine($"Cluster center: {cluster.CenterLatitude:F6}, {cluster.CenterLongitude:F6}");
Console.WriteLine($"Points in cluster: {cluster.Count}");
Console.WriteLine($"Bounding box: [{cluster.MinLatitude:F6}-{cluster.MaxLatitude:F6}, {cluster.MinLongitude:F6}-{cluster.MaxLongitude:F6}]");

// Create a cluster response with multiple clusters
var response = new ClusterResponseDto
{
    Clusters = new List<LocationClusterDto>
    {
        new LocationClusterDto(
            CenterLatitude: 40.7128,
            CenterLongitude: -74.0060,
            Count: 42,
            MinLatitude: 40.6984,
            MaxLatitude: 40.7272,
            MinLongitude: -74.0201,
            MaxLongitude: -73.9819
        ),
        new LocationClusterDto(
            CenterLatitude: 40.7306,
            CenterLongitude: -73.9352,
            Count: 28,
            MinLatitude: 40.7189,
            MaxLatitude: 40.7423,
            MinLongitude: -73.9512,
            MaxLongitude: -73.9192
        )
    },
    TotalPoints = 70,
    GridCellKm = 0.5,
    ComputedAt = DateTime.UtcNow
};

// Access response properties
Console.WriteLine($"Total clusters: {response.Clusters.Count}");
Console.WriteLine($"Total points processed: {response.TotalPoints}");
Console.WriteLine($"Grid cell size: {response.GridCellKm} km");
```

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

## RouteDto

The `RouteDto` class represents a data transfer object for route information, providing essential details about planned and actual routes including vehicle assignments, timing, distances, and waypoints. It is commonly used for transmitting route data between layers of the application and tracking real-time vehicle movements.

### Usage Example

```csharp
// Create a new route DTO
var routeDto = new RouteDto
{
    Id = 1,
    Name = "Morning Delivery Route",
    Description = "Daily delivery route for downtown area",
    VehicleId = 5,
    Vehicle = new VehicleDto
    {
        Id = 5,
        LicensePlate = "KA-123-AB",
        Model = "Mercedes-Benz Sprinter",
        DriverName = "John Smith"
    },
    AssignedUserId = 10,
    AssignedUser = new UserDto
    {
        Id = 10,
        Username = "delivery_driver_1",
        FullName = "John Smith",
        PhoneNumber = "+1-555-0123"
    },
    PlannedDepartureTime = new DateTime(2024, 7, 16, 8, 0, 0),
    EstimatedArrivalTime = new DateTime(2024, 7, 16, 12, 0, 0),
    TotalDistance = 45.5,
    ActualDistance = 47.2,
    IsActive = true,
    IsCompleted = false,
    Waypoints = new List<WaypointDto>
    {
        new WaypointDto
        {
            Id = 1,
            Sequence = 1,
            Name = "Warehouse",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Address = "123 Main St, New York, NY",
            IsCompleted = true,
            ContactName = "Warehouse Manager",
            ContactPhone = "+1-555-0001"
        },
        new WaypointDto
        {
            Id = 2,
            Sequence = 2,
            Name = "Customer A",
            Latitude = 40.7306,
            Longitude = -73.9352,
            Address = "456 Oak Ave, Brooklyn, NY",
            IsCompleted = false,
            ContactName = "Alice Johnson",
            ContactPhone = "+1-555-0002"
        }
    },
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Access route properties
Console.WriteLine($"Route: {routeDto.Name} (ID: {routeDto.Id})");
Console.WriteLine($"Status: {(routeDto.IsActive ? "Active" : "Inactive")}, Completed: {(routeDto.IsCompleted ? "Yes" : "No")}");
Console.WriteLine($"Distance: {routeDto.TotalDistance?.ToString("F1") ?? "N/A"} km planned, {routeDto.ActualDistance?.ToString("F1") ?? "N/A"} km actual");
Console.WriteLine($"Departure: {routeDto.PlannedDepartureTime:g}, Arrival: {routeDto.EstimatedArrivalTime:g}");
```

// ... (rest of file remains unchanged)

## LocationClusterDto

The `LocationClusterDto` record represents a cluster of geographic coordinates that have been grouped together based on spatial proximity. It contains the centroid coordinates of the cluster along with the bounding box that encompasses all points in the cluster, making it ideal for visualizing dense location data on maps.

### Usage Example

```csharp
// Create a new location cluster
var cluster = new LocationClusterDto(
    CenterLatitude: 40.7128,
    CenterLongitude: -74.0060,
    Count: 42,
    MinLatitude: 40.6984,
    MaxLatitude: 40.7272,
    MinLongitude: -74.0201,
    MaxLongitude: -73.9819
);

// Access cluster properties
Console.WriteLine($"Cluster center: {cluster.CenterLatitude:F6}, {cluster.CenterLongitude:F6}");
Console.WriteLine($"Points in cluster: {cluster.Count}");
Console.WriteLine($"Bounding box: [{cluster.MinLatitude:F6}-{cluster.MaxLatitude:F6}, {cluster.MinLongitude:F6}-{cluster.MaxLongitude:F6}]");

// Create a cluster response with multiple clusters
var response = new ClusterResponseDto
{
    Clusters = new List<LocationClusterDto>
    {
        new LocationClusterDto(
            CenterLatitude: 40.7128,
            CenterLongitude: -74.0060,
            Count: 42,
            MinLatitude: 40.6984,
            MaxLatitude: 40.7272,
            MinLongitude: -74.0201,
            MaxLongitude: -73.9819
        ),
        new LocationClusterDto(
            CenterLatitude: 40.7306,
            CenterLongitude: -73.9352,
            Count: 28,
            MinLatitude: 40.7189,
            MaxLatitude: 40.7423,
            MinLongitude: -73.9512,
            MaxLongitude: -73.9192
        )
    },
    TotalPoints = 70,
    GridCellKm = 0.5,
    ComputedAt = DateTime.UtcNow
};

// Access response properties
Console.WriteLine($"Total clusters: {response.Clusters.Count}");
Console.WriteLine($"Total points processed: {response.TotalPoints}");
Console.WriteLine($"Grid cell size: {response.GridCellKm} km");
```

// ... (rest of file remains unchanged)
