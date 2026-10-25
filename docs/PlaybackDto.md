# PlaybackDto

Source: `src/SignalRMapRealtime/DTOs/PlaybackDto.cs`

The file lives in the `SignalRMapRealtime.DTOs` namespace and defines the contracts for route playback and route timelines. It contains three enums, seven positional records, and one class:

| Type | Kind | Purpose |
|------|------|---------|
| `PlaybackStatus` | `enum` | Operational state of a playback session |
| `TimelineEventType` | `enum` | Classification of a notable event in a timeline |
| `SegmentType` | `enum` | Behavioural classification of a movement segment |
| `PlaybackFrameDto` | `record` | One rendered frame in a playback stream |
| `PlaybackSessionDto` | `record` | Current state and progress of a playback session |
| `TimelineEntryDto` | `record` | One annotated location point in a timeline |
| `TimelineSegmentDto` | `record` | One contiguous movement segment in a timeline |
| `RouteTimelineDto` | `record` | Complete annotated timeline for one tracking session |
| `SpeedDistributionDto` | `record` | One speed band in an analytics histogram |
| `PlaybackStatisticsDto` | `record` | Aggregate analytics report for a tracking session |
| `StartPlaybackRequest` | `class` | Parameters for starting a playback session |

The file has no methods and no logic. It does not compute frames, timelines, or statistics. The XML docs of `StartPlaybackRequest` refer to `Services.IRoutePlaybackService.StartPlaybackAsync`, which is where the request is consumed.

## Enums

### PlaybackStatus

Lifecycle states of a playback session, in declaration order:

| Member | Documented meaning |
|--------|--------------------|
| `Idle` | Session is initialising and has not yet emitted frames. |
| `Playing` | Session is emitting frames in real time. |
| `Paused` | Session is suspended; its position is kept so it can resume. |
| `Completed` | All frames were emitted to the end. |
| `Error` | Session stopped because of an unrecoverable error. |

### TimelineEventType

Event classification for a `TimelineEntryDto`, intended for client-side icons and colours:

| Member | Documented meaning |
|--------|--------------------|
| `LocationUpdate` | Ordinary GPS update with no special significance. |
| `SpeedAlert` | Speed exceeded the configured alert threshold. |
| `Stop` | Vehicle came to a complete stop (speed near zero). |
| `Start` | Vehicle departed from a stationary position. |
| `WaypointReached` | A planned route waypoint was reached and marked complete. |
| `SessionStart` | The tracking session began recording. |
| `SessionEnd` | The tracking session stopped recording. |

### SegmentType

Behavioural class of a `TimelineSegmentDto`:

| Member | Documented meaning |
|--------|--------------------|
| `Moving` | Active transit above the idle speed threshold. |
| `Idle` | Slow movement below the idle threshold, lasting less than the minimum idle duration. |
| `Stopped` | Stationary for longer than the minimum idle duration. |

The thresholds are not defined in this file. The XML docs refer to them only in general terms.

## Records

All records are immutable positional records. Their values are set only through the primary constructor.

### PlaybackFrameDto

One frame in a playback stream. Each frame corresponds to one recorded location point.

```csharp
public record PlaybackFrameDto(
    Guid PlaybackId,
    int FrameIndex,
    int TotalFrames,
    DateTime Timestamp,
    double Latitude,
    double Longitude,
    double? Speed,
    double? Bearing,
    double? Altitude,
    double DistanceCoveredKm,
    double? RemainingDistanceKm,
    int CompletionPercentage,
    TimeSpan ElapsedTime,
    string? Address);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `PlaybackId` | `Guid` | Parent playback session. The XML docs say `Guid.Empty` is used for snapshot queries. |
| `FrameIndex` | `int` | Zero-based position of the frame in the sequence. |
| `TotalFrames` | `int` | Number of frames available in the session. |
| `Timestamp` | `DateTime` | UTC time at which the location was recorded. |
| `Latitude` | `double` | Latitude in decimal degrees. |
| `Longitude` | `double` | Longitude in decimal degrees. |
| `Speed` | `double?` | Speed in km/h, if the GPS device recorded it. |
| `Bearing` | `double?` | Heading in degrees, 0–360 clockwise from north, if recorded. |
| `Altitude` | `double?` | Altitude above sea level in metres, if recorded. |
| `DistanceCoveredKm` | `double` | Distance travelled from the session start, in kilometres. |
| `RemainingDistanceKm` | `double?` | Distance left to the end of the route, in kilometres. |
| `CompletionPercentage` | `int` | Progress as a whole number from 0 to 100. |
| `ElapsedTime` | `TimeSpan` | Time since the start of the tracked session at this frame. |
| `Address` | `string?` | Reverse-geocoded address, if available. |

### PlaybackSessionDto

Current state and progress of a running or paused session.

```csharp
public record PlaybackSessionDto(
    Guid PlaybackId,
    int TrackingSessionId,
    PlaybackStatus Status,
    DateTime StartedAt,
    DateTime PlaybackStart,
    DateTime PlaybackEnd,
    double SpeedMultiplier,
    DateTime CurrentTimestamp,
    int CurrentFrame,
    int TotalFrames,
    int CompletionPercentage,
    bool Loop);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `PlaybackId` | `Guid` | Session identifier assigned by the playback engine. |
