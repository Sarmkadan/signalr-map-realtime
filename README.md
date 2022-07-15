# SignalRMapRealtime

A real-time mapping application using SignalR for live updates and tracking.

## ApiResponseExtensions

The `ApiResponseExtensions` class provides convenient extension methods for working with `ApiResponse` and `ApiResponse<T>` types. These methods simplify common operations like checking success status, retrieving messages, ensuring successful responses, and transforming non-generic responses into generic ones.

### Usage Example

```csharp
// Example API response from a service call
var apiResponse = new ApiResponse
{
    Success = true,
    Message = "Asset retrieved successfully",
    StatusCode = 200,
    Timestamp = DateTime.UtcNow,
    TraceId = "abc123"
};

// Check if response is successful
bool isSuccessful = apiResponse.IsSuccessful(); // Returns true

// Get message with null safety
string message = apiResponse.GetMessageOrDefault(); // Returns "Asset retrieved successfully"

// Ensure response is successful (throws InvalidOperationException on failure)
apiResponse.EnsureSuccess();

// Transform non-generic response to generic with data
var asset = new Asset { Id = 1, Name = "GPS Tracker" };
var genericResponse = apiResponse.WithData(asset);

// Work with generic response
bool genericIsSuccessful = genericResponse.IsSuccessful<Asset>(); // Returns true
string genericMessage = genericResponse.GetMessageOrDefault<Asset>(); // Returns "Asset retrieved successfully"
genericResponse.EnsureSuccess<Asset>();
```

## PagedRequestExtensions

The `PagedRequestExtensions` class provides utility methods for handling pagination, sorting, and filtering in paged requests. It includes helpers to calculate skip/take values, validate page parameters, and apply query parameters to `PagedRequest` instances.

### Usage Example

```csharp
var baseRequest = new PagedRequest();
var processedRequest = baseRequest
  .WithPagination(2, 20) // Page 2, 20 items per page
  .WithSortBy("name", true) // Sort by name (descending)
  .WithSearchQuery("active=true"); // Filter by active status

int pageSize = PagedRequestExtensions.GetValidPageSize(20, 10, 100); // Returns 20 (clamped between 10-100)
int pageNumber = PagedRequestExtensions.GetValidPageNumber(0, 1, 1000); // Returns 1 (minimum 1)
int skip = PagedRequestExtensions.CalculateSkip(pageNumber, pageSize); // Returns 0 (1-1)*20
int take = PagedRequestExtensions.CalculateTake(pageNumber, pageSize); // Returns 20
bool hasFilters = PagedRequestExtensions.HasFilters(processedRequest); // Returns true
```

## PlaybackOptionsExtensions

The `PlaybackOptionsExtensions` class provides methods to configure and validate playback settings for real-time or simulated data streams. It includes utilities for clamping speed multipliers, calculating frame intervals, and checking speed/idle alerts.

### Usage Example

```csharp
var playbackOptions = new PlaybackOptions
{
    SpeedMultiplier = 2.5,
    IsRealTime = false
};

double clampedSpeed = PlaybackOptionsExtensions.ClampSpeedMultiplier(playbackOptions, 2.0, 5.0); // Returns 2.5
bool isRealTime = playbackOptions.IsRealTime; // Returns false
int frameInterval = PlaybackOptionsExtensions.CalculateFrameIntervalMs(clampedSpeed); // Returns calculated interval based on speed
bool hasSpeedAlert = PlaybackOptionsExtensions.IsSpeedAlert(playbackOptions.SpeedMultiplier); // Returns true if speed exceeds threshold
bool isIdle = PlaybackOptionsExtensions.IsIdle(playbackOptions); // Returns true if playback is paused/idle
```

## AssetControllerExtensions

The `AssetControllerExtensions` class provides a set of extension methods for the `AssetController` to handle various asset-related operations. These methods enable retrieving assets by type, condition, date range, and sorted by date, as well as fetching asset statistics.

