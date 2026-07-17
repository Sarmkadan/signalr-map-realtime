// ... (rest of the file remains the same)

## LocationController

`LocationController` is an API controller that manages location data for vehicle tracking. It provides endpoints for creating, updating, retrieving, and deleting location records, demonstrating proper REST conventions and API response formatting.

### Usage Example

```csharp
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalRMapRealtime.DTOs;

class Program
{
    static async Task Main()
    {
        // Example using WebApplicationFactory for integration testing
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Create a new location
        var newLocation = new CreateLocationDto
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            VehicleId = 1
        };

        var createResponse = await client.PostAsJsonAsync("api/location", newLocation);
        var createdLocation = await createResponse.Content.ReadFromJsonAsync<ApiResponse<LocationDto>>();
        Console.WriteLine($"Created location: {createdLocation?.Data?.Id}");

        // Get all locations
        var getAllResponse = await client.GetAsync("api/location?pageNumber=1&pageSize=10");
        var allLocations = await getAllResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<LocationDto>>>();
        Console.WriteLine($"Total locations: {allLocations?.Data?.TotalCount}");

        // Get a specific location by ID
        var getByIdResponse = await client.GetAsync($"api/location/{createdLocation?.Data?.Id}");
        var singleLocation = await getByIdResponse.Content.ReadFromJsonAsync<ApiResponse<LocationDto>>();
        Console.WriteLine($"Location: {singleLocation?.Data?.Latitude}, {singleLocation?.Data?.Longitude}");

        // Update a location
        var updateLocation = new UpdateLocationDto
        {
            Latitude = 37.7859,
            Longitude = -122.4364
        };
        var updateResponse = await client.PutAsJsonAsync($"api/location/{createdLocation?.Data?.Id}", updateLocation);
        var updatedLocation = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<LocationDto>>();
        Console.WriteLine($"Updated location: {updatedLocation?.Data?.Latitude}, {updatedLocation?.Data?.Longitude}");

        // Delete a location
        var deleteResponse = await client.DeleteAsync($"api/location/{createdLocation?.Data?.Id}");
        Console.WriteLine($"Delete status: {deleteResponse.StatusCode}");
    }
}
```
