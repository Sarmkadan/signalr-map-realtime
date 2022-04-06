// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Configuration options for the real-time route playback and historical timeline engine.
/// Loaded from the <c>RoutePlayback</c> section of <c>appsettings.json</c>.
/// </summary>
/// <example>
/// <code>
/// "RoutePlayback": {
///   "MaxConcurrentPlaybacks": 100,
///   "DefaultSpeedMultiplier": 1.0,
///   "MaxSpeedMultiplier": 64.0,
///   "MinSpeedMultiplier": 0.1,
///   "MinFrameIntervalMs": 50,
///   "MaxFrameIntervalMs": 10000,
///   "IdleSpeedThresholdKmh": 2.0,
///   "IdleMinDurationSeconds": 60,
///   "SpeedAlertThresholdKmh": 120.0,
///   "MaxLocationsPerPlayback": 50000,
///   "PlaybackSessionTimeoutMinutes": 30
/// }
/// </code>
/// </example>
public class PlaybackOptions
{
    /// <summary>
    /// Configuration section key in <c>appsettings.json</c>.
    /// </summary>
    public const string SectionName = "RoutePlayback";

    /// <summary>
    /// Maximum number of concurrent playback sessions the engine will manage simultaneously.
    /// Exceeding this limit causes <see cref="InvalidOperationException"/> on session start.
    /// </summary>
    public int MaxConcurrentPlaybacks { get; set; } = 100;

    /// <summary>
    /// Default playback speed multiplier applied when the caller does not specify one.
    /// 1.0 produces real-time playback; values greater than 1.0 accelerate playback proportionally.
    /// </summary>
    public double DefaultSpeedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Upper bound for the speed multiplier accepted from client requests.
    /// Values above this are silently clamped to prevent flooding SignalR connections.
    /// </summary>
    public double MaxSpeedMultiplier { get; set; } = 64.0;

    /// <summary>
    /// Lower bound for the speed multiplier accepted from client requests.
    /// A value of 0.1 produces one-tenth of real-time playback speed.
    /// </summary>
    public double MinSpeedMultiplier { get; set; } = 0.1;

    /// <summary>
    /// Minimum interval in milliseconds enforced between successive frame emissions,
    /// regardless of the inter-frame gap in the source data or the active speed multiplier.
    /// A value of 50 ms caps output at roughly 20 frames per second.
    /// </summary>
    public int MinFrameIntervalMs { get; set; } = 50;

    /// <summary>
    /// Maximum interval in milliseconds permitted between successive frame emissions.
    /// Prevents excessive pauses when source data contains large temporal gaps (e.g., tunnel or signal loss).
    /// </summary>
    public int MaxFrameIntervalMs { get; set; } = 10_000;

    /// <summary>
    /// Speed in km/h below which a vehicle is considered idle rather than moving.
    /// Used during segment classification and idle-time aggregation.
    /// </summary>
    public double IdleSpeedThresholdKmh { get; set; } = 2.0;

    /// <summary>
    /// Minimum continuous duration in seconds at or below <see cref="IdleSpeedThresholdKmh"/>
    /// required to classify a segment as <c>Stopped</c> rather than <c>Idle</c>.
    /// </summary>
    public int IdleMinDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Speed in km/h above which a location entry is annotated as a <c>SpeedAlert</c> event
    /// in the generated timeline. Applicable to timeline building, not real-time enforcement.
    /// </summary>
    public double SpeedAlertThresholdKmh { get; set; } = 120.0;

    /// <summary>
    /// Maximum number of location records loaded into memory per playback session.
    /// Sessions with more records are truncated to the most recent N records to bound memory use.
    /// </summary>
    public int MaxLocationsPerPlayback { get; set; } = 50_000;

    /// <summary>
    /// Duration in minutes after which a playback session in <c>Paused</c> or <c>Completed</c>
    /// state is eligible for automatic cleanup by a background housekeeping process.
    /// </summary>
    public int PlaybackSessionTimeoutMinutes { get; set; } = 30;
}