### Usage Example

```csharp
var assetController = new AssetController();
var assetType = "Vehicle";

// Retrieve assets by type
var assetsByTypeResult = await AssetControllerExtensions.GetAssetsByType(assetController, assetType);
if (assetsByTypeResult is OkObjectResult okResult)
{
    var assets = okResult.Value as List<Asset>;
    // Process assets
}

// Get asset statistics
var assetStatisticsResult = await AssetControllerExtensions.GetAssetStatistics(assetController);
if (assetStatisticsResult is OkObjectResult statisticsResult)
{
    var statistics = statisticsResult.Value as AssetStatistics;
    // Process statistics
}

// Retrieve assets sorted by date
var assetsSortedByDateResult = await AssetControllerExtensions.GetAssetsSortedByDate(assetController, sortDescending: true);
if (assetsSortedByDateResult is OkObjectResult sortedAssetsResult)
{
    var sortedAssets = sortedAssetsResult.Value as List<Asset>;
    // Process sorted assets
}
```

## ValidationException

The `ValidationException` class represents an exception thrown when validation fails. It contains a collection of error messages that can be accessed through the `Errors` property.

### Usage Example

```csharp
try
{
    // Attempt to validate some data
    var validationException = new ValidationException();
    validationException.Errors.Add("Error message 1");
    validationException.Errors.Add("Error message 2");
    throw validationException;
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine(error); // Prints "Error message 1" and "Error message 2"
    }
}
```

## LocationTrackingException

The `LocationTrackingException` class serves as the base exception for all location tracking-related errors in the system. It provides three standard constructors for creating exceptions and is inherited by more specific tracking exceptions like `VehicleNotFoundException`, `InvalidLocationException`, `AssetNotFoundException`, and `TrackingSessionNotFoundException`.


### Usage Example

```csharp
// Basic exception usage
try
{
    // Some location tracking operation
    throw new LocationTrackingException("Location tracking service unavailable");
}
catch (LocationTrackingException ex)
{
    Console.WriteLine($"Location tracking error: {ex.Message}");
}

// Using with inner exception
try
{
    // Nested operation
}
catch (Exception innerEx)
{
    throw new LocationTrackingException("Failed to process location update", innerEx);
}

// Derived exception usage
try
{
    var vehicleId = 123;
    // Attempt to find vehicle
    if (vehicle == null)
    {
        throw new VehicleNotFoundException(vehicleId);
    }
}
catch (VehicleNotFoundException ex)
{
    Console.WriteLine($"Vehicle {ex.VehicleId} not found: {ex.Message}");
}
```

## DomainEvent

The `DomainEvent` class is the base class for all domain events in the application. Domain events represent significant business occurrences and enable the system to react to changes in state. The base `DomainEvent` provides common properties like `EventId`, `OccurredAt`, `TriggeredBy`, and `CorrelationId`, while derived event types add domain-specific data.

### Usage Examples

#### Publishing a LocationUpdatedEvent
```csharp
var locationEvent = new LocationUpdatedEvent
{
    VehicleId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
    Latitude = 40.7128,
    Longitude = -74.0060,
    Accuracy = 5.2,
    Speed = 45.5,
    Heading = 90.0,
    PreviousLatitude = 40.7127,
    PreviousLongitude = -74.0059,
    TriggeredBy = "system-tracker",
    CorrelationId = "req-12345"
};

// Publish the event via event bus
await eventBus.PublishAsync(locationEvent);
```

