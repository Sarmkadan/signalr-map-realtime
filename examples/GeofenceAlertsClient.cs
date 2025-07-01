// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Example client demonstrating geofencing capabilities.
/// Listens for geofence entry/exit alerts and processes them.
/// </summary>
public class GeofenceAlertsClient
{
    private readonly HubConnection _connection;
    private readonly string _vehicleId;
    private readonly List<Geofence> _geofences;

    private class Geofence
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusKm { get; set; }
    }

    public GeofenceAlertsClient(string hubUrl, string vehicleId)
    {
        _vehicleId = vehicleId;
        _geofences = new List<Geofence>();

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        SetupEventHandlers();
    }

    /// <summary>
    /// Sets up SignalR event handlers for geofence alerts and other notifications.
    /// </summary>
    private void SetupEventHandlers()
    {
        _connection.On<Alert>("Alert", alert =>
        {
            Console.WriteLine($"\n🚨 ALERT: {alert.Type}");
            Console.WriteLine($"   Vehicle: {alert.VehicleId}");
            Console.WriteLine($"   Message: {alert.Message}");
            Console.WriteLine($"   Time: {DateTime.Now:HH:mm:ss}");

            switch (alert.Type)
            {
                case "GeofenceEntry":
                    HandleGeofenceEntry(alert);
                    break;
                case "GeofenceExit":
                    HandleGeofenceExit(alert);
                    break;
                case "SpeedViolation":
                    HandleSpeedViolation(alert);
                    break;
                case "OutOfService":
                    HandleOutOfService(alert);
                    break;
            }
        });

        _connection.On<LocationDto>("LocationUpdated", location =>
        {
            if (location.VehicleId == _vehicleId)
            {
                CheckGeofences(location);
            }
        });

        _connection.Reconnecting += error =>
        {
            Console.WriteLine("⚠ Reconnecting to hub...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            Console.WriteLine("✓ Reconnected to hub");
            return Task.CompletedTask;
        };

        _connection.Closed += error =>
        {
            Console.WriteLine("✗ Connection closed");
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Adds a geofence to monitor.
    /// </summary>
    public void AddGeofence(string name, double latitude, double longitude, double radiusKm)
    {
        var geofence = new Geofence
        {
            Name = name,
            Latitude = latitude,
            Longitude = longitude,
            RadiusKm = radiusKm
        };

        _geofences.Add(geofence);
        Console.WriteLine($"✓ Geofence added: {name} ({radiusKm}km radius at {latitude}, {longitude})");
    }

    /// <summary>
    /// Checks if vehicle is within any monitored geofences.
    /// </summary>
    private void CheckGeofences(LocationDto location)
    {
        foreach (var geofence in _geofences)
        {
            var distance = CalculateDistance(
                location.Latitude, location.Longitude,
                geofence.Latitude, geofence.Longitude
            );

            if (distance <= geofence.RadiusKm)
            {
                Console.WriteLine($"\n📍 Vehicle is within geofence: {geofence.Name}");
                Console.WriteLine($"   Distance: {distance:F2}km");
            }
        }
    }

    /// <summary>
    /// Handles geofence entry alerts.
    /// </summary>
    private void HandleGeofenceEntry(Alert alert)
    {
        Console.WriteLine("→ Vehicle ENTERED restricted area");
        Console.WriteLine("→ Initiating location tracking...");
        Console.WriteLine("→ Notifying fleet manager...");
    }

    /// <summary>
    /// Handles geofence exit alerts.
    /// </summary>
    private void HandleGeofenceExit(Alert alert)
    {
        Console.WriteLine("← Vehicle LEFT restricted area");
        Console.WriteLine("← Recording exit time...");
        Console.WriteLine("← Updating route status...");
    }

    /// <summary>
    /// Handles speed violation alerts.
    /// </summary>
    private void HandleSpeedViolation(Alert alert)
    {
        Console.WriteLine("⚡ Speed violation detected");
        Console.WriteLine("⚡ Recording incident...");
        Console.WriteLine("⚡ Notifying driver...");
    }

    /// <summary>
    /// Handles out-of-service alerts (communication loss).
    /// </summary>
    private void HandleOutOfService(Alert alert)
    {
        Console.WriteLine("📡 Vehicle out of service");
        Console.WriteLine("📡 Last known location being monitored...");
        Console.WriteLine("📡 Escalating to fleet manager...");
    }

    /// <summary>
    /// Calculates distance between two coordinates (Haversine formula).
    /// Returns distance in kilometers.
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
    /// Connects to the SignalR hub.
    /// </summary>
    public async Task Connect()
    {
        try
        {
            await _connection.StartAsync();
            Console.WriteLine("✓ Connected to SignalR hub");

            await _connection.InvokeAsync("SubscribeToVehicle", _vehicleId);
            Console.WriteLine($"✓ Subscribed to vehicle: {_vehicleId}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Disconnects from the SignalR hub.
    /// </summary>
    public async Task Disconnect()
    {
        try
        {
            await _connection.InvokeAsync("UnsubscribeFromVehicle", _vehicleId);
            await _connection.StopAsync();
            Console.WriteLine("✓ Disconnected from hub");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Disconnection error: {ex.Message}");
        }
    }

    /// <summary>
    /// Runs the geofence monitoring example.
    /// </summary>
    public async Task RunExample()
    {
        Console.WriteLine("=== Geofence Alerts Client ===\n");

        await Connect();

        AddGeofence("Warehouse", 40.7128, -74.0060, 1.0);
        AddGeofence("Office", 40.7489, -73.9680, 0.5);
        AddGeofence("Restricted Zone", 40.7614, -73.9776, 2.0);

        Console.WriteLine("\n✓ Listening for geofence alerts (press Enter to exit)...\n");
        Console.ReadLine();

        await Disconnect();
    }

    private class Alert
    {
        public string Type { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    private class LocationDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// Main entry point for the geofence alerts client.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var hubUrl = args.Length > 0 ? args[0] : "https://localhost:5001/locationHub";
        var vehicleId = args.Length > 1 ? args[1] : Guid.NewGuid().ToString();

        var client = new GeofenceAlertsClient(hubUrl, vehicleId);
        await client.RunExample();
    }
}
