using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// NOTE: Ensure your server is running before executing this example.
// Configure the baseUrl accordingly.

namespace SignalRMapRealtime.Examples
{
    public class BasicUsage
    {
        public static async Task RunAsync()
        {
            var client = new HttpClient();
            var baseUrl = "https://localhost:5001"; // Update with your actual server URL

            Console.WriteLine("Creating a vehicle...");
            
            // 1. Create a vehicle
            var vehicle = new { 
                licensePlate = "XYZ-789",
                manufacturer = "Ford",
                model = "Transit",
                year = 2024,
                status = "Active"
            };

            var response = await client.PostAsJsonAsync($"{baseUrl}/api/v1/vehicles", vehicle);
            response.EnsureSuccessStatusCode();
            var vehicleId = await response.Content.ReadAsStringAsync(); // Assume API returns the ID directly

            Console.WriteLine($"Vehicle created with ID: {vehicleId}");

            // 2. Simulate sending a location update
            Console.WriteLine("Sending location update...");
            
            var location = new {
                vehicleId = vehicleId.Trim('"'), // Cleanup JSON string quotes
                latitude = 40.7128,
                longitude = -74.0060,
                accuracy = 10.0,
                speed = 35.5,
                heading = 90,
                timestamp = DateTime.UtcNow
            };

            var locationResponse = await client.PostAsJsonAsync($"{baseUrl}/api/v1/locations", location);
            locationResponse.EnsureSuccessStatusCode();
            
            Console.WriteLine("Location update successful.");
        }
    }
}
