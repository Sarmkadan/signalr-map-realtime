# DockerDeploymentExample

The `DockerDeploymentExample` type encapsulates the data and behavior needed to run a demonstration of deploying a SignalR‑based real‑time map application inside a Docker container. It aggregates metadata about the deployment environment and a simulated vehicle telemetry record, and provides an asynchronous method that orchestrates the Docker‑based execution of the example.

## API

### RunDockerExampleAsync
```csharp
public async Task RunDockerExampleAsync()
```
**Purpose** – Executes the Docker deployment workflow for the example. This method pulls the required Docker image (if not present), creates and starts a container, waits for the SignalR service to become healthy, and then tears down the container upon completion or error.  
**Parameters** – None.  
**Return value** – A `Task` that completes when the Docker container has been started, the example has run, and the container has been stopped and removed.  
**Exceptions** –  
* `DockerException` – Thrown when the Docker daemon is unreachable, the image cannot be pulled, or container creation fails.  
* `InvalidOperationException` – Thrown if required properties such as `Version` or `Environment` are null or empty when the method is invoked.  
* `TimeoutException` – Thrown if the container does not report a healthy status within the configured timeout period.  
* `ObjectDisposedException` – Thrown if the instance has been disposed prior to calling this method (if a dispose pattern is implemented elsewhere).

### Version
```csharp
public string Version { get; set; }
```
**Purpose** – Specifies the version label of the Docker image or the example release to be deployed.  
**Type** – `string`.  
**Remarks** – Expected to follow semantic versioning (e.g., `"1.2.3"`). A null or empty value will cause `RunDockerExampleAsync` to throw an `InvalidOperationException`.

### Environment
```csharp
public string Environment { get; set; }
```
**Purpose** – Identifies the target deployment environment (e.g., `"development"`, `"staging"`, `"production"`).  
**Type** – `string`.  
**Remarks** – Used to select appropriate configuration files or environment variables inside the container. A null or empty value triggers an `InvalidOperationException` in `RunDockerExampleAsync`.

### Id (first)
```csharp
public string Id { get; set; }
```
**Purpose** – Primary identifier for the deployment example instance.  
**Type** – `string`.  
**Remarks** – Intended for logging or correlation purposes. No validation is performed by the type itself; callers should ensure a meaningful value if required.

### LicensePlate
```csharp
public string LicensePlate { get; set; }
```
**Purpose** – Stores the license plate string associated with the simulated vehicle telemetry.  
**Type** – `string`.  
**Remarks** – May be null or empty if the license plate is unknown.

### Manufacturer
```csharp
public string Manufacturer { get; set; }
```
**Purpose** – Manufacturer name of the simulated vehicle.  
**Type** – `string`.  
**Remarks** – Optional metadata; may be null.

### Model
```csharp
public string Model { get; set; }
```
**Purpose** – Model name of the simulated vehicle.  
**Type** – `string`.  
**Remarks** – Optional metadata; may be null.

### Year
```csharp
public int Year { get; set; }
```
**Purpose** – Model year of the simulated vehicle.  
**Type** – `int`.  
**Remarks** – Values outside a realistic range (e.g., `< 1900` or `> DateTime.UtcNow.Year + 1`) are not validated by the type.

### Status
```csharp
public string Status { get; set; }
```
**Purpose** – Current operational status of the simulated vehicle (e.g., `"Running"`, `"Stopped"`, `"Maintenance"`).  
**Type** – `string`.  
**Remarks** – Free‑form; callers may define their own enumeration of status strings.

### Id (second)
```csharp
public string Id { get; set; }
```
**Purpose** – Secondary identifier that may represent a different concept (e.g., a database record ID) depending on the context of use.  
**Type** – `string`.  
**Remarks** – Because this member shares a name with the first `Id` property, the latter hides the former in the class’s public surface. Consumers should access the intended identifier via the appropriate qualifier or by relying on the ordering defined in the source file.

### VehicleId
```csharp
public string VehicleId { get; set; }
```
**Purpose** – Unique identifier for the simulated vehicle whose telemetry is being reported.  
**Type** – `string`.  
**Remarks** – Should be unique within the scope of the example; null or empty values are permitted but may hinder correlation.

### Latitude
```csharp
public double Latitude { get; set; }
```
**Purpose** – Latitude coordinate of the vehicle’s current position, expressed in decimal degrees.  
**Type** – `double`.  
**Remarks** – Expected to be in the range `[-90, 90]`. Values outside this range are not automatically corrected.

### Longitude
```csharp
public double Longitude { get; set; }
```
**Purpose** – Longitude coordinate of the vehicle’s current position, expressed in decimal degrees.  
**Type** – `double`.  
**Remarks** – Expected to be in the range `[-180, 180]`. Values outside this range are not automatically corrected.

### Accuracy
```csharp
public double Accuracy { get; set; }
```
**Purpose** – Estimated horizontal accuracy of the position fix, in meters.  
**Type** – `double`.  
**Remarks** – Lower values indicate higher precision. Negative values are not meaningful but are not rejected by the type.

