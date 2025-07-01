// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Example client demonstrating basic vehicle tracking operations.
/// Creates vehicles, records locations, and retrieves tracking data.
/// </summary>
public class VehicleTrackerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public VehicleTrackerClient(string baseUrl, string apiKey)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task RunExample()
    {
        Console.WriteLine("=== Vehicle Tracker Client Example ===\n");

        try
        {
            var vehicleId = await CreateVehicle();
            await RecordLocations(vehicleId);
            await RetrieveLocationHistory(vehicleId);
            await GetVehicleDetails(vehicleId);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new vehicle in the system.
    /// </summary>
    private async Task<string> CreateVehicle()
    {
        Console.WriteLine("1. Creating Vehicle...");

        var vehicle = new
        {
            licensePlate = $"DEMO-{Guid.NewGuid().ToString().Substring(0, 8)}",
            manufacturer = "Toyota",
            model = "Camry",
            year = 2024,
            vin = "1234567890ABCDEF0"
        };

        var json = JsonSerializer.Serialize(vehicle);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/vehicles", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create vehicle: {error}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(result);
        var vehicleId = doc.RootElement.GetProperty("id").GetString();

        Console.WriteLine($"✓ Vehicle created: {vehicle.licensePlate} (ID: {vehicleId})\n");
        return vehicleId ?? throw new InvalidOperationException("No vehicle ID returned");
    }

    /// <summary>
    /// Records multiple location updates for a vehicle.
    /// </summary>
    private async Task RecordLocations(string vehicleId)
    {
        Console.WriteLine("2. Recording Location Updates...");

        var locations = new[]
        {
            (40.7128, -74.0060), // NYC Times Square
            (40.7489, -73.9680), // Midtown
            (40.7614, -73.9776), // Central Park
            (40.7282, -73.7949)  // Queens
        };

        foreach (var (lat, lon) in locations)
        {
            var location = new
            {
                vehicleId = vehicleId,
                latitude = lat,
                longitude = lon,
                accuracy = 5.0,
                speed = 35.5,
                heading = 90,
                type = "GPS",
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(location);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-API-Key", _apiKey);

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/locations", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"  ✓ Location recorded: ({lat}, {lon})");
            }

            await Task.Delay(500); // Small delay between updates
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Retrieves location history for a vehicle.
    /// </summary>
    private async Task RetrieveLocationHistory(string vehicleId)
    {
        Console.WriteLine("3. Retrieving Location History...");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/api/v1/locations/vehicle/{vehicleId}?take=10"
        );
        request.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to retrieve locations");
            return;
        }

        var result = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(result);
        var data = doc.RootElement.GetProperty("data");

        foreach (var location in data.EnumerateArray())
        {
            var lat = location.GetProperty("latitude").GetDouble();
            var lon = location.GetProperty("longitude").GetDouble();
            var timestamp = location.GetProperty("timestamp").GetString();

            Console.WriteLine($"  • {lat:F4}, {lon:F4} @ {timestamp}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Gets detailed information about a vehicle.
    /// </summary>
    private async Task GetVehicleDetails(string vehicleId)
    {
        Console.WriteLine("4. Getting Vehicle Details...");

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/vehicles/{vehicleId}");
        request.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to retrieve vehicle details");
            return;
        }

        var result = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(result);
        var vehicle = doc.RootElement;

        Console.WriteLine($"  License Plate: {vehicle.GetProperty("licensePlate").GetString()}");
        Console.WriteLine($"  Model: {vehicle.GetProperty("model").GetString()}");
        Console.WriteLine($"  Status: {vehicle.GetProperty("status").GetString()}");
        Console.WriteLine($"  Created: {vehicle.GetProperty("createdAt").GetString()}");

        Console.WriteLine("\n✓ Example completed successfully!");
    }

    /// <summary>
    /// Main entry point for the example.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var baseUrl = args.Length > 0 ? args[0] : "https://localhost:5001";
        var apiKey = args.Length > 1 ? args[1] : "default-api-key";

        var client = new VehicleTrackerClient(baseUrl, apiKey);
        await client.RunExample();
    }
}
