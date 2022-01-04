# Advanced Configuration Example

This example demonstrates complex configuration scenarios for signalr-map-realtime v2.0, including:
- Multi-vehicle tracking with sessions
- Route playback with timeline navigation
- Geofence monitoring
- Bulk operations
- Performance optimization

## Prerequisites

- .NET SDK 10.0 or later
- SQL Server 2019+ or PostgreSQL
- Redis (optional, for caching and SignalR backplane)

## Scenario: Fleet Management System

This example simulates a fleet management system tracking multiple delivery vehicles with route optimization and geofence monitoring.

## Project Structure

```
examples/advanced-configuration/
├── AdvancedFleetManagement.cs         # Main example code
├── FleetManagementConfig.cs            # Configuration settings
├── Models/                           # Custom DTOs and models
│   ├── FleetVehicle.cs
│   ├── DeliveryRoute.cs
│   └── GeofenceZone.cs
├── Services/                         # Helper services
│   ├── FleetService.cs
│   └── AnalyticsService.cs
└── README.md                         # This file
```

## Running the Example

### 1. Set Up Configuration

Create `appsettings.Development.json` in the example project:

```json
{
  "BaseUrl": "https://localhost:5001",
  "DatabaseConnection": "Server=(localdb)\\mssqllocaldb;Database=SignalRMapRealtimeDb;...",
  "RedisConnection": "localhost:6379",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 2. Build and Run

```bash
# Navigate to the example directory
cd examples/advanced-configuration

# Restore packages
dotnet restore

# Build
 dotnet build

# Run the example
 dotnet run
```

### 3. Expected Output

The example will:
1. Create multiple vehicles with different types
2. Set up geofence zones
3. Simulate route tracking with historical playback
4. Demonstrate bulk location imports
5. Show analytics and reporting
6. Handle errors and edge cases

## Code Walkthrough


### Step 1: Configuration Setup

```csharp
// FleetManagementConfig.cs
public class FleetManagementConfig
{
    public string BaseUrl { get; set; } = "https://localhost:5001";
    public string DatabaseConnection { get; set; }
    public string RedisConnection { get; set; }
    public bool EnableCaching { get; set; } = true;
    public int BulkBatchSize { get; set; } = 100;
    public int MaxConcurrentVehicles { get; set; } = 50;
}
```


### Step 2: Fleet Vehicle Model


```csharp
// Models/FleetVehicle.cs
public class FleetVehicle
{
    public string Id { get; set; }
    public string LicensePlate { get; set; }
    public VehicleType Type { get; set; }
    public string DriverName { get; set; }
    public string DriverPhone { get; set; }
    public double FuelCapacity { get; set; } // liters
    public double CurrentFuelLevel { get; set; }
    public string Status { get; set; }
    public DateTime LastMaintenanceDate { get; set; }
    public List<VehicleFeature> Features { get; set; }
}

public enum VehicleType
{
    Van,
    Truck,
    Motorcycle,
    ElectricVehicle
}

public enum VehicleFeature
{
    Refrigeration,
    GPS,
    TemperatureMonitoring,
    HazardousMaterials
}
```


### Step 3: Geofence Zone Setup

```csharp
// Models/GeofenceZone.cs
public class GeofenceZone
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; }
    public GeofenceType Type { get; set; }
    public string[] AllowedVehicleTypes { get; set; }
}

public enum GeofenceType
{
    Restricted,    // Entry/exit triggers alerts
    DeliveryZone,   // Only delivery vehicles allowed
    ServiceArea,    // Service vehicles only
    CustomerSite     // Specific customer locations
}
```

### Step 4: Delivery Route Planning


```csharp
// Models/DeliveryRoute.cs
public class DeliveryRoute
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string VehicleId { get; set; }
    public string DriverId { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public DateTime PlannedEndTime { get; set; }
    public List<Waypoint> Waypoints { get; set; }
    public double PlannedDistanceKm { get; set; }
    public int PlannedStops { get; set; }
    public RouteStatus Status { get; set; }
    public string[] RequiredFeatures { get; set; }
}

public class Waypoint
{
    public int Order { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; }
    public string CustomerName { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    public DateTime? ActualArrivalTime { get; set; }
    public string Status { get; set; }
}

public enum RouteStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled
}
```

### Step 5: Main Fleet Management Service

```csharp
// Services/FleetService.cs
public class FleetService
{
    private readonly HttpClient _httpClient;
    private readonly FleetManagementConfig _config;
    private readonly List<FleetVehicle> _vehicles = new();
    private readonly List<GeofenceZone> _geofences = new();
    private readonly List<DeliveryRoute> _routes = new();

