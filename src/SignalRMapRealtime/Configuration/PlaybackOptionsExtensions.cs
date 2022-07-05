#nullable enable

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Provides extension methods for <see cref="PlaybackOptions"/> to simplify common playback configuration scenarios.
/// </summary>
public static class PlaybackOptionsExtensions
{
    /// <summary>
    /// Clamps the specified speed multiplier to the valid range defined in <see cref="PlaybackOptions"/>.
    /// </summary>
    /// <param name="options">The playback options instance.</param>
    /// <param name="speedMultiplier">The speed multiplier to clamp.</param>
    /// <returns>The clamped speed multiplier within [MinSpeedMultiplier, MaxSpeedMultiplier].</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static double ClampSpeedMultiplier(this PlaybackOptions options, double speedMultiplier)
    {
        ArgumentNullException.ThrowIfNull(options);

        return Math.Clamp(
            speedMultiplier,
            options.MinSpeedMultiplier,
            options.MaxSpeedMultiplier
        );
    }

    /// <summary>
    /// Determines whether the specified speed multiplier is considered "real-time" based on the configured thresholds.
    /// </summary>
    /// <param name="options">The playback options instance.</param>
    /// <param name="speedMultiplier">The speed multiplier to check.</param>
    /// <returns>True if the speed multiplier is within 5% of real-time (1.0); otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsRealTime(this PlaybackOptions options, double speedMultiplier)
    {
        ArgumentNullException.ThrowIfNull(options);

        const double RealTimeTolerance = 0.05; // 5% tolerance
        var clamped = options.ClampSpeedMultiplier(speedMultiplier);
        return Math.Abs(clamped - 1.0) <= RealTimeTolerance;
    }

    /// <summary>
    /// Calculates the effective frame interval in milliseconds based on the configured minimum/maximum intervals
    /// and the current speed multiplier.
    /// </summary>
    /// <param name="options">The playback options instance.</param>
    /// <param name="speedMultiplier">The current speed multiplier.</param>
    /// <returns>The calculated frame interval in milliseconds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static int CalculateFrameIntervalMs(this PlaybackOptions options, double speedMultiplier)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Frame interval is inversely proportional to speed multiplier
        // Higher speed multiplier = faster playback = shorter frame interval
        var normalizedSpeed = options.ClampSpeedMultiplier(speedMultiplier);
        var intervalRange = options.MaxFrameIntervalMs - options.MinFrameIntervalMs;
        var speedRange = options.MaxSpeedMultiplier - options.MinSpeedMultiplier;

        // Calculate proportional interval based on speed
        var proportionalInterval = options.MaxFrameIntervalMs -
            (int)Math.Round((normalizedSpeed - options.MinSpeedMultiplier) / speedRange * intervalRange);

        // Ensure within bounds
        return Math.Clamp(
            proportionalInterval,
            options.MinFrameIntervalMs,
            options.MaxFrameIntervalMs
        );
    }

    /// <summary>
    /// Determines whether the specified speed in km/h should trigger a speed alert based on the configured threshold.
    /// </summary>
    /// <param name="options">The playback options instance.</param>
    /// <param name="speedKmh">The speed to check in kilometers per hour.</param>
    /// <returns>True if the speed exceeds the configured speed alert threshold; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsSpeedAlert(this PlaybackOptions options, double speedKmh)
    {
        ArgumentNullException.ThrowIfNull(options);

        return speedKmh > options.SpeedAlertThresholdKmh;
    }

    /// <summary>
    /// Determines whether the specified speed in km/h indicates an idle state based on the configured threshold.
    /// </summary>
    /// <param name="options">The playback options instance.</param>
    /// <param name="speedKmh">The speed to check in kilometers per hour.</param>
    /// <returns>True if the speed is below the configured idle threshold; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsIdle(this PlaybackOptions options, double speedKmh)
    {
        ArgumentNullException.ThrowIfNull(options);

        return speedKmh <= options.IdleSpeedThresholdKmh;
    }
}