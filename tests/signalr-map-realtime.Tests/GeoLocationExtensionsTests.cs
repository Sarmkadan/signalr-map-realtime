// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Utilities;
using Xunit;

public class GeoLocationExtensionsTests
{
    [Fact]
    public void DistanceBetween_SameCoordinates_ReturnsZero()
    {
        // Arrange
        const double lat = 51.5074;
        const double lon = -0.1278;

        // Act
        var distance = GeoLocationExtensions.DistanceBetween(lat, lon, lat, lon);

        // Assert
        distance.Should().BeApproximately(0.0, 0.0001);
    }

    [Fact]
    public void DistanceBetween_LondonToNewYork_ReturnsApproximateDistance()
    {
        // Arrange — London: 51.5074, -0.1278 | New York: 40.7128, -74.0060
        const double londonLat = 51.5074;
        const double londonLon = -0.1278;
        const double newYorkLat = 40.7128;
        const double newYorkLon = -74.0060;

        // Act
        var distance = GeoLocationExtensions.DistanceBetween(londonLat, londonLon, newYorkLat, newYorkLon);

        // Assert — great-circle distance is approximately 5,570 km
        distance.Should().BeInRange(5500.0, 5640.0);
    }

    [Fact]
    public void KilometersToMiles_OneKilometer_ReturnsExpectedMiles()
    {
        // Arrange
        const double oneKilometer = 1.0;

        // Act
        var miles = oneKilometer.KilometersToMiles();

        // Assert
        miles.Should().BeApproximately(0.621371, 0.000001);
    }

    [Fact]
    public void IsWithinRadius_PointBeyondRadius_ReturnsFalse()
    {
        // Arrange — center in London, point ~170 km away in Birmingham area
        var center = new Location { Latitude = 51.5074, Longitude = -0.1278 };
        var farPoint = new Location { Latitude = 52.4862, Longitude = -1.8904 };

        // Act
        var withinOneKm = farPoint.IsWithinRadius(center, 1.0);

        // Assert
        withinOneKm.Should().BeFalse();
    }
}
