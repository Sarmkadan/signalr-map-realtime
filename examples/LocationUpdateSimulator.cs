// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Simulates real-time GPS location updates for multiple vehicles.
/// Useful for testing, development, and load testing.
/// </summary>
public class LocationUpdateSimulator
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly Random _random = new();

    private readonly List<(double Latitude, double Longitude, string Name)> _routes = new()
    {
        (40.7128, -74.0060, "Times Square"),
        (40.7489, -73.9680, "Midtown"),
        (40.7614, -73.9776, "Central Park"),
        (40.7282, -73.7949, "Queens"),
        (40.6892, -74.0445, "Staten Island")
    };

    public LocationUpdateSimulator(string baseUrl, string apiKey)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task RunSimulation(int vehicleCount = 5, int durationSeconds = 60)
    {
        Console.WriteLine($"=== Location Update Simulator ===\n");
        Console.WriteLine($"Simulating {vehicleCount} vehicles for {durationSeconds} seconds...\n");

        var vehicleIds = new List<string>();

        try
        {
            vehicleIds = await CreateVehicles(vehicleCount);
            await SimulateMovement(vehicleIds, durationSeconds);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates multiple vehicles in the system.
    /// </summary>
    private async Task<List<string>> CreateVehicles(int count)
    {
        Console.WriteLine($"Creating {count} vehicles...\n");

        var vehicleIds = new List<string>();
        var manufacturers = new[] { "Toyota", "Ford", "BMW", "Mercedes", "Tesla" };
        var models = new[] { "Camry", "Transit", "X5", "E-Class", "Model 3" };

        for (int i = 0; i < count; i++)
        {
            var vehicle = new
            {
                licensePlate = $"DEMO-{i:D3}",
                manufacturer = manufacturers[i % manufacturers.Length],
                model = models[i % models.Length],
                year = 2024,
                vin = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 17)
            };

            var json = JsonSerializer.Serialize(vehicle);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-API-Key", _apiKey);

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/vehicles", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(result);
                var vehicleId = doc.RootElement.GetProperty("id").GetString();

                if (vehicleId != null)
                {
                    vehicleIds.Add(vehicleId);
                    Console.WriteLine($"✓ Created {vehicle.licensePlate}");
                }
            }
        }

        Console.WriteLine();
        return vehicleIds;
    }

    /// <summary>
    /// Simulates vehicle movement by sending periodic location updates.
    /// </summary>
    private async Task SimulateMovement(List<string> vehicleIds, int durationSeconds)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddSeconds(durationSeconds);
        var locationCount = 0;

        while (DateTime.UtcNow < endTime)
        {
            foreach (var vehicleId in vehicleIds)
            {
                var location = GenerateLocation(vehicleId);
                var success = await SendLocation(location);

                if (success)
                {
                    locationCount++;
                }
            }

            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            var remaining = Math.Max(0, durationSeconds - elapsed);
            Console.WriteLine($"[{elapsed:F0}s] Sent {locationCount} updates (remaining: {remaining:F0}s)");

            await Task.Delay(2000); // Update every 2 seconds
        }

        Console.WriteLine($"\n✓ Simulation completed: {locationCount} total location updates sent");
    }

    /// <summary>
    /// Generates a realistic location update with slight variations.
    /// </summary>
    private object GenerateLocation(string vehicleId)
    {
        var route = _routes[_random.Next(_routes.Count)];

        var latitude = route.Latitude + (_random.NextDouble() - 0.5) * 0.01;
        var longitude = route.Longitude + (_random.NextDouble() - 0.5) * 0.01;
        var speed = 20 + _random.Next(50); // 20-70 km/h
        var heading = _random.Next(360);
        var accuracy = 3 + _random.NextDouble() * 7; // 3-10 meters

        return new
        {
            vehicleId = vehicleId,
            latitude = latitude,
            longitude = longitude,
            accuracy = accuracy,
            speed = speed,
            heading = heading,
            type = "GPS",
            timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sends a location update to the API.
    /// </summary>
    private async Task<bool> SendLocation(object location)
    {
        try
        {
            var json = JsonSerializer.Serialize(location);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-API-Key", _apiKey);

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/locations", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Main entry point for the simulator.
    /// Usage: LocationUpdateSimulator.exe https://localhost:5001 api-key [vehicle-count] [duration-seconds]
    /// </summary>
    public static async Task Main(string[] args)
    {
        var baseUrl = args.Length > 0 ? args[0] : "https://localhost:5001";
        var apiKey = args.Length > 1 ? args[1] : "default-api-key";
        var vehicleCount = args.Length > 2 ? int.Parse(args[2]) : 5;
        var durationSeconds = args.Length > 3 ? int.Parse(args[3]) : 60;

        var simulator = new LocationUpdateSimulator(baseUrl, apiKey);
        await simulator.RunSimulation(vehicleCount, durationSeconds);
    }
}
