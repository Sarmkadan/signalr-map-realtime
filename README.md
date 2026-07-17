// ... (rest of the file remains the same)

## VehicleController

`VehicleController` is an API controller that manages vehicle data, providing endpoints for CRUD operations on vehicles with status tracking. Vehicles represent entities being tracked on the map (cars, trucks, couriers, etc.). The controller supports pagination, filtering by status, and returns vehicle data in a standardized API response format.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new vehicle
        var newVehicle = new CreateVehicleDto
        {
            RegistrationNumber = "ABC123",
            Name = "Test Vehicle"
        };

        var createResponse = await client.PostAsJsonAsync("api/Vehicle", newVehicle);
        var createdVehicle = await createResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Console.WriteLine($"Created vehicle: {createdVehicle?.Data?.Id}");

        // Get all vehicles
        var getAllResponse = await client.GetAsync("api/Vehicle?pageNumber=1&pageSize=10");
        var allVehicles = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<VehicleDto>>>();
        Console.WriteLine($"Total vehicles: {allVehicles?.Data?.TotalCount}");

        // Get a specific vehicle by ID
        var getByIdResponse = await client.GetAsync($"api/Vehicle/{createdVehicle?.Data?.Id}");
        var singleVehicle = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Console.WriteLine($"Vehicle: {singleVehicle?.Data?.Name}");

        // Update a vehicle
        var updateVehicle = new UpdateVehicleDto
        {
            Name = "Updated Test Vehicle"
        };
        var updateResponse = await client.PutAsJsonAsync($"api/Vehicle/{createdVehicle?.Data?.Id}", updateVehicle);
        var updatedVehicle = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
        Console.WriteLine($"Updated vehicle: {updatedVehicle?.Data?.Name}");

        // Delete a vehicle
        var deleteResponse = await client.DeleteAsync($"api/Vehicle/{createdVehicle?.Data?.Id}");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");

        // Get vehicle status
        var statusResponse = await client.GetAsync($"api/Vehicle/{createdVehicle?.Data?.Id}/status");
        var vehicleStatus = await statusResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Console.WriteLine($"Vehicle status: {vehicleStatus?.Data}");
    }
}
```

## AssetController

`AssetController` is an API controller that manages asset tracking data, providing endpoints for CRUD operations on assets with status tracking. Assets represent trackable items like packages, equipment, or parcels that can be monitored on the map. The controller supports pagination, returns asset data in a standardized API response format, and includes proper error handling and logging.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

class Program
{
    static async Task Main()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new asset
        var newAsset = new AssetDto
        {
            Name = "Package #12345",
            AssetType = "Package",
            Condition = "New"
        };

        var createResponse = await client.PostAsJsonAsync("api/Asset", newAsset);
        var createdAsset = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AssetDto>>();
        Console.WriteLine($"Created asset: {createdAsset?.Data?.Id}");

        // Get all assets with pagination
        var getAllResponse = await client.GetAsync("api/Asset?pageNumber=1&pageSize=20");
        var allAssets = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<AssetDto>>>();
        Console.WriteLine($"Total assets: {allAssets?.Data?.TotalCount}");

        // Get a specific asset by ID
        var getByIdResponse = await client.GetAsync($"api/Asset/{createdAsset?.Data?.Id}");
        var singleAsset = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<AssetDto>>();
        Console.WriteLine($"Asset: {singleAsset?.Data?.Name}");

        // Update an asset
        var updateAsset = new AssetDto
        {
            Name = "Package #12345 - Updated",
            AssetType = "Package",
            Condition = "Good"
        };
        var updateResponse = await client.PutAsJsonAsync($"api/Asset/{createdAsset?.Data?.Id}", updateAsset);
        var updatedAsset = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<AssetDto>>();
        Console.WriteLine($"Updated asset: {updatedAsset?.Data?.Name}");

        // Delete an asset
        var deleteResponse = await client.DeleteAsync($"api/Asset/{createdAsset?.Data?.Id}");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}
```

## GeofenceController

`GeofenceController` is an API controller that manages geofence zones and evaluates vehicle positions against configured boundaries. It provides endpoints for retrieving active zones, registering new zones, removing existing zones, and checking vehicle locations against geofence boundaries to generate boundary-crossing alerts. The controller supports circle and polygon zone shapes and emits alerts only on state changes (entry/exit events).

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

class Program
{
    static async Task Main()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Get all active geofence zones
        var getZonesResponse = await client.GetAsync("api/Geofence");
        var zones = await getZonesResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<GeofenceDto>>>();
        Console.WriteLine($"Active zones: {zones?.Data?.Count}");

        // Register a new circular geofence zone
        var newZone = new CreateGeofenceDto
        {
            Name = "Warehouse District",
            ShapeType = GeofenceShapeType.Circle,
            Coordinates = new[] { -73.9857, 40.7484 },
            RadiusMeters = 500,
            Description = "Restricted area around warehouse"
        };

        var registerResponse = await client.PostAsJsonAsync("api/Geofence", newZone);
        var registeredZone = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<GeofenceDto>>();
        Console.WriteLine($"Registered zone: {registeredZone?.Data?.Id}");

        // Check vehicle location against geofences
        var vehicleId = Guid.NewGuid();
        var checkResponse = await client.PostAsJsonAsync(
            $"api/Geofence/check?vehicleId={vehicleId}&latitude=40.7484&longitude=-73.9857",
            (object)null);
        var alerts = await checkResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<GeofenceAlertDto>>>();
        Console.WriteLine($"Geofence alerts: {alerts?.Data?.Count}");

        // Remove the zone
        var deleteResponse = await client.DeleteAsync($"api/Geofence/{registeredZone?.Data?.Id}");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}
