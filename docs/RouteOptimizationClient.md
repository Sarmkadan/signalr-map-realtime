# RouteOptimizationClient

The `RouteOptimizationClient` type encapsulates the logic for defining waypoints, computing an optimal travel order, generating a delivery route, and simulating its execution. It is intended to be used in real‑time mapping scenarios where route data is refreshed via SignalR connections.

## API

### Order (int)
Gets or sets the sequential index of the waypoint within the route.  
- **Parameters:** none  
- **Return value:** the current order value  
- **Exceptions:** none  

### Latitude (double)
Gets or sets the latitude coordinate of the waypoint in decimal degrees. Valid values are between -90 and 90.  
- **Parameters:** none  
- **Return value:** the current latitude  
- **Exceptions:** none  

### Longitude (double)
Gets or sets the longitude coordinate of the waypoint in decimal degrees. Valid values are between -180 and 180.  
- **Parameters:** none  
- **Return value:** the current longitude  
- **Exceptions:** none  

### Name (string)
Gets or sets a descriptive label for the waypoint (e.g., customer name or landmark).  
- **Parameters:** none  
- **Return value:** the current name, or `null` if not set  
- **Exceptions:** none  

### RouteOptimizationClient ()
Initializes a new instance of the client with default empty state.  
- **Parameters:** none  
- **Return value:** a new `RouteOptimizationClient` object  
- **Exceptions:** none  

### CreateDeliveryRoute ()
Asynchronously constructs a delivery route using the waypoints currently stored in the instance (based on `Latitude`, `Longitude`, `Name`, and `Order`).  
- **Parameters:** none  
- **Return value:** a `Task` that completes when the route has been generated  
- **Exceptions:**  
  - `InvalidOperationException` – if fewer than two waypoints have been defined or any required property is missing/invalid.  
  - `HttpRequestException` – if the underlying routing service cannot be reached or returns an error.  

### OptimizeWaypointOrder ()
Calculates the optimal sequence for visiting all defined waypoints and returns a new list reflecting that order.  
- **Parameters:** none  
- **Return value:** a `List<Waypoint>` containing the waypoints sorted by the optimizer.  
- **Exceptions:**  
  - `InvalidOperationException` – if fewer than two waypoints are present or any waypoint lacks valid coordinates.  

### SimulateRouteExecution ()
Asynchronously runs a simulation of traveling along the route produced by `CreateDeliveryRoute`, updating internal state to reflect progress.  
- **Parameters:** none  
- **Return value:** a `Task` that completes when the simulation ends.  
- **Exceptions:**  
  - `InvalidOperationException` – if `CreateDeliveryRoute` has not been called successfully beforehand.  

### RunExample ()
Asynchronous demonstration method that creates a client, adds sample waypoints, optimizes their order, builds a route, and runs the simulation.  
- **Parameters:** none  
- **Return value:** a `Task` that completes when the example workflow finishes.  
- **Exceptions:** propagates any exceptions thrown by the called methods.  

### Main ()
Static entry point for the application; invokes `RunExample` and handles unexpected errors.  
- **Parameters:** none  
- **Return value:** a `Task` representing the application lifecycle.  
- **Exceptions:** none (exceptions are caught and logged within the method).  

## Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Example 1: Basic waypoint definition and route creation
public async Task BasicRouteExample()
{
    var client = new RouteOptimizationClient();

    // Define three waypoints
    client.Name = "Warehouse";
    client.Latitude = 40.7128;
    client.Longitude = -74.0060;
    client.Order = 0;
    // (Assume the client internally stores the waypoint; repeat for others)

    client.Name = "Customer A";
    client.Latitude = 40.730610;
    client.Longitude = -73.935242;
    client.Order = 1;

    client.Name = "Customer B";
    client.Latitude = 40.758896;
    client.Longitude = -73.985130;
    client.Order = 2;

    // Optimize the visiting order
    List<Waypoint> optimized = client.OptimizeWaypointOrder();
    Console.WriteLine($"Optimized order contains {optimized.Count} waypoints.");

    // Generate the route for the optimized sequence
    await client.CreateDeliveryRoute();
    Console.WriteLine("Delivery route created.");
}
```

```csharp
using System;
using System.Threading.Tasks;

// Example 2: Full workflow with simulation and error handling
public async Task FullWorkflowExample()
{
    var client = new RouteOptimizationClient();

    try
    {
        // Populate waypoints (omitted for brevity)
        // ...

        await client.RunExample(); // Demonstrates optimize → create → simulate
        Console.WriteLine("Example completed successfully.");
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Invalid state: {ex.Message}");
    }
    catch (HttpRequestException ex)
    {
        Console.Error.WriteLine($"Routing service error: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Unexpected error: {ex.Message}");
    }
}
```

## Notes

- All instance members (`Order`, `Latitude`, `Longitude`, `Name`, and the async methods) operate on mutable internal state; therefore the class is **not thread‑safe**. Concurrent calls from multiple threads should be synchronized externally.  
- The static `Main` method is safe to use as the application entry point because it does not share mutable state across invocations.  
- Before calling `OptimizeWaypointOrder` or `CreateDeliveryRoute`, at least two waypoints must have valid `Latitude` and `Longitude` values; otherwise an `InvalidOperationException` is thrown.  
- `Latitude` must be within [-90, 90] and `Longitude` within [-180, 180]; values outside these ranges are considered invalid and will cause the route‑creation methods to fail.  
- The `Name` property may be left empty, but leaving it `null` or whitespace may lead to unclear logs; the implementation does not enforce non‑null names.  
- If the underlying routing service is unavailable, `CreateDeliveryRoute` will throw an `HttpRequestException`; callers should handle this appropriately (e.g., retry or fallback).  
- The `SimulateRouteExecution` method assumes that a route has already been successfully created; invoking it prior to `CreateDeliveryRoute` results in an `InvalidOperationException`.  
- The `RunExample` method is provided solely for demonstration and may be removed or altered in future versions without affecting the core contract of the type.
