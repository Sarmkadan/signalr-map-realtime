// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using SignalRMapRealtime.DTOs;

/// <summary>
/// Service contract for the real-time route playback and historical timeline engine.
/// </summary>
/// <remarks>
/// <para>
/// The playback service loads historical location data from completed or active tracking
/// sessions and re-emits it as a time-controlled stream of <see cref="PlaybackFrameDto"/>
/// frames pushed to SignalR clients via <c>RoutePlaybackHub</c>. Playback speed, cursor
/// position, and session lifecycle are managed entirely through this interface.
/// </para>
/// <para>
/// Implementations are expected to be singleton-scoped. Database access must be obtained
/// through a scope factory to avoid captive-dependency violations.
/// </para>
/// </remarks>
public interface IRoutePlaybackService
{
    /// <summary>
    /// Initiates a new playback session for the specified tracking session and begins
    /// streaming <see cref="PlaybackFrameDto"/> frames to the associated SignalR group.
    /// </summary>
    /// <param name="request">
    /// Playback configuration: source session ID, speed multiplier, optional start timestamp,
    /// and loop flag.
    /// </param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    /// <returns>
    /// A <see cref="Guid"/> that uniquely identifies the newly created playback session.
    /// Clients use this ID to subscribe to the frame stream and issue control commands.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the referenced tracking session does not exist or contains no location records.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the engine's concurrent playback limit has been reached.
    /// </exception>
    Task<Guid> StartPlaybackAsync(StartPlaybackRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends an active playback session at its current frame, retaining position for resumption.
    /// The corresponding SignalR group stops receiving <c>PlaybackFrame</c> events until resumed.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to pause.</param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    Task PausePlaybackAsync(Guid playbackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues a previously paused playback session from its retained cursor position.
    /// Frame emission to the associated SignalR group resumes immediately.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to resume.</param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    Task ResumePlaybackAsync(Guid playbackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently terminates a playback session, cancels its background frame loop, and
    /// releases all associated in-memory resources.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to stop.</param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    Task StopPlaybackAsync(Guid playbackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Repositions the playback cursor to the recorded frame whose timestamp is closest
    /// to the specified UTC value. Effective on both running and paused sessions.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to seek.</param>
    /// <param name="timestamp">
    /// Target UTC timestamp. Values outside the session's recorded range are clamped
    /// to the nearest boundary.
    /// </param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    Task SeekToTimestampAsync(Guid playbackId, DateTime timestamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts the playback speed of an active or paused session without interrupting the stream.
    /// The supplied value is silently clamped to the engine's configured min/max range.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session.</param>
    /// <param name="speedMultiplier">
    /// Desired speed factor (1.0 = real-time, 4.0 = 4× faster, 0.5 = half speed).
    /// </param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    Task SetPlaybackSpeedAsync(Guid playbackId, double speedMultiplier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a snapshot of the current state of a specific playback session.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to query.</param>
    /// <returns>
    /// The current <see cref="PlaybackSessionDto"/>, or <c>null</c> if no session with
    /// that ID is managed by the engine.
    /// </returns>
    Task<PlaybackSessionDto?> GetPlaybackStateAsync(Guid playbackId);

    /// <summary>
    /// Returns the states of all playback sessions currently managed by the engine.
    /// </summary>
    /// <returns>Read-only list of all active, paused, and recently completed session states.</returns>
    Task<IReadOnlyList<PlaybackSessionDto>> GetActivePlaybacksAsync();

    /// <summary>
    /// Constructs a fully annotated timeline for a tracking session from its raw location history.
    /// </summary>
    /// <remarks>
    /// The resulting <see cref="RouteTimelineDto"/> contains ordered location entries enriched
    /// with event annotations, contiguous movement segments classified by behavioural type
    /// (moving, idle, stopped), and aggregate journey statistics. This is typically requested
    /// before initiating playback so that the client can render a timeline scrubber.
    /// </remarks>
    /// <param name="sessionId">Identifier of the tracking session to analyse.</param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    /// <returns>
    /// A fully built <see cref="RouteTimelineDto"/>, or <c>null</c> if the session
    /// contains no recorded location data.
    /// </returns>
    Task<RouteTimelineDto?> BuildTimelineAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the vehicle state at the recorded moment closest to a given UTC timestamp,
    /// without initiating a streaming playback session.
    /// </summary>
    /// <remarks>
    /// Useful for rendering a historical map state in response to a user scrubbing the timeline
    /// without starting full playback. The returned frame's <see cref="PlaybackFrameDto.PlaybackId"/>
    /// is <see cref="Guid.Empty"/> to distinguish it from streamed frames.
    /// </remarks>
    /// <param name="sessionId">Identifier of the tracking session to query.</param>
    /// <param name="timestamp">Target UTC timestamp. Clamped to the session's recorded range.</param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    /// <returns>
    /// An interpolated or nearest-match <see cref="PlaybackFrameDto"/>, or <c>null</c>
    /// if the session has no recorded data.
    /// </returns>
    Task<PlaybackFrameDto?> GetSnapshotAtTimestampAsync(int sessionId, DateTime timestamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes and returns aggregated performance statistics for a tracking session.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session to analyse.</param>
    /// <param name="cancellationToken">Cooperative cancellation support.</param>
    /// <returns>
    /// A <see cref="PlaybackStatisticsDto"/> summary, or <c>null</c> if the session
    /// contains no recorded data.
    /// </returns>
    Task<PlaybackStatisticsDto?> GetPlaybackStatisticsAsync(int sessionId, CancellationToken cancellationToken = default);
}