```

## GeofenceDtoExtensions
`GeofenceDtoExtensions` provides extension methods for `GeofenceDto` to facilitate common geofence operations including point-in-zone testing, polygon parsing, distance calculations, and geofence type identification. These methods simplify working with both circular and polygonal geofence zones.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.DTOs;

class Program
{
    static void Main()
    {
        // Create a circular geofence (500m radius around a warehouse)
        var circleGeofence = new GeofenceDto
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse Zone",
            Type = nameof(GeofenceType.Circle),
            CenterLatitude = 40.7484,
            CenterLongitude = -73.9857,
            RadiusKm = 0.5, // 500 meters
            PolygonCoordinates = null
        };

        // Test if a point is inside the circular geofence
        bool isInsideCircle = circleGeofence.ContainsPoint(40.7484, -73.9857);
        Console.WriteLine($"Point inside circle: {isInsideCircle}"); // True

        // Calculate distance from center to a point
        double? distance = circleGeofence.DistanceTo(40.7485, -73.9858);
        Console.WriteLine($"Distance from center: {distance:F3} km");

        // Check if it's a circle geofence
        bool isCircle = circleGeofence.IsCircle();
        Console.WriteLine($"Is circle geofence: {isCircle}"); // True

        // Check if it's a polygon geofence
        bool isPolygon = circleGeofence.IsPolygon();
        Console.WriteLine($"Is polygon geofence: {isPolygon}"); // False

        // Create a polygonal geofence (warehouse perimeter)
        var polygonGeofence = new GeofenceDto
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse Perimeter",
            Type = nameof(GeofenceType.Polygon),
            CenterLatitude = null,
            CenterLongitude = null,
            RadiusKm = null,
            PolygonCoordinates = "40.7480,-73.9860;40.7480,-73.9850;40.7490,-73.9850;40.7490,-73.9860"
        };

        // Get polygon points
        var polygonPoints = polygonGeofence.GetPolygonPoints();
        Console.WriteLine($"Polygon has {polygonPoints.Count} points");

        // Test if a point is inside the polygonal geofence
        bool isInsidePolygon = polygonGeofence.ContainsPoint(40.7485, -73.9855);
        Console.WriteLine($"Point inside polygon: {isInsidePolygon}");
    }
}
```

## GeoJsonSerializer

`GeoJsonSerializer` converts location data, routes, and geofences to GeoJSON format, a standardized geographic data format used by mapping libraries like Leaflet and Mapbox. It provides static methods to serialize single locations, collections of locations, routes, and geofence circles into valid GeoJSON strings with proper coordinate ordering and camelCase property naming.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Formatters;

class Program
{
    static void Main()
    {
        // Serialize a single location to GeoJSON Point feature
        var location = new Location
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Latitude = 40.7128,
            Longitude = -74.0060,
            Accuracy = 5.2,
            Altitude = 10.5,
            LocationType = "GPS"
        };
        
        string locationJson = GeoJsonSerializer.SerializeLocation(location);
        Console.WriteLine(locationJson);
        
        // Serialize multiple locations to GeoJSON FeatureCollection
        var locations = new List<Location>();
        for (int i = 0; i < 5; i++)
        {
            locations.Add(new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7128 + (i * 0.001),
                Longitude = -74.0060 + (i * 0.001),
                Accuracy = 5.0,
                Altitude = 10.0,
                LocationType = "GPS"
            });
        }
        
        string locationsJson = GeoJsonSerializer.SerializeLocations(locations);
        Console.WriteLine(locationsJson);
        
        // Serialize a route to GeoJSON LineString feature
        var route = new Route
        {
            Id = Guid.NewGuid(),
            Name = "Downtown to Warehouse",
            Description = "Delivery route",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var waypoints = new List<(double Latitude, double Longitude)>
        {
            (40.7128, -74.0060),      // Start: Manhattan
            (40.7484, -73.9857),      // Mid: Brooklyn Bridge
            (40.7580, -73.9855)       // End: Downtown Brooklyn
        };
        
        string routeJson = GeoJsonSerializer.SerializeRoute(route, waypoints);
        Console.WriteLine(routeJson);
        
        // Serialize a geofence circle to GeoJSON
        string geofenceJson = GeoJsonSerializer.SerializeGeofence(
            Guid.NewGuid(),
            40.7484,
            -73.9857,
            500.0
        );
        Console.WriteLine(geofenceJson);
    }
}
```

## GeofenceAlertsClient

`GeofenceAlertsClient` is a SignalR client that connects to a real-time location tracking hub to receive and process geofence alerts for vehicles. It monitors vehicle movements against configured geofence zones and handles various alert types including geofence entry/exit, speed violations, and communication loss. The client can be used to track vehicle movements in restricted areas and trigger appropriate responses.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using SignalRMapRealtime.Examples;

class Program
{
    static async Task Main()
    {
        // Create a geofence alerts client for a specific vehicle
        var hubUrl = "https://localhost:5001/locationHub";
        var vehicleId = Guid.NewGuid().ToString();
        var client = new GeofenceAlertsClient(hubUrl, vehicleId);

        // Connect to the SignalR hub
        await client.Connect();

        // Add geofence zones to monitor
        client.AddGeofence("Warehouse District", 40.7128, -74.0060, 1.0);
        client.AddGeofence("Office Park", 40.7489, -73.9680, 0.5);
        client.AddGeofence("Restricted Zone Alpha", 40.7614, -73.9776, 2.0);

        Console.WriteLine("Listening for geofence alerts... Press Enter to exit");
        Console.ReadLine();

        // Disconnect when done
        await client.Disconnect();
    }
}
```

### Complete Usage with Alert Handling

```csharp
using System;
using System.Threading.Tasks;
using SignalRMapRealtime.Examples;

class Program
{
    static async Task Main()
    {
        // Create client with custom hub URL
        var client = new GeofenceAlertsClient(
            "https://api.example.com/locationHub",
            "TRUCK-999-FLEET"
        );

        // Connect to the hub
        await client.Connect();

        // Configure geofence zones
        client.AddGeofence("Warehouse", 40.7128, -74.0060, 1.0);
        client.AddGeofence("Loading Dock", 40.7484, -73.9857, 0.5);
        client.AddGeofence("Security Perimeter", 40.7580, -73.9855, 1.5);

        Console.WriteLine("=== Geofence Monitoring Started ===");
        Console.WriteLine("Waiting for alerts... (press Enter to stop)");
        
        // Keep running to receive alerts
        Console.ReadLine();

        // Clean up
        await client.Disconnect();
        Console.WriteLine("Monitoring stopped.");
    }
}
```

## RouteOptimizationClient

