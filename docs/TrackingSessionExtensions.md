# TrackingSessionExtensions

Provides a set of extension methods for querying the state and timing information of a `TrackingSession` instance. These helpers simplify common checks such as determining session length, activity status, presence of recorded locations, and formatting idle periods for display or logging.

## API

### GetDuration
```csharp
public static TimeSpan GetDuration(this TrackingSession session)
```
**Purpose** – Returns the elapsed time since the session started.  
**Parameters** – `session`: The `TrackingSession` to evaluate.  
**Return value** – A `TimeSpan` representing the total duration; returns `TimeSpan.Zero` if the session has not been started.  
**Exceptions** – Throws `ArgumentNullException` if `session` is `null`.

### IsActive
```csharp
public static bool IsActive(this TrackingSession session)
```
**Purpose** – Indicates whether the session is currently active (i.e., started and not yet stopped or paused).  
**Parameters** – `session`: The `TrackingSession` to evaluate.  
**Return value** – `true` if the session is active; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `session` is `null`.

### HasLocations
```csharp
public static bool HasLocations(this TrackingSession session)
```
**Purpose** – Determines whether any location points have been recorded for the session.  
**Parameters** – `session`: The `TrackingSession` to evaluate.  
**Return value** – `true` if at least one location entry exists; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `session` is `null`.

### GetFormattedIdleTime
```csharp
public static string GetFormattedIdleTime(this TrackingSession session)
```
**Purpose** – Produces a human‑readable string representing the total idle time accumulated during the session.  
**Parameters** – `session`: The `TrackingSession` to evaluate.  
**Return value** – A formatted string (e.g., `"00:05:12"` for hours:minutes:seconds) or `"00:00:00"` when no idle time is recorded.  
**Exceptions** – Throws `ArgumentNullException` if `session` is `null`.

## Usage
```csharp
// Example 1: Check session state and log duration
if (trackingSession.IsActive())
{
    var elapsed = trackingSession.GetDuration();
    Console.WriteLine($"Session active for {elapsed.TotalMinutes:F1} minutes.");
}
else
{
    Console.WriteLine("Session is not active.");
}

// Example 2: Report location presence and idle time
if (trackingSession.HasLocations())
{
    Console.WriteLine($"Locations recorded: {trackingSession.GetLocationCount()}");
}
else
{
    Console.WriteLine("No location data available.");
}

string idle = trackingSession.GetFormattedIdleTime();
Console.WriteLine($"Total idle time: {idle}");
```

## Notes
- All extension methods are safe to call from multiple threads concurrently, provided the underlying `TrackingSession` instance is not being mutated in a way that invalidates its state during the call. The methods themselves only read properties and do not modify the session.
- If a `TrackingSession` is disposed or otherwise rendered invalid after these methods are invoked, the behavior of subsequent calls is undefined; callers should ensure the session remains valid for the duration of use.
- `GetFormattedIdleTime` relies on the session's internal idle‑time counter, which may be updated only when the session is actively tracking; therefore, the returned string may not reflect idle periods that occur after the session has been stopped.
- Return values are deterministic for a given session state; they do not depend on external factors such as system clock adjustments, as they are based on monotonic timestamps stored within the session.
