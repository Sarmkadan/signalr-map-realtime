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