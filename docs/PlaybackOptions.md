# PlaybackOptions
The `PlaybackOptions` type in the `signalr-map-realtime` project provides a set of configuration options for customizing the playback of location data in real-time mapping applications. These options allow developers to fine-tune the performance and behavior of the playback feature, including speed multipliers, frame intervals, idle thresholds, and session timeouts.

## API
The `PlaybackOptions` type exposes the following public members:
* `MaxConcurrentPlaybacks`: An integer specifying the maximum number of concurrent playback sessions allowed.
* `DefaultSpeedMultiplier`: A double representing the default speed multiplier applied to playback sessions.
* `MaxSpeedMultiplier`: A double specifying the maximum allowed speed multiplier for playback sessions.
* `MinSpeedMultiplier`: A double specifying the minimum allowed speed multiplier for playback sessions.
* `MinFrameIntervalMs`: An integer representing the minimum interval between frames in milliseconds.
* `MaxFrameIntervalMs`: An integer representing the maximum interval between frames in milliseconds.
* `IdleSpeedThresholdKmh`: A double specifying the speed threshold below which a playback session is considered idle, in kilometers per hour.
* `IdleMinDurationSeconds`: An integer representing the minimum duration for which a playback session must be idle before it is considered inactive.
* `SpeedAlertThresholdKmh`: A double specifying the speed threshold above which a speed alert is triggered, in kilometers per hour.
* `MaxLocationsPerPlayback`: An integer specifying the maximum number of locations that can be played back in a single session.
* `PlaybackSessionTimeoutMinutes`: An integer representing the timeout period for playback sessions, in minutes.

## Usage
Here are two examples of using the `PlaybackOptions` type in C#:
```csharp
// Example 1: Configuring playback options for a real-time mapping application
PlaybackOptions options = new PlaybackOptions
{
    MaxConcurrentPlaybacks = 5,
    DefaultSpeedMultiplier = 1.5,
    MinFrameIntervalMs = 100,
    MaxFrameIntervalMs = 500,
    IdleSpeedThresholdKmh = 5.0,
    IdleMinDurationSeconds = 30
};

// Example 2: Customizing playback behavior for a specific use case
PlaybackOptions customOptions = new PlaybackOptions
{
    MaxSpeedMultiplier = 2.0,
    MinSpeedMultiplier = 0.5,
    SpeedAlertThresholdKmh = 80.0,
    MaxLocationsPerPlayback = 100,
    PlaybackSessionTimeoutMinutes = 10
};
```

## Notes
When using the `PlaybackOptions` type, consider the following edge cases and thread-safety remarks:
* The `MaxConcurrentPlaybacks` value should be carefully chosen to avoid overwhelming the system with too many concurrent playback sessions.
* The `DefaultSpeedMultiplier` and `MaxSpeedMultiplier` values can significantly impact the performance of the playback feature, and should be tuned accordingly.
* The `IdleSpeedThresholdKmh` and `IdleMinDurationSeconds` values determine when a playback session is considered idle, and can affect the overall behavior of the application.
* The `PlaybackOptions` type is not thread-safe by default, and should be properly synchronized when accessed from multiple threads.
* The `PlaybackSessionTimeoutMinutes` value should be chosen based on the specific requirements of the application, taking into account factors such as user engagement and system resources.
