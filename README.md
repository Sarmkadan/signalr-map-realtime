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
