using Xunit;
using SignalRMapRealtime.Configuration;

namespace SignalRMapRealtime.Tests;

public class PlaybackOptionsTests
{
    [Fact]
    public void Constructor_InitializesWithCorrectDefaults()
    {
        // Arrange & Act
        var options = new PlaybackOptions();

        // Assert
        Assert.Equal(100, options.MaxConcurrentPlaybacks);
        Assert.Equal(1.0, options.DefaultSpeedMultiplier);
        Assert.Equal(8.0, options.MaxSpeedMultiplier);
        Assert.Equal(0.25, options.MinSpeedMultiplier);
        Assert.Equal(50, options.MinFrameIntervalMs);
        Assert.Equal(10_000, options.MaxFrameIntervalMs);
        Assert.Equal(2.0, options.IdleSpeedThresholdKmh);
        Assert.Equal(60, options.IdleMinDurationSeconds);
        Assert.Equal(120.0, options.SpeedAlertThresholdKmh);
        Assert.Equal(50_000, options.MaxLocationsPerPlayback);
        Assert.Equal(30, options.PlaybackSessionTimeoutMinutes);
    }

    [Fact]
    public void MaxConcurrentPlaybacks_CanBeSetAndGet()
    {
        // Arrange
        var options = new PlaybackOptions();
        int expected = 200;

        // Act
        options.MaxConcurrentPlaybacks = expected;

        // Assert
        Assert.Equal(expected, options.MaxConcurrentPlaybacks);
    }

    [Fact]
    public void SpeedMultipliers_CanBeSetAndGet()
    {
        // Arrange
        var options = new PlaybackOptions();

        // Act
        options.DefaultSpeedMultiplier = 2.5;
        options.MaxSpeedMultiplier = 16.0;
        options.MinSpeedMultiplier = 0.5;

        // Assert
        Assert.Equal(2.5, options.DefaultSpeedMultiplier);
        Assert.Equal(16.0, options.MaxSpeedMultiplier);
        Assert.Equal(0.5, options.MinSpeedMultiplier);
    }

    [Fact]
    public void FrameIntervals_CanBeSetAndGet()
    {
        // Arrange
        var options = new PlaybackOptions();

        // Act
        options.MinFrameIntervalMs = 100;
        options.MaxFrameIntervalMs = 20_000;

        // Assert
        Assert.Equal(100, options.MinFrameIntervalMs);
        Assert.Equal(20_000, options.MaxFrameIntervalMs);
    }

    [Fact]
    public void Thresholds_CanBeSetAndGet()
    {
        // Arrange
        var options = new PlaybackOptions();

        // Act
        options.IdleSpeedThresholdKmh = 5.0;
        options.IdleMinDurationSeconds = 120;
        options.SpeedAlertThresholdKmh = 130.0;

        // Assert
        Assert.Equal(5.0, options.IdleSpeedThresholdKmh);
        Assert.Equal(120, options.IdleMinDurationSeconds);
        Assert.Equal(130.0, options.SpeedAlertThresholdKmh);
    }

    [Fact]
    public void Limits_CanBeSetAndGet()
    {
        // Arrange
        var options = new PlaybackOptions();

        // Act
        options.MaxLocationsPerPlayback = 100_000;
        options.PlaybackSessionTimeoutMinutes = 60;

        // Assert
        Assert.Equal(100_000, options.MaxLocationsPerPlayback);
        Assert.Equal(60, options.PlaybackSessionTimeoutMinutes);
    }
}
