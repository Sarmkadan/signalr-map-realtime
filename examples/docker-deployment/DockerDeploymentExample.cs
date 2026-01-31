using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Examples.DockerDeployment
{
    /// <summary>
    /// Example demonstrating deployment and usage of signalr-map-realtime with Docker.
    /// This shows how to interact with the API once deployed in Docker containers.
    /// </summary>
    public class DockerDeploymentExample
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DockerDeploymentExample(string baseUrl = "http://localhost:5000")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Demonstrates basic API operations when running in Docker.
        /// </summary>
        public async Task RunDockerExampleAsync()
        {
            Console.WriteLine("Starting Docker deployment example...");
            Console.WriteLine($"Connecting to API at: {_baseUrl}");

            try
            {
                // 1. Check API health
                Console.WriteLine("\n1. Checking API health...");
                var healthResponse = await _httpClient.GetAsync($"{_baseUrl}/health");
                healthResponse.EnsureSuccessStatusCode();
                var health = await healthResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Health check: {health}");

                // 2. Check API info
                Console.WriteLine("\n2. Checking API information...");
                var infoResponse = await _httpClient.GetAsync($"{_baseUrl}/api/info");
                infoResponse.EnsureSuccessStatusCode();
                var info = await infoResponse.Content.ReadFromJsonAsync<ApiInfo>();
                Console.WriteLine($"API Version: {info?.Version}");
                Console.WriteLine($"Environment: {info?.Environment}");

                // 3. List existing vehicles (if any)
                Console.WriteLine("\n3. Listing existing vehicles...");
                var vehiclesResponse = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles?take=10");
                if (vehiclesResponse.IsSuccessStatusCode)
                {
                    var vehicles = await vehiclesResponse.Content.ReadFromJsonAsync<PaginatedResponse<VehicleDto>>();
                    Console.WriteLine($"Found {vehicles?.Data.Count ?? 0} vehicles in the system");
                }
                else
                {
                    Console.WriteLine("No vehicles found or error retrieving vehicles");
                }

                // 4. Create a test vehicle
                Console.WriteLine("\n4. Creating a test vehicle...");
                var testVehicle = new VehicleDto
                {
                    LicensePlate = "DOCKER-001",
                    Manufacturer = "TestCorp",
                    Model = "TestVan",
                    Year = 2024,
                    Status = "Available"
                };

                var createResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/vehicles", testVehicle);
                createResponse.EnsureSuccessStatusCode();
                var vehicleId = await createResponse.Content.ReadFromJsonAsync<string>();
                Console.WriteLine($"Created vehicle: {vehicleId}");

                // 5. Record a location update
                Console.WriteLine("\n5. Recording a location update...");
                var location = new LocationDto
                {
                    VehicleId = vehicleId,
                    Latitude = 40.7128,
                    Longitude = -74.0060,
                    Accuracy = 5.0,
                    Speed = 25.5,
                    Heading = 90,
                    Timestamp = DateTime.UtcNow
                };

                var locationResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/locations", location);
                locationResponse.EnsureSuccessStatusCode();
                var locationId = await locationResponse.Content.ReadFromJsonAsync<string>();
                Console.WriteLine($"Recorded location: {locationId}");

                // 6. Retrieve the location
                Console.WriteLine("\n6. Retrieving location...");
                var getLocationResponse = await _httpClient.GetAsync($"{_baseUrl}/api/v1/locations/{locationId}");
                getLocationResponse.EnsureSuccessStatusCode();
                var retrievedLocation = await getLocationResponse.Content.ReadFromJsonAsync<LocationDto>();
                Console.WriteLine($"Retrieved location: Vehicle {retrievedLocation?.VehicleId} at ({retrievedLocation?.Latitude}, {retrievedLocation?.Longitude})");

                // 7. List vehicles again to see our new vehicle
                Console.WriteLine("\n7. Listing vehicles after creation...");
                var updatedVehiclesResponse = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles?take=10");
                if (updatedVehiclesResponse.IsSuccessStatusCode)
                {
                    var updatedVehicles = await updatedVehiclesResponse.Content.ReadFromJsonAsync<PaginatedResponse<VehicleDto>>();
                    var newVehicle = updatedVehicles?.Data.Find(v => v.Id == vehicleId);
                    Console.WriteLine($"Vehicle found: {newVehicle?.LicensePlate} - Status: {newVehicle?.Status}");
                }

                Console.WriteLine("\n✅ Docker deployment example completed successfully!");
                Console.WriteLine("\nTips for production Docker deployment:");
                Console.WriteLine("- Use proper HTTPS configuration");
                Console.WriteLine("- Configure authentication (API keys, JWT)");
                Console.WriteLine("- Set up monitoring and logging");
                Console.WriteLine("- Configure database backups");
                Console.WriteLine("- Use Redis backplane for SignalR in production");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error during Docker example: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine("\nMake sure Docker containers are running:");
                Console.WriteLine("  docker-compose ps");
                Console.WriteLine("\nCheck container logs:");
                Console.WriteLine("  docker-compose logs signalr-map-realtime");
            }
        }

        private class ApiInfo
        {
            public string Version { get; set; }
            public string Environment { get; set; }
        }
    }

    public class VehicleDto
    {
        public string Id { get; set; }
        public string LicensePlate { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
    }

    public class LocationDto
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public System.Collections.Generic.List<T> Data { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
