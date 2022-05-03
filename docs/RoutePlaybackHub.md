# RoutePlaybackHub

`RoutePlaybackHub` is a SignalR hub class responsible for managing and orchestrating real-time route playback sessions. It provides a server-side interface for connected clients to control playback operations—such as starting, pausing, resuming, and seeking—while also enabling clients to subscribe to telemetry updates and query the state, timeline, snapshots, and statistics of a specific playback session.

## API

### Constructor
*   `public RoutePlaybackHub()`
    Initializes a new instance of the `RoutePlaybackHub` class.

### Lifecycle Methods
*   `public override async Task OnConnectedAsync()`
    Invoked by SignalR when a client connects to the hub. Handles necessary initialization for the connection context.
*   `public override async Task OnDisconnectedAsync(Exception? exception)`
    Invoked by SignalR when a client disconnects. Ensures cleanup of active subscriptions and resources associated with the connection.

### Control Methods
*   `public async Task StartPlayback(string routeId)`
    Starts the playback for the specified route. May throw an exception if the `routeId` is invalid or if playback is already active.
*   `public async Task PausePlayback()`
    Pauses the currently active playback session.
*   `public async Task ResumePlayback()`
    Resumes a previously paused playback session.
*   `public async Task StopPlayback()`
    Terminates the current playback session and resets the state.
*   `public async Task SeekTo(TimeSpan position)`
    Moves the playback cursor to the specified `position` within the timeline.
*   `public async Task SetSpeed(double speed)`
    Adjusts the playback rate. The `speed` parameter typically represents a multiplier (e.g., 1.0 for normal speed).

### Subscription and Retrieval Methods
*   `public async Task SubscribeToPlayback(string routeId)`
    Registers the connection to receive real-time updates for the specified route.
*   `public async Task UnsubscribeFromPlayback(string routeId)`
    Removes the connection from receiving updates for the specified route.
*   `public async Task<PlaybackState> GetPlaybackState()`
    Returns the current status of the playback session (e.g., Playing, Paused, Stopped).
*   `public async Task<Timeline> RequestTimeline()`
    Retrieves the full timeline data for the active playback session.
*   `public async Task<Snapshot> RequestSnapshot()`
    Retrieves a snapshot of the current playback position and state.
*   `public async Task<PlaybackStatistics> RequestStatistics()`
    Retrieves performance metrics and statistics related to the current playback session.

## Usage

### Client-side SignalR connection
```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("/routePlaybackHub")
    .Build();

await connection.StartAsync();

// Subscribe to playback updates for a specific route
await connection.InvokeAsync("SubscribeToPlayback", "route-123");

// Control playback
await connection.InvokeAsync("StartPlayback", "route-123");
await connection.InvokeAsync("SetSpeed", 2.0);
```

### Retrieving state and data
```csharp
// Get the current playback state and timeline from the server
var state = await connection.InvokeAsync<PlaybackState>("GetPlaybackState");
var timeline = await connection.InvokeAsync<Timeline>("RequestTimeline");

if (state == PlaybackState.Paused)
{
    await connection.InvokeAsync("ResumePlayback");
}
```

## Notes

*   **Thread Safety:** SignalR hubs are scoped to the lifetime of a single client connection, and each method call runs in its own context. However, the underlying services or data stores that `RoutePlaybackHub` interacts with must be thread-safe to handle concurrent requests from multiple clients or asynchronous operations.
*   **Edge Cases:**
    *   Invoking control methods (`StartPlayback`, `PausePlayback`, etc.) without an active session or a valid subscription may result in errors or no-op behavior, depending on the implementation state.
    *   Rapid, consecutive calls to `SeekTo` or `SetSpeed` may introduce latency or race conditions; it is recommended to implement client-side throttling or debouncing.
    *   If a client disconnects unexpectedly, `OnDisconnectedAsync` is responsible for ensuring the client is unsubscribed from all active sessions to prevent memory leaks or ghost subscriptions.
