# PlaybackFrameDto

The `PlaybackFrameDto` is a C# record type within the `signalr-map-realtime` project designed to serve as a Data Transfer Object (DTO) for configuring and controlling map playback sessions. It encapsulates the essential parameters required to initialize or modify a playback sequence, including session identification, temporal constraints, speed adjustments, and looping behavior. As a record, it provides immutable value semantics by default, ensuring that playback configurations remain consistent once instantiated, while the `required` modifier on key properties enforces compile-time safety for critical initialization data.

## API

The following members define the public contract of the `PlaybackFrameDto` type:

### `SessionId`
*   **Signature**: `public required int SessionId`
*   **Purpose**: Uniquely identifies the playback session associated with this frame configuration. This property is marked as `required`, meaning it must be initialized during object creation using an object initializer or constructor.
*   **Parameters**: None (Property).
*   **Return Value**: Returns an integer representing the session identifier.
*   **Throws**: Throws a `NullReferenceException` (or compiler error depending on context) if accessed before initialization, though the `required` modifier primarily enforces assignment at compile time.

### `SpeedMultiplier`
*   **Signature**: `public double SpeedMultiplier`
*   **Purpose**: Defines the rate at which the playback timeline advances relative to real-time. A value of `1.0` represents normal speed, values greater than `1.0` accelerate playback, and values between `0.0` and `1.0` slow it down.
*   **Parameters**: None (Property).
*   **Return Value**: Returns a double-precision floating-point number.
*   **Throws**: Does not throw exceptions directly; however, passing non-positive values may result in logical errors depending on the consumer implementation.

### `StartFromTimestamp`
*   **Signature**: `public DateTime? StartFromTimestamp`
*   **Purpose**: Specifies an optional absolute start time for the playback session. If set, the playback begins from this specific point in the timeline; if `null`, the session typically defaults to the beginning of the available data or the current live edge.
*   **Parameters**: None (Property).
*   **Return Value**: Returns a nullable `DateTime` structure.
*   **Throws**: Does not throw exceptions.

### `Loop`
*   **Signature**: `public bool Loop`
*   **Purpose**: Determines whether the playback sequence should automatically restart upon reaching the end of the timeline. When `true`, the session repeats indefinitely; when `false`, it stops after the final frame.
*   **Parameters**: None (Property).
*   **Return Value**: Returns a boolean value.
*   **Throws**: Does not throw exceptions.

## Usage

The following examples demonstrate realistic instantiation and usage patterns for `PlaybackFrameDto` within a C# application.

### Example 1: Initializing a Standard Playback Session
This example creates a new playback configuration for a specific session, setting a custom speed and enabling looping, while leaving the start timestamp as default.

```csharp
using SignalRMapRealtime.Models;

// Initialize a playback frame for session 1024
var playbackConfig = new PlaybackFrameDto
{
    SessionId = 1024,
    SpeedMultiplier = 2.5, // Play at 2.5x speed
    Loop = true,
    StartFromTimestamp = null // Start from the beginning
};

// The record can be passed to a service handling SignalR transmission
await playbackService.StartPlaybackAsync(playbackConfig);
```

### Example 2: Resuming Playback from a Specific Timestamp
This example demonstrates how to configure a session to resume from a specific historical point without looping.

```csharp
using SignalRMapRealtime.Models;

var specificTime = new DateTime(2023, 10, 27, 14, 30, 0, DateTimeKind.Utc);

var resumeConfig = new PlaybackFrameDto
{
    SessionId = 559,
    SpeedMultiplier = 1.0, // Real-time speed
    StartFromTimestamp = specificTime,
    Loop = false // Stop when data ends
};

// Validate configuration before sending
if (resumeConfig.StartFromTimestamp.HasValue)
{
    Console.WriteLine($"Resuming session {resumeConfig.SessionId} from {resumeConfig.StartFromTimestamp.Value}");
}
```

## Notes

*   **Immutability**: As a `record` type, `PlaybackFrameDto` is immutable by default. Modifying any property after initialization requires creating a new instance using the `with` expression (e.g., `var updated = config with { SpeedMultiplier = 3.0 };`). This ensures thread-safety for read operations across concurrent consumers without requiring explicit locking mechanisms.
*   **Required Initialization**: The `SessionId` property is marked as `required`. Failure to initialize this property in the object initializer will result in a compile-time error, preventing runtime failures related to missing session context.
*   **Nullable Timestamps**: Consumers of `StartFromTimestamp` must explicitly check the `HasValue` property before accessing the underlying `DateTime` value to avoid `InvalidOperationException`.
*   **Speed Validation**: While the type system allows any `double` for `SpeedMultiplier`, logic consuming this DTO should validate that the value is strictly greater than zero to prevent infinite loops or stalled timelines in the playback engine.
