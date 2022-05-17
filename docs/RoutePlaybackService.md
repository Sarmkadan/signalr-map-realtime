# RoutePlaybackService

Manages the lifecycle and state of a single route playback session. It provides controls to start, pause, resume, stop, and seek within a recorded route, while exposing real-time playback state and computed timeline data. The service operates on a predefined sequence of locations and supports configurable playback speed and looping.

## API

### Constructors

```csharp
public RoutePlaybackService(int trackingSessionId, IReadOnlyList<Location> locations, bool loop = false)
```

Creates a new playback session bound to the given tracking session and route geometry. The `locations` list defines the path to replay; `loop` controls whether playback restarts automatically upon reaching the end. A new `PlaybackId` is assigned internally.

### Properties

```csharp
public Guid PlaybackId { get; }
```

Unique identifier for this playback session, generated on construction.

```csharp
public int TrackingSessionId { get; }
```

Identifier of the original tracking session whose recorded route is being replayed.

```csharp
public IReadOnlyList<Location> Locations { get; }
```

The ordered list of geographic positions that constitute the route. This sequence is immutable for the lifetime of the service.

```csharp
public double[] CumulativeDistances { get; }
```

Precomputed cumulative distances in meters from the first location to each subsequent location. The array length matches `Locations.Count`; the first element is always `0.0`.

```csharp
public DateTime StartedAt { get; }
```

UTC timestamp indicating when playback was most recently started or resumed. Remains unchanged while paused.

```csharp
public bool Loop { get; }
```

Indicates whether the playback will automatically restart from the beginning after reaching the final location.

```csharp
public CancellationTokenSource Cts { get; }
```

Token source used to cancel internal playback operations. Disposed when the service is disposed.

### Methods

```csharp
public async Task<Guid> StartPlaybackAsync(DateTime? startFrom = null)
```

Begins playback from the specified UTC timestamp, or from `StartedAt` if `null`. Returns the `PlaybackId` of the session. Throws `InvalidOperationException` if playback is already running.

```csharp
public Task PausePlaybackAsync()
```

Suspends playback progress while preserving the current position and state. Has no effect if already paused or stopped. Does not throw.

```csharp
public Task ResumePlaybackAsync()
```

Resumes playback from the point where it was paused. Has no effect if already running or stopped. Does not throw.

```csharp
public async Task StopPlaybackAsync()
```

Terminates playback and resets internal state. The session can be restarted afterward via `StartPlaybackAsync`. Does not throw.

```csharp
public Task SeekToTimestampAsync(DateTime targetTimestamp)
```

Jumps playback position to the point along the route corresponding to `targetTimestamp`. If the timestamp falls before the route start, the position snaps to the first location; if after the route end, it snaps to the last location (or wraps when `Loop` is `true`). Throws `ArgumentOutOfRangeException` if `targetTimestamp` is `default(DateTime)`.

```csharp
public Task SetPlaybackSpeedAsync(double speedMultiplier)
```

Adjusts the playback rate relative to real time. A value of `1.0` represents original speed, `2.0` double speed, `0.5` half speed. Values must be greater than `0.0`. Throws `ArgumentOutOfRangeException` for zero or negative multipliers.

```csharp
public Task<PlaybackSessionDto?> GetPlaybackStateAsync()
```

Returns a snapshot of the current playback state for this session, or `null` if the session has been disposed. The DTO includes `PlaybackId`, status, current position, speed, and timestamp.

```csharp
public Task<IReadOnlyList<PlaybackSessionDto>> GetActivePlaybacksAsync()
```

Returns state snapshots for all active playback sessions managed by the underlying system. This is a static-like operation scoped to the broader playback infrastructure, not limited to the current instance.

```csharp
public async Task<RouteTimelineDto?> BuildTimelineAsync()
```

Constructs a timeline representation of the entire route, including interpolated points and cumulative timestamps. Returns `null` if `Locations` is empty or the service is disposed. The result is suitable for rendering a full-route preview.

```csharp
public async Task<PlaybackFrameDto?> GetSnapshotAtTimestampAsync(DateTime timestamp)
```

Computes the interpolated position, bearing, and metadata at the given `timestamp` along the route. Returns `null` if the timestamp is outside the route bounds and `Loop` is `false`, or if the service is disposed.

```csharp
public async Task<PlaybackStatisticsDto?> GetPlaybackStatisticsAsync()
```

Aggregates statistics for the current session, such as total distance, elapsed playback time, average speed, and completion percentage. Returns `null` if the session has not started or is disposed.

```csharp
public async ValueTask DisposeAsync()
```

Cancels any ongoing playback, releases the internal `CancellationTokenSource`, and cleans up resources. After disposal, most methods return `null` or become no-ops. This method is idempotent.

## Usage

### Example 1: Basic Playback with Seek and Speed Control

```csharp
var locations = LoadRouteLocations(trackingSessionId: 42);
var service = new RoutePlaybackService(42, locations, loop: false);

// Start playback from the original recording start time.
Guid playbackId = await service.StartPlaybackAsync();

// After some time, speed up and jump to a specific moment.
await service.SetPlaybackSpeedAsync(2.0);
await service.SeekToTimestampAsync(new DateTime(2025, 3, 15, 10, 30, 0, DateTimeKind.Utc));

// Inspect current state.
PlaybackSessionDto? state = await service.GetPlaybackStateAsync();
Console.WriteLine($"Status: {state?.Status}, Speed: {state?.SpeedMultiplier}");

// Stop and dispose.
await service.StopPlaybackAsync();
await service.DisposeAsync();
```

### Example 2: Looping Playback with Timeline and Statistics

```csharp
var locations = LoadRouteLocations(trackingSessionId: 99);
var service = new RoutePlaybackService(99, locations, loop: true);

await service.StartPlaybackAsync();

// Build a full-route timeline for UI rendering.
RouteTimelineDto? timeline = await service.BuildTimelineAsync();
RenderTimelineChart(timeline);

// Periodically poll statistics.
while (true)
{
    PlaybackStatisticsDto? stats = await service.GetPlaybackStatisticsAsync();
    if (stats?.CompletionPercentage >= 100.0 && !service.Loop)
        break;

    UpdateDashboard(stats);
    await Task.Delay(1000);
}

await service.DisposeAsync();
```

## Notes

- **Thread safety:** Instance members are not designed for concurrent use from multiple threads. Consumers should serialize calls to control methods (`Start`, `Pause`, `Resume`, `Stop`, `Seek`, `SetSpeed`) or synchronize externally.
- **Disposal state:** After `DisposeAsync`, the internal `Cts` is cancelled and disposed. Methods that return nullable DTOs (`GetPlaybackStateAsync`, `BuildTimelineAsync`, `GetSnapshotAtTimestampAsync`, `GetPlaybackStatisticsAsync`) return `null` rather than throwing.
- **Empty route handling:** If `Locations` is empty, `BuildTimelineAsync` returns `null`, and playback control methods may complete immediately without progressing position.
- **Timestamp boundaries:** `SeekToTimestampAsync` clamps or wraps the target timestamp based on the `Loop` property. `GetSnapshotAtTimestampAsync` returns `null` for out-of-bounds timestamps when `Loop` is `false`.
- **Speed multiplier:** `SetPlaybackSpeedAsync` with a value of `0.0` or negative throws. Extremely small positive values (e.g., `1e-6`) are technically valid but may cause playback to appear stalled.
- **`GetActivePlaybacksAsync`:** This method reflects global playback state across all sessions, not just the current instance. Its result may include sessions created by other components.
