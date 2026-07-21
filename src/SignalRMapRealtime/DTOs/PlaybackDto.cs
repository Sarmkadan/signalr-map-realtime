#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Defines the operational status of a route playback session.
/// </summary>
public enum PlaybackStatus
{
    /// <summary>Session is initialising but has not started emitting frames.</summary>
    Idle,

    /// <summary>Session is actively emitting frames in real time.</summary>
    Playing,

    /// <summary>Session is temporarily suspended; position is retained for resumption.</summary>
    Paused,

    /// <summary>Session has successfully emitted all frames to completion.</summary>
    Completed,

    /// <summary>Session terminated due to an unrecoverable error.</summary>
    Error
}

/// <summary>
/// Classifies notable events within a route timeline for annotation and client-side filtering.
/// </summary>
public enum TimelineEventType
{
    /// <summary>Standard GPS position update with no special significance.</summary>
    LocationUpdate,

    /// <summary>Vehicle speed exceeded the configured alert threshold.</summary>
    SpeedAlert,

    /// <summary>Vehicle came to a complete stop (speed near zero).</summary>
    Stop,

    /// <summary>Vehicle departed from a stationary position.</summary>
    Start,

    /// <summary>A planned route waypoint was reached and marked complete.</summary>
    WaypointReached,

    /// <summary>The tracking session began recording location data.</summary>
    SessionStart,

    /// <summary>The tracking session ended recording location data.</summary>
    SessionEnd
}

/// <summary>
/// Classifies a contiguous span of movement within a tracked journey.
/// </summary>
public enum SegmentType
{
    /// <summary>Vehicle was in active transit above the idle speed threshold.</summary>
    Moving,

    /// <summary>Vehicle was moving slowly, below the idle speed threshold but for less than the minimum idle duration.</summary>
    Idle,

    /// <summary>Vehicle was stationary for longer than the minimum idle duration threshold.</summary>
    Stopped
}

/// <summary>
/// Represents a single rendered frame within a route playback stream.
/// Each frame corresponds to one recorded location point in the session's history.
/// </summary>
/// <param name="PlaybackId">Unique identifier of the parent playback session. <see cref="Guid.Empty"/> for snapshot queries.</param>
/// <param name="FrameIndex">Zero-based position of this frame within the sequence.</param>
/// <param name="TotalFrames">Total number of frames available in the session.</param>
/// <param name="Timestamp">UTC timestamp at which this location was originally recorded.</param>
/// <param name="Latitude">Geographic latitude of the vehicle at this frame, in decimal degrees.</param>
/// <param name="Longitude">Geographic longitude of the vehicle at this frame, in decimal degrees.</param>
/// <param name="Speed">Vehicle speed in km/h, if recorded by the GPS device.</param>
/// <param name="Bearing">Heading direction in degrees (0–360, clockwise from north), if recorded.</param>
/// <param name="Altitude">Altitude above sea level in metres, if recorded.</param>
/// <param name="DistanceCoveredKm">Cumulative distance travelled from the session start, in kilometres.</param>
/// <param name="RemainingDistanceKm">Remaining distance to the end of the route from this frame's position, in kilometres.</param>
/// <param name="CompletionPercentage">Journey progress as a whole-number percentage from 0 to 100.</param>
/// <param name="ElapsedTime">Time elapsed since the beginning of the tracked session at this frame.</param>
/// <param name="Address">Reverse-geocoded address at this location, if available.</param>
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

/// <summary>
/// Describes the current state and progress of a running or paused playback session.
/// </summary>
/// <param name="PlaybackId">Unique session identifier assigned by the playback engine.</param>
/// <param name="TrackingSessionId">ID of the underlying tracking session being replayed.</param>
/// <param name="Status">Current operational status of the playback.</param>
/// <param name="StartedAt">UTC wall-clock time when playback was first initiated.</param>
/// <param name="PlaybackStart">Earliest timestamp present in the historical data being replayed.</param>
/// <param name="PlaybackEnd">Latest timestamp present in the historical data being replayed.</param>
/// <param name="SpeedMultiplier">Active playback speed factor relative to real time.</param>
/// <param name="CurrentTimestamp">The historical UTC timestamp currently represented by the playback cursor.</param>
/// <param name="CurrentFrame">Index of the most recently emitted frame.</param>
/// <param name="TotalFrames">Total number of frames available in the session.</param>
/// <param name="CompletionPercentage">Playback progress as a whole-number percentage from 0 to 100.</param>
/// <param name="Loop">Whether playback restarts automatically after reaching the final frame.</param>
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