#### Handling a VehicleStatusChangedEvent
```csharp
var statusEvent = new VehicleStatusChangedEvent
{
    VehicleId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
    VehiclePlate = "ABC123",
    PreviousStatus = "Inactive",
    NewStatus = "Active",
    Reason = "Route started",
    Metadata = new Dictionary<string, object>
    {
        ["routeId"] = "route-456",
        ["driverId"] = "driver-789"
    }
};

// Subscribe to status change events
var eventBus = new InMemoryEventBus();
eventBus.Subscribe<VehicleStatusChangedEvent>(async (vehicleStatusEvent) =>
{
    Console.WriteLine($"Vehicle {vehicleStatusEvent.VehiclePlate} changed status from {vehicleStatusEvent.PreviousStatus} to {vehicleStatusEvent.NewStatus}");
    Console.WriteLine($"Reason: {vehicleStatusEvent.Reason}");
    
    foreach (var kvp in vehicleStatusEvent.Metadata)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
});

// Publish the event
await eventBus.PublishAsync(statusEvent);
```

#### Tracking Session Completion
```csharp
var sessionEvent = new TrackingSessionCompletedEvent
{
    SessionId = Guid.Parse("789e4567-e89b-12d3-a456-426614174001"),
    VehicleId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
    StartedAt = DateTime.UtcNow.AddMinutes(-30),
    EndedAt = DateTime.UtcNow,
    TotalDistanceKm = 15.75,
    LocationCount = 452,
    AverageSpeedKmh = 31.5
};

// Process completed session
await eventBus.PublishAsync(sessionEvent);
```

## IEventBus

The `IEventBus` interface defines a contract for event handling and publishing. It provides methods for subscribing and unsubscribing event handlers, as well as publishing events to all registered handlers.

### Usage Example

```csharp
// Subscribe to events of type 'MyEvent'
var eventBus = new InMemoryEventBus();
eventBus.Subscribe<MyEvent>(async (event) =>
{
    Console.WriteLine($"Received event: {event.Message}");
});

// Publish an event
await eventBus.PublishAsync(new MyEvent { Message = "Hello, world!" });
```

## IWebhookHandler

The `IWebhookHandler` interface and its implementation (`WebhookHandler`) provide functionality for receiving, validating, and processing webhooks from external services. It handles signature validation to ensure authenticity, deserializes payloads based on the provider, and routes to appropriate handlers for tracking updates, notifications, and route optimization.


### Usage Example

```csharp
// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSingleton<IEventBus, InMemoryEventBus>();
services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Webhooks:TrackingService:Secret"] = "your-secret-key-here"
    })
    .Build());

var serviceProvider = services.BuildServiceProvider();

// Create webhook handler
var webhookHandler = new WebhookHandler(
    serviceProvider.GetRequiredService<ILogger<WebhookHandler>>(),
    serviceProvider.GetRequiredService<IEventBus>(),
    serviceProvider.GetRequiredService<IConfiguration>()
);

// Example 1: Process a tracking service webhook with valid signature
var trackingPayload = @"{
    "vehicleId": "123e4567-e89b-12d3-a456-426614174000",
    "latitude": 40.7128,
    "longitude": -74.0060,
    "speed": 45.5,
    "heading": 90.0,
    "accuracy": 5.2,
    "timestamp": "2024-07-14T10:30:00Z"
}";

var trackingHeaders = new Dictionary<string, string>
{
    ["X-Signature"] = "valid-hmac-signature-here"
};

var trackingResult = await webhookHandler.ProcessWebhookAsync("tracking-service", trackingPayload, trackingHeaders);

if (trackingResult.Success)
{
    Console.WriteLine($"Tracking webhook processed successfully at {trackingResult.ProcessedAt}");
}
else
{
    Console.WriteLine($"Error: {trackingResult.ErrorMessage}");
}

// Example 2: Validate signature before processing
var isValid = webhookHandler.ValidateSignature("tracking-service", trackingPayload, trackingHeaders);
Console.WriteLine($"Signature valid: {isValid}");

// Example 3: Process a notification service webhook
var notificationPayload = @"{
    "deliveryId": "789e4567-e89b-12d3-a456-426614174001",
    "status": "delivered",
    "timestamp": "2024-07-14T10:35:00Z"
}";

var notificationHeaders = new Dictionary<string, string>
{
    ["X-Webhook-Secret"] = "valid-notification-secret-here"
};

var notificationResult = await webhookHandler.ProcessWebhookAsync("notification-service", notificationPayload, notificationHeaders);
```