`RouteOptimizationClient` is a helper class for creating, optimizing, and managing delivery routes with multiple waypoints. It provides functionality to calculate distances between locations, optimize waypoint order using nearest-neighbor algorithm, simulate route execution, and interact with a route management API. The client is useful for logistics applications that need to plan efficient delivery routes.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using SignalRMapRealtime.Examples;

class Program
{
static async Task Main()
{
// Create a route optimization client
var client = new RouteOptimizationClient(
"https://localhost:5001",
"your-api-key-here"
);

// Run the route optimization example with a vehicle
var vehicleId = Guid.NewGuid().ToString();
await client.RunExample(vehicleId);
}
}
```

### Complete Usage Example with Custom Waypoints

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SignalRMapRealtime.Examples;

class Program
{
static async Task Main()
{
// Create a route optimization client
var client = new RouteOptimizationClient(
"https://api.example.com",
"sk-1234567890abcdef"
);

// Create custom waypoints for a delivery route
var waypoints = new List<Waypoint>
{
new Waypoint { Order = 1, Latitude = 40.7128, Longitude = -74.0060, Name = "Warehouse - Start" },
new Waypoint { Order = 2, Latitude = 40.7489, Longitude = -73.9680, Name = "Customer A" },
new Waypoint { Order = 3, Latitude = 40.7614, Longitude = -73.9776, Name = "Customer B" },
new Waypoint { Order = 4, Latitude = 40.7282, Longitude = -73.7949, Name = "Customer C" },
new Waypoint { Order = 5, Latitude = 40.6892, Longitude = -74.0445, Name = "Warehouse - End" }
};

// Optimize the waypoint order
var optimizedWaypoints = client.OptimizeWaypointOrder(waypoints);

Console.WriteLine("Optimized Route Order:");
foreach (var wp in optimizedWaypoints)
{
Console.WriteLine($" {wp.Order}. {wp.Name} ({wp.Latitude:F4}, {wp.Longitude:F4})");
}

// Run the example
var vehicleId = Guid.NewGuid().ToString();
await client.RunExample(vehicleId);
}
}
```

## PlaybackController

`PlaybackController` provides REST endpoints for managing historical route playback sessions, retrieving timelines, snapshots, and statistics. It enables clients to start a playback session, query active sessions, obtain the state of a specific session, stop a session, and fetch detailed timeline or snapshot data for a tracking session.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

class Program
{
    static async Task Main()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Start a playback session
        var startRequest = new StartPlaybackRequest
        {
            SessionId = 1,
            SpeedMultiplier = 1.0
        };
        var startResponse = await client.PostAsJsonAsync("api/Playback/sessions", startRequest);
        var startResult = await startResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        var playbackId = startResult?.Data?.PlaybackId;
        Console.WriteLine($"Started playback: {playbackId}");

        // Get all active sessions
        var sessionsResponse = await client.GetAsync("api/Playback/sessions");
        var sessions = await sessionsResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PlaybackSessionDto>>>();
        Console.WriteLine($"Active sessions: {sessions?.Data?.Count}");

        // Get state of the started session
        var stateResponse = await client.GetAsync($"api/Playback/sessions/{playbackId}");
        var state = await stateResponse.Content.ReadFromJsonAsync<ApiResponse<PlaybackSessionDto>>();
        Console.WriteLine($"Session state: {state?.Data?.Status}");

        // Get timeline for a tracking session
        var timelineResponse = await client.GetAsync("api/Playback/timeline/1");
        var timeline = await timelineResponse.Content.ReadFromJsonAsync<ApiResponse<RouteTimelineDto>>();
        Console.WriteLine($"Timeline points: {timeline?.Data?.Points?.Count}");

        // Get a snapshot at a specific time
        var snapshotResponse = await client.GetAsync($"api/Playback/snapshot/1?timestamp={DateTime.UtcNow:o}");
        var snapshot = await snapshotResponse.Content.ReadFromJsonAsync<ApiResponse<PlaybackFrameDto>>();
        Console.WriteLine($"Snapshot vehicle: {snapshot?.Data?.Vehicle?.Id}");

        // Get statistics for the session
        var statsResponse = await client.GetAsync("api/Playback/statistics/1");
        var stats = await statsResponse.Content.ReadFromJsonAsync<ApiResponse<PlaybackStatisticsDto>>();
        Console.WriteLine($"Total distance: {stats?.Data?.TotalDistanceKm} km");

        // Stop the playback session
        var stopResponse = await client.DeleteAsync($"api/Playback/sessions/{playbackId}");
        Console.WriteLine($"Stop status: {stopResponse.StatusCode}");
    }
}
```

## AssetControllerTests

`AssetControllerTests` contains integration tests for the `AssetController` API controller, which manages CRUD operations for assets in the SignalRMapRealtime application. The test class verifies that the controller endpoints correctly handle creating, reading, updating, and deleting assets, including proper status codes, content types, and error handling. Tests cover scenarios like invalid model validation, non-existent resource lookups, and ID mismatch scenarios.

### Usage Example

```csharp
using System;

## LocationControllerTests

`LocationControllerTests` contains integration tests for the `LocationController` API controller, which manages CRUD operations for location data in the SignalRMapRealtime application. The test class verifies that the controller endpoints correctly handle creating, reading, updating, and deleting locations, including proper status codes, content types, and error handling. Tests cover scenarios like invalid model validation, non-existent resource lookups, and ID mismatch scenarios.

