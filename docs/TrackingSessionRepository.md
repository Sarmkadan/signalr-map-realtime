# TrackingSessionRepository

`TrackingSessionRepository` provides a specialized data access layer for querying and aggregating `TrackingSession` entities within the `signalr-map-realtime` application. It extends a generic repository base to offer pre-built queries targeting active sessions, vehicle-specific histories, status filtering, date-range lookups, speed thresholds, expiration checks, and distance aggregation.

## API

### Constructor

```csharp
public TrackingSessionRepository(ApplicationDbContext context) : base(context)
```

Initializes a new instance of the repository, passing the Entity Framework Core `ApplicationDbContext` to the base class. The context must be registered and injected via dependency injection; it is not validated for null at this level.

### GetActiveSessionByVehicleAsync

```csharp
public async Task<TrackingSession?> GetActiveSessionByVehicleAsync(int vehicleId)
```

Returns the single active `TrackingSession` for the specified vehicle, or `null` if no active session exists. An active session is one whose status indicates it is currently in progress. Throws `ArgumentOutOfRangeException` when `vehicleId` is less than or equal to zero.

### GetSessionsByVehicleAsync

```csharp
public async Task<IEnumerable<TrackingSession>> GetSessionsByVehicleAsync(int vehicleId)
```

Retrieves all tracking sessions associated with the given vehicle, ordered by start time descending. Returns an empty collection when the vehicle has no recorded sessions. Throws `ArgumentOutOfRangeException` when `vehicleId` is less than or equal to zero.

### GetSessionsByStatusAsync

```csharp
public async Task<IEnumerable<TrackingSession>> GetSessionsByStatusAsync(SessionStatus status)
```

Returns all sessions matching the specified `SessionStatus` enumeration value. The result set is unfiltered by vehicle or date. Returns an empty collection when no sessions match the status.

### GetSessionsByDateRangeAsync

```csharp
public async Task<IEnumerable<TrackingSession>> GetSessionsByDateRangeAsync(DateTime start, DateTime end)
```

Returns sessions whose start time falls within the inclusive `start` and exclusive `end` range. Throws `ArgumentException` when `start` is later than `end`. Returns an empty collection when no sessions fall within the window.

### GetSessionWithDetailsAsync

```csharp
public async Task<TrackingSession?> GetSessionWithDetailsAsync(int sessionId)
```

Returns a single `TrackingSession` with its related navigation properties (e.g., location points, vehicle data) eagerly loaded, or `null` when no session with the given identifier exists. Throws `ArgumentOutOfRangeException` when `sessionId` is less than or equal to zero.

### GetHighSpeedSessionsAsync

```csharp
public async Task<IEnumerable<TrackingSession>> GetHighSpeedSessionsAsync(double speedThresholdKmh)
```

Returns all sessions where the recorded maximum speed exceeds the supplied `speedThresholdKmh`. The threshold is compared strictly (greater than). Throws `ArgumentOutOfRangeException` when `speedThresholdKmh` is negative. Returns an empty collection when no sessions breach the threshold.

### GetExpiredSessionsAsync

```csharp
public async Task<IEnumerable<TrackingSession>> GetExpiredSessionsAsync()
```

Returns sessions whose status is still active but whose last recorded activity timestamp is older than a configured expiration period. This method takes no arguments and relies on internal time-span configuration. Returns an empty collection when no sessions are stale.

### GetTotalDistanceTraveledAsync

```csharp
public async Task<double> GetTotalDistanceTraveledAsync(int vehicleId, DateTime? since = null)
```

Computes the cumulative distance in kilometers across all sessions for the given vehicle. When `since` is provided, only sessions starting on or after that date are included; otherwise all sessions are summed. Throws `ArgumentOutOfRangeException` when `vehicleId` is less than or equal to zero. Returns `0.0` when no qualifying sessions exist.

### GetActiveSessionCountAsync

```csharp
public async Task<int> GetActiveSessionCountAsync()
```

Returns the total number of sessions currently in the active status. Takes no parameters. Returns zero when no active sessions are present.

## Usage

### Retrieve and Inspect an Active Session

```csharp
var repo = serviceProvider.GetRequiredService<TrackingSessionRepository>();
var activeSession = await repo.GetActiveSessionByVehicleAsync(vehicleId: 42);

if (activeSession is not null)
{
    var detailed = await repo.GetSessionWithDetailsAsync(activeSession.Id);
    Console.WriteLine($"Session {detailed.Id} has {detailed.LocationPoints.Count} points.");
}
else
{
    Console.WriteLine("Vehicle 42 has no active session.");
}
```

### Dashboard Aggregation for a Fleet Vehicle

```csharp
var repo = serviceProvider.GetRequiredService<TrackingSessionRepository>();
var vehicleId = 15;
var lastWeek = DateTime.UtcNow.AddDays(-7);

var totalKm = await repo.GetTotalDistanceTraveledAsync(vehicleId, since: lastWeek);
var highSpeedSessions = await repo.GetHighSpeedSessionsAsync(speedThresholdKmh: 120);
var expired = await repo.GetExpiredSessionsAsync();

Console.WriteLine($"Vehicle {vehicleId} traveled {totalKm:F2} km since last week.");
Console.WriteLine($"Fleet-wide high-speed sessions: {highSpeedSessions.Count()}");
Console.WriteLine($"Stale active sessions requiring cleanup: {expired.Count()}");
```

## Notes

- All methods returning collections may return empty enumerables; callers should guard against null results only for methods explicitly returning nullable single entities (`GetActiveSessionByVehicleAsync`, `GetSessionWithDetailsAsync`).
- `GetExpiredSessionsAsync` depends on a configurable expiration threshold. If the threshold is not set or is zero, the method may return all active sessions or none, depending on implementation defaults.
- `GetTotalDistanceTraveledAsync` performs aggregation in the database when possible; floating-point precision is subject to the underlying store's arithmetic behavior.
- Date-range filtering in `GetSessionsByDateRangeAsync` uses inclusive start and exclusive end semantics. Passing equal `start` and `end` values returns an empty collection.
- None of the read methods mutate context state, but they share the `ApplicationDbContext` instance. Concurrent calls on the same repository instance are safe only if the context is scoped per operation or unit of work. In a typical ASP.NET Core request scope, parallel awaits on the same repository instance may cause concurrency exceptions from EF Core's `DbContext`; serialize data access or use a scope factory for parallel operations.
- Parameter validation throws synchronously; database-level exceptions (e.g., timeout, connection failure) propagate asynchronously and should be handled by the caller.
