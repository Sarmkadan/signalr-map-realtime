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
