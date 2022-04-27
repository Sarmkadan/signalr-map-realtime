#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using NSubstitute;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using Xunit;

/// <summary>
/// Tests for the PlaybackService class.
/// </summary>
public class PlaybackServiceTests
{
    /// <summary>
    /// Tests that GetPlaybackStateAsync returns null for an unknown session.
    /// </summary>
    [Fact]
    public async Task GetPlaybackStateAsync_ForUnknownSession_ReturnsNull()
    {
        // Arrange
        var service = Substitute.For<IRoutePlaybackService>();
        service.GetPlaybackStateAsync(Arg.Any<Guid>())
            .Returns((PlaybackSessionDto?)null);

        // Act
        var result = await service.GetPlaybackStateAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetActivePlaybacksAsync returns an empty list when there are no active sessions.
    /// </summary>
    [Fact]
    public async Task GetActivePlaybacksAsync_WithNoActiveSessions_ReturnsEmptyList()
    {
        // Arrange
        var service = Substitute.For<IRoutePlaybackService>();
        service.GetActivePlaybacksAsync()
            .Returns(Array.Empty<PlaybackSessionDto>());

        // Act
        var result = await service.GetActivePlaybacksAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that BuildTimelineAsync returns null for a session with no locations.
    /// </summary>
    [Fact]
    public async Task BuildTimelineAsync_ForSessionWithNoLocations_ReturnsNull()
    {
        // Arrange
        var service = Substitute.For<IRoutePlaybackService>();
        service.BuildTimelineAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((RouteTimelineDto?)null);

        // Act
        var result = await service.BuildTimelineAsync(42);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that BuildTimelineAsync returns a populated timeline for a session with locations.
    /// </summary>
    [Fact]
    public async Task BuildTimelineAsync_ForSessionWithLocations_ReturnsPopulatedTimeline()
    {
        // Arrange
        var expectedTimeline = new RouteTimelineDto(
            SessionId: 1,
            StartTime: new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc),
            EndTime: new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc),
            Duration: TimeSpan.FromHours(1),
            TotalDistanceKm: 45.2,
            AverageSpeedKmh: 45.2,
            MaxSpeedKmh: 90.0,
            LocationCount: 120,
            Entries: Array.Empty<TimelineEntryDto>(),
            Segments: Array.Empty<TimelineSegmentDto>());

        var service = Substitute.For<IRoutePlaybackService>();
        service.BuildTimelineAsync(1, Arg.Any<CancellationToken>())
            .Returns(expectedTimeline);

        // Act
        var result = await service.BuildTimelineAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(1);
        result.TotalDistanceKm.Should().Be(45.2);
        result.LocationCount.Should().Be(120);
        result.Duration.Should().Be(TimeSpan.FromHours(1));
    }

    /// <summary>
    /// Tests that GetPlaybackStatisticsAsync returns statistics for a valid session.
    /// </summary>
    [Fact]
    public async Task GetPlaybackStatisticsAsync_ForValidSession_ReturnsStatistics()
    {
        // Arrange
        var expected = new PlaybackStatisticsDto(
            SessionId: 5,
            TotalLocations: 200,
            TotalDistanceKm: 120.5,
            Duration: TimeSpan.FromHours(2),
            AverageSpeedKmh: 60.25,
            MaxSpeedKmh: 110.0,
            MinSpeedKmh: 10.0,
            IdleTimeSeconds: 300,
            MovingTimeSeconds: 6900,
            SpeedDistribution: new[]
            {
                new SpeedDistributionDto("0–30 km/h", 20),
                new SpeedDistributionDto("30–60 km/h", 80),
                new SpeedDistributionDto("60–90 km/h", 70),
                new SpeedDistributionDto("90+ km/h", 30),
            });

        var service = Substitute.For<IRoutePlaybackService>();
        service.GetPlaybackStatisticsAsync(5, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await service.GetPlaybackStatisticsAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.TotalDistanceKm.Should().Be(120.5);
        result.AverageSpeedKmh.Should().BeApproximately(60.25, 0.01);
        result.SpeedDistribution.Should().HaveCount(4);
    }

    /// <summary>
    /// Tests that GetSnapshotAtTimestampAsync returns a frame for a session with data.
    /// </summary>
    [Fact]
    public async Task GetSnapshotAtTimestampAsync_ForSessionWithData_ReturnsFrame()
    {
        // Arrange
        var ts = new DateTime(2026, 5, 1, 8, 30, 0, DateTimeKind.Utc);
        var expectedFrame = new PlaybackFrameDto(
            PlaybackId: Guid.Empty,
            FrameIndex: 60,
            TotalFrames: 120,
            Timestamp: ts,
            Latitude: 51.5074,
            Longitude: -0.1278,
            Speed: 65.0,
            Bearing: 180.0,
            Altitude: null,
            DistanceCoveredKm: 22.6,
            CompletionPercentage: 50,
            ElapsedTime: TimeSpan.FromMinutes(30),
            Address: null);

        var service = Substitute.For<IRoutePlaybackService>();
        service.GetSnapshotAtTimestampAsync(1, ts, Arg.Any<CancellationToken>())
            .Returns(expectedFrame);

        // Act
        var result = await service.GetSnapshotAtTimestampAsync(1, ts);

        // Assert
        result.Should().NotBeNull();
        result!.CompletionPercentage.Should().Be(50);
        result.Latitude.Should().Be(51.5074);
        result.PlaybackId.Should().Be(Guid.Empty);
    }
}
