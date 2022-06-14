# SessionAnalyticsReporter

The `SessionAnalyticsReporter` class serves as a dedicated utility for aggregating, calculating, and exporting telemetry data associated with vehicle tracking sessions within the `signalr-map-realtime` project. It encapsulates both session-specific metrics and cumulative statistical aggregates, providing asynchronous methods to generate detailed reports and export findings to CSV format for further analysis or archival.

## API

### Constructors

#### `public SessionAnalyticsReporter()`
Initializes a new instance of the `SessionAnalyticsReporter` class. This constructor sets up the internal state required to track session metrics, initializing numeric aggregates to zero and status fields to their default states.

### Methods

#### `public async Task GenerateVehicleAnalyticsReport()`
Processes the collected location data for the specified vehicle to compute analytical metrics such as total distance, average speed, and maximum speed.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` that completes when the calculation logic has finished updating the instance properties.
*   **Exceptions**: May throw an exception if the underlying data source is unavailable or if the data format is invalid for calculation.

#### `public async Task ExportAnalyticsToCsv()`
Serializes the current analytics data held by the instance into a Comma-Separated Values (CSV) format and writes it to the configured output destination.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` that completes when the file write operation is finished.
*   **Exceptions**: May throw `IOException` or `UnauthorizedAccessException` if the file system is inaccessible or permissions are insufficient.

#### `public async Task RunExample()`
Executes a demonstration workflow that instantiates data, runs the analytics generation, and triggers the CSV export. This method is intended for validation, testing, or illustrative purposes.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` representing the completion of the entire example workflow.
*   **Exceptions**: Propagates any exceptions thrown by `GenerateVehicleAnalyticsReport` or `ExportAnalyticsToCsv`.

### Properties

#### `public string Id`
Gets or sets the unique identifier for this specific analytics reporter instance or session record.

#### `public string VehicleId`
Gets or sets the identifier of the vehicle associated with this analytics session.

#### `public DateTime StartTime`
Gets or sets the timestamp marking the beginning of the tracking session.

#### `public DateTime EndTime`
Gets or sets the timestamp marking the conclusion of the tracking session.

#### `public double TotalDistance`
Gets or sets the cumulative distance traveled during the session, typically measured in kilometers or miles depending on system configuration.
*Note: This property appears in the public interface as both a session-specific metric and a cumulative aggregate.*

#### `public double AverageSpeed`
Gets or sets the calculated mean speed over the duration of the session.
*Note: This property represents both the session average and the global average across multiple sessions.*

#### `public double MaxSpeed`
Gets or sets the highest recorded speed observed during the session.
*Note: This property represents both the session maximum and the global maximum across multiple sessions.*

#### `public int LocationCount`
Gets or sets the total number of discrete location points recorded during the session.

#### `public string Status`
Gets or sets the current operational state of the reporter (e.g., "Active", "Completed", "Error").

#### `public int SessionCount`
Gets or sets the total number of sessions aggregated in this report. This property is relevant when the reporter summarizes data across multiple distinct sessions.

#### `public double TotalDuration`
Gets or sets the sum of time elapsed across all aggregated sessions.

#### `public int TotalLocations`
Gets or sets the cumulative count of location points across all aggregated sessions.

#### `public double AverageDurationPerSession`
Gets or sets the arithmetic mean of the duration for all sessions included in the aggregation.

## Usage

### Example 1: Generating and Exporting a Single Session Report
This example demonstrates how to instantiate the reporter, populate key session identifiers, generate the analytics, and export the result to a CSV file.

```csharp
using System;
using System.Threading.Tasks;

public class VehicleMonitoring
{
    public async Task ProcessSingleSessionAsync()
    {
        var reporter = new SessionAnalyticsReporter
        {
            VehicleId = "VH-1024",
            Id = "SESSION-2023-001",
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        // Calculate metrics based on ingested data
        await reporter.GenerateVehicleAnalyticsReport();

        // Update status upon completion
        reporter.Status = "ReportGenerated";

        // Export results to storage
        await reporter.ExportAnalyticsToCsv();
        
        Console.WriteLine($"Report for {reporter.VehicleId} exported. Total Distance: {reporter.TotalDistance}");
    }
}
```

### Example 2: Running the Built-in Example Workflow
For quick validation or demonstration purposes, the `RunExample` method can be invoked to execute the full pipeline internally without manual property configuration.

```csharp
using System;
using System.Threading.Tasks;

public class DemoRunner
{
    public async Task ExecuteDemoAsync()
    {
        var reporter = new SessionAnalyticsReporter();
        
        try 
        {
            // Executes internal setup, calculation, and export logic
            await reporter.RunExample();
            
            Console.WriteLine($"Demo completed. Sessions analyzed: {reporter.SessionCount}");
            Console.WriteLine($"Global Average Speed: {reporter.AverageSpeed}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Demo execution failed: {ex.Message}");
        }
    }
}
```

## Notes

*   **Property Redundancy**: The public interface exposes properties such as `TotalDistance`, `AverageSpeed`, and `MaxSpeed` which serve dual purposes: representing metrics for a single session and acting as aggregates when `SessionCount` exceeds one. Consumers should verify the context of the instance (single session vs. multi-session aggregation) when interpreting these values.
*   **Thread Safety**: The class exposes mutable public properties alongside asynchronous methods that modify internal state (`GenerateVehicleAnalyticsReport`). It is not inherently thread-safe. Concurrent access to properties while an async generation or export task is running may result in race conditions or inconsistent read states. External synchronization is required if instances are shared across threads.
*   **Asynchronous Operations**: Both report generation and CSV export are I/O bound or computationally intensive operations exposed via `Task`. Callers must await these methods to ensure data consistency before accessing calculated properties like `AverageSpeed` or `TotalDistance`.
*   **Edge Cases**: If `StartTime` equals `EndTime`, calculations involving duration (such as `AverageSpeed` or `AverageDurationPerSession`) may result in division by zero scenarios depending on the internal implementation logic; consumers should ensure valid time ranges are set prior to calling `GenerateVehicleAnalyticsReport`.
