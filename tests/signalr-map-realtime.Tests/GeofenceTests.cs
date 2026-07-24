using Xunit;
using SignalRMapRealtime.Domain.Models;
using FluentAssertions;
using System;

namespace SignalRMapRealtime.Tests;

public class GeofenceTests
{
    [Fact]
    public void Constructor_InitializesProperties_WithDefaultValues()
    {
        // Act
        var geofence = new Geofence();

        // Assert
        geofence.Id.Should().NotBe(Guid.Empty);
        geofence.Name.Should().BeEmpty();
        geofence.Description.Should().BeNull();
        geofence.Type.Should().Be(GeofenceType.Circle);
        geofence.IsActive.Should().BeTrue();
        geofence.CenterLatitude.Should().BeNull();
        geofence.CenterLongitude.Should().BeNull();
        geofence.RadiusKm.Should().BeNull();
        geofence.PolygonCoordinates.Should().BeNull();
        geofence.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        geofence.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        geofence.MaxDwellMinutes.Should().Be(60);
        geofence.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Geofence";
        var description = "Test description";
        var type = GeofenceType.Polygon;
        var isActive = false;
        var centerLat = 40.7128;
        var centerLon = -74.0060;
        var radius = 5.5;
        var polygonCoords = "40.7128,-74.0060;40.7328,-73.9860";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow.AddHours(-1);
        var maxDwell = 30;
        var createdBy = "testuser";

        // Act
        var geofence = new Geofence
        {
            Id = id,
            Name = name,
            Description = description,
            Type = type,
            IsActive = isActive,
            CenterLatitude = centerLat,
            CenterLongitude = centerLon,
            RadiusKm = radius,
            PolygonCoordinates = polygonCoords,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            MaxDwellMinutes = maxDwell,
            CreatedBy = createdBy
        };

        // Assert
        geofence.Id.Should().Be(id);
        geofence.Name.Should().Be(name);
        geofence.Description.Should().Be(description);
        geofence.Type.Should().Be(type);
        geofence.IsActive.Should().Be(isActive);
        geofence.CenterLatitude.Should().Be(centerLat);
        geofence.CenterLongitude.Should().Be(centerLon);
        geofence.RadiusKm.Should().Be(radius);
        geofence.PolygonCoordinates.Should().Be(polygonCoords);
        geofence.CreatedAt.Should().Be(createdAt);
        geofence.UpdatedAt.Should().Be(updatedAt);
        geofence.MaxDwellMinutes.Should().Be(maxDwell);
        geofence.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void ContainsPoint_CircleType_WithValidCenterAndRadius_ReturnsTrue_WhenPointInside()
    {
        // Arrange
        var centerLat = 40.7128;
        var centerLon = -74.0060;
        var radius = 10.0;
        var pointLat = 40.7200;
        var pointLon = -74.0100;

        var geofence = new Geofence
        {
            Type = GeofenceType.Circle,
            CenterLatitude = centerLat,
            CenterLongitude = centerLon,
            RadiusKm = radius
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPoint_CircleType_WithValidCenterAndRadius_ReturnsFalse_WhenPointOutside()
    {
        // Arrange
        var centerLat = 40.7128;
        var centerLon = -74.0060;
        var radius = 1.0;
        var pointLat = 40.7200;
        var pointLon = -74.0100;

        var geofence = new Geofence
        {
            Type = GeofenceType.Circle,
            CenterLatitude = centerLat,
            CenterLongitude = centerLon,
            RadiusKm = radius
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_CircleType_ReturnsFalse_WhenCenterIsNull()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Circle,
            CenterLatitude = null,
            CenterLongitude = null,
            RadiusKm = 10.0
        };

        // Act
        var result = geofence.ContainsPoint(40.7128, -74.0060);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_CircleType_ReturnsFalse_WhenRadiusIsNull()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Circle,
            CenterLatitude = 40.7128,
            CenterLongitude = -74.0060,
            RadiusKm = null
        };

        // Act
        var result = geofence.ContainsPoint(40.7128, -74.0060);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_CircleType_ReturnsTrue_WhenPointIsExactlyOnBoundary()
    {
        // Arrange - point exactly at radius distance from center
        var centerLat = 40.7128;
        var centerLon = -74.0060;
        var radius = 1.0;

        // Calculate a point exactly 1km away (approximately)
        // Using simple approximation: 1 degree ≈ 111km, so 0.009 degrees ≈ 1km
        var pointLat = centerLat + 0.009;
        var pointLon = centerLon;

        var geofence = new Geofence
        {
            Type = GeofenceType.Circle,
            CenterLatitude = centerLat,
            CenterLongitude = centerLon,
            RadiusKm = radius
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert - should be true since we're using <= comparison
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPoint_PolygonType_WithValidPolygon_ReturnsTrue_WhenPointInside()
    {
        // Arrange - simple square polygon around NYC
        var polygonCoords = "40.6892,-74.0445;40.8792,-74.0445;40.8792,-73.9099;40.6892,-73.9099";
        var pointLat = 40.75;
        var pointLon = -73.98;

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPoint_PolygonType_WithValidPolygon_ReturnsFalse_WhenPointOutside()
    {
        // Arrange - simple square polygon around NYC
        var polygonCoords = "40.6892,-74.0445;40.8792,-74.0445;40.8792,-73.9099;40.6892,-73.9099";
        var pointLat = 41.0; // North of the polygon
        var pointLon = -74.0;

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_PolygonType_ReturnsFalse_WhenPolygonHasFewerThan3Points()
    {
        // Arrange - polygon with only 2 points (not a valid polygon)
        var polygonCoords = "40.7128,-74.0060;40.7228,-74.0160";
        var pointLat = 40.7150;
        var pointLon = -74.0100;

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_PolygonType_ReturnsFalse_WhenPolygonCoordinatesIsNull()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = null
        };

        // Act
        var result = geofence.ContainsPoint(40.7128, -74.0060);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_PolygonType_ReturnsFalse_WhenPolygonCoordinatesIsEmpty()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = ""
        };

        // Act
        var result = geofence.ContainsPoint(40.7128, -74.0060);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_PolygonType_ReturnsFalse_WhenPolygonCoordinatesIsWhitespace()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = "   "
        };

        // Act
        var result = geofence.ContainsPoint(40.7128, -74.0060);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPoint_UnknownGeofenceType_ReturnsFalse()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = (GeofenceType)99 // Unknown type
        };

        // Act
        var result = geofence.ContainsPoint(40.7128, -74.0060);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPolygonPoints_WithValidCoordinates_ReturnsCorrectPoints()
    {
        // Arrange
        var polygonCoords = "40.7128,-74.0060;40.7228,-74.0160;40.7328,-74.0260";
        var expectedPoints = new[]
        {
            (40.7128, -74.0060),
            (40.7228, -74.0160),
            (40.7328, -74.0260)
        };

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var points = geofence.GetPolygonPoints();

        // Assert
        points.Should().HaveCount(3);
        points[0].Should().Be(expectedPoints[0]);
        points[1].Should().Be(expectedPoints[1]);
        points[2].Should().Be(expectedPoints[2]);
    }

    [Fact]
    public void GetPolygonPoints_WithMalformedCoordinates_ReturnsEmptyList()
    {
        // Arrange - malformed coordinates (missing comma, invalid numbers)
        var polygonCoords = "40.7128-74.0060;invalid;40.7328,74.0260";

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var points = geofence.GetPolygonPoints();

        // Assert
        points.Should().BeEmpty();
    }

    [Fact]
    public void GetPolygonPoints_WithNullCoordinates_ReturnsEmptyList()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = null
        };

        // Act
        var points = geofence.GetPolygonPoints();

        // Assert
        points.Should().BeEmpty();
    }

    [Fact]
    public void GetPolygonPoints_WithEmptyCoordinates_ReturnsEmptyList()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = ""
        };

        // Act
        var points = geofence.GetPolygonPoints();

        // Assert
        points.Should().BeEmpty();
    }

