# DomainModelBehaviorTests

Unit tests for domain model behaviors in the signalr-map-realtime project, validating core business rules for vehicle tracking, speed limit monitoring, and session management. These tests ensure that domain entities (`Vehicle`, `TrackingSession`, `Asset`) behave as expected under various conditions, including edge cases and invalid inputs.

## API

### `Vehicle_HasExceededSpeedLimit_WhenCurrentSpeedExceedsMax_ReturnsTrue`
Validates that a `Vehicle` correctly identifies when its current speed exceeds the configured maximum speed limit.

- **Parameters**: None
- **Return value**: `void`
- **Behavior**: Executes a test case where the vehicle's current speed is set above the maximum speed limit. Asserts that the `HasExceededSpeedLimit` method returns `true`.
- **Throws**: Does not throw under normal conditions. Fails the test if the assertion is not met.

---

### `Vehicle_HasExceededSpeedLimit_WhenMaxSpeedNotConfigured_ReturnsFalse`
Ensures that a `Vehicle` without a configured maximum speed limit does not falsely report speeding.

- **Parameters**: None
- **Return value**: `void`
- **Behavior**: Executes a test case where the vehicle's maximum speed limit is not set. Asserts that the `HasExceededSpeedLimit` method returns `false`.
- **Throws**: Does not throw under normal conditions. Fails the test if the assertion is not met.

---
### `TrackingSession_StartSession_SetsActiveStatusAndRecordsStartTime`
Verifies that starting a tracking session correctly updates the session state and records the initiation timestamp.

- **Parameters**: None
- **Return value**: `void`
- **Behavior**: Executes a test case where a new `TrackingSession` is started. Asserts that the session's `IsActive` property is set to `true` and that the `StartTime` is recorded with a non-null value.
- **Throws**: Does not throw under normal conditions. Fails the test if the session remains inactive or the start time is not set.

---
### `TrackingSession_RecordLocation_WhenSessionIsPending_ThrowsInvalidOperationException`
Confirms that recording a location on a non-active tracking session throws an appropriate exception.

- **Parameters**: None
- **Return value**: `void`
- **Behavior**: Executes a test case where `RecordLocation` is called on a `TrackingSession` that has not been started (i.e., `IsActive` is `false`). Asserts that an `InvalidOperationException` is thrown.
- **Throws**: `InvalidOperationException` when the session is not active.

---
### `Asset_EnableSpecialHandling_WithEmptyInstructions_ThrowsArgumentException`
Ensures that enabling special handling for an `Asset` with empty instructions throws an `ArgumentException`.

- **Parameters**: None
- **Return value**: `void`
- **Behavior**: Executes a test case where `EnableSpecialHandling` is called with an empty string for instructions. Asserts that an `ArgumentException` is thrown.
- **Throws**: `ArgumentException` when the instructions parameter is null or whitespace.

## Usage
