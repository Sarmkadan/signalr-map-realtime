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
using System.Threading.Tasks;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Example client for route optimization and management.
/// Creates routes, calculates distances, and tracks route progress.
/// </summary>
public class RouteOptimizationClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    private class Waypoint
    {
        public int Order { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public RouteOptimizationClient(string baseUrl, string apiKey)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Creates an optimized delivery route with multiple stops.
    /// </summary>
    public async Task CreateDeliveryRoute(string vehicleId)
    {
        Console.WriteLine("=== Route Optimization Example ===\n");

        var waypoints = new[]
        {
            new Waypoint { Order = 1, Latitude = 40.7128, Longitude = -74.0060, Name = "Warehouse (Start)" },
            new Waypoint { Order = 2, Latitude = 40.7489, Longitude = -73.9680, Name = "Delivery 1" },
            new Waypoint { Order = 3, Latitude = 40.7614, Longitude = -73.9776, Name = "Delivery 2" },
            new Waypoint { Order = 4, Latitude = 40.7282, Longitude = -73.7949, Name = "Delivery 3" },
            new Waypoint { Order = 5, Latitude = 40.6892, Longitude = -74.0445, Name = "Warehouse (End)" }
        };

        var totalDistance = CalculateRouteDistance(waypoints);

        Console.WriteLine("Creating route with following stops:");
        foreach (var wp in waypoints)
        {
            Console.WriteLine($"  {wp.Order}. {wp.Name} ({wp.Latitude:F4}, {wp.Longitude:F4})");
        }

        var route = new
        {
            name = $"Delivery Route {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            vehicleId = vehicleId,
            status = "Pending",
            totalDistance = totalDistance,
            estimatedDuration = (int)(totalDistance / 30 * 60), // Assume 30 km/h average
            notes = "Automated delivery route with 3 stops"
        };

        var json = JsonSerializer.Serialize(route);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/routes", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(result);
            var routeId = doc.RootElement.GetProperty("id").GetString();

            Console.WriteLine($"\n✓ Route created: {routeId}");
            Console.WriteLine($"  Total Distance: {totalDistance:F2} km");
            Console.WriteLine($"  Estimated Duration: {route.estimatedDuration} minutes");

            await GetRouteDetails(routeId ?? "");
        }
        else
        {
            Console.Error.WriteLine("Failed to create route");
        }
    }

    /// <summary>
    /// Retrieves and displays detailed route information.
    /// </summary>
    private async Task GetRouteDetails(string routeId)
    {
        if (string.IsNullOrEmpty(routeId))
            return;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/routes/{routeId}");
        request.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return;

        var result = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(result);
        var route = doc.RootElement;

        Console.WriteLine("\nRoute Details:");
        Console.WriteLine($"  Status: {route.GetProperty("status").GetString()}");
        Console.WriteLine($"  Created: {route.GetProperty("createdAt").GetString()}");
    }

    /// <summary>
    /// Calculates total distance for a route using Haversine formula.
    /// </summary>
    private double CalculateRouteDistance(Waypoint[] waypoints)
    {
        double totalDistance = 0;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            var distance = CalculateDistance(
                waypoints[i].Latitude, waypoints[i].Longitude,
                waypoints[i + 1].Latitude, waypoints[i + 1].Longitude
            );
            totalDistance += distance;
        }

        return totalDistance;
    }

    /// <summary>
    /// Calculates distance between two coordinates (Haversine formula).
    /// </summary>
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Optimizes route waypoint order using a simple nearest-neighbor algorithm.
    /// </summary>
    public List<Waypoint> OptimizeWaypointOrder(List<Waypoint> waypoints)
    {
        if (waypoints.Count <= 2)
            return waypoints;

        var optimized = new List<Waypoint> { waypoints[0] }; // Start point
        var remaining = waypoints.Skip(1).ToList();

        while (remaining.Count > 0)
        {
            var current = optimized.Last();
            var nearest = remaining.OrderBy(w =>
                CalculateDistance(current.Latitude, current.Longitude, w.Latitude, w.Longitude)
            ).First();

            optimized.Add(nearest);
            remaining.Remove(nearest);
        }

        // Update order numbers
        for (int i = 0; i < optimized.Count; i++)
        {
            optimized[i].Order = i + 1;
        }

        return optimized;
    }

    /// <summary>
    /// Simulates route execution and progress tracking.
    /// </summary>
    public async Task SimulateRouteExecution(string routeId)
    {
        Console.WriteLine("\nSimulating route execution...");

        var steps = new[] { 25, 50, 75, 100 };
        var statuses = new[] { "In Progress", "In Progress", "In Progress", "Completed" };

        for (int i = 0; i < steps.Length; i++)
        {
            Console.WriteLine($"  [{steps[i]}%] {statuses[i]}");
            await Task.Delay(1000);
        }

        Console.WriteLine("\n✓ Route execution simulation completed");
    }

    /// <summary>
    /// Runs the route optimization example.
    /// </summary>
    public async Task RunExample(string vehicleId)
    {
        try
        {
            await CreateDeliveryRoute(vehicleId);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Main entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var baseUrl = args.Length > 0 ? args[0] : "https://localhost:5001";
        var apiKey = args.Length > 1 ? args[1] : "default-api-key";
        var vehicleId = args.Length > 2 ? args[2] : Guid.NewGuid().ToString();

        var client = new RouteOptimizationClient(baseUrl, apiKey);
        await client.RunExample(vehicleId);
    }
}