| `TrackingSessionId` | `int` | ID of the tracking session being replayed. |
| `Status` | `PlaybackStatus` | Current operational status. |
| `StartedAt` | `DateTime` | UTC wall-clock time when playback was first started. |
| `PlaybackStart` | `DateTime` | Earliest timestamp in the historical data being replayed. |
| `PlaybackEnd` | `DateTime` | Latest timestamp in the historical data being replayed. |
| `SpeedMultiplier` | `double` | Playback speed relative to real time. |
| `CurrentTimestamp` | `DateTime` | Historical UTC time at the playback cursor. |
| `CurrentFrame` | `int` | Index of the most recently emitted frame. |
| `TotalFrames` | `int` | Number of frames available in the session. |
| `CompletionPercentage` | `int` | Progress as a whole number from 0 to 100. |
| `Loop` | `bool` | Whether playback restarts after the final frame. |

Note that `PlaybackStart`/`PlaybackEnd` describe the historical data, while `StartedAt` describes wall-clock time. `CurrentTimestamp` is a historical time too.

### TimelineEntryDto

One annotated location point in a timeline.

```csharp
public record TimelineEntryDto(
    DateTime Timestamp,
    double Latitude,
    double Longitude,
    double? Speed,
    double? Bearing,
    string? EventLabel,
    TimelineEventType EventType);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `Timestamp` | `DateTime` | UTC time at which the location was recorded. |
| `Latitude` | `double` | Latitude in decimal degrees. |
| `Longitude` | `double` | Longitude in decimal degrees. |
| `Speed` | `double?` | Speed in km/h, if available. |
| `Bearing` | `double?` | Heading in degrees, if available. |
| `EventLabel` | `string?` | Human-readable event description. The XML docs say it is set for notable entries only. |
| `EventType` | `TimelineEventType` | Event classification for rendering and filtering. |

### TimelineSegmentDto

One run of consecutive points with the same behavioural classification.

```csharp
public record TimelineSegmentDto(
    int Index,
    DateTime StartTime,
    DateTime EndTime,
    TimeSpan Duration,
    double DistanceKm,
    double AverageSpeedKmh,
    SegmentType Type);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `Index` | `int` | Zero-based position of the segment in the timeline. |
| `StartTime` | `DateTime` | UTC time of the segment's first point. |
| `EndTime` | `DateTime` | UTC time of the segment's last point. |
| `Duration` | `TimeSpan` | Time covered by the segment. |
| `DistanceKm` | `double` | Distance covered in the segment, in kilometres. |
| `AverageSpeedKmh` | `double` | Mean speed across the segment, in km/h. |
| `Type` | `SegmentType` | Behavioural classification of the segment. |

### RouteTimelineDto

The complete timeline for one tracking session. The XML docs call it the primary data contract for client-side timeline scrubber controls.

```csharp
public record RouteTimelineDto(
    int SessionId,
    DateTime StartTime,
    DateTime EndTime,
    TimeSpan Duration,
    double TotalDistanceKm,
    double AverageSpeedKmh,
    double MaxSpeedKmh,
    int LocationCount,
    IReadOnlyList<TimelineEntryDto> Entries,
    IReadOnlyList<TimelineSegmentDto> Segments);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `SessionId` | `int` | Tracking session this timeline describes. |
| `StartTime` | `DateTime` | UTC time of the earliest recorded location. |
| `EndTime` | `DateTime` | UTC time of the latest recorded location. |
| `Duration` | `TimeSpan` | Time from the first to the last fix. |
| `TotalDistanceKm` | `double` | Total distance for the session, in kilometres. |
| `AverageSpeedKmh` | `double` | Mean speed across all points, in km/h. |
| `MaxSpeedKmh` | `double` | Peak speed in the session, in km/h. |
| `LocationCount` | `int` | Number of GPS fixes in the session. |
| `Entries` | `IReadOnlyList<TimelineEntryDto>` | Ordered, annotated entries that form the playback source data. |
| `Segments` | `IReadOnlyList<TimelineSegmentDto>` | Behavioural segments derived from the location stream. |

### SpeedDistributionDto

One bucket in a speed histogram.

```csharp
public record SpeedDistributionDto(string Band, int SampleCount);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `Band` | `string` | Human-readable speed range label, for example `"50–80 km/h"`. |
| `SampleCount` | `int` | Number of readings in this band. |

### PlaybackStatisticsDto

Aggregate analytics for a historical session. The XML docs name fleet dashboards, compliance reporting, and driver performance review as intended uses.

