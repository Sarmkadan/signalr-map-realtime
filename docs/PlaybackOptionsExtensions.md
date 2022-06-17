# PlaybackOptionsExtensions

Static utility class providing helper methods for adjusting and evaluating playback speed and timing options in real-time data visualization scenarios.

## API

### `ClampSpeedMultiplier`
Ensures a speed multiplier value remains within the valid operational range for playback systems.

- **Return value**: `double` – The clamped speed multiplier, guaranteed to be within the acceptable bounds for real-time playback.
- **Notes**: If the input value is outside the valid range, it is silently adjusted to the nearest boundary value without raising an exception.

### `IsRealTime`
Determines whether the current playback configuration is operating in real-time mode.

- **Return value**: `bool` – `true` if the playback is configured for real-time operation; otherwise, `false`.
- **Notes**: The method evaluates internal playback state flags and does not perform any I/O or external checks.

### `CalculateFrameIntervalMs`
Computes the target frame interval in milliseconds based on the current playback speed and timing constraints.

- **Return value**: `int` – The calculated frame interval in milliseconds, suitable for use in timing loops or frame scheduling.
- **Notes**: The result may be zero or negative if the speed multiplier exceeds safe thresholds; callers should validate the return value before use.

### `IsSpeedAlert`
Indicates whether the current playback speed has triggered a performance or stability alert threshold.

- **Return value**: `bool` – `true` if the playback speed is outside normal operating parameters; otherwise, `false`.
- **Notes**: This flag is typically used to trigger UI warnings or automatic speed adjustments.

### `IsIdle`
Checks whether the playback system is currently in an idle state with no active data processing.

- **Return value**: `bool` – `true` if the playback is idle; otherwise, `false`.
- **Notes**: Idle detection may depend on internal timers or activity counters and is not guaranteed to reflect instantaneous state.

## Usage
