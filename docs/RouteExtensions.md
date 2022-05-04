# RouteExtensions
Provides helper methods to retrieve timing information for a route.

## API
### GetEstimatedDurationMinutes
- **Purpose:** Returns the estimated duration of the route in minutes, if available.  
- **Parameters:** None.  
- **Return value:** `int?` representing the estimated minutes, or `null` when the estimate is unknown.  
- **Exceptions:** Does not throw under normal operation; returns `null` when data is unavailable.

### GetActualDurationMinutes
- **Purpose:** Returns the actual duration of the route in minutes, if available.  
- **Parameters:** None.  
- **Return value:** `int?` representing the actual minutes, or `null` when the actual duration is unknown.  
- **Exceptions:** Does not throw under normal operation; returns `null` when data is unavailable.

### IsDelayed
- **Purpose:** Indicates whether the route is currently delayed.  
- **Parameters:** None.  
- **Return value:** `true` if the route is delayed, `false` if it is not delayed or the status is unknown.  
- **Exceptions:** Does not throw under normal operation.

### GetDelayMinutes
- **Purpose:** Returns the delay amount in minutes, if the route is delayed.  
- **Parameters:** None.  
- **Return value:** `int?` representing the delay in minutes, or `null` when the route is not delayed or the delay is unknown.  
- **Exceptions:** Does not throw under normal operation; returns `null` when delay information is unavailable.

## Usage
```csharp
using SignalRMapRealtime; // namespace containing RouteExtensions

// Example 1: Check delay status and report delay minutes
bool? delayed = RouteExtensions.IsDelayed();
if (delayed.GetValueOrDefault())
{
    int? delayMinutes = RouteExtensions.GetDelayMinutes();
    Console.WriteLine($"Route delayed by {delayMinutes ?? 0} minutes.");
}
else
{
    Console.WriteLine("Route is on time.");
}
```

```csharp
// Example 2: Log estimated and actual durations for reporting
int? estimated = RouteExtensions.GetEstimatedDurationMinutes();
int? actual = RouteExtensions.GetActualDurationMinutes();
Console.WriteLine($"Estimated: {estimated ?? -1} min, Actual: {actual ?? -1} min");
```

## Notes
- All methods return `null` when the requested information is not available; callers should handle null values appropriately.  
- As stateless static methods, they are thread‑safe and safe to invoke concurrently from multiple threads.  
- The methods perform only queries and do not modify any route state.  
- Under normal operation the methods do not throw exceptions; any exceptions would originate from the underlying data source and are propagated as‑is.