### Speed
```csharp
public double Speed { get; set; }
```
**Purpose** – Instantaneous speed of the vehicle, in meters per second.  
**Type** – `double`.  
**Remarks** – Should be non‑negative; negative values are not validated.

### Heading
```csharp
public double Heading { get; set; }
```
**Purpose** – Direction of travel, expressed as degrees clockwise from true north.  
**Type** – `double`.  
**Remarks** – Normalized to `[0, 360)`. Values outside this range are not automatically wrapped.

### Timestamp
```csharp
public DateTime Timestamp { get; set; }
```
**Purpose** – Date and time at which the telemetry sample was recorded.  
**Type** – `DateTime`.  
**Remarks** – Should be supplied in UTC to avoid ambiguity; the type does not perform any conversion.

### Data
```csharp
public System.Collections.Generic.List<T> Data { get; set; }
```
**Purpose** – Container for arbitrary payload data associated with the telemetry record. The element type `T` is determined by the concrete generic instantiation of `DockerDeploymentExample<T>`.  
**Type** – `System.Collections.Generic.List<T>`.  
**Remarks** – The list may be null; callers should initialize it before adding items. The type does not enforce any constraints on `T`.

### PageSize
```csharp
public int PageSize { get; set; }
```
**Purpose** – Number of items to retrieve per page when the `Data` list is used for paged queries.  
**Type** – `int`.  
**Remarks** – Should be a positive integer; a value ≤ 0 may cause undefined behavior in consuming code.

## Usage

### Example 1: Basic deployment run
```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var example = new DockerDeploymentExample
        {
            Version = "2.0.0",
            Environment = "development",
            Id = "example-001",
            LicensePlate = "ABC-123",
            Manufacturer = "Contoso",
            Model = "Sedan",
            Year = 2022,
            Status = "Running",
            VehicleId = "veh-001",
            Latitude = 47.6062,
            Longitude = -122.3321,
            Accuracy = 5.0,
            Speed = 13.4,
            Heading = 90.0,
            Timestamp = DateTime.UtcNow,
            Data = new System.Collections.Generic.List<string> { "sample1", "sample2" },
            PageSize = 100
        };

        try
        {
            await example.RunDockerExampleAsync();
            Console.WriteLine("Docker deployment example completed successfully.");
        }
        catch (DockerException dex)
        {
            Console.Error.WriteLine($"Docker error: {dex.Message}");
        }
        catch (InvalidOperationException iox)
        {
            Console.Error.WriteLine($"Invalid operation: {iox.Message}");
        }
    }
}
```

### Example 2: Using the generic Data property
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TelemetryPoint
{
    public double Value { get; set; }
    public string Unit { get; set; }
}

class Program
{
    static async Task Main()
    {
        var example = new DockerDeploymentExample<List<TelemetryPoint>>
        {
            Version = "1.5.3",
            Environment = "staging",
            Id = "telemetry-run",
            VehicleId = "veh-42",
            Latitude = 34.0522,
            Longitude = -118.2437,
            Timestamp = DateTime.UtcNow,
            Data = new List<TelemetryPoint>
            {
                new TelemetryPoint { Value = 23.5, Unit = "C" },
                new TelemetryPoint { Value = 101.3, Unit = "kPa" }
            },
            PageSize = 50
        };

        await example.RunDockerExampleAsync();
        // After execution, example.Data contains the processed telemetry points.
    }
}
```

## Notes
- **Thread safety** – The type does not provide internal synchronization. Concurrent reads and writes to any of its properties (including `Data`) from multiple threads may result in race conditions. It is safe to invoke `RunDockerExampleAsync` from a single thread at a time; calling it concurrently on the same instance is not supported and may lead to undefined Docker client behavior.  
- **Null handling** – Properties of reference type (`string`, `List<T>`, etc.) accept null values. However, `RunDockerExampleAsync` treats null or empty `Version` and `Environment` as invalid and will throw an `InvalidOperationException`. Consumers should validate these fields before invocation.  
- **Data list** – The `Data` property is covariant only with respect to the generic type argument `T`. If the list is null, attempting to enumerate or modify it will throw a `NullReferenceException`. Initialize the list prior to use.  
- **Numeric ranges** – The type does not enforce realistic ranges for coordinates, speed, heading, accuracy, or year. Supplying out‑of‑range values will not cause immediate exceptions but may produce nonsensical results in the simulated telemetry or downstream consumers.  
- **Duplicate Id members** – Because two members named `Id` appear in the public surface, the latter declaration hides the former. Access to the first `Id` requires qualifying it with the base type (if any) or using reflection; typical usage should rely on the second `Id` or the explicitly named `VehicleId` for identification purposes.  
- **Exception propagation** – Any exception thrown by the underlying Docker SDK is propagated unchanged; callers should catch `DockerException` or its derived types to handle Docker‑specific failures.  
- **Resource cleanup** – `RunDockerExampleAsync` ensures that any container it creates is stopped and removed before completing, even if an error occurs. However, if the method is interrupted (e.g., via `Thread.Abort` or process termination), orphaned containers may remain; external cleanup scripts are advised.
