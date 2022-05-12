# TrackingService

The `TrackingService` class provides the core asynchronous API for managing real-time tracking sessions within the `signalr-map-realtime` project. It handles the full lifecycle of a session, including initialization, state transitions (pause, resume, complete, cancel), and retrieval of historical or active session data. Additionally, it offers calculated metrics such as total distance and average speed for completed or active sessions, serving as the primary interface between the SignalR real-time layer and the underlying session storage.

## API

### `public TrackingService`
Initializes a new instance of the `TrackingService` class. This constructor typically resolves required dependencies for session storage and real-time communication via dependency injection.

### `public async Task<int> StartTrackingSessionAsync`
Initiates a new tracking session and returns the unique identifier for that session.
*   **Parameters**: None explicitly listed in the signature; configuration is typically handled via dependency injection or context.
*   **Return Value**: A `Task<int>` representing the asynchronous operation, containing the newly created session ID upon completion.
*   **Exceptions**: May throw if the underlying storage provider is unavailable or if system limits on concurrent sessions are reached.

### `public async Task<bool> PauseSessionAsync`
Temporarily suspends data recording for an active session without terminating it.
*   **Parameters**: Implicitly targets the current or specified active session context.
*   **Return Value**: A `Task<bool>` indicating success (`true`) or failure (`false`) if the session was not found or already paused.
*   **Exceptions**: May throw if the session state machine does not allow a transition to the paused state.

### `public async Task<bool> ResumeSessionAsync`
Resumes data recording for a previously paused session.
*   **Parameters**: Implicitly targets the specific paused session context.
*   **Return Value**: A `Task<bool>` indicating success (`true`) or failure (`false`) if the session was not found or not in a paused state.
*   **Exceptions**: May throw if the session has expired or been terminated while paused.

### `public async Task<bool> CompleteSessionAsync`
Finalizes a session, marking it as successfully finished and triggering any final calculations (e.g., total distance).
*   **Parameters**: Implicitly targets the active session context.
*   **Return Value**: A `Task<bool>` indicating success (`true`) or failure (`false`) if the session could not be completed.
*   **Exceptions**: May throw if the session is not active or paused.

### `public async Task<bool> CancelSessionAsync`
Terminates a session immediately, marking it as cancelled and discarding or archiving partial data based on business rules.
*   **Parameters**: Implicitly targets the active session context.
*   **Return Value**: A `Task<bool>` indicating success (`true`) or failure (`false`) if the session could not be cancelled.
*   **Exceptions**: May throw if the session is already in a terminal state (Completed or Cancelled).

### `public async Task<object?> GetActiveSessionAsync`
Retrieves the data object for the currently active session, if one exists.
*   **Parameters**: None.
*   **Return Value**: A `Task<object?>` containing the session data if a session is active, or `null` if no session is currently running.
*   **Exceptions**: Generally does not throw for missing sessions (returns `null`), but may throw on data access errors.

### `public async Task<IEnumerable<object>> GetSessionHistoryAsync`
Retrieves a collection of all past sessions regardless of their final status.
*   **Parameters**: None.
*   **Return Value**: A `Task<IEnumerable<object>>` containing a list of historical session objects. Returns an empty enumerable if no history exists.
*   **Exceptions**: May throw if the history store is inaccessible.

### `public async Task<IEnumerable<object>> GetSessionsByStatusAsync`
Filters and retrieves sessions based on a specific status criteria.
*   **Parameters**: Implicitly accepts a status filter (logic internal to implementation or context).
*   **Return Value**: A `Task<IEnumerable<object>>` containing sessions matching the requested status.
*   **Exceptions**: May throw if the status filter is invalid or data access fails.

### `public async Task<IEnumerable<object>> GetExpiredSessionsAsync`
Retrieves sessions that have timed out or exceeded their maximum allowed duration without being explicitly completed.
*   **Parameters**: None.
*   **Return Value**: A `Task<IEnumerable<object>>` containing a list of expired session objects.
*   **Exceptions**: May throw if the expiration logic encounters a system error.

### `public async Task<bool> IsSessionActiveAsync`
Checks the current state to determine if a session is actively tracking.
*   **Parameters**: None.
*   **Return Value**: A `Task<bool>` returning `true` if a session is active, otherwise `false`.
*   **Exceptions**: Unlikely to throw; returns `false` on error conditions unless the error is critical.

