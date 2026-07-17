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
