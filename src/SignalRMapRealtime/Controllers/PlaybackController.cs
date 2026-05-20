#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Services;

/// <summary>
/// Provides REST endpoints for historical route playback, timeline inspection,
/// point-in-time snapshots, and session analytics.
/// Streaming playback frames are delivered via the <c>RoutePlaybackHub</c> SignalR hub.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlaybackController : ControllerBase
{
    private readonly IRoutePlaybackService _playbackService;
    private readonly ILogger<PlaybackController> _logger;

    /// <summary>Initializes a new instance of <see cref="PlaybackController"/>.</summary>
    public PlaybackController(IRoutePlaybackService playbackService, ILogger<PlaybackController> logger)
    {
        ArgumentNullException.ThrowIfNull(playbackService);
        ArgumentNullException.ThrowIfNull(logger);
        _playbackService = playbackService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a new historical route playback session and returns the session identifier.
    /// Connect to <c>/hubs/playback</c> and subscribe with the returned <c>playbackId</c>
    /// to receive a real-time stream of <c>PlaybackFrame</c> events.
    /// </summary>
    /// <param name="request">
    /// Playback configuration including the source tracking session ID,
    /// optional speed multiplier, optional start timestamp, and loop flag.
    /// </param>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    public async Task<IActionResult> StartSession([FromBody] StartPlaybackRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorResponse.ValidationError(
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .GroupBy(e => "ValidationError")
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    "Validation failed",
                    HttpContext.TraceIdentifier));

            var playbackId = await _playbackService.StartPlaybackAsync(request).ConfigureAwait(false);

            var response = ApiResponse<object>.SuccessResponse(
                new { PlaybackId = playbackId },
                "Playback session started. Connect to /hubs/playback to receive frames.",
                201,
                HttpContext.TraceIdentifier);

            return CreatedAtAction(nameof(GetSessionState), new { playbackId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid playback request. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Playback engine at capacity. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return Conflict(ErrorResponse.ConflictError(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting playback session. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to start playback session", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Returns a list of all playback sessions currently managed by the engine,
    /// including active, paused, and recently completed sessions.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlaybackSessionDto>>), 200)]
    public async Task<IActionResult> GetActiveSessions()
    {
        try
        {
            var sessions = await _playbackService.GetActivePlaybacksAsync().ConfigureAwait(false);

            return Ok(ApiResponse<IReadOnlyList<PlaybackSessionDto>>.SuccessResponse(
                sessions, "Active playback sessions retrieved", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving playback sessions. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve sessions", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Returns the current state of a specific playback session, including
    /// progress, speed multiplier, and operational status.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session.</param>
    [HttpGet("sessions/{playbackId}")]
    [ProducesResponseType(typeof(ApiResponse<PlaybackSessionDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSessionState(Guid playbackId)
    {
        try
        {
            var state = await _playbackService.GetPlaybackStateAsync(playbackId).ConfigureAwait(false);

            if (state is null)
                return NotFound(ErrorResponse.NotFoundError(
                    $"Playback session {playbackId} not found", HttpContext.TraceIdentifier));

            return Ok(ApiResponse<PlaybackSessionDto>.SuccessResponse(
                state, "Playback state retrieved", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving state for session {PlaybackId}. TraceId: {TraceId}",
                playbackId, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve session state", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Stops and removes a playback session, cancelling its frame loop
    /// and releasing all associated in-memory resources.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the session to stop.</param>
    [HttpDelete("sessions/{playbackId}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> StopSession(Guid playbackId)
    {
        try
        {
            await _playbackService.StopPlaybackAsync(playbackId).ConfigureAwait(false);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping session {PlaybackId}. TraceId: {TraceId}",
                playbackId, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to stop playback session", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Builds and returns the fully annotated timeline for a tracking session.
    /// The timeline contains ordered location entries enriched with event annotations,
    /// classified movement segments, and aggregate journey statistics.
    /// Request this before initiating playback to populate the client-side timeline scrubber.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session to analyse.</param>
    [HttpGet("timeline/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<RouteTimelineDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTimeline(int sessionId)
    {
        try
        {
            var timeline = await _playbackService.BuildTimelineAsync(sessionId).ConfigureAwait(false);

            if (timeline is null)
                return NotFound(ErrorResponse.NotFoundError(
                    $"No location data found for tracking session {sessionId}", HttpContext.TraceIdentifier));

            return Ok(ApiResponse<RouteTimelineDto>.SuccessResponse(
                timeline, "Timeline built successfully", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building timeline for session {SessionId}. TraceId: {TraceId}",
                sessionId, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to build timeline", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Returns the vehicle state at the recorded moment closest to the specified UTC timestamp,
    /// without initiating a streaming playback session.
    /// Useful for rendering a historical map state in response to the user scrubbing the timeline.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session to query.</param>
    /// <param name="timestamp">Target UTC timestamp. Clamped to the session's recorded range.</param>
    [HttpGet("snapshot/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<PlaybackFrameDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSnapshot(int sessionId, [FromQuery] DateTime timestamp)
    {
        try
        {
            var snapshot = await _playbackService
                .GetSnapshotAtTimestampAsync(sessionId, timestamp)
                .ConfigureAwait(false);

            if (snapshot is null)
                return NotFound(ErrorResponse.NotFoundError(
                    $"No snapshot available for session {sessionId}", HttpContext.TraceIdentifier));

            return Ok(ApiResponse<PlaybackFrameDto>.SuccessResponse(
                snapshot, "Snapshot retrieved", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snapshot for session {SessionId}. TraceId: {TraceId}",
                sessionId, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve snapshot", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Returns aggregated performance statistics for a historical tracking session.
    /// Includes total distance, duration, speed distribution, and idle time breakdown.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session to analyse.</param>
    [HttpGet("statistics/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<PlaybackStatisticsDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStatistics(int sessionId)
    {
        try
        {
            var stats = await _playbackService
                .GetPlaybackStatisticsAsync(sessionId)
                .ConfigureAwait(false);

            if (stats is null)
                return NotFound(ErrorResponse.NotFoundError(
                    $"No statistics available for session {sessionId}", HttpContext.TraceIdentifier));

            return Ok(ApiResponse<PlaybackStatisticsDto>.SuccessResponse(
                stats, "Statistics retrieved", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for session {SessionId}. TraceId: {TraceId}",
                sessionId, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve statistics", HttpContext.TraceIdentifier));
        }
    }
}
