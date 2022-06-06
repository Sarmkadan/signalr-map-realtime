# PlaybackController

Provides endpoints for managing real-time playback sessions of map data, including starting, stopping, and querying session state, retrieving timeline snapshots, and collecting playback statistics.

## API

### `public PlaybackController`

Initializes a new instance of the `PlaybackController` with required services for session management and data access.

### `public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)`

Starts a new playback session with the provided configuration.

- **Parameters**:
  - `request` (`StartSessionRequest`): Contains session parameters such as session name, playback speed, and data source identifiers.
- **Return value**:
  - `201 Created` with the newly created `SessionInfo` on success.
  - `400 Bad Request` if the request is invalid or session already exists.
- **Exceptions**:
  - Throws `ArgumentException` if required parameters are missing or invalid.

### `public async Task<IActionResult> GetActiveSessions()`

Retrieves a list of currently active playback sessions.

- **Return value**:
  - `200 OK` with a collection of `SessionInfo` objects representing active sessions.
  - `204 No Content` if no active sessions exist.

### `public async Task<IActionResult> GetSessionState([FromRoute] string sessionId)`

Retrieves the current state of the specified playback session.

- **Parameters**:
  - `sessionId` (`string`): Unique identifier of the session to query.
- **Return value**:
  - `200 OK` with the `SessionState` object on success.
  - `404 Not Found` if the session does not exist.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sessionId` is null or empty.

### `public async Task<IActionResult> StopSession([FromRoute] string sessionId)`

Stops the specified playback session and releases associated resources.

- **Parameters**:
  - `sessionId` (`string`): Unique identifier of the session to stop.
- **Return value**:
  - `200 OK` with confirmation on success.
  - `404 Not Found` if the session does not exist.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sessionId` is null or empty.

### `public async Task<IActionResult> GetTimeline([FromRoute] string sessionId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)`

Retrieves a timeline of events within the specified time range for the given session.

- **Parameters**:
  - `sessionId` (`string`): Unique identifier of the session.
  - `from` (`DateTime?`): Optional start of the time range.
  - `to` (`DateTime?`): Optional end of the time range.
- **Return value**:
  - `200 OK` with a collection of timeline events.
  - `404 Not Found` if the session does not exist.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sessionId` is null or empty.

### `public async Task<IActionResult> GetSnapshot([FromRoute] string sessionId, [FromQuery] DateTime timestamp)`

Retrieves a snapshot of the map state at the specified timestamp for the given session.

- **Parameters**:
  - `sessionId` (`string`): Unique identifier of the session.
  - `timestamp` (`DateTime`): Point in time for the snapshot.
- **Return value**:
  - `200 OK` with the snapshot data.
  - `404 Not Found` if the session does not exist or timestamp is out of range.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sessionId` is null or empty.
  - Throws `ArgumentOutOfRangeException` if `timestamp` is invalid.

### `public async Task<IActionResult> GetStatistics([FromRoute] string sessionId)`

Retrieves aggregated playback statistics for the specified session.

- **Parameters**:
  - `sessionId` (`string`): Unique identifier of the session.
- **Return value**:
  - `200 OK` with the statistics object.
  - `404 Not Found` if the session does not exist.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sessionId` is null or empty.

## Usage

### Starting a Playback Session
