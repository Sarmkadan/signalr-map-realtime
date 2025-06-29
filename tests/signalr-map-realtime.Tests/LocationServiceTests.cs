// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using Moq;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using Xunit;

public class LocationServiceTests
{
    [Fact]
    public async Task GetLatestLocationAsync_WhenVehicleHasNoRecordedLocations_ReturnsNull()
    {
        // Arrange
        var serviceMock = new Mock<ILocationService>();
        serviceMock
            .Setup(s => s.GetLatestLocationAsync(99, default))
            .ReturnsAsync((LocationDto?)null);

        // Act
        var result = await serviceMock.Object.GetLatestLocationAsync(99);

        // Assert
        result.Should().BeNull();
        serviceMock.Verify(s => s.GetLatestLocationAsync(99, default), Times.Once());
    }

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

        var serviceMock = new Mock<ILocationService>();
        serviceMock
            .Setup(s => s.GetLatestLocationAsync(5, default))
            .ReturnsAsync(expected);

        // Act
        var result = await serviceMock.Object.GetLatestLocationAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(51.5074);
        result.Speed.Should().Be(55.0);
        result.VehicleId.Should().Be(5);
    }

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

        var serviceMock = new Mock<ILocationService>();
        serviceMock
            .Setup(s => s.GetLocationStatsAsync(1, start, end, default))
            .ReturnsAsync(expected);

        // Act
        var stats = await serviceMock.Object.GetLocationStatsAsync(1, start, end);

        // Assert
        stats.PointCount.Should().Be(12);
        stats.MaxSpeed.Should().BeApproximately(87.3, 0.001);
        stats.TotalDistance.Should().BePositive();
        serviceMock.Verify(s => s.GetLocationStatsAsync(1, start, end, default), Times.Once());
    }
}
