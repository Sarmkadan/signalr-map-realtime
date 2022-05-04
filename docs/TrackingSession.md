# TrackingSession

The `TrackingSession` class represents a recorded tracking session for a vehicle within the `signalr-map-realtime` project. It captures telemetry data such as location history, speed metrics, and session state (e.g., active, paused, or completed) for a specific vehicle and optionally associated route. This class is used to persist and manage tracking data for real-time or historical analysis.

## API

### Properties

#### `public int Id`
Unique identifier for the tracking session. Assigned by the persistence layer.

#### `public string SessionName`
Human-readable name for the session. May be used for display purposes.

#### `public int VehicleId`
Foreign key referencing the `Vehicle` associated with this session. Must correspond to an existing vehicle record.

#### `public Vehicle? Vehicle`
Navigation property to the associated `Vehicle` entity. Populated via lazy or eager loading depending on the data access configuration. May be `null` if the vehicle is not loaded.

#### `public int? RouteId`
Optional foreign key referencing the `Route` associated with this session. May be `null` if the session is not tied to a predefined route.

#### `public Route? Route`
Navigation property to the associated `Route` entity. Populated via lazy or eager loading. May be `null` if no route is assigned or if the route is not loaded.

#### `public SessionStatus Status`
Current state of the session. Possible values are defined in the `SessionStatus` enum:
- `NotStarted`: Session has been created but not yet started.
- `Active`: Session is currently running.
- `Paused`: Session is temporarily paused.
- `Completed`: Session has ended.

#### `public DateTime StartTime`
Timestamp indicating when the session was started. Set automatically when `StartSession()` is called.

#### `public DateTime? EndTime`
Timestamp indicating when the session was completed. `null` if the session is still active or paused. Set automatically when the session transitions to `Completed`.

#### `public double TotalDistance`
Total distance traveled during the session, measured in the unit defined by the application (e.g., kilometers or miles). Updated in real-time as location data is recorded.

#### `public double AverageSpeed`
Average speed during the session, calculated from the total distance and active duration (excluding idle time). Measured in the unit defined by the application (e.g., km/h or mph).

#### `public double MaxSpeed`
Highest recorded speed during the session. Updated in real-time as new location data is processed.

#### `public long TotalIdleSeconds`
Total time spent idle (speed below a configurable threshold) during the session, measured in seconds. Updated in real-time as location data is recorded.

#### `public ICollection<Location> Locations`
Collection of `Location` records associated with this session. Each entry represents a GPS coordinate and timestamp. Populated in real-time as the session progresses.

#### `public string? Notes`
Optional user-provided notes or comments about the session. May be `null`.

#### `public DateTime CreatedAt`
Timestamp indicating when the session record was created. Automatically set by the persistence layer.

#### `public DateTime UpdatedAt`
Timestamp indicating the last time the session record was updated. Automatically updated by the persistence layer on changes.

### Methods

#### `public void StartSession()`
Starts the tracking session. Sets `Status` to `Active` and `StartTime` to the current UTC timestamp.
- **Throws**:
  - `InvalidOperationException`: If the session is already active, paused, or completed.

#### `public void PauseSession()`
Pauses the tracking session. Sets `Status` to `Paused`.
- **Throws**:
  - `InvalidOperationException`: If the session is not active.

#### `public void ResumeSession()`
Resumes a paused tracking session. Sets `Status` back to `Active`.
- **Throws**:
  - `InvalidOperationException`: If the session is not paused.

## Usage

### Example 1: Starting and Managing a Tracking Session