/// <summary>
/// A single annotated entry in a route timeline. Entries correspond directly to recorded location
/// points, enriched with event metadata for client-side rendering and filtering.
/// </summary>
/// <param name="Timestamp">UTC time at which this location was originally recorded.</param>
/// <param name="Latitude">Geographic latitude at this point, in decimal degrees.</param>
/// <param name="Longitude">Geographic longitude at this point, in decimal degrees.</param>
/// <param name="Speed">Speed in km/h at this point, if available.</param>
/// <param name="Bearing">Heading in degrees at this point, if available.</param>
/// <param name="EventLabel">Human-readable description of the event, populated for notable entries only.</param>
/// <param name="EventType">Classification of the event for client-side icon and colour rendering.</param>
public record TimelineEntryDto(
    DateTime Timestamp,
    double Latitude,
    double Longitude,
    double? Speed,
    double? Bearing,
    string? EventLabel,
    TimelineEventType EventType);

/// <summary>
/// A contiguous movement segment within a route timeline, grouping consecutive location points
/// that share the same behavioural classification (moving, idle, or stopped).
/// </summary>
/// <param name="Index">Zero-based ordinal position of this segment in the timeline sequence.</param>
/// <param name="StartTime">UTC timestamp of the first location point in the segment.</param>
/// <param name="EndTime">UTC timestamp of the last location point in the segment.</param>
/// <param name="Duration">Total elapsed time covered by the segment.</param>
/// <param name="DistanceKm">Distance covered during the segment in kilometres.</param>
/// <param name="AverageSpeedKmh">Mean speed across the segment in km/h.</param>
/// <param name="Type">Behavioural classification of the segment.</param>
public record TimelineSegmentDto(
    int Index,
    DateTime StartTime,
    DateTime EndTime,
    TimeSpan Duration,
    double DistanceKm,
    double AverageSpeedKmh,
    SegmentType Type);

/// <summary>
/// A complete annotated timeline for a single tracking session.
/// Combines raw location entries with derived movement segments and aggregate statistics.
/// This is the primary data contract for populating client-side timeline scrubber controls.
/// </summary>
/// <param name="SessionId">Identifier of the tracking session this timeline describes.</param>
/// <param name="StartTime">UTC timestamp of the earliest recorded location.</param>
/// <param name="EndTime">UTC timestamp of the latest recorded location.</param>
/// <param name="Duration">Total elapsed time from the first to the last recorded fix.</param>
/// <param name="TotalDistanceKm">Cumulative distance covered throughout the session in kilometres.</param>
/// <param name="AverageSpeedKmh">Mean speed across all recorded points in km/h.</param>
/// <param name="MaxSpeedKmh">Peak recorded speed across the session in km/h.</param>
/// <param name="LocationCount">Total number of GPS fixes recorded in the session.</param>
/// <param name="Entries">Ordered, annotated location entries forming the playback source data.</param>
/// <param name="Segments">Contiguous behavioural segments derived from the location stream.</param>
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

/// <summary>
/// An aggregated speed distribution bucket for analytics visualisation.
/// </summary>
/// <param name="Band">Human-readable speed range label (e.g., "50–80 km/h").</param>
/// <param name="SampleCount">Number of location readings that fall within this speed band.</param>
public record SpeedDistributionDto(string Band, int SampleCount);

/// <summary>
/// Comprehensive analytics report for a historical tracking session.
/// Suitable for fleet dashboards, compliance reporting, and driver performance review.
/// </summary>
/// <param name="SessionId">Identifier of the analysed tracking session.</param>
/// <param name="TotalLocations">Total number of GPS fixes recorded in the session.</param>
/// <param name="TotalDistanceKm">Cumulative distance travelled in kilometres.</param>
/// <param name="Duration">Total elapsed time of the session from start to end.</param>
/// <param name="AverageSpeedKmh">Mean speed across all moving samples in km/h.</param>
/// <param name="MaxSpeedKmh">Peak speed recorded during the session in km/h.</param>
/// <param name="MinSpeedKmh">Lowest non-zero speed recorded in km/h.</param>
/// <param name="IdleTimeSeconds">Total seconds during which the vehicle was classified as idle or stopped.</param>
/// <param name="MovingTimeSeconds">Total seconds during which the vehicle was classified as actively moving.</param>
/// <param name="SpeedDistribution">Breakdown of location-sample counts across discrete speed bands.</param>
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

/// <summary>
/// Request parameters for initiating a new route playback session via
/// <see cref="Services.IRoutePlaybackService.StartPlaybackAsync"/>.
/// </summary>
public class StartPlaybackRequest
{
    /// <summary>
    /// Identifier of the tracking session whose location history will be replayed.
    /// Must refer to a session that contains at least one location record.
    /// </summary>
    public required int SessionId { get; init; }

    /// <summary>
    /// Playback speed multiplier relative to real time.
    /// 1.0 = real-time, 2.0 = double speed, 0.5 = half speed.
    /// Will be clamped to the engine's configured min/max range.
    /// </summary>
    public double SpeedMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Optional UTC timestamp within the session from which to begin playback.
    /// Playback starts from the beginning of the session when not provided.
    /// </summary>
    public DateTime? StartFromTimestamp { get; init; }

    /// <summary>
    /// When <c>true</c>, playback automatically restarts from the first frame
    /// after the final frame has been emitted.
    /// </summary>
    public bool Loop { get; init; }
}
