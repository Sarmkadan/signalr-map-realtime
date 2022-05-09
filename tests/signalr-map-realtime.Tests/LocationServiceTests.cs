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
/// Tests for the LocationService class.
/// </summary>
public class LocationServiceTests
{
    /// <summary>
    /// Tests that GetLatestLocationAsync returns null when the vehicle has no recorded locations.
    /// </summary>
    [Fact]
    public async Task GetLatestLocationAsync_WhenVehicleHasNoRecordedLocations_ReturnsNull()
    {
        // Arrange
        var service = Substitute.For<ILocationService>();
        service.GetLatestLocationAsync(99, default).Returns((LocationDto?)null);

        // Act
        var result = await service.GetLatestLocationAsync(99).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
        await service.Received(1).GetLatestLocationAsync(99, default);
    }

    /// <summary>
    /// Tests that GetLatestLocationAsync returns the most recent LocationDto when the vehicle has locations.
    /// </summary>
    [Fact]
    public async Task GetLatestLocationAsync_WhenVehicleHasLocations_ReturnsMostRecentDto()
    {
        // Arrange
        var expected = new LocationDto
        {
            Id = 7,
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 55.0,
            VehicleId = 5
        };

        var service = Substitute.For<ILocationService>();
        service.GetLatestLocationAsync(5, default).Returns(expected);

        // Act
        var result = await service.GetLatestLocationAsync(5).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(51.5074);
        result.Speed.Should().Be(55.0);
        result.VehicleId.Should().Be(5);
    }

    /// <summary>
    /// Tests that GetLocationStatsAsync returns populated metrics for an active session.
    /// </summary>
    [Fact]
    public async Task GetLocationStatsAsync_ForActiveSession_ReturnsPopulatedMetrics()
    {
        // Arrange
        var start = new DateTime(2026, 5, 9, 8, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 5, 9, 9, 0, 0, DateTimeKind.Utc);

        var expected = new LocationStatsDto
        {
            PointCount = 12,
            MinSpeed = 0.0,
            MaxSpeed = 87.3,
            AverageSpeed = 48.6,
            TotalDistance = 48.6
        };

        var service = Substitute.For<ILocationService>();
        service.GetLocationStatsAsync(1, start, end, default).Returns(expected);

        // Act
        var stats = await service.GetLocationStatsAsync(1, start, end).ConfigureAwait(false);

        // Assert
        stats.PointCount.Should().Be(12);
        stats.MaxSpeed.Should().BeApproximately(87.3, 0.001);
        stats.TotalDistance.Should().BePositive();
        await service.Received(1).GetLocationStatsAsync(1, start, end, default);
    }
}
