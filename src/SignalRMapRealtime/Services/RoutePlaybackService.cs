#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Hubs;

/// <summary>
/// Production-grade implementation of the real-time route playback and historical timeline engine.
/// </summary>
/// <remarks>
/// <para>
/// Registered as a singleton so that active playback session state persists across HTTP requests.
/// Scoped database repositories are resolved through <see cref="IServiceScopeFactory"/> to avoid
/// captive-dependency lifetime violations.
/// </para>
/// <para>
/// Each playback session runs an independent background <see cref="Task"/> that advances through
/// the preloaded location sequence, calculates per-frame delays scaled by the active speed
/// multiplier, and pushes <see cref="PlaybackFrameDto"/> objects to the corresponding SignalR
/// client group. Pause/resume is coordinated via a <see cref="ManualResetEventSlim"/> gate, and
/// stop/cancellation is handled through a per-session <see cref="CancellationTokenSource"/>.
/// </para>
/// </remarks>
public sealed class RoutePlaybackService : IRoutePlaybackService, IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<RoutePlaybackHub> _hubContext;
    private readonly PlaybackOptions _options;
    private readonly ILogger<RoutePlaybackService> _logger;
    private readonly ConcurrentDictionary<Guid, PlaybackState> _sessions = new();

    /// <summary>
    /// Initialises a new instance of <see cref="RoutePlaybackService"/>.
    /// </summary>
    public RoutePlaybackService(
        IServiceScopeFactory scopeFactory,
        IHubContext<RoutePlaybackHub> hubContext,
        IOptions<PlaybackOptions> options,
        ILogger<RoutePlaybackService> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(hubContext);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> StartPlaybackAsync(StartPlaybackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_sessions.Count >= _options.MaxConcurrentPlaybacks)
            throw new InvalidOperationException(
                $"The engine has reached its maximum of {_options.MaxConcurrentPlaybacks} concurrent playback sessions.");

        var locations = await LoadLocationsAsync(request.SessionId, cancellationToken);

        if (locations.Count == 0)
            throw new ArgumentException(
                $"Tracking session {request.SessionId} contains no location records and cannot be replayed.",
                nameof(request));

        var speed = Math.Clamp(request.SpeedMultiplier, _options.MinSpeedMultiplier, _options.MaxSpeedMultiplier);
        var playbackId = Guid.NewGuid();
        var state = new PlaybackState(playbackId, request.SessionId, locations, speed) { Loop = request.Loop };

        if (request.StartFromTimestamp.HasValue)
            ApplySeek(state, request.StartFromTimestamp.Value);

        _sessions[playbackId] = state;
        state.PlaybackTask = Task.Run(() => RunPlaybackLoopAsync(state), CancellationToken.None);

        _logger.LogInformation(
            "Playback {PlaybackId} started for session {SessionId}: {FrameCount} frames at {Speed}× speed",
            playbackId, request.SessionId, locations.Count, speed);

        return playbackId;
    }

    /// <inheritdoc />
    public Task PausePlaybackAsync(Guid playbackId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(playbackId, out var state) && state.Status == PlaybackStatus.Playing)
        {
            state.Status = PlaybackStatus.Paused;
            state.PauseGate.Reset();
            _logger.LogInformation("Playback {PlaybackId} paused at frame {Frame}/{Total}",
                playbackId, state.CurrentFrameIndex, state.Locations.Count);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ResumePlaybackAsync(Guid playbackId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(playbackId, out var state) && state.Status == PlaybackStatus.Paused)
        {
            state.Status = PlaybackStatus.Playing;
            state.PauseGate.Set();
            _logger.LogInformation("Playback {PlaybackId} resumed from frame {Frame}/{Total}",
                playbackId, state.CurrentFrameIndex, state.Locations.Count);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopPlaybackAsync(Guid playbackId, CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryRemove(playbackId, out var state))
            return;

        state.Status = PlaybackStatus.Idle;
        state.PauseGate.Set();
        await state.Cts.CancelAsync();

        if (state.PlaybackTask is not null)
            await state.PlaybackTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        state.Dispose();
        _logger.LogInformation("Playback {PlaybackId} stopped and resources released", playbackId);
    }

    /// <inheritdoc />
    public Task SeekToTimestampAsync(Guid playbackId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(playbackId, out var state))
        {
            ApplySeek(state, timestamp);
            _logger.LogInformation("Playback {PlaybackId} seeked to {Timestamp} (frame {Frame})",
                playbackId, timestamp, state.CurrentFrameIndex);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetPlaybackSpeedAsync(Guid playbackId, double speedMultiplier, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(playbackId, out var state))
        {
            state.SpeedMultiplier = Math.Clamp(speedMultiplier, _options.MinSpeedMultiplier, _options.MaxSpeedMultiplier);
            _logger.LogInformation("Playback {PlaybackId} speed set to {Speed}×", playbackId, state.SpeedMultiplier);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<PlaybackSessionDto?> GetPlaybackStateAsync(Guid playbackId) =>
        Task.FromResult(_sessions.TryGetValue(playbackId, out var state) ? MapToDto(state) : null);

    /// <inheritdoc />
    public Task<IReadOnlyList<PlaybackSessionDto>> GetActivePlaybacksAsync()
    {
        IReadOnlyList<PlaybackSessionDto> result = _sessions.Values.Select(MapToDto).ToList();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<RouteTimelineDto?> BuildTimelineAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var locations = await LoadLocationsAsync(sessionId, cancellationToken);
        if (locations.Count == 0) return null;

        var entries = BuildTimelineEntries(locations);
        var segments = BuildTimelineSegments(locations);
        var totalDistanceKm = ComputeTotalDistance(locations);
        var speedSamples = locations.Where(l => l.Speed.HasValue).Select(l => l.Speed!.Value).ToList();

        return new RouteTimelineDto(
            SessionId: sessionId,
            StartTime: locations[0].RecordedAt,
            EndTime: locations[^1].RecordedAt,
            Duration: locations[^1].RecordedAt - locations[0].RecordedAt,
            TotalDistanceKm: Math.Round(totalDistanceKm, 3),
            AverageSpeedKmh: speedSamples.Count > 0 ? Math.Round(speedSamples.Average(), 2) : 0,
            MaxSpeedKmh: speedSamples.Count > 0 ? Math.Round(speedSamples.Max(), 2) : 0,
            LocationCount: locations.Count,
            Entries: entries,
            Segments: segments);
    }

    /// <inheritdoc />
    public async Task<PlaybackFrameDto?> GetSnapshotAtTimestampAsync(int sessionId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        var locations = await LoadLocationsAsync(sessionId, cancellationToken);
        if (locations.Count == 0) return null;

        var clamped = timestamp < locations[0].RecordedAt ? locations[0].RecordedAt
            : timestamp > locations[^1].RecordedAt ? locations[^1].RecordedAt
            : timestamp;

        var index = FindClosestFrameIndex(locations, clamped);
        var cumDistances = ComputeCumulativeDistances(locations);
        return BuildFrame(Guid.Empty, locations, cumDistances, index);
    }

    /// <inheritdoc />
    public async Task<PlaybackStatisticsDto?> GetPlaybackStatisticsAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var locations = await LoadLocationsAsync(sessionId, cancellationToken);
        if (locations.Count == 0) return null;

        var speedSamples = locations.Where(l => l.Speed.HasValue).Select(l => l.Speed!.Value).ToList();
        var totalDistanceKm = ComputeTotalDistance(locations);
        var duration = locations[^1].RecordedAt - locations[0].RecordedAt;
        var idleSeconds = ComputeIdleTimeSeconds(locations);

        var distribution = new List<SpeedDistributionDto>
        {
            new("0–20 km/h",   speedSamples.Count(s => s < 20)),
            new("20–50 km/h",  speedSamples.Count(s => s is >= 20 and < 50)),
            new("50–80 km/h",  speedSamples.Count(s => s is >= 50 and < 80)),
            new("80–120 km/h", speedSamples.Count(s => s is >= 80 and < 120)),
            new("120+ km/h",   speedSamples.Count(s => s >= 120))
        };

        var nonZeroSpeeds = speedSamples.Where(s => s > 0).ToList();

        return new PlaybackStatisticsDto(
            SessionId: sessionId,
            TotalLocations: locations.Count,
            TotalDistanceKm: Math.Round(totalDistanceKm, 3),
            Duration: duration,
            AverageSpeedKmh: speedSamples.Count > 0 ? Math.Round(speedSamples.Average(), 2) : 0,
            MaxSpeedKmh: speedSamples.Count > 0 ? Math.Round(speedSamples.Max(), 2) : 0,
            MinSpeedKmh: nonZeroSpeeds.Count > 0 ? Math.Round(nonZeroSpeeds.Min(), 2) : 0,
            IdleTimeSeconds: idleSeconds,
            MovingTimeSeconds: Math.Max(0, (int)duration.TotalSeconds - idleSeconds),
            SpeedDistribution: distribution);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<IReadOnlyList<Location>> LoadLocationsAsync(int sessionId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<LocationRepository>();
        var raw = await repo.GetLocationsBySessionAsync(sessionId);
        var ordered = raw.OrderBy(l => l.RecordedAt).ToList();

        return ordered.Count > _options.MaxLocationsPerPlayback
            ? ordered.TakeLast(_options.MaxLocationsPerPlayback).ToList()
            : ordered;
    }

    private async Task RunPlaybackLoopAsync(PlaybackState state)
    {
        try
        {
            while (!state.Cts.Token.IsCancellationRequested)
            {
                // Blocks here when paused; throws OperationCanceledException when stopped.
                state.PauseGate.Wait(state.Cts.Token);

                if (state.CurrentFrameIndex >= state.Locations.Count)
                {
                    if (state.Loop)
                    {
                        state.CurrentFrameIndex = 0;
                        continue;
                    }
                    break;
                }

                var frame = BuildFrame(
                    state.PlaybackId,
                    state.Locations,
                    state.CumulativeDistances,
                    state.CurrentFrameIndex);

                await _hubContext.Clients
                    .Group(PlaybackGroup(state.PlaybackId))
                    .SendAsync("PlaybackFrame", frame, state.Cts.Token);

                var emittedIndex = state.CurrentFrameIndex;
                state.CurrentFrameIndex = emittedIndex + 1;

                if (state.CurrentFrameIndex < state.Locations.Count)
                {
                    var gap = state.Locations[state.CurrentFrameIndex].RecordedAt
                              - state.Locations[emittedIndex].RecordedAt;

                    var delayMs = (int)Math.Clamp(
                        gap.TotalMilliseconds / state.SpeedMultiplier,
                        _options.MinFrameIntervalMs,
                        _options.MaxFrameIntervalMs);

                    await Task.Delay(delayMs, state.Cts.Token);
                }
            }

            if (!state.Cts.Token.IsCancellationRequested)
            {
                state.Status = PlaybackStatus.Completed;
                await _hubContext.Clients
                    .Group(PlaybackGroup(state.PlaybackId))
                    .SendAsync("PlaybackCompleted", new
                    {
                        state.PlaybackId,
                        CompletedAt = DateTime.UtcNow,
                        TotalFrames = state.Locations.Count
                    });

                _logger.LogInformation("Playback {PlaybackId} reached the final frame and completed",
                    state.PlaybackId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Playback {PlaybackId} loop exited via cancellation", state.PlaybackId);
        }
        catch (Exception ex)
        {
            state.Status = PlaybackStatus.Error;
            _logger.LogError(ex, "Playback {PlaybackId} terminated with an unhandled error", state.PlaybackId);

            await _hubContext.Clients
                .Group(PlaybackGroup(state.PlaybackId))
                .SendAsync("PlaybackError", new { state.PlaybackId, Error = ex.Message });
        }
    }

    private static PlaybackFrameDto BuildFrame(
        Guid playbackId,
        IReadOnlyList<Location> locations,
        double[] cumulativeDistances,
        int frameIndex)
    {
        var loc = locations[frameIndex];
        var totalFrames = locations.Count;
        var completion = totalFrames > 1 ? frameIndex * 100 / (totalFrames - 1) : 100;

        return new PlaybackFrameDto(
            PlaybackId: playbackId,
            FrameIndex: frameIndex,
            TotalFrames: totalFrames,
            Timestamp: loc.RecordedAt,
            Latitude: loc.Latitude,
            Longitude: loc.Longitude,
            Speed: loc.Speed,
            Bearing: loc.Bearing,
            Altitude: loc.Altitude,
            DistanceCoveredKm: Math.Round(cumulativeDistances[frameIndex], 3),
            CompletionPercentage: completion,
            ElapsedTime: loc.RecordedAt - locations[0].RecordedAt,
            Address: loc.Address);
    }

    private IReadOnlyList<TimelineEntryDto> BuildTimelineEntries(IReadOnlyList<Location> locations)
    {
        var entries = new List<TimelineEntryDto>(locations.Count);

        for (int i = 0; i < locations.Count; i++)
        {
            var loc = locations[i];

            var eventType = i == 0 ? TimelineEventType.SessionStart
                : i == locations.Count - 1 ? TimelineEventType.SessionEnd
                : (loc.Speed ?? 0) >= _options.SpeedAlertThresholdKmh ? TimelineEventType.SpeedAlert
                : (loc.Speed ?? 1) < 1.0 ? TimelineEventType.Stop
                : TimelineEventType.LocationUpdate;

            var label = eventType switch
            {
                TimelineEventType.SessionStart => "Journey Started",
                TimelineEventType.SessionEnd => "Journey Ended",
                TimelineEventType.SpeedAlert => $"Speed Alert: {loc.Speed:F1} km/h",
                TimelineEventType.Stop => "Vehicle Stopped",
                _ => null
            };

            entries.Add(new TimelineEntryDto(
                Timestamp: loc.RecordedAt,
                Latitude: loc.Latitude,
                Longitude: loc.Longitude,
                Speed: loc.Speed,
                Bearing: loc.Bearing,
                EventLabel: label,
                EventType: eventType));
        }

        return entries;
    }

    private IReadOnlyList<TimelineSegmentDto> BuildTimelineSegments(IReadOnlyList<Location> locations)
    {
        if (locations.Count < 2) return [];

        var segments = new List<TimelineSegmentDto>();
        int segmentIndex = 0;
        int segStart = 0;

        SegmentType Classify(double? speed) =>
            (speed ?? 0) >= _options.IdleSpeedThresholdKmh ? SegmentType.Moving : SegmentType.Idle;

        var currentType = Classify(locations[0].Speed);

        for (int i = 1; i <= locations.Count; i++)
        {
            var isLast = i == locations.Count;
            var nextType = isLast ? currentType : Classify(locations[i].Speed);

            if (nextType == currentType && !isLast)
                continue;

            var endIdx = i - 1;
            var segLocs = locations.Skip(segStart).Take(endIdx - segStart + 1).ToList();

            if (segLocs.Count > 0)
            {
                var duration = segLocs[^1].RecordedAt - segLocs[0].RecordedAt;

                // Promote long idle periods to Stopped
                var finalType = currentType == SegmentType.Idle
                    && duration.TotalSeconds >= _options.IdleMinDurationSeconds
                        ? SegmentType.Stopped
                        : currentType;

                var dist = ComputeTotalDistance(segLocs);
                var avgSpd = segLocs.Any(l => l.Speed.HasValue)
                    ? segLocs.Where(l => l.Speed.HasValue).Average(l => l.Speed!.Value)
                    : 0;

                segments.Add(new TimelineSegmentDto(
                    Index: segmentIndex++,
                    StartTime: segLocs[0].RecordedAt,
                    EndTime: segLocs[^1].RecordedAt,
                    Duration: duration,
                    DistanceKm: Math.Round(dist, 3),
                    AverageSpeedKmh: Math.Round(avgSpd, 2),
                    Type: finalType));
            }

            segStart = i;
            currentType = nextType;
        }

        return segments;
    }

    private int ComputeIdleTimeSeconds(IReadOnlyList<Location> locations)
    {
        var total = 0;
        for (int i = 1; i < locations.Count; i++)
        {
            if ((locations[i - 1].Speed ?? 0) < _options.IdleSpeedThresholdKmh)
                total += (int)(locations[i].RecordedAt - locations[i - 1].RecordedAt).TotalSeconds;
        }

        return total;
    }

    private static double ComputeTotalDistance(IReadOnlyList<Location> locations)
    {
        double total = 0;
        for (int i = 1; i < locations.Count; i++)
            total += locations[i - 1].CalculateDistanceTo(locations[i]);
        return total;
    }

    private static double[] ComputeCumulativeDistances(IReadOnlyList<Location> locations)
    {
        var distances = new double[locations.Count];
        for (int i = 1; i < locations.Count; i++)
            distances[i] = distances[i - 1] + locations[i - 1].CalculateDistanceTo(locations[i]);
        return distances;
    }

    private static int FindClosestFrameIndex(IReadOnlyList<Location> locations, DateTime timestamp)
    {
        int closest = 0;
        var minDiff = TimeSpan.MaxValue;

        for (int i = 0; i < locations.Count; i++)
        {
            var diff = (locations[i].RecordedAt - timestamp).Duration();
            if (diff >= minDiff) continue;
            minDiff = diff;
            closest = i;
        }

        return closest;
    }

    private static void ApplySeek(PlaybackState state, DateTime timestamp)
    {
        var clamped = timestamp < state.PlaybackStart ? state.PlaybackStart
            : timestamp > state.PlaybackEnd ? state.PlaybackEnd
            : timestamp;

        state.CurrentFrameIndex = FindClosestFrameIndex(state.Locations, clamped);
    }

    private static PlaybackSessionDto MapToDto(PlaybackState state)
    {
        var totalFrames = state.Locations.Count;
        var pct = totalFrames > 1 ? state.CurrentFrameIndex * 100 / (totalFrames - 1) : 100;
        var currentTs = state.CurrentFrameIndex < totalFrames
            ? state.Locations[state.CurrentFrameIndex].RecordedAt
            : state.PlaybackEnd;

        return new PlaybackSessionDto(
            PlaybackId: state.PlaybackId,
            TrackingSessionId: state.TrackingSessionId,
            Status: state.Status,
            StartedAt: state.StartedAt,
            PlaybackStart: state.PlaybackStart,
            PlaybackEnd: state.PlaybackEnd,
            SpeedMultiplier: state.SpeedMultiplier,
            CurrentTimestamp: currentTs,
            CurrentFrame: state.CurrentFrameIndex,
            TotalFrames: totalFrames,
            CompletionPercentage: pct,
            Loop: state.Loop);
    }

    private static string PlaybackGroup(Guid playbackId) => $"playback-{playbackId}";

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var id in _sessions.Keys.ToArray())
            await StopPlaybackAsync(id);
    }

    // -------------------------------------------------------------------------
    // Internal session state
    // -------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates all mutable state for a single active playback session.
    /// </summary>
    private sealed class PlaybackState : IDisposable
    {
        /// <summary>Unique playback session identifier.</summary>
        public Guid PlaybackId { get; }

        /// <summary>Source tracking session identifier.</summary>
        public int TrackingSessionId { get; }

        /// <summary>Ordered, preloaded location sequence for this session.</summary>
        public IReadOnlyList<Location> Locations { get; }

        /// <summary>Precomputed cumulative distances indexed by frame position.</summary>
        public double[] CumulativeDistances { get; }

        /// <summary>UTC wall-clock time when this playback was created.</summary>
        public DateTime StartedAt { get; } = DateTime.UtcNow;

        /// <summary>Current playback speed relative to real time.</summary>
        public double SpeedMultiplier;

        /// <summary>Current lifecycle status of the session.</summary>
        public volatile PlaybackStatus Status = PlaybackStatus.Playing;

        /// <summary>Zero-based index of the next frame to emit.</summary>
        public int CurrentFrameIndex;

        /// <summary>Whether playback restarts after the final frame.</summary>
        public bool Loop { get; init; }

        /// <summary>Controls cooperative cancellation of the background playback task.</summary>
        public CancellationTokenSource Cts { get; } = new();

        /// <summary>Background loop task reference, set immediately after construction.</summary>
        public Task? PlaybackTask { get; set; }

        /// <summary>
        /// Gate that blocks the playback loop when the session is paused.
        /// Initialised to set (unblocked) so playback begins immediately.
        /// </summary>
        public ManualResetEventSlim PauseGate { get; } = new(initialState: true);

        /// <summary>UTC timestamp of the first recorded location.</summary>
        public DateTime PlaybackStart => Locations.Count > 0 ? Locations[0].RecordedAt : DateTime.MinValue;

        /// <summary>UTC timestamp of the last recorded location.</summary>
        public DateTime PlaybackEnd => Locations.Count > 0 ? Locations[^1].RecordedAt : DateTime.MinValue;

        public PlaybackState(Guid playbackId, int trackingSessionId, IReadOnlyList<Location> locations, double speedMultiplier)
        {
            PlaybackId = playbackId;
            TrackingSessionId = trackingSessionId;
            Locations = locations;
            SpeedMultiplier = speedMultiplier;
            CumulativeDistances = BuildCumulativeDistances(locations);
        }

        private static double[] BuildCumulativeDistances(IReadOnlyList<Location> locations)
        {
            var distances = new double[locations.Count];
            for (int i = 1; i < locations.Count; i++)
                distances[i] = distances[i - 1] + locations[i - 1].CalculateDistanceTo(locations[i]);
            return distances;
        }

        public void Dispose()
        {
            Cts.Dispose();
            PauseGate.Dispose();
        }
    }
}