## LocationHub

The `LocationHub` class is a SignalR hub that enables real-time location tracking and vehicle updates. It handles client connections, location updates, vehicle status changes, and route progress broadcasts. The hub requires API key authentication and manages client groups for efficient message routing.

### Usage Example

```csharp
// Setup SignalR client connection
var connection = new HubConnectionBuilder()
    .WithUrl("https://your-server.com/locationHub", options =>
    {
        options.Headers.Add("X-API-Key", "your-api-key-here");
    })
    .WithAutomaticReconnect()
    .Build();

// Start the connection
await connection.StartAsync();

// Subscribe to vehicle updates
await connection.InvokeAsync("SubscribeToVehicle", 123);

// Send a location update from a vehicle tracker
var locationUpdate = new CreateLocationDto
{
    VehicleId = 123,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Accuracy = 5.2,
    Speed = 45.5,
    Heading = 90.0,
    Timestamp = DateTime.UtcNow
};
await connection.InvokeAsync("SendLocationUpdate", locationUpdate);

// Request all vehicle locations on reconnect
await connection.InvokeAsync("RequestAllVehicleLocations");

// Broadcast vehicle status change
await connection.InvokeAsync("BroadcastVehicleStatusChange", 123, "Active");

// Request latest location for a specific vehicle
await connection.InvokeAsync("RequestVehicleLocation", 123);

// Subscribe to route progress updates
await connection.InvokeAsync("BroadcastRouteProgress", 456, 75, "InProgress");

// Send an alert
await connection.InvokeAsync("SendAlert", 123, "SpeedViolation", "Vehicle exceeded speed limit");

// Get online vehicle count
var vehicleCount = await connection.InvokeAsync<int>("RequestOnlineVehicleCount");

// Broadcast fleet status
await connection.InvokeAsync("BroadcastFleetStatus", "DeliveryFleet", 15, 20);

// Handle incoming messages from server
connection.On<LocationDto>("LocationUpdated", location =>
{
    Console.WriteLine($"Vehicle {location.VehicleId} updated at ({location.Latitude}, {location.Longitude})");
});

connection.On<VehicleStatusChangedDto>("VehicleStatusChanged", status =>
{
    Console.WriteLine($"Vehicle {status.VehicleId} status changed to {status.NewStatus}");
});

connection.On<int>("AssetRemoved", vehicleId =>
{
    Console.WriteLine($"Asset {vehicleId} removed from tracking");
});

// Keep connection alive
connection.On("Pong", () => Console.WriteLine("Ping received, connection alive"));

// Send heartbeat ping
await connection.InvokeAsync("Ping");
```

## Documentation

This project uses a variety of extension methods and utility classes to simplify common tasks and provide a more intuitive API. The following sections provide a brief overview of each extension method and utility class, along with examples of how to use them.

### Usage

To use this project, simply reference the `SignalRMapRealtime` assembly and start using the extension methods and utility classes. For example, to use the `ApiResponseExtensions` class, you can add the following using statement to your code:

```csharp
using SignalRMapRealtime.Extensions.ApiResponse;
```

You can then use the extension methods on the `ApiResponse` class, like this:

```csharp
var apiResponse = new ApiResponse
{
    Success = true,
    Message = "Asset retrieved successfully",
    StatusCode = 200,
    Timestamp = DateTime.UtcNow,
    TraceId = "abc123"
};

bool isSuccessful = apiResponse.IsSuccessful(); // Returns true
string message = apiResponse.GetMessageOrDefault(); // Returns "Asset retrieved successfully"
```

Note that this is just a brief overview of the project's API. For more information, please refer to the individual documentation sections for each extension method and utility class.
```