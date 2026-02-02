#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Hubs;

using Microsoft.AspNetCore.SignalR;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;

/// <summary>
/// SignalR hub that provides real-time route playback streaming and historical timeline interaction.
/// </summary>
/// <remarks>
/// <para>
/// Clients connect to this hub to initiate historical route playback, control playback
/// lifecycle (pause, resume, seek, speed adjustment), and receive streamed
/// <see cref="PlaybackFrameDto"/> frames pushed by the <see cref="IRoutePlaybackService"/>.
/// </para>
/// <para>
/// <b>Client-to-server methods</b> (callable from JavaScript/TypeScript):
/// <list type="bullet">
///   <item><see cref="StartPlayback"/> – create a new playback session</item>
///   <item><see cref="PausePlayback"/> – suspend a running session</item>
///   <item><see cref="ResumePlayback"/> – resume a paused session</item>
///   <item><see cref="StopPlayback"/> – terminate a session</item>
///   <item><see cref="SeekTo"/> – reposition the playback cursor</item>
///   <item><see cref="SetSpeed"/> – change the playback speed multiplier</item>
///   <item><see cref="SubscribeToPlayback"/> – join an existing session's frame stream</item>
///   <item><see cref="UnsubscribeFromPlayback"/> – leave a session's frame stream</item>
///   <item><see cref="GetPlaybackState"/> – poll current session state</item>
///   <item><see cref="RequestTimeline"/> – retrieve annotated journey timeline</item>
///   <item><see cref="RequestSnapshot"/> – get vehicle state at a specific timestamp</item>
///   <item><see cref="RequestStatistics"/> – retrieve session analytics</item>
/// </list>
/// </para>
/// <para>
/// <b>Server-to-client events</b> (received by JavaScript/TypeScript):
/// <list type="bullet">
///   <item><c>PlaybackStarted</c> – emitted to the caller after session creation</item>
///   <item><c>PlaybackFrame</c> – streamed repeatedly with a <see cref="PlaybackFrameDto"/> payload</item>
///   <item><c>PlaybackPaused</c> / <c>PlaybackResumed</c> – broadcast to the session group</item>
///   <item><c>PlaybackStopped</c> / <c>PlaybackCompleted</c> / <c>PlaybackError</c></item>
///   <item><c>PlaybackSeeked</c> – emitted to the group after a seek operation</item>
///   <item><c>PlaybackSpeedChanged</c> – emitted to the group after a speed update</item>
///   <item><c>SubscribedToPlayback</c> / <c>UnsubscribedFromPlayback</c> – caller confirmation</item>
///   <item><c>PlaybackState</c> – response to <see cref="GetPlaybackState"/></item>
///   <item><c>TimelineReady</c> – response to <see cref="RequestTimeline"/></item>
///   <item><c>SnapshotReady</c> – response to <see cref="RequestSnapshot"/></item>
///   <item><c>StatisticsReady</c> – response to <see cref="RequestStatistics"/></item>
/// </list>
/// </para>
/// </remarks>
public class RoutePlaybackHub : Hub
{
    private readonly IRoutePlaybackService _playbackService;
    private readonly ILogger<RoutePlaybackHub> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="RoutePlaybackHub"/>.
    /// </summary>
    public RoutePlaybackHub(IRoutePlaybackService playbackService, ILogger<RoutePlaybackHub> logger)
    {
        ArgumentNullException.ThrowIfNull(playbackService);
        ArgumentNullException.ThrowIfNull(logger);

        _playbackService = playbackService;
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Playback client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Playback client {ConnectionId} disconnected. Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "clean disconnect");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Creates a new playback session and subscribes the calling client to its frame stream.
    /// The caller immediately receives a <c>PlaybackStarted</c> event followed by a continuous
    /// <c>PlaybackFrame</c> stream on the session's SignalR group.
    /// </summary>
    /// <param name="request">Session configuration including source session ID, speed, optional start timestamp, and loop flag.</param>
    public async Task StartPlayback(StartPlaybackRequest request)
    {
        try
        {
            var playbackId = await _playbackService.StartPlaybackAsync(request);
            await Groups.AddToGroupAsync(Context.ConnectionId, PlaybackGroup(playbackId));

            await Clients.Caller.SendAsync("PlaybackStarted", new
            {
                PlaybackId = playbackId,
                request.SessionId,
                AppliedSpeedMultiplier = request.SpeedMultiplier,
                request.Loop,
                StartedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Client {ConnectionId} started playback {PlaybackId} for session {SessionId}",
                Context.ConnectionId, playbackId, request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Client {ConnectionId} failed to start playback for session {SessionId}",
                Context.ConnectionId, request.SessionId);
            await Clients.Caller.SendAsync("PlaybackError", new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Pauses an active playback session. Frame emission stops and the cursor position is retained.
    /// A <c>PlaybackPaused</c> event is broadcast to all members of the session's SignalR group.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to pause.</param>
    public async Task PausePlayback(Guid playbackId)
    {
        try
        {
            await _playbackService.PausePlaybackAsync(playbackId);
            var state = await _playbackService.GetPlaybackStateAsync(playbackId);
            await Clients.Group(PlaybackGroup(playbackId)).SendAsync("PlaybackPaused", state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing playback {PlaybackId}", playbackId);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Resumes a previously paused playback session from its retained cursor position.
    /// A <c>PlaybackResumed</c> event is broadcast to all members of the session's SignalR group.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to resume.</param>
    public async Task ResumePlayback(Guid playbackId)
    {
        try
        {
            await _playbackService.ResumePlaybackAsync(playbackId);
            var state = await _playbackService.GetPlaybackStateAsync(playbackId);
            await Clients.Group(PlaybackGroup(playbackId)).SendAsync("PlaybackResumed", state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming playback {PlaybackId}", playbackId);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Stops and removes a playback session, releasing all engine resources.
    /// A <c>PlaybackStopped</c> event is broadcast to the session group before teardown.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to stop.</param>
    public async Task StopPlayback(Guid playbackId)
    {
        try
        {
            await Clients.Group(PlaybackGroup(playbackId)).SendAsync("PlaybackStopped", new
            {
                PlaybackId = playbackId,
                StoppedAt = DateTime.UtcNow
            });

            await _playbackService.StopPlaybackAsync(playbackId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, PlaybackGroup(playbackId));

            _logger.LogInformation("Client {ConnectionId} stopped playback {PlaybackId}",
                Context.ConnectionId, playbackId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping playback {PlaybackId}", playbackId);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Repositions the playback cursor to the recorded frame whose timestamp is closest to
    /// the specified UTC value. A <c>PlaybackSeeked</c> event is broadcast to the session group.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session.</param>
    /// <param name="timestamp">Target UTC timestamp. Values outside the session's range are clamped.</param>
    public async Task SeekTo(Guid playbackId, DateTime timestamp)
    {
        try
        {
            await _playbackService.SeekToTimestampAsync(playbackId, timestamp);
            var state = await _playbackService.GetPlaybackStateAsync(playbackId);

            await Clients.Group(PlaybackGroup(playbackId)).SendAsync("PlaybackSeeked", new
            {
                PlaybackId = playbackId,
                TargetTimestamp = timestamp,
                State = state
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeking playback {PlaybackId} to {Timestamp}", playbackId, timestamp);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Updates the playback speed multiplier on a running or paused session.
    /// The value is clamped to the engine's configured range.
    /// A <c>PlaybackSpeedChanged</c> event is broadcast to all members of the session group.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session.</param>
    /// <param name="speedMultiplier">Desired speed factor (1.0 = real-time, 4.0 = 4× faster).</param>
    public async Task SetSpeed(Guid playbackId, double speedMultiplier)
    {
        try
        {
            await _playbackService.SetPlaybackSpeedAsync(playbackId, speedMultiplier);
            var state = await _playbackService.GetPlaybackStateAsync(playbackId);

            await Clients.Group(PlaybackGroup(playbackId)).SendAsync("PlaybackSpeedChanged", new
            {
                PlaybackId = playbackId,
                SpeedMultiplier = state?.SpeedMultiplier ?? speedMultiplier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting speed for playback {PlaybackId}", playbackId);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Subscribes the calling client to the frame stream of an existing playback session.
    /// The caller receives a <c>SubscribedToPlayback</c> confirmation containing the current session state.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to observe.</param>
    public async Task SubscribeToPlayback(Guid playbackId)
    {
        try
        {
            var state = await _playbackService.GetPlaybackStateAsync(playbackId);

            if (state is null)
            {
                await Clients.Caller.SendAsync("PlaybackError", new
                {
                    PlaybackId = playbackId,
                    Error = $"Playback session {playbackId} does not exist or has already completed."
                });
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, PlaybackGroup(playbackId));
            await Clients.Caller.SendAsync("SubscribedToPlayback", state);

            _logger.LogInformation("Client {ConnectionId} subscribed to playback {PlaybackId}",
                Context.ConnectionId, playbackId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing client {ConnectionId} to playback {PlaybackId}",
                Context.ConnectionId, playbackId);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Removes the calling client from a playback session's SignalR group.
    /// The caller receives an <c>UnsubscribedFromPlayback</c> confirmation.
    /// The session itself continues running for other subscribers.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to leave.</param>
    public async Task UnsubscribeFromPlayback(Guid playbackId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, PlaybackGroup(playbackId));
            await Clients.Caller.SendAsync("UnsubscribedFromPlayback", playbackId);

            _logger.LogInformation("Client {ConnectionId} unsubscribed from playback {PlaybackId}",
                Context.ConnectionId, playbackId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing client {ConnectionId} from playback {PlaybackId}",
                Context.ConnectionId, playbackId);
        }
    }

    /// <summary>
    /// Returns the current state of a playback session to the calling client via a <c>PlaybackState</c> event.
    /// </summary>
    /// <param name="playbackId">Unique identifier of the playback session to query.</param>
    public async Task GetPlaybackState(Guid playbackId)
    {
        try
        {
            var state = await _playbackService.GetPlaybackStateAsync(playbackId);
            await Clients.Caller.SendAsync("PlaybackState", state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching state for playback {PlaybackId}", playbackId);
            await Clients.Caller.SendAsync("PlaybackError", new { PlaybackId = playbackId, Error = ex.Message });
        }
    }

    /// <summary>
    /// Builds and delivers the fully annotated timeline for a tracking session to the calling client.
    /// The response is sent as a <c>TimelineReady</c> event containing a <see cref="RouteTimelineDto"/>.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session whose timeline to build.</param>
    public async Task RequestTimeline(int sessionId)
    {
        try
        {
            var timeline = await _playbackService.BuildTimelineAsync(sessionId);
            await Clients.Caller.SendAsync("TimelineReady", timeline);

            _logger.LogInformation("Timeline for session {SessionId} delivered to client {ConnectionId}",
                sessionId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building timeline for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("PlaybackError", new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Returns a vehicle position snapshot at the closest recorded moment to a given timestamp,
    /// without initiating a streaming playback session.
    /// The response is sent as a <c>SnapshotReady</c> event containing a <see cref="PlaybackFrameDto"/>.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session to query.</param>
    /// <param name="timestamp">Target UTC timestamp. Clamped to the session's recorded range.</param>
    public async Task RequestSnapshot(int sessionId, DateTime timestamp)
    {
        try
        {
            var snapshot = await _playbackService.GetSnapshotAtTimestampAsync(sessionId, timestamp);
            await Clients.Caller.SendAsync("SnapshotReady", snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching snapshot for session {SessionId} at {Timestamp}",
                sessionId, timestamp);
            await Clients.Caller.SendAsync("PlaybackError", new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Computes and returns aggregated performance statistics for a tracking session.
    /// The response is sent as a <c>StatisticsReady</c> event containing a <see cref="PlaybackStatisticsDto"/>.
    /// </summary>
    /// <param name="sessionId">Identifier of the tracking session to analyse.</param>
    public async Task RequestStatistics(int sessionId)
    {
        try
        {
            var statistics = await _playbackService.GetPlaybackStatisticsAsync(sessionId);
            await Clients.Caller.SendAsync("StatisticsReady", statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing statistics for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("PlaybackError", new { Error = ex.Message });
        }
    }

    private static string PlaybackGroup(Guid playbackId) => $"playback-{playbackId}";
}