### `public async Task<object?> GetSessionDetailsAsync`
Fetches detailed information for a specific session ID.
*   **Parameters**: Implicitly identifies the target session (likely via context or internal state).
*   **Return Value**: A `Task<object?>` containing the detailed session object, or `null` if the session ID is not found.
*   **Exceptions**: May throw if the session ID format is invalid or data corruption is detected.

### `public async Task<double> GetSessionDistanceAsync`
Calculates and returns the total distance covered during a specific session.
*   **Parameters**: Implicitly targets the session context.
*   **Return Value**: A `Task<double>` representing the distance (typically in kilometers or miles, depending on system configuration). Returns `0.0` if no data is available.
*   **Exceptions**: May throw if the geometric calculation fails due to malformed coordinate data.

### `public async Task<double> GetSessionAverageSpeedAsync`
Calculates and returns the average speed maintained during a specific session.
*   **Parameters**: Implicitly targets the session context.
*   **Return Value**: A `Task<double>` representing the average speed. Returns `0.0` if the session duration is zero or data is missing.
*   **Exceptions**: May throw if division by zero occurs internally due to invalid timestamps, though typically handled to return `0.0`.

## Usage

### Example 1: Managing a Full Session Lifecycle
This example demonstrates starting a session, pausing it during a break, resuming, and finally completing it while retrieving the final distance.

```csharp
public async Task RunTrackingLifecycleAsync(TrackingService tracker)
{
    // Start a new session and capture the ID
    int sessionId = await tracker.StartTrackingSessionAsync();
    Console.WriteLine($"Session started: {sessionId}");

    // Simulate tracking activity...
    await Task.Delay(5000);

    // Pause the session
    bool paused = await tracker.PauseSessionAsync();
    if (paused)
    {
        Console.WriteLine("Session paused.");
        await Task.Delay(2000); // Simulate break
        
        // Resume the session
        await tracker.ResumeSessionAsync();
        Console.WriteLine("Session resumed.");
    }

    // Complete the session
    bool completed = await tracker.CompleteSessionAsync();
    if (completed)
    {
        double totalDistance = await tracker.GetSessionDistanceAsync();
        Console.WriteLine($"Session completed. Total distance: {totalDistance:F2} km");
    }
}
```

### Example 2: Auditing Active and Expired Sessions
This example checks for an active session and retrieves a list of expired sessions for cleanup or reporting.

```csharp
public async Task AuditSessionsAsync(TrackingService tracker)
{
    // Check if a user currently has an active session
    bool isActive = await tracker.IsSessionActiveAsync();
    if (isActive)
    {
        var activeData = await tracker.GetActiveSessionAsync();
        if (activeData != null)
        {
            Console.WriteLine("Active session detected.");
        }
    }
    else
    {
        Console.WriteLine("No active session found.");
    }

    // Retrieve all expired sessions for archival
    var expiredSessions = await tracker.GetExpiredSessionsAsync();
    int count = expiredSessions.Count();
    
    Console.WriteLine($"Found {count} expired sessions requiring attention.");
    
    // Optionally fetch full history
    var allHistory = await tracker.GetSessionHistoryAsync();
    Console.WriteLine($"Total historical records: {allHistory.Count()}");
}
```

## Notes

*   **Thread Safety**: As an asynchronous service interacting with shared state (active sessions) and external storage, `TrackingService` implementations should be treated as thread-safe for read operations. However, state-modifying methods (`Start`, `Pause`, `Resume`, `Complete`, `Cancel`) should not be called concurrently for the same session ID without external synchronization, as race conditions may lead to invalid state transitions.
*   **Null Handling**: Methods returning single objects (`GetActiveSessionAsync`, `GetSessionDetailsAsync`) return `null` rather than throwing exceptions when the requested resource is not found. Callers must handle null checks appropriately.
*   **Metric Accuracy**: `GetSessionDistanceAsync` and `GetSessionAverageSpeedAsync` rely on the accumulation of coordinate data points. If a session is cancelled early or has sparse data, these methods may return `0.0` or approximate values. These calculations are typically performed on the server side upon request or finalization.
*   **State Transitions**: Attempting to `Resume` a session that is not `Paused`, or `Complete` a session that is `Cancelled`, will result in the method returning `false`. It is recommended to check `IsSessionActiveAsync` or retrieve session details before attempting state changes in high-concurrency scenarios.
*   **Expiration Logic**: `GetExpiredSessionsAsync` relies on a server-side time threshold. Sessions that are active but idle for longer than the configured timeout will appear in this collection and may be automatically cleaned up by background processes depending on the host configuration.