### Usage Example

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using Xunit;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new location
        var newLocation = new LocationDto
        {
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Accuracy = 5.2,
            Speed = 45.5,
            Heading = 90
        };

        var createResponse = await client.PostAsync("/api/Location",
            new StringContent(JsonConvert.SerializeObject(newLocation), Encoding.UTF8, "application/json"));
        var createdLocation = await createResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Created location response status: {createResponse.StatusCode}");

        // Get all locations with pagination
        var getAllResponse = await client.GetAsync("/api/Location?pageNumber=1&pageSize=20");
        var allLocations = await getAllResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get all locations response status: {getAllResponse.StatusCode}");

        // Get a specific location by ID
        var locationId = JsonConvert.DeserializeObject<LocationDto>(createdLocation)?.Id ?? Guid.Empty;
        var getByIdResponse = await client.GetAsync($"/api/Location/{locationId}");
        var singleLocation = await getByIdResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get location by ID response status: {getByIdResponse.StatusCode}");

        // Update a location
        var updateLocation = new LocationDto
        {
            Id = locationId,
            Latitude = 40.7484,
            Longitude = -73.9857,
            Timestamp = DateTime.UtcNow.AddMinutes(1),
            Accuracy = 6.1,
            Speed = 55.0,
            Heading = 180
        };
        var updateResponse = await client.PutAsync($"/api/Location/{locationId}",
            new StringContent(JsonConvert.SerializeObject(updateLocation), Encoding.UTF8, "application/json"));
        Console.WriteLine($"Update location response status: {updateResponse.StatusCode}");

        // Delete a location
        var deleteResponse = await client.DeleteAsync($"/api/Location/{locationId}");
        Console.WriteLine($"Delete location response status: {deleteResponse.StatusCode}");
    }
}
```

## VehicleControllerTests

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new asset
        var newAsset = new AssetDto
        {
            Name = "Medical Equipment #XYZ-789",
            Type = AssetType.Equipment,
            Status = "Available"
        };

        var createResponse = await client.PostAsJsonAsync("api/Asset", newAsset);
        var createdAsset = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AssetDto>>();
        Console.WriteLine($"Created asset: {createdAsset?.Data?.Id} - {createdAsset?.Data?.Name}");

        // Get all assets with pagination
        var getAllResponse = await client.GetAsync("api/Asset?pageNumber=1&pageSize=20");
        var allAssets = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<AssetDto>>>();
        Console.WriteLine($"Total assets: {allAssets?.Data?.TotalCount}");

        // Get a specific asset by ID
        var getByIdResponse = await client.GetAsync($"api/Asset/{createdAsset?.Data?.Id}");
        var singleAsset = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<AssetDto>>();
        Console.WriteLine($"Asset: {singleAsset?.Data?.Name} (Status: {singleAsset?.Data?.Status})");

        // Update an asset
        var updateAsset = new AssetDto
        {
            Id = createdAsset?.Data?.Id ?? Guid.Empty,
            Name = "Medical Equipment #XYZ-789 - Updated",
            Type = AssetType.Equipment,
            Status = "InUse"
        };
        var updateResponse = await client.PutAsJsonAsync($"api/Asset/{createdAsset?.Data?.Id}", updateAsset);
        Console.WriteLine($"Update status: {updateResponse.StatusCode}");

        // Delete an asset
        var deleteResponse = await client.DeleteAsync($"api/Asset/{createdAsset?.Data?.Id}");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}
```

## LocationControllerTests

`LocationControllerTests` contains integration tests for the `LocationController` API controller, which manages CRUD operations for location data in the SignalRMapRealtime application. The test class verifies that the controller endpoints correctly handle creating, reading, updating, and deleting locations, including proper status codes, content types, and error handling. Tests cover scenarios like invalid model validation, non-existent resource lookups, and ID mismatch scenarios.

### Usage Example

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using Xunit;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new location
        var newLocation = new LocationDto
        {
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Accuracy = 5.2,
            Speed = 45.5,
            Heading = 90
        };

        var createResponse = await client.PostAsync("/api/Location",
            new StringContent(JsonConvert.SerializeObject(newLocation), Encoding.UTF8, "application/json"));
        var createdLocation = await createResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Created location response status: {createResponse.StatusCode}");

        // Get all locations with pagination
        var getAllResponse = await client.GetAsync("/api/Location?pageNumber=1&pageSize=20");
        var allLocations = await getAllResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get all locations response status: {getAllResponse.StatusCode}");

        // Get a specific location by ID
        var locationId = JsonConvert.DeserializeObject<LocationDto>(createdLocation)?.Id ?? Guid.Empty;
        var getByIdResponse = await client.GetAsync($"/api/Location/{locationId}");
        var singleLocation = await getByIdResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get location by ID response status: {getByIdResponse.StatusCode}");

        // Update a location
        var updateLocation = new LocationDto
        {
            Id = locationId,
            Latitude = 40.7484,
            Longitude = -73.9857,
            Timestamp = DateTime.UtcNow.AddMinutes(1),
            Accuracy = 6.1,
            Speed = 55.0,
            Heading = 180
        };
        var updateResponse = await client.PutAsync($"/api/Location/{locationId}",
            new StringContent(JsonConvert.SerializeObject(updateLocation), Encoding.UTF8, "application/json"));
        Console.WriteLine($"Update location response status: {updateResponse.StatusCode}");

        // Delete a location
        var deleteResponse = await client.DeleteAsync($"/api/Location/{locationId}");
        Console.WriteLine($"Delete location response status: {deleteResponse.StatusCode}");
    }
}
```

## VehicleControllerTests

### Usage Example

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using Xunit;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new vehicle
        var newVehicle = new VehicleDto
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2022,
            LicensePlate = "CAMRY2022",
            Status = VehicleStatus.Available
        };

        var createResponse = await client.PostAsync("/api/Vehicle", 
            new StringContent(JsonConvert.SerializeObject(newVehicle), Encoding.UTF8, "application/json"));
        var createdVehicle = await createResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Created vehicle response status: {createResponse.StatusCode}");

        // Get all vehicles
        var getAllResponse = await client.GetAsync("/api/Vehicle");
        var allVehicles = await getAllResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get all vehicles response status: {getAllResponse.StatusCode}");

        // Get a specific vehicle by ID
        var vehicleId = JsonConvert.DeserializeObject<VehicleDto>(createdVehicle)?.Id ?? Guid.Empty;
        var getByIdResponse = await client.GetAsync($"/api/Vehicle/{vehicleId}");
        var singleVehicle = await getByIdResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get vehicle by ID response status: {getByIdResponse.StatusCode}");

        // Update a vehicle
        var updateVehicle = new VehicleDto
        {
            Id = vehicleId,
            Make = "Toyota",
            Model = "Camry LE",
            Year = 2023,
            LicensePlate = "CAMRY2022",
            Status = VehicleStatus.InUse
        };
        var updateResponse = await client.PutAsync($"/api/Vehicle/{vehicleId}",
            new StringContent(JsonConvert.SerializeObject(updateVehicle), Encoding.UTF8, "application/json"));
        Console.WriteLine($"Update vehicle response status: {updateResponse.StatusCode}");

        // Delete a vehicle
        var deleteResponse = await client.DeleteAsync($"/api/Vehicle/{vehicleId}");
        Console.WriteLine($"Delete vehicle response status: {deleteResponse.StatusCode}");
    }
}
```

