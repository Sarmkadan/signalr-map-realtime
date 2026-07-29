using Xunit;
using SignalRMapRealtime.Configuration;

namespace SignalRMapRealtime.Tests;

public class PlaybackOptionsExtensionsTests
{
    [Fact]
    public void ClampSpeedMultiplier_HappyPath_ReturnsClampedValue()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedMultiplier = 10.0;
        double expected = Math.Min(Math.Max(speedMultiplier, options.MinSpeedMultiplier), options.MaxSpeedMultiplier);

        // Act
        double actual = options.ClampSpeedMultiplier(speedMultiplier);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ClampSpeedMultiplier_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        PlaybackOptions options = null;
        double speedMultiplier = 10.0;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => options.ClampSpeedMultiplier(speedMultiplier));
    }

    [Fact]
    public void IsRealTime_HappyPath_ReturnsTrue()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedMultiplier = 1.0;
        double realTimeTolerance = 0.05;

        // Act
        bool actual = options.IsRealTime(speedMultiplier);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsRealTime_SpeedMultiplierBelowMin_ReturnsFalse()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedMultiplier = 0.9;
        double realTimeTolerance = 0.05;

        // Act
        bool actual = options.IsRealTime(speedMultiplier);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void CalculateFrameIntervalMs_HappyPath_ReturnsCorrectValue()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedMultiplier = 1.0;
        int expected = options.MaxFrameIntervalMs;

        // Act
        int actual = options.CalculateFrameIntervalMs(speedMultiplier);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculateFrameIntervalMs_SpeedMultiplierAboveMax_ReturnsMinFrameInterval()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedMultiplier = 10.0;
        int expected = options.MinFrameIntervalMs;

        // Act
        int actual = options.CalculateFrameIntervalMs(speedMultiplier);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsSpeedAlert_HappyPath_ReturnsTrue()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedKmh = 120.0;

        // Act
        bool actual = options.IsSpeedAlert(speedKmh);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsSpeedAlert_SpeedBelowThreshold_ReturnsFalse()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedKmh = 100.0;

        // Act
        bool actual = options.IsSpeedAlert(speedKmh);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsIdle_HappyPath_ReturnsTrue()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedKmh = 2.0;

        // Act
        bool actual = options.IsIdle(speedKmh);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsIdle_SpeedAboveThreshold_ReturnsFalse()
    {
        // Arrange
        var options = new PlaybackOptions();
        double speedKmh = 5.0;

        // Act
        bool actual = options.IsIdle(speedKmh);

        // Assert
        Assert.False(actual);
    }
}