    [Fact]
    public void GetPolygonPoints_WithWhitespaceCoordinates_ReturnsEmptyList()
    {
        // Arrange
        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = "   ;   ;   "
        };

        // Act
        var points = geofence.GetPolygonPoints();

        // Assert
        points.Should().BeEmpty();
    }

    [Fact]
    public void GetPolygonPoints_WithMixedValidAndInvalidCoordinates_ReturnsOnlyValidPoints()
    {
        // Arrange - mix of valid and invalid coordinates
        var polygonCoords = "40.7128,-74.0060;invalid;40.7328,-74.0260;;51.50,-0.12";
        var expectedPoints = new[]
        {
            (40.7128, -74.0060),
            (40.7328, -74.0260),
            (51.50, -0.12)
        };

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var points = geofence.GetPolygonPoints();

        // Assert
        points.Should().HaveCount(3);
        points.Should().BeEquivalentTo(expectedPoints);
    }

    [Fact]
    public void ContainsPoint_CircleType_WithBoundaryValues_ReturnsCorrectResults()
    {
        // Arrange - test with extreme coordinate values
        var centerLat = 0.0;
        var centerLon = 0.0;
        var radius = 1000.0; // Large radius
        var pointLat = 89.9; // Near north pole
        var pointLon = 179.9; // Near international date line

        var geofence = new Geofence
        {
            Type = GeofenceType.Circle,
            CenterLatitude = centerLat,
            CenterLongitude = centerLon,
            RadiusKm = radius
        };

        // Act
        var result = geofence.ContainsPoint(pointLat, pointLon);

        // Assert - should be true since point is within large radius
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPoint_PolygonType_WithComplexPolygon_ReturnsCorrectResults()
    {
        // Arrange - complex polygon (triangle)
        var polygonCoords = "0.0,0.0;10.0,0.0;5.0,10.0";
        var pointInside = (5.0, 3.0);
        var pointOutside = (15.0, 5.0);

        var geofence = new Geofence
        {
            Type = GeofenceType.Polygon,
            PolygonCoordinates = polygonCoords
        };

        // Act
        var resultInside = geofence.ContainsPoint(pointInside.Item1, pointInside.Item2);
        var resultOutside = geofence.ContainsPoint(pointOutside.Item1, pointOutside.Item2);

        // Assert
        resultInside.Should().BeTrue();
        resultOutside.Should().BeFalse();
    }

    [Fact]
    public void UpdatedAt_IsSetToCurrentTime_WhenGeofenceIsCreated()
    {
        // Arrange
        var initialUpdatedAt = DateTime.UtcNow.AddDays(-1);

        var geofence = new Geofence
        {
            UpdatedAt = initialUpdatedAt
        };

        // Act - wait a bit and then modify
        System.Threading.Thread.Sleep(10);
        geofence.Name = "Updated Geofence";

        // Assert
        geofence.UpdatedAt.Should().BeAfter(initialUpdatedAt);
    }

    [Fact]
    public void MaxDwellMinutes_HasDefaultValueOf60()
    {
        // Arrange & Act
        var geofence = new Geofence();

        // Assert
        geofence.MaxDwellMinutes.Should().Be(60);
    }

    [Fact]
    public void MaxDwellMinutes_CanBeModified()
    {
        // Arrange
        var geofence = new Geofence();

        // Act
        geofence.MaxDwellMinutes = 120;

        // Assert
        geofence.MaxDwellMinutes.Should().Be(120);
    }
}