## DockerDeploymentExample

`DockerDeploymentExample` demonstrates how to deploy and interact with the SignalR Map Realtime API when running in Docker containers. This example shows basic API operations including health checks, vehicle management, and location tracking through the Docker-deployed service.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using SignalRMapRealtime.Examples.DockerDeployment;

class Program
{
    static async Task Main()
    {
        // Create a Docker deployment example instance
        // Default URL is http://localhost:5000 (adjust if using Docker network)
        var dockerExample = new DockerDeploymentExample("http://signalr-map-realtime:5000");
        
        // Run the complete Docker deployment example
        await dockerExample.RunDockerExampleAsync();
        
        Console.WriteLine("\nExample completed. Docker containers are now running the API!");
    }
}
```

## SessionAnalyticsReporter

`SessionAnalyticsReporter` analyzes and reports on tracking session statistics. It generates comprehensive analytics reports for vehicles, including distance traveled, speed metrics, duration, and other key performance indicators. The reporter can display formatted console output and export data to CSV format for further analysis.

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using SignalRMapRealtime.Examples;

class Program
{
    static async Task Main()
    {
        // Create a session analytics reporter
        var reporter = new SessionAnalyticsReporter(
            baseUrl: "https://api.example.com",
            apiKey: "your-api-key-here"
        );

        // Generate analytics report for a vehicle
        var vehicleId = Guid.NewGuid().ToString();
        await reporter.GenerateVehicleAnalyticsReport(vehicleId);

        // Export analytics to CSV
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "vehicle_analytics.csv");
        await reporter.ExportAnalyticsToCsv(vehicleId, outputPath);
        Console.WriteLine($"Analytics exported to: {outputPath}");

        // Run the complete example
        await reporter.RunExample(vehicleId);
    }
}
```

## RouteControllerTests

`RouteControllerTests` contains integration tests for the `RouteController` API controller, which manages CRUD operations for routes in the SignalRMapRealtime application. The test class verifies that the controller endpoints correctly handle creating, reading, updating, and deleting routes, including proper status codes, content types, and error handling. Tests cover scenarios like invalid model validation, non-existent resource lookups, and ID mismatch scenarios.

### Usage Example

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using Xunit;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new route
        var newRoute = new RouteDto
        {
            Name = "Downtown to Airport",
            Description = "Delivery route from downtown to airport"
        };

        var createResponse = await client.PostAsync("/api/Route",
            new StringContent(JsonConvert.SerializeObject(newRoute), Encoding.UTF8, "application/json"));
        var createdRoute = await createResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Created route response status: {createResponse.StatusCode}");

        // Get all routes with pagination
        var getAllResponse = await client.GetAsync("/api/Route?pageNumber=1&pageSize=20");
        var allRoutes = await getAllResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get all routes response status: {getAllResponse.StatusCode}");

        // Get a specific route by ID
        var routeId = JsonConvert.DeserializeObject<RouteDto>(createdRoute)?.Id ?? Guid.Empty;
        var getByIdResponse = await client.GetAsync($"/api/Route/{routeId}");
        var singleRoute = await getByIdResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Get route by ID response status: {getByIdResponse.StatusCode}");

        // Update a route
        var updateRoute = new RouteDto
        {
            Id = routeId,
            Name = "Downtown to Airport - Updated",
            Description = "Updated delivery route description"
        };
        var updateResponse = await client.PutAsync($"/api/Route/{routeId}",
            new StringContent(JsonConvert.SerializeObject(updateRoute), Encoding.UTF8, "application/json"));
        Console.WriteLine($"Update route response status: {updateResponse.StatusCode}");

        // Delete a route
        var deleteResponse = await client.DeleteAsync($"/api/Route/{routeId}");
        Console.WriteLine($"Delete route response status: {deleteResponse.StatusCode}");
    }
}
```

## VehicleDtoExtensions

`VehicleDtoExtensions` provides extension methods for `VehicleDto` to facilitate common vehicle operations including online status checking, status comparisons, status string formatting, location details extraction, basic information formatting, and attention requirement determination. These methods simplify working with vehicle data and provide convenient utility functions for vehicle management.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Domain.Enums;

class Program
{
    static void Main()
    {
        // Create a sample vehicle
        var vehicle = new VehicleDto
        {
            Id = Guid.NewGuid(),
            Name = "Delivery Truck #42",
            RegistrationNumber = "TRK-42-ABC",
            Make = "Ford",
            Model = "F-150",
            Year = 2023,
            Status = VehicleStatus.Available,
            IsOnline = true,
            LastLocation = new LocationDto
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow,
                Accuracy = 5.2,
                Speed = 45.5
            }
        };

        // Check if vehicle is online
        bool isOnline = vehicle.IsOnline();
        Console.WriteLine($"Vehicle online: {isOnline}"); // True

        // Check if vehicle is in a specific status
        bool isAvailable = vehicle.IsInStatus(VehicleStatus.Available);
        Console.WriteLine($"Vehicle is available: {isAvailable}"); // True

        // Get human-readable status string
        string statusString = vehicle.GetStatusString();
        Console.WriteLine($"Vehicle status: {statusString}"); // "Available"

        // Get location details
        string? locationDetails = vehicle.GetLocationDetails();
        Console.WriteLine($"Location: {locationDetails}");
        // Output: Location: Lat: 40.712800, Lng: -74.006000

        // Get formatted vehicle information
        string infoString = vehicle.GetInfoString();
        Console.WriteLine($"Vehicle info: {infoString}");
        // Output: Vehicle info: [guid-here] TRK-42-ABC - Delivery Truck #42 (Available)

        // Check if vehicle requires attention
        bool requiresAttention = vehicle.RequiresAttention();
        Console.WriteLine($"Vehicle requires attention: {requiresAttention}"); // False

        // Create a maintenance vehicle
        var maintenanceVehicle = new VehicleDto
        {
            Id = Guid.NewGuid(),
            Name = "Service Van #7",
            RegistrationNumber = "VAN-07-XYZ",
            Status = VehicleStatus.Maintenance,
            IsOnline = false
        };

        // Check attention requirement for maintenance vehicle
        bool maintenanceNeedsAttention = maintenanceVehicle.RequiresAttention();
        Console.WriteLine($"Maintenance vehicle requires attention: {maintenanceNeedsAttention}"); // True
    }
}
```

