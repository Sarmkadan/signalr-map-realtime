# TrackingServiceExtensions

`TrackingServiceExtensions` provides a comprehensive set of static extension methods designed to simplify the management and analysis of tracking sessions within the `signalr-map-realtime` project. These methods offer a high-level interface for controlling the lifecycle of tracking sessions—including initiation, suspension, resumption, completion, and cancellation—and provide mechanisms for retrieving historical session data, filtering sessions by status, and calculating analytical metrics such as total distance traveled and average speed.

## API

### StartTrackingSessionAsync
Initiates a new tracking session.
*   **Parameters:** None.
*   **Returns:** A `Task<int>` representing the unique identifier of the newly created session.
*   **Throws:** Throws an exception if the underlying storage or tracking service is unavailable.

### GetActiveSessionAsync
Retrieves the currently active tracking session.
*   **Parameters:** None.
*   **Returns:** A `Task<TrackingSession?>` containing the active session, or `null` if no session is currently active.
*   **Throws:** Throws if the service encounters a data access error.

### GetSessionHistoryAsync
Retrieves the complete history of all tracking sessions.
*   **Parameters:** None.
*   **Returns:** A `Task<IReadOnlyList<TrackingSession>>` containing a read-only list of all historical sessions.
*   **Throws:** Throws if data retrieval fails.

### GetSessionsByStatusAsync
Retrieves all sessions matching a specified status.
*   **Parameters:** `string status` (The status identifier to filter by).
*   **Returns:** A `Task<IReadOnlyList<TrackingSession>>` containing sessions that match the provided status.
*   **Throws:** Throws if the status lookup fails.

### TryPauseSessionAsync
Attempts to pause an active tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session to pause).
*   **Returns:** A `Task<bool>` that resolves to `true` if the session was successfully paused, `false` otherwise.
*   **Throws:** Throws if the session identifier is invalid or the service state prevents the operation.

### TryResumeSessionAsync
Attempts to resume a previously paused tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session to resume).
*   **Returns:** A `Task<bool>` that resolves to `true` if the session was successfully resumed, `false` otherwise.
*   **Throws:** Throws if the session identifier is invalid.

### TryCompleteSessionAsync
Attempts to complete a tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session to complete).
*   **Returns:** A `Task<bool>` that resolves to `true` if the session was successfully completed, `false` otherwise.
*   **Throws:** Throws if the session cannot be completed in its current state.

### TryCancelSessionAsync
Attempts to cancel an active or paused tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session to cancel).
*   **Returns:** A `Task<bool>` that resolves to `true` if the session was successfully canceled, `false` otherwise.
*   **Throws:** Throws if the session cannot be canceled.

### GetSessionDetailsAsync
Retrieves detailed information for a specific tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session).
*   **Returns:** A `Task<TrackingSession?>` containing the session details, or `null` if the session is not found.
*   **Throws:** Throws if data access fails.

### GetExpiredSessionsAsync
Retrieves sessions that have been flagged as expired.
*   **Parameters:** None.
*   **Returns:** A `Task<IReadOnlyList<TrackingSession>>` containing a list of expired sessions.
*   **Throws:** Throws if data retrieval fails.

### GetSessionDistanceAsync
Calculates the total distance traveled for a given tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session).
*   **Returns:** A `Task<double>` representing the total distance in meters.
*   **Throws:** Throws if the session identifier is invalid or calculation data is corrupted.

### GetSessionAverageSpeedAsync
Calculates the average speed for a given tracking session.
*   **Parameters:** `int sessionId` (The unique identifier of the session).
*   **Returns:** A `Task<double>` representing the average speed in meters per second (m/s).
*   **Throws:** Throws if the session identifier is invalid or calculation data is corrupted.

## Usage

### Example 1: Managing a Tracking Session Lifecycle
```csharp
// Start a new session
int sessionId = await TrackingServiceExtensions.StartTrackingSessionAsync();

// Perform operations
bool isPaused = await TrackingServiceExtensions.TryPauseSessionAsync(sessionId);

if (isPaused)
{
    // Resume session
    await TrackingServiceExtensions.TryResumeSessionAsync(sessionId);
}

// Complete the session
await TrackingServiceExtensions.TryCompleteSessionAsync(sessionId);
```

### Example 2: Analyzing Session Metrics
```csharp
// Get a specific session to analyze
TrackingSession? session = await TrackingServiceExtensions.GetSessionDetailsAsync(targetSessionId);

if (session != null)
{
    double distance = await TrackingServiceExtensions.GetSessionDistanceAsync(session.Id);
    double avgSpeed = await TrackingServiceExtensions.GetSessionAverageSpeedAsync(session.Id);
    
    Console.WriteLine($"Session {session.Id}: Distance={distance}m, Avg Speed={avgSpeed}m/s");
}
```

## Notes

*   **Thread Safety:** While these methods are `async` and designed for concurrent application environments, they depend on the thread safety of the underlying service provider being extended. Ensure that the injected tracking service infrastructure is configured for concurrent access (e.g., singleton service with thread-safe data stores).
*   **Nullability:** Methods returning `Task<TrackingSession?>` may return `null` if the requested session does not exist or if no active session is found. Always check for null before accessing members of the returned `TrackingSession` object.
*   **Error Handling:** These methods generally throw exceptions for operational failures (e.g., database connection issues, invalid service state). Implement robust `try-catch` blocks around these calls when handling critical business logic.
*   **Operation Atomicity:** The `Try...` methods (e.g., `TryPauseSessionAsync`) are intended to be atomic operations from the perspective of the service layer. However, they may still fail if the session status changes concurrently due to another request. Always handle the boolean result of these operations to ensure consistent application state.