    public FleetService(FleetManagementConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        InitializeAsync().Wait();
    }

    private async Task InitializeAsync()
    {
        // Create sample fleet
        await CreateSampleFleetAsync();
        
        // Create geofence zones
        await CreateGeofenceZonesAsync();
        
        // Create delivery routes
        await CreateDeliveryRoutesAsync();
    }

    public async Task RunFleetOperationsAsync()
    {
        // Monitor vehicles in real-time
        await MonitorFleetAsync();
        
        // Handle route completion
        await CompleteRoutesAsync();
        
        // Generate reports
        await GenerateReportsAsync();
    }

    // Additional methods for vehicle tracking, geofence checks, etc.
}
```

### Step 6: Advanced Operations

#### Bulk Location Import

```csharp
public async Task ImportHistoricalDataAsync(List<LocationDto> historicalLocations)
{
    var client = new HttpClient();
    
    // Batch locations for better performance
    foreach (var batch in historicalLocations.Batch(_config.BulkBatchSize))
    {
        try
        {
            var response = await client.PostAsJsonAsync(
                $"{_config.BaseUrl}/api/v1/locations/bulk",
                batch
            );
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Batch failed: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing batch: {ex.Message}");
        }
    }
}
```

#### Geofence Monitoring

```csharp
public async Task CheckGeofenceAsync(LocationDto location)
{
    var nearbyGeofences = _geofences
        .Where(g => GeoHelper.Distance(
            g.Latitude, g.Longitude,
            location.Latitude, location.Longitude
        ) <= g.RadiusKm)
        .ToList();
    
    foreach (var geofence in nearbyGeofences)
    {
        // Check if vehicle is allowed in this zone
        var vehicle = _vehicles.FirstOrDefault(v => v.Id == location.VehicleId);
        
        if (vehicle != null && !geofence.AllowedVehicleTypes.Contains(vehicle.Type.ToString()))
        {
            Console.WriteLine($"⚠️ Vehicle {vehicle.LicensePlate} entered restricted zone: {geofence.Name}");
            
            // Send alert via SignalR or webhook
            await SendGeofenceAlertAsync(location.VehicleId, geofence.Name, "Entry");
        }
    }
}
```

#### Route Playback with Timeline

```csharp
public async Task PlaybackRouteAsync(string routeId, DateTime startTime, DateTime endTime)
{
    // Get route details
    var route = _routes.FirstOrDefault(r => r.Id == routeId);
    
    if (route == null)
    {
        Console.WriteLine("Route not found");
        return;
    }
    
    // Get location history for the vehicle during route time window
    var history = await _httpClient.GetFromJsonAsync<List<LocationDto>>(
        $"{_config.BaseUrl}/api/v1/locations/vehicle/{route.VehicleId}?startDate={startTime:o}&endDate={endTime:o}"
    );
    
    // Sort by timestamp
    var sortedLocations = history.OrderBy(l => l.Timestamp).ToList();
    
    Console.WriteLine($"Route Playback: {route.Name}");
    Console.WriteLine($"Vehicle: {route.VehicleId}");
    Console.WriteLine($"Duration: {(endTime - startTime).TotalMinutes} minutes");
    Console.WriteLine($"Total distance: {route.PlannedDistanceKm} km");
    
    // Display timeline
    foreach (var location in sortedLocations)
    {
        var progress = (location.Timestamp - startTime).TotalSeconds / 
                       (endTime - startTime).TotalSeconds * 100;
        
        Console.WriteLine($"[{location.Timestamp:HH:mm:ss}] " +
                        $"Progress: {progress:F1}% - " +
                        $"({location.Latitude:F4}, {location.Longitude:F4}) - " +
                        $"Speed: {location.Speed} km/h");
        
        await Task.Delay(100); // Simulate playback speed
    }
}
```

### Step 7: Analytics and Reporting

```csharp
// Services/AnalyticsService.cs
public class AnalyticsService
{
    public async Task<FleetAnalytics> GenerateFleetAnalyticsAsync(DateTime from, DateTime to)
    {
        var analytics = new FleetAnalytics
        {
            PeriodStart = from,
            PeriodEnd = to,
            TotalVehicles = 0,
            ActiveVehicles = 0,
            TotalDistanceKm = 0,
            TotalDurationHours = 0,
            AverageSpeed = 0,
            MaxSpeed = 0,
            FuelConsumption = 0,
            GeofenceViolations = 0,
            CompletedRoutes = 0,
            AverageRouteDuration = TimeSpan.Zero
        };
        
        // Query API for analytics data
        var response = await _httpClient.GetFromJsonAsync<AnalyticsResponse>(
            $"{_config.BaseUrl}/api/v1/analytics/fleet?from={from:o}&to={to:o}"
        );
        
        // Process and return analytics
        return analytics;
    }
}