## LocationTrackingExceptionExtensions

`LocationTrackingExceptionExtensions` provides extension methods for handling and extracting information from location tracking exceptions. This utility class helps extract contextual information from specific exception types like `VehicleNotFoundException`, `AssetNotFoundException`, `TrackingSessionNotFoundException`, and `InvalidLocationException`, and provides helper methods to determine error types and format error messages.

### Usage Example

```csharp
using System;
using SignalRMapRealtime.Exceptions;

class Program
{
    static void Main()
    {
        try
        {
            // Simulate a location tracking operation that throws an exception
            throw new VehicleNotFoundException("Vehicle with ID 42 not found", 42);
        }
        catch (LocationTrackingException ex)
        {
            // Extract vehicle ID from the exception
            int? vehicleId = ex.GetVehicleId();
            Console.WriteLine($"Vehicle ID: {vehicleId}"); // Output: Vehicle ID: 42

            // Extract asset ID (returns null for VehicleNotFoundException)
            int? assetId = ex.GetAssetId();
            Console.WriteLine($"Asset ID: {assetId}"); // Output: Asset ID: 

            // Extract session ID (returns null for VehicleNotFoundException)
            int? sessionId = ex.GetSessionId();
            Console.WriteLine($"Session ID: {sessionId}"); // Output: Session ID: 

            // Get coordinates (returns null for non-location exceptions)
            ex.GetCoordinates(out double? latitude, out double? longitude);
            Console.WriteLine($"Coordinates: Latitude={latitude}, Longitude={longitude}");

            // Check if it's a not-found error
            bool isNotFound = ex.IsNotFoundError();
            Console.WriteLine($"Is not found error: {isNotFound}"); // Output: Is not found error: True

            // Check if it's an invalid location error
            bool isInvalidLocation = ex.IsInvalidLocationError();
            Console.WriteLine($"Is invalid location error: {isInvalidLocation}"); // Output: Is invalid location error: False

            // Get formatted error message
            string errorMessage = ex.ToErrorMessage();
            Console.WriteLine($"Error message: {errorMessage}");
            // Output: Error message: Vehicle with ID 42 not found | Vehicle ID: 42
        }

        // Example with InvalidLocationException
        try
        {
            throw new InvalidLocationException("Invalid coordinates provided", 181.0, -181.0);
        }
        catch (LocationTrackingException ex)
        {
            // Get coordinates from the exception
            ex.GetCoordinates(out double? latitude, out double? longitude);
            Console.WriteLine($"Invalid coordinates: Latitude={latitude}, Longitude={longitude}");

            // Check error types
            Console.WriteLine($"Is invalid location error: {ex.IsInvalidLocationError()}"); // Output: True
            Console.WriteLine($"Is not found error: {ex.IsNotFoundError()}"); // Output: False

            // Get formatted error message
            Console.WriteLine($"Error message: {ex.ToErrorMessage()}");
            // Output: Error message: Invalid coordinates provided | Coordinates: Latitude=181.000000, Longitude=-181.000000
        }
    }
}
```

## DomainModelBehaviorTests

`DomainModelBehaviorTests` contains unit tests that verify the behavioral contracts of domain models in the SignalRMapRealtime application. This test class validates that domain entities like `Vehicle`, `TrackingSession`, and `Asset` behave correctly under various conditions, ensuring proper state transitions, validation rules, and business logic execution. The tests use FluentAssertions for clear, readable assertions and cover edge cases such as null configurations, invalid operations, and argument validation.

### Usage Example

```csharp
using FluentAssertions;
using SignalRMapRealtime.Domain.Models;

class Program
{
    static void Main()
    {
        // Test Vehicle speed limit behavior
        var vehicle = new Vehicle
        {
            MaxSpeed = 100.0,
            LastLocation = new Location { Speed = 120.0 }
        };
        
        bool exceeded = vehicle.HasExceededSpeedLimit();
        Console.WriteLine($"Vehicle exceeded speed limit: {exceeded}"); // True
        
        // Test Vehicle with no speed limit configured
        var vehicleNoLimit = new Vehicle
        {
            MaxSpeed = null,
            LastLocation = new Location { Speed = 200.0 }
        };
        
        bool exceededNoLimit = vehicleNoLimit.HasExceededSpeedLimit();
        Console.WriteLine($"Vehicle with no limit exceeded: {exceededNoLimit}"); // False
        
        // Test TrackingSession state management
        var session = new TrackingSession();
        session.StartSession();
        Console.WriteLine($"Session status: {session.Status}"); // Active
        Console.WriteLine($"Session start time: {session.StartTime}");
        
        // Test that recording location throws when session is not active
        try
        {
            session.RecordLocation(new Location { Latitude = 51.5074, Longitude = -0.1278 });
            Console.WriteLine("ERROR: Should have thrown exception");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Correctly threw exception: {ex.Message}");
        }
        
        // Test Asset special handling validation
        var asset = new Asset { Name = "Fragile Electronics" };
        try
        {
            asset.EnableSpecialHandling(string.Empty);
            Console.WriteLine("ERROR: Should have thrown exception");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Correctly threw exception: {ex.Message}");
        }
    }
}
```

## PlaybackServiceTests

`PlaybackServiceTests` contains unit tests for the `PlaybackService` class, which manages playback sessions for tracking data. The test class verifies functionality for retrieving playback states, building timelines from location data, obtaining playback statistics, and generating snapshots at specific timestamps. It ensures that the service correctly handles edge cases like unknown sessions, empty location data, and validates proper behavior across different scenarios.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Services;
using Xunit;

public class PlaybackServiceTestsExample
{
    private readonly PlaybackService _playbackService;
    private readonly Mock<IPlaybackRepository> _mockRepository;

    public PlaybackServiceTestsExample()
    {
        _mockRepository = new Mock<IPlaybackRepository>();
        _playbackService = new PlaybackService(_mockRepository.Object);
    }

    public async Task Example_GetPlaybackStateAsync_ForUnknownSession_ReturnsNull()
    {
        // Arrange
        var unknownSessionId = 999;
        _mockRepository.Setup(x => x.GetPlaybackStateAsync(unknownSessionId))
                     .ReturnsAsync((PlaybackState?)null);

        // Act
        var result = await _playbackService.GetPlaybackStateAsync(unknownSessionId);

        // Assert
        Assert.Null(result);
    }

    public async Task Example_GetActivePlaybacksAsync_WithNoActiveSessions_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetActivePlaybacksAsync())
                     .ReturnsAsync(new List<PlaybackSession>());

        // Act
        var result = await _playbackService.GetActivePlaybacksAsync();

        // Assert
        Assert.Empty(result);
    }

    public async Task Example_BuildTimelineAsync_ForSessionWithNoLocations_ReturnsNull()
    {
        // Arrange
        var sessionId = 1;
        _mockRepository.Setup(x => x.GetLocationsAsync(sessionId))
                     .ReturnsAsync(new List<Location>());

        // Act
        var result = await _playbackService.BuildTimelineAsync(sessionId);

        // Assert
        Assert.Null(result);
    }

    public async Task Example_BuildTimelineAsync_ForSessionWithLocations_ReturnsPopulatedTimeline()
    {
        // Arrange
        var sessionId = 1;
        var locations = new List<Location>
        {
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            },
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7484,
                Longitude = -73.9857,
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            }
        };
        _mockRepository.Setup(x => x.GetLocationsAsync(sessionId))
                     .ReturnsAsync(locations);

        // Act
        var result = await _playbackService.BuildTimelineAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Points);
    }

    public async Task Example_GetPlaybackStatisticsAsync_ForValidSession_ReturnsStatistics()
    {
        // Arrange
        var sessionId = 1;
        var locations = new List<Location>
        {
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            },
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7484,
                Longitude = -73.9857,
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            }
        };
        _mockRepository.Setup(x => x.GetLocationsAsync(sessionId))
                     .ReturnsAsync(locations);

        // Act
        var result = await _playbackService.GetPlaybackStatisticsAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalDistanceKm > 0);
        Assert.True(result.DurationMinutes > 0);
    }

    public async Task Example_GetSnapshotAtTimestampAsync_ForSessionWithData_ReturnsFrame()
    {
        // Arrange
        var sessionId = 1;
        var timestamp = DateTime.UtcNow.AddMinutes(-7);
        var locations = new List<Location>
        {
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            },
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7484,
                Longitude = -73.9857,
                Timestamp = timestamp
            },
            new Location
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                Latitude = 40.7580,
                Longitude = -73.9855,
                Timestamp = DateTime.UtcNow.AddMinutes(-3)
            }
        };
        _mockRepository.Setup(x => x.GetLocationsAsync(sessionId))
                     .ReturnsAsync(locations);

        // Act
        var result = await _playbackService.GetSnapshotAtTimestampAsync(sessionId, timestamp);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicle);
        Assert.Equal(timestamp, result.Timestamp);
    }
}
```

## GeofenceServiceTests

`GeofenceServiceTests` contains unit tests for the `GeofenceService` class, which manages geofence zone registration, removal, and location checking functionality. The test class verifies that zones can be registered and removed correctly, and that vehicle locations are properly evaluated against geofence boundaries to generate boundary-crossing alerts (entry/exit events). Tests cover both circle and polygon zone shapes and ensure that alerts are only emitted on state changes, preventing duplicate alerts for vehicles that remain inside zones.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;

class Program
{
    static async Task Main()
    {
        // Create the geofence service
        var eventBus = new InMemoryEventBus();
        var service = new GeofenceService(eventBus, Microsoft.Extensions.Logging.Abstractions.NullLogger<GeofenceService>.Instance);

        // Register a new circular geofence zone (500m radius around London coordinates)
        var newZone = new CreateGeofenceDto
        {
            Name = "Central London Zone",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 51.5074,
            CenterLongitude = -0.1278,
            RadiusKm = 0.5, // 500 meters
            Description = "Restricted area in central London"
        };

        var registeredZone = await service.RegisterZoneAsync(newZone);
        Console.WriteLine($"Registered zone: {registeredZone.Id} - {registeredZone.Name}");

        // Get all active zones
        var activeZones = await service.GetActiveZonesAsync();
        Console.WriteLine($"Active zones count: {activeZones.Count}");

        // Check vehicle location against geofences (vehicle enters the zone)
        var vehicleId = Guid.NewGuid();
        var alertsOnEntry = await service.CheckLocationAsync(vehicleId, 51.5074, -0.1278);
        Console.WriteLine($"Alerts on entry: {alertsOnEntry.Count}");
        if (alertsOnEntry.Count > 0)
        {
            Console.WriteLine($"  - Violation: {alertsOnEntry[0].ViolationType}");
            Console.WriteLine($"  - Vehicle: {alertsOnEntry[0].VehicleId}");
        }

        // Check vehicle location again (vehicle remains inside the zone - no duplicate alert)
        var alertsOnStay = await service.CheckLocationAsync(vehicleId, 51.5075, -0.1279);
        Console.WriteLine($"Alerts while staying inside: {alertsOnStay.Count}");

        // Move vehicle outside the zone (vehicle exits)
        var alertsOnExit = await service.CheckLocationAsync(vehicleId, 52.0, 1.0);
        Console.WriteLine($"Alerts on exit: {alertsOnExit.Count}");
        if (alertsOnExit.Count > 0)
        {
            Console.WriteLine($"  - Violation: {alertsOnExit[0].ViolationType}");
        }

        // Remove the zone
        var removed = await service.RemoveZoneAsync(registeredZone.Id);
        Console.WriteLine($"Zone removed: {removed}");

        // Verify zone is gone
        var remainingZones = await service.GetActiveZonesAsync();
        Console.WriteLine($"Remaining zones: {remainingZones.Count}");
    }
}
```

## SignalrMapRealtimeOptionsTests

