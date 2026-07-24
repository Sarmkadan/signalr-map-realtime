using Xunit;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;
using FluentAssertions;

namespace SignalRMapRealtime.Tests;

public class LocationTests
{
    private static readonly Location _nyc = new() { Latitude = 40.7128, Longitude = -74.0060 };
    private static readonly Location _la = new() { Latitude = 34.0522, Longitude = -118.2437 };
    [Fact]
    public void Constructor_WithDefaultValues_InitializesProperties()
    {
        // Arrange & Act
        var location = new Location();

        // Assert
        location.Id.Should().Be(0);
        location.Latitude.Should().Be(0);
        location.Longitude.Should().Be(0);
        location.Altitude.Should().BeNull();
        location.Accuracy.Should().BeNull();
        location.Speed.Should().BeNull();
        location.Bearing.Should().BeNull();
        location.LocationType.Should().Be(LocationType.TrackPoint);
        location.Address.Should().BeNull();
        location.Notes.Should().BeNull();
    }


    [Fact]
    public void CalculateDistanceTo_WithValidLocations_ReturnsCorrectDistance()
    {
        var distance = _nyc.CalculateDistanceTo(_la);
        distance.Should().BeGreaterThan(3900);
        distance.Should().BeLessThan(3950);
    }

    [Fact]
    public void CalculateDistanceTo_WithSameLocation_ReturnsZero()
    {
        var distance = _nyc.CalculateDistanceTo(_nyc);
        distance.Should().Be(0);
    }

    [Fact]
    public void CalculateDistanceTo_WithNullOtherLocation_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _nyc.CalculateDistanceTo(null!));
    }

    [Fact]
    public void IsValidCoordinate_WithValidCoordinates_ReturnsTrue()
    {
        var isValid = _nyc.IsValidCoordinate();
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-90.0, 0.0)]  // South Pole
    [InlineData(90.0, 0.0)]   // North Pole
    [InlineData(0.0, -180.0)] // International Date Line West
    [InlineData(0.0, 180.0)]  // International Date Line East
    public void IsValidCoordinate_WithBoundaryValues_ReturnsTrue(double latitude, double longitude)
    {
        // Arrange
        var location = new Location
        {
            Latitude = latitude,
            Longitude = longitude
        };

        // Act
        var isValid = location.IsValidCoordinate();

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-90.1, 0.0)]    // Below minimum latitude
    [InlineData(90.1, 0.0)]     // Above maximum latitude
    [InlineData(0.0, -180.1)]   // Below minimum longitude
    [InlineData(0.0, 180.1)]    // Above maximum longitude
    public void IsValidCoordinate_WithInvalidCoordinates_ReturnsFalse(double latitude, double longitude)
    {
        // Arrange
        var location = new Location
        {
            Latitude = latitude,
            Longitude = longitude
        };

        // Act
        var isValid = location.IsValidCoordinate();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsDifferentFrom_WithSameLocation_ReturnsFalse()
    {
        var isDifferent = _nyc.IsDifferentFrom(_nyc);
        isDifferent.Should().BeFalse();
    }

    [Fact]
    public void IsDifferentFrom_WithDifferentLocations_ReturnsTrue()
    {
        var isDifferent = _nyc.IsDifferentFrom(_la);
        isDifferent.Should().BeTrue();
    }

    [Fact]
    public void IsDifferentFrom_WithNullOtherLocation_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _nyc.IsDifferentFrom(null!));
    }

}