public class FleetAnalytics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalVehicles { get; set; }
    public int ActiveVehicles { get; set; }
    public double TotalDistanceKm { get; set; }
    public double TotalDurationHours { get; set; }
    public double AverageSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double FuelConsumption { get; set; }
    public int GeofenceViolations { get; set; }
    public int CompletedRoutes { get; set; }
    public TimeSpan AverageRouteDuration { get; set; }
}
```

### Step 8: Error Handling and Retry Logic

```csharp
public async Task<bool> SafeApiCallAsync(Func<Task<HttpResponseMessage>> apiCall, int maxRetries = 3)
{
    int attempt = 0;
    
    while (attempt < maxRetries)
    {
        try
        {
            var response = await apiCall();
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            // Log error and retry
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Attempt {attempt + 1} failed: {errorContent}");
            
            attempt++;
            await Task.Delay(1000 * attempt); // Exponential backoff
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Attempt {attempt + 1} failed with exception: {ex.Message}");
            attempt++;
            await Task.Delay(1000 * attempt);
        }
    }
    
    return false;
}
```

## Advanced Configuration Scenarios

### Scenario 1: Multi-Tenant Fleet Management

```csharp
// Configure for multiple customers
public class TenantConfig
{
    public string TenantId { get; set; }
    public string ConnectionString { get; set; }
    public string[] AllowedVehicleTypes { get; set; }
    public string[] GeofenceIds { get; set; }
}

// In your service:
var tenantConfigs = new[]
{
    new TenantConfig { TenantId = "customer-a", ConnectionString = "...", ... },
    new TenantConfig { TenantId = "customer-b", ConnectionString = "...", ... }
};

foreach (var config in tenantConfigs)
{
    var fleetService = new FleetService(config);
    await fleetService.RunFleetOperationsAsync();
}
```

### Scenario 2: Real-time Monitoring Dashboard

```csharp
// Connect to SignalR hub for real-time updates
public class RealTimeMonitor
{
    private HubConnection _connection;
    
    public async Task StartMonitoringAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_config.BaseUrl}/locationHub")
            .WithAutomaticReconnect()
            .Build();
        
        _connection.On<LocationDto>("LocationUpdated", location =>
        {
            Console.WriteLine($"Real-time update: Vehicle {location.VehicleId} at ({location.Latitude}, {location.Longitude})");
            
            // Update dashboard
            UpdateDashboard(location);
        });
        
        _connection.On<AlertDto>("Alert", alert =>
        {
            Console.WriteLine($"🚨 Alert: {alert.Type} - {alert.Message}");
            
            // Handle alert
            HandleAlert(alert);
        });
        
        await _connection.StartAsync();
    }
}
```

### Scenario 3: Route Optimization Integration

```csharp
// Integrate with external route optimization service
public async Task OptimizeRouteAsync(string routeId)
{
    // Get current route
    var route = await _httpClient.GetFromJsonAsync<DeliveryRoute>(
        $"{_config.BaseUrl}/api/v1/routes/{routeId}"
    );
    
    // Call external optimization API
    var optimizationRequest = new RouteOptimizationRequest
    {
        Waypoints = route.Waypoints.Select(w => new OptimizationWaypoint
        {
            Latitude = w.Latitude,
            Longitude = w.Longitude,
            TimeWindowStart = w.EstimatedArrivalTime,
            TimeWindowEnd = w.EstimatedArrivalTime.AddMinutes(30)
        }).ToList(),
        VehicleType = route.RequiredFeatures.Contains("Refrigeration") ? "Refrigerated" : "Standard",
        Constraints = new OptimizationConstraints
        {
            MaxDistance = route.PlannedDistanceKm * 1.2,
            MaxDuration = (route.PlannedEndTime - route.PlannedStartTime).TotalMinutes * 1.2
        }
    };
    
    var optimizationResponse = await _optimizationClient.PostAsJsonAsync(
        "https://route-optimizer.example.com/api/optimize",
        optimizationRequest
    );
    
    var optimizedRoute = await optimizationResponse.Content.ReadFromJsonAsync<OptimizedRoute>();
    
    // Update route with optimized waypoints
    route.Waypoints = optimizedRoute.Waypoints.Select((w, i) => new Waypoint
    {
        Order = i + 1,
        Latitude = w.Latitude,
        Longitude = w.Longitude,
        Address = w.Address,
        EstimatedArrivalTime = w.TimeWindowStart,
        Status = "Planned"
    }).ToList();
    
    // Save updated route
    await _httpClient.PutAsJsonAsync(
        $"{_config.BaseUrl}/api/v1/routes/{routeId}",
        route
    );
}
```

## Performance Optimization Techniques

### 1. Bulk Operations

```csharp
// Use bulk endpoints for better performance
public async Task BulkUpdateLocationsAsync(List<LocationDto> locations)
{
    var client = new HttpClient();
    
    // Split into batches
    foreach (var batch in locations.Batch(100))
    {
        await client.PostAsJsonAsync(
            $"{_config.BaseUrl}/api/v1/locations/bulk",
            batch
        );
    }
}
```

### 2. Caching Strategy

```csharp
// Cache frequently accessed data
public async Task<List<VehicleDto>> GetCachedVehiclesAsync()
{
    var cacheKey = "all_vehicles";
    
    // Try to get from cache
    var cached = await _cacheService.GetAsync<List<VehicleDto>>(cacheKey);
    if (cached != null)
    {
        return cached;
    }
    
    // Fetch from API and cache
    var vehicles = await _httpClient.GetFromJsonAsync<PaginatedResponse<VehicleDto>>(
        $"{_config.BaseUrl}/api/v1/vehicles?take=1000"
    );
    
    await _cacheService.SetAsync(cacheKey, vehicles.Data, TimeSpan.FromMinutes(5));
    return vehicles.Data;
}
```

### 3. Connection Pooling

```csharp
// Configure HttpClient for better performance
var httpClientHandler = new HttpClientHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 100
};

