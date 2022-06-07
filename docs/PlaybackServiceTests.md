# PlaybackServiceTests

Unit tests for the `PlaybackService` class, verifying playback state retrieval, timeline construction, and snapshot generation functionality. These tests validate behavior for known and unknown sessions, empty datasets, and edge cases around session state and location data.

## API

### `GetPlaybackStateAsync_ForUnknownSession_ReturnsNull`
Verifies that attempting to retrieve playback state for a non-existent session returns `null` rather than throwing an exception. Ensures graceful handling of invalid session identifiers.

**Returns**
`Task`: A task that completes when the test assertion is validated. The test asserts that the result is `null`.

### `GetActivePlaybacksAsync_WithNoActiveSessions_ReturnsEmptyList`
Ensures that querying for active playbacks when no sessions are active returns an empty collection instead of `null`. Validates correct behavior in idle system states.

**Returns**
`Task`: A task that completes when the test assertion is validated. The test asserts that the result is an empty list.

### `BuildTimelineAsync_ForSessionWithNoLocations_ReturnsNull`
Confirms that attempting to build a playback timeline for a session with no recorded locations returns `null`. Validates that missing location data is handled appropriately.

**Returns**
`Task`: A task that completes when the test assertion is validated. The test asserts that the result is `null`.

### `BuildTimelineAsync_ForSessionWithLocations_ReturnsPopulatedTimeline`
Validates that a session with recorded locations produces a populated timeline with frames corresponding to the available location data. Ensures correct timeline construction from valid input.

**Returns**
`Task`: A task that completes when the test assertion is validated. The test asserts that the result is a non-null timeline containing frames.

### `GetPlaybackStatisticsAsync_ForValidSession_ReturnsStatistics`
Checks that retrieving playback statistics for a valid session returns a populated statistics object with expected metrics. Ensures data integrity and correct aggregation.

**Returns**
`Task`: A task that completes when the test assertion is validated. The test asserts that the result contains valid statistics for the session.

### `GetSnapshotAtTimestampAsync_ForSessionWithData_ReturnsFrame`
Verifies that retrieving a snapshot at a specific timestamp for a session with data returns a valid frame. Ensures accurate state reconstruction at arbitrary points in time.

**Returns**
`Task`: A task that completes when the test assertion is validated. The test asserts that the result is a non-null frame representing the session state at the requested time.

## Usage