`SignalrMapRealtimeOptionsTests` contains unit tests for the `SignalrMapRealtimeOptions` configuration class, which validates application settings for the SignalR Map Realtime application. The test class verifies configuration validation rules for the main options object and all its nested configuration sections including AppInfo, HealthChecks, ApiKeyAuthentication, Performance, SignalRHubs, WebSockets, BackgroundJobs, and Security. Tests ensure that invalid configurations (wrong API versions, invalid environments, out-of-range values) are properly rejected while valid configurations pass validation.

### Usage Example

```csharp
using Microsoft.Extensions.Configuration;
using SignalRMapRealtime.Configuration;

class Program
{
    static void Main()
    {
        // Create valid configuration
        var validConfig = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "SignalR Map Realtime API",
                Environment = "Production",
                RequestTimeoutSeconds = 30,
                LocationUpdateIntervalSeconds = 60
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions { Enabled = true },
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions { Enabled = true },
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions
            {
                EnableDetailedMetrics = true,
                MaxConcurrentConnections = 1000,
                MaxConnectionsPerHub = 50000
            },
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions
            {
                Enabled = true,
                MaxConnectionsPerHub = 1000
            },
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions { Enabled = true },
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions { Enabled = true },
            Security = new SignalrMapRealtimeOptions.SecurityOptions { EnableRateLimiting = true }
        };

        // Validate configuration
        bool isValid = validConfig.Validate(out var validationResults);
        Console.WriteLine($"Valid configuration: {isValid}");
        if (!isValid)
        {
            foreach (var result in validationResults)
            {
                Console.WriteLine($"Validation error: {result.ErrorMessage}");
            }
        }
    }
}
```

## GeoLocationBenchmarks

`GeoLocationBenchmarks` is a benchmark suite that measures the performance of geo-location related operations such as distance calculation, cardinal direction determination, bounding box calculation, and coordinate validation. It uses BenchmarkDotNet to provide detailed performance metrics including execution time, memory allocation, and other diagnostic information. This benchmark class helps identify performance bottlenecks in location-based calculations used throughout the application.

### Usage Example

```csharp
using BenchmarkDotNet.Running;
using SignalRMapRealtime.Utilities;
using SignalRMapRealtime.Domain.Models;

class Program
{
    static void Main()
    {
        // Create test locations
        var location1 = new Location { Latitude = 40.7128, Longitude = -74.0060 }; // New York
        var location2 = new Location { Latitude = 34.0522, Longitude = -118.2437 }; // Los Angeles
        
        // Calculate distance between two points (benchmark method)
        double distance = GeoLocationExtensions.DistanceBetween(
            location1.Latitude, location1.Longitude,
            location2.Latitude, location2.Longitude
        );
        Console.WriteLine($"Distance: {distance:F2} km");
        
        // Get cardinal direction from an angle
        string direction = GeoLocationExtensions.GetCardinalDirection(45.0); // NE
        Console.WriteLine($"Cardinal direction: {direction}");
        
        // Calculate bounding box for a location with radius
        var boundingBox = location1.GetBoundingBox(10.0); // 10km radius
        Console.WriteLine($"Bounding box: MinLat={boundingBox.Item1:F6}, MaxLat={boundingBox.Item2:F6}, " +
                         $"MinLng={boundingBox.Item3:F6}, MaxLng={boundingBox.Item4:F6}");
        
        // Validate coordinates
        bool isValid = location1.Latitude.IsValidCoordinate(location1.Longitude);
        Console.WriteLine($"Coordinates valid: {isValid}");
        
        // Run all benchmarks
        BenchmarkRunner.Run<GeoLocationBenchmarks>();
    }
}
```
using SignalRMapRealtime.Configuration;

class Program
{
    static void Main()
    {
        // Create valid configuration
        var validConfig = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "SignalR Map Realtime API",
                Environment = "Production",
                RequestTimeoutSeconds = 30,
                LocationUpdateIntervalSeconds = 60
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions { Enabled = true },
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions { Enabled = true },
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions
            {
                EnableDetailedMetrics = true,
                MaxConcurrentConnections = 1000,
                MaxConnectionsPerHub = 50000
            },
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions
            {
                Enabled = true,
                MaxConnectionsPerHub = 1000
            },
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions { Enabled = true },
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions { Enabled = true },
            Security = new SignalrMapRealtimeOptions.SecurityOptions { EnableRateLimiting = true }
        };

        // Validate configuration
        bool isValid = validConfig.Validate(out var validationResults);
        Console.WriteLine($"Valid configuration: {isValid}");
        if (!isValid)
        {
            foreach (var result in validationResults)
            {
                Console.WriteLine($"Validation error: {result.ErrorMessage}");
            }
        }

        // Create invalid configuration (out of range values)
        var invalidConfig = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "SignalR Map Realtime API",
                Environment = "Production",
                RequestTimeoutSeconds = 2, // Too low (< 5)
                LocationUpdateIntervalSeconds = 700 // Too high (> 600)
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions { Enabled = true },
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions { Enabled = true },
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions
            {
                EnableDetailedMetrics = true,
                MaxConcurrentConnections = 99, // Too low (< 100)
                MaxConnectionsPerHub = 50000
            },
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions
            {
                Enabled = true,
                MaxConnectionsPerHub = 50001 // Too high (> 50000)
            },
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions { Enabled = true },
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions { Enabled = true },
            Security = new SignalrMapRealtimeOptions.SecurityOptions { EnableRateLimiting = true }
        };

        // Validate invalid configuration
        isValid = invalidConfig.Validate(out validationResults);
        Console.WriteLine($"Invalid configuration: {isValid}");
        foreach (var result in validationResults)
        {
            Console.WriteLine($"Expected validation error: {result.ErrorMessage}");
        }

        // Verify section name
        string sectionName = SignalrMapRealtimeOptions.SectionName;
        Console.WriteLine($"Configuration section name: {sectionName}");

        // Verify default values
        var defaultOptions = new SignalrMapRealtimeOptions();
        Console.WriteLine($"Default API version: {defaultOptions.AppInfo.ApiVersion}");
        Console.WriteLine($"Default environment: {defaultOptions.AppInfo.Environment}");
        Console.WriteLine($"Default performance metrics enabled: {defaultOptions.Performance.EnableDetailedMetrics}");
        Console.WriteLine($"Default SignalR hubs enabled: {defaultOptions.SignalRHubs.Enabled}");
    }
}
```