```csharp
public record PlaybackStatisticsDto(
    int SessionId,
    int TotalLocations,
    double TotalDistanceKm,
    TimeSpan Duration,
    double AverageSpeedKmh,
    double MaxSpeedKmh,
    double MinSpeedKmh,
    int IdleTimeSeconds,
    int MovingTimeSeconds,
    IReadOnlyList<SpeedDistributionDto> SpeedDistribution);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `SessionId` | `int` | Tracking session that was analysed. |
| `TotalLocations` | `int` | Number of GPS fixes in the session. |
| `TotalDistanceKm` | `double` | Total distance travelled, in kilometres. |
| `Duration` | `TimeSpan` | Time from session start to end. |
| `AverageSpeedKmh` | `double` | Mean speed across moving samples, in km/h. |
| `MaxSpeedKmh` | `double` | Peak speed, in km/h. |
| `MinSpeedKmh` | `double` | Lowest non-zero speed, in km/h. |
| `IdleTimeSeconds` | `int` | Seconds classified as idle or stopped. |
| `MovingTimeSeconds` | `int` | Seconds classified as moving. |
| `SpeedDistribution` | `IReadOnlyList<SpeedDistributionDto>` | Sample counts per speed band. |

The file does not say how the speed bands are chosen. Only the `Band` label format is shown, in the `SpeedDistributionDto` example.

## StartPlaybackRequest

Mutable class with `init`-only properties. It is set with an object initializer. It is the input to `IRoutePlaybackService.StartPlaybackAsync`, according to its XML docs.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SessionId` | `int` | required | Tracking session to replay. The XML docs say it must contain at least one location record. |
| `SpeedMultiplier` | `double` | `1.0` | Speed relative to real time: `1.0` is real time, `2.0` is double speed, `0.5` is half speed. The XML docs say the engine clamps it to its configured range. |
| `StartFromTimestamp` | `DateTime?` | `null` | UTC time within the session to start from. When `null`, playback starts at the beginning of the session. |
| `Loop` | `bool` | `false` | When `true`, playback restarts from the first frame after the last frame. |

`SessionId` is declared with `required`. The compiler therefore rejects any object initializer that does not set it.

## Usage

### Describing a session's state

```csharp
using SignalRMapRealtime.DTOs;

var session = new PlaybackSessionDto(
    PlaybackId: Guid.NewGuid(),
    TrackingSessionId: 42,
    Status: PlaybackStatus.Playing,
    StartedAt: DateTime.UtcNow,
    PlaybackStart: new DateTime(2026, 10, 24, 8, 0, 0, DateTimeKind.Utc),
    PlaybackEnd: new DateTime(2026, 10, 24, 17, 30, 0, DateTimeKind.Utc),
    SpeedMultiplier: 2.0,
    CurrentTimestamp: new DateTime(2026, 10, 24, 12, 15, 0, DateTimeKind.Utc),
    CurrentFrame: 480,
    TotalFrames: 1200,
    CompletionPercentage: 40,
    Loop: false);
```

### Building a timeline entry

```csharp
var entry = new TimelineEntryDto(
    Timestamp: new DateTime(2026, 10, 24, 12, 15, 0, DateTimeKind.Utc),
    Latitude: 40.7129,
    Longitude: -74.0059,
    Speed: 92.5,
    Bearing: 180.0,
    EventLabel: "Speed Alert: 92.5 km/h",
    EventType: TimelineEventType.SpeedAlert);
```

The `EventLabel` text is an example. The file defines no label format.

### Starting a request

```csharp
var request = new StartPlaybackRequest
{
    SessionId = 42,
    SpeedMultiplier = 0.5,
    StartFromTimestamp = new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
    Loop = true
};
```

Leave `SpeedMultiplier` unset to get real-time playback, and leave `StartFromTimestamp` unset to start from the beginning of the session.

## Notes

- **Units:** Coordinates are in decimal degrees. Distances are in kilometres, speeds in km/h, and altitude in metres. The `*Seconds` fields in `PlaybackStatisticsDto` are whole seconds.
- **Time zone:** The XML docs describe every `DateTime` as UTC. Pass UTC values, or convert to UTC before creating a record.
- **Nullability:** The file is compiled with `#nullable enable`. The optional sensor values (`Speed`, `Bearing`, `Altitude`, `RemainingDistanceKm`), `EventLabel`, `Address`, and the `StartFromTimestamp` request field are nullable.
- **Identifiers:** `PlaybackId` is a `Guid`. Tracking session IDs are `int` in every type in the file.
- **Immutability:** The records cannot be changed after construction. `StartPlaybackRequest` is a class, but its properties are `init`-only, so it cannot be changed after it is created.
- **Collections:** `Entries`, `Segments`, and `SpeedDistribution` are typed `IReadOnlyList<T>`. A caller that passes a mutable `List<T>` can still change it through the original reference.
- **Enum storage:** The file does not set explicit numeric values for the enums. Their integer values follow declaration order, so inserting a member in the middle changes the values of the members after it.
