# ILocationService

The `ILocationService` interface provides a read-only view of aggregated telemetry data derived from a stream of location points, typically within the context of the `signalr-map-realtime` project. It exposes calculated metrics regarding movement, including point density, speed variations, and total distance traveled, allowing consumers to monitor real-time statistical summaries without accessing the underlying raw data collection.

## API

The following members represent the complete public surface area of the interface.

### `PointCount`
```csharp
public int PointCount { get; }
```
Retrieves the total number of location points currently processed and stored in the service. This value increments as new data arrives and may reset if the underlying session or tracking context is cleared. It does not accept parameters and returns a non-negative integer. No exceptions are thrown during retrieval.

### `MinSpeed`
```csharp
public double MinSpeed { get; }
```
Returns the lowest recorded speed value among all processed location points. The unit of measurement depends on the ingestion configuration (typically meters per second or kilometers per hour). If no points have been processed, the value may default to `0.0` or `double.MaxValue` depending on implementation specifics, but access itself does not throw exceptions.

### `MaxSpeed`
```csharp
public double MaxSpeed { get; }
```
Returns the highest recorded speed value among all processed location points. This metric is useful for identifying peak velocity events. Like other speed properties, it returns a `double` and requires no parameters. Accessing this property is safe and does not throw exceptions.

### `AverageSpeed`
```csharp
public double AverageSpeed { get; }
```
Provides the arithmetic mean of speed values calculated across all available location points. This value updates dynamically as new points are added. It returns a `double`. If the dataset is empty, the return value is implementation-defined (commonly `0.0`), but the getter itself does not throw exceptions.

### `TotalDistance`
```csharp
public double TotalDistance { get; }
```
Calculates and returns the cumulative distance traveled based on the geometric sequence of location points. The calculation typically sums the haversine or euclidean distance between consecutive points. It returns a `double` representing the total distance in the configured unit. No parameters are required, and access does not throw exceptions.

## Usage

### Example 1: Monitoring Real-Time Statistics
This example demonstrates how to poll the service to display current trip statistics in a logging or UI context.

```csharp
public void DisplayTripStats(ILocationService locationService)
{
    if (locationService.PointCount == 0)
    {
        Console.WriteLine("No location data received yet.");
        return;
    }

    Console.WriteLine($"Points Received: {locationService.PointCount}");
    Console.WriteLine($"Total Distance: {locationService.TotalDistance:F2} km");
    Console.WriteLine($"Speed Range: {locationService.MinSpeed:F1} - {locationService.MaxSpeed:F1} km/h");
    Console.WriteLine($"Average Speed: {locationService.AverageSpeed:F1} km/h");
}
```

### Example 2: Validating Data Quality
This example checks for anomalies, such as unrealistic speeds or insufficient data points, before processing further.

```csharp
public bool IsDataValid(ILocationService locationService, double maxAllowedSpeed)
{
    // Ensure we have enough points to calculate meaningful distance
    if (locationService.PointCount < 2)
    {
        return false;
    }

    // Check for sensor errors indicating impossible speeds
    if (locationService.MaxSpeed > maxAllowedSpeed)
    {
        return false;
    }

    // Verify that distance has been accumulated
    if (locationService.TotalDistance <= 0.0)
    {
        return false;
    }

    return true;
}
```

## Notes

*   **Read-Only Nature**: All members exposed by `ILocationService` are properties with only getters. There are no methods to manually inject data or modify the calculated metrics directly; state changes occur solely through the internal ingestion pipeline.
*   **Empty State Behavior**: When `PointCount` is zero, dependent metrics (`MinSpeed`, `MaxSpeed`, `AverageSpeed`, `TotalDistance`) will return default numeric values. Consumers should explicitly check `PointCount` before interpreting these values to avoid logical errors in calculations (e.g., dividing by zero if calculating derived metrics externally).
*   **Thread Safety**: Given the real-time nature of the `signalr-map-realtime` project, these properties are likely updated concurrently by background threads processing SignalR messages. While the primitive types (`int`, `double`) ensure atomic reads, the values represent a snapshot in time. It is possible for `MinSpeed` and `MaxSpeed` to reflect slightly different states if read sequentially during a high-frequency update burst. For consistent snapshots across all properties, external synchronization or a dedicated snapshot method (if provided by the concrete implementation) may be required.
*   **Precision**: Speed and distance values are returned as `double`. Floating-point precision errors may accumulate in `TotalDistance` over extremely long sessions or high-frequency updates.
