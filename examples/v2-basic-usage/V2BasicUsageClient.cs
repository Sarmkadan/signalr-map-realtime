#nullable enable
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Examples.V2BasicUsage
{
    /// <summary>
    /// A minimal example demonstrating v2.0 features including real-time route tracking and historical playback.
    /// </summary>
    public class V2BasicUsageClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public V2BasicUsageClient(string baseUrl = "https://localhost:5001")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Demonstrates basic v2.0 workflow: create vehicle, start tracking, and retrieve route history.
        /// </summary>
        public async Task RunBasicWorkflowAsync()
        {
            Console.WriteLine("Starting v2.0 basic usage example...");

            // 1. Create a vehicle for tracking
            var vehicleDto = new VehicleDto
            {
                LicensePlate = "DEMO-001",
                Manufacturer = "DemoCorp",
                Model = "DemoVehicle",
                Year = 2024,
                Status = "Available"
            };

            var vehicleResponse = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/api/v1/vehicles",
                vehicleDto
            );

            vehicleResponse.EnsureSuccessStatusCode();
            var vehicleId = await vehicleResponse.Content.ReadFromJsonAsync<string>();
            Console.WriteLine($"Created vehicle with ID: {vehicleId}");

            // 2. Start a tracking session
            var sessionDto = new TrackingSessionDto
            {
                VehicleId = vehicleId,
                SessionName = "Demo Route",
                StartTime = DateTime.UtcNow,
                Status = "Active"
            };

            var sessionResponse = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/api/v1/tracking/sessions",
                sessionDto
            );

            sessionResponse.EnsureSuccessStatusCode();
            var sessionId = await sessionResponse.Content.ReadFromJsonAsync<string>();
            Console.WriteLine($"Started tracking session: {sessionId}");

            // 3. Simulate sending location updates (in a real app, this would come from GPS)
            var locations = new[]
            {
                new LocationDto { VehicleId = vehicleId, Latitude = 40.7128, Longitude = -74.0060, Accuracy = 5.0, Speed = 0, Heading = 0 },
                new LocationDto { VehicleId = vehicleId, Latitude = 40.7130, Longitude = -74.0058, Accuracy = 5.0, Speed = 15, Heading = 45 },
                new LocationDto { VehicleId = vehicleId, Latitude = 40.7135, Longitude = -74.0055, Accuracy = 5.0, Speed = 25, Heading = 90 },
                new LocationDto { VehicleId = vehicleId, Latitude = 40.7140, Longitude = -74.0050, Accuracy = 5.0, Speed = 20, Heading = 135 },
                new LocationDto { VehicleId = vehicleId, Latitude = 40.7145, Longitude = -74.0045, Accuracy = 5.0, Speed = 10, Heading = 180 }
            };

            foreach (var location in locations)
            {
                var locationResponse = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/v1/locations",
                    location
                );

                locationResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Sent location update: ({location.Latitude}, {location.Longitude})");

                // Simulate time between updates
                await Task.Delay(1000);
            }

            // 4. Complete the tracking session
            var completeSessionDto = new TrackingSessionDto
            {
                Id = sessionId,
                VehicleId = vehicleId,
                SessionName = "Demo Route",
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow,
                Status = "Completed",
                TotalDistance = 1.2, // kilometers
                AverageSpeed = 15.0, // km/h
                MaxSpeed = 25.0, // km/h
                LocationCount = locations.Length
            };

            var completeResponse = await _httpClient.PutAsJsonAsync(
                $"{_baseUrl}/api/v1/tracking/sessions/{sessionId}/complete",
                completeSessionDto
            );

            completeResponse.EnsureSuccessStatusCode();
            Console.WriteLine("Completed tracking session");

            // 5. Retrieve route playback/history
            var historyResponse = await _httpClient.GetFromJsonAsync<PaginatedResponse<LocationDto>>(
                $"{_baseUrl}/api/v1/locations/vehicle/{vehicleId}?limit=100"
            );

            Console.WriteLine($"Retrieved {historyResponse.Data.Count} location points for playback");

            // Display the route
            Console.WriteLine("\nRoute Playback:");
            foreach (var loc in historyResponse.Data)
            {
                Console.WriteLine($"  [{loc.Timestamp:HH:mm:ss}] ({loc.Latitude:F4}, {loc.Longitude:F4}) - {loc.Speed} km/h");
            }

            Console.WriteLine("\nBasic v2.0 usage example completed!");
        }
    }
}