var httpClient = new HttpClient(httpClientHandler);
```

### 4. Parallel Operations

```csharp
// Process multiple vehicles in parallel
public async Task ProcessFleetInParallelAsync(List<FleetVehicle> vehicles)
{
    var tasks = vehicles.Select(async vehicle =>
    {
        try
        {
            await ProcessVehicleAsync(vehicle);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {vehicle.LicensePlate}: {ex.Message}");
        }
    });
    
    await Task.WhenAll(tasks);
}
```

## Best Practices

### 1. Configuration Management

- Use environment variables for sensitive data
- Validate configuration on startup
- Provide sensible defaults
- Document all configuration options

### 2. Error Handling
- Implement retry logic with exponential backoff
- Log all errors with context
- Provide fallback mechanisms
- Graceful degradation when services are unavailable

### 3. Performance
- Use bulk operations for data imports
- Implement caching for read-heavy operations
- Use parallel processing for independent operations
- Monitor and optimize database queries

### 4. Monitoring
- Track API response times
- Monitor error rates
- Set up alerts for anomalies
- Log important events

### 5. Security
- Validate all inputs
- Use parameterized queries
- Implement proper authentication
- Encrypt sensitive data
- Use HTTPS in production

## Troubleshooting

### Common Issues

#### Issue: API timeouts
**Solution**: Increase timeout settings, use bulk operations, optimize database queries

#### Issue: Memory leaks
**Solution**: Dispose HttpClient properly, use connection pooling, monitor memory usage

#### Issue: Slow performance
**Solution**: Add caching, use bulk operations, optimize database indexes, scale horizontally

#### Issue: Connection failures
**Solution**: Check network connectivity, verify service URLs, implement retry logic

### Debugging Tools

```bash
# Check API health
dotnet run -- check-health

# Monitor API performance
curl -w "@curl-format.txt" http://localhost:5000/health

# View logs
cat logs/fleet-management.log
```

## Next Steps

1. **Integrate with your frontend**: Connect the dashboard to display real-time fleet data
2. **Set up database**: Configure SQL Server/PostgreSQL with proper indexes
3. **Configure authentication**: Set up API key or JWT authentication
4. **Deploy to production**: Use Docker with proper monitoring and scaling
5. **Monitor performance**: Set up APM (Application Performance Monitoring)
6. **Scale horizontally**: Add load balancer and multiple API instances

---

**See Also:**
- [Docker Deployment Example](../docker-deployment/README.md)
- [API Reference](../../docs/api-reference.md)
- [Configuration Guide](../../docs/configuration-guide.md)
