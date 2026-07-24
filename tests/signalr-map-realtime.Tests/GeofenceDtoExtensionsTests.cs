using Xunit;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Tests
{
    public class GeofenceDtoExtensionsTests
    {
        [Fact]
        public void ContainsPoint_CircleGeofence_PointInside_ReturnsTrue()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                CenterLatitude = 40.7128,
                CenterLongitude = -74.0060,
                RadiusKm = 1.0
            };

            // Act
            var result = geofence.ContainsPoint(40.7128, -74.0060);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPoint_CircleGeofence_PointOutside_ReturnsFalse()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                CenterLatitude = 40.7128,
                CenterLongitude = -74.0060,
                RadiusKm = 1.0
            };

            // Act
            var result = geofence.ContainsPoint(40.7129, -74.0061);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPoint_CircleGeofence_PointOnBoundary_ReturnsTrue()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                CenterLatitude = 40.7128,
                CenterLongitude = -74.0060,
                RadiusKm = 1.0
            };

            // Act
            var result = geofence.ContainsPoint(40.7128, -74.0060 + 1.0 / 111.32);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPoint_PolygonGeofence_PointInside_ReturnsTrue()
        {
            // Arrange - Simple square polygon around (0,0)
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Polygon),
                PolygonCoordinates = "0,0;1,0;1,1;0,1"
            };

            // Act - Point inside the square
            var result = geofence.ContainsPoint(0.5, 0.5);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPoint_PolygonGeofence_PointOutside_ReturnsFalse()
        {
            // Arrange - Simple square polygon around (0,0)
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Polygon),
                PolygonCoordinates = "0,0;1,0;1,1;0,1"
            };

            // Act - Point outside the square
            var result = geofence.ContainsPoint(2.0, 2.0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPoint_PolygonGeofence_PointOnEdge_ReturnsTrue()
        {
            // Arrange - Simple square polygon around (0,0)
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Polygon),
                PolygonCoordinates = "0,0;1,0;1,1;0,1"
            };

            // Act - Point on the edge
            var result = geofence.ContainsPoint(0.5, 0.0);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPoint_NullGeofence_ThrowsArgumentNullException()
        {
            // Arrange
            GeofenceDto? geofence = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geofence!.ContainsPoint(0, 0));
        }

        [Fact]
        public void ContainsPoint_CircleWithoutRequiredProperties_ReturnsFalse()
        {
            // Arrange - Circle without CenterLatitude
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                CenterLongitude = -74.0060,
                RadiusKm = 1.0
            };

            // Act
            var result = geofence.ContainsPoint(40.7128, -74.0060);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPolygonPoints_ValidPolygonCoordinates_ReturnsPoints()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                PolygonCoordinates = "40.7128,-74.0060;34.0522,-118.2437;41.8781,-87.6298"
            };

            // Act
            var result = geofence.GetPolygonPoints();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(40.7128, result[0].Latitude);
            Assert.Equal(-74.0060, result[0].Longitude);
            Assert.Equal(34.0522, result[1].Latitude);
            Assert.Equal(-118.2437, result[1].Longitude);
            Assert.Equal(41.8781, result[2].Latitude);
            Assert.Equal(-87.6298, result[2].Longitude);
        }

        [Fact]
        public void GetPolygonPoints_EmptyPolygonCoordinates_ReturnsEmptyList()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                PolygonCoordinates = ""
            };

            // Act
            var result = geofence.GetPolygonPoints();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPolygonPoints_NullPolygonCoordinates_ReturnsEmptyList()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                PolygonCoordinates = null
            };

            // Act
            var result = geofence.GetPolygonPoints();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPolygonPoints_WhitespacePolygonCoordinates_ReturnsEmptyList()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                PolygonCoordinates = "   "
            };

            // Act
            var result = geofence.GetPolygonPoints();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPolygonPoints_MalformedCoordinates_ReturnsOnlyValidPoints()
        {
            // Arrange - Contains some malformed entries
            var geofence = new GeofenceDto
            {
                PolygonCoordinates = "40.7128,-74.0060;invalid;41.8781,-87.6298;"
            };

            // Act
            var result = geofence.GetPolygonPoints();

            // Assert
            Assert.Single(result);
            Assert.Equal(40.7128, result[0].Latitude);
            Assert.Equal(-74.0060, result[0].Longitude);
        }

        [Fact]
        public void GetPolygonPoints_NullGeofence_ThrowsArgumentNullException()
        {
            // Arrange
            GeofenceDto? geofence = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geofence!.GetPolygonPoints());
        }

        [Fact]
        public void DistanceTo_CircleGeofence_WithValidCenter_ReturnsDistance()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                CenterLatitude = 40.7128,
                CenterLongitude = -74.0060,
                RadiusKm = 10.0
            };

            // Act
            var result = geofence.DistanceTo(40.7128, -74.0060);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void DistanceTo_CircleGeofence_WithDifferentPoint_ReturnsNonZeroDistance()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                CenterLatitude = 40.7128,
                CenterLongitude = -74.0060,
                RadiusKm = 10.0
            };

            // Act
            var result = geofence.DistanceTo(40.7200, -74.0100);

            // Assert
            Assert.NotNull(result);
            Assert.True(result > 0);
        }

        [Fact]
        public void DistanceTo_CircleGeofence_WithoutCenter_ReturnsNull()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle),
                RadiusKm = 10.0
            };

            // Act
            var result = geofence.DistanceTo(40.7128, -74.0060);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DistanceTo_NullGeofence_ThrowsArgumentNullException()
        {
            // Arrange
            GeofenceDto? geofence = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geofence!.DistanceTo(0, 0));
        }

        [Fact]
        public void IsCircle_CircleGeofence_ReturnsTrue()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle)
            };

            // Act
            var result = geofence.IsCircle();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsCircle_PolygonGeofence_ReturnsFalse()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Polygon)
            };

            // Act
            var result = geofence.IsCircle();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCircle_NullGeofence_ThrowsArgumentNullException()
        {
            // Arrange
            GeofenceDto? geofence = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geofence!.IsCircle());
        }

        [Fact]
        public void IsPolygon_CircleGeofence_ReturnsFalse()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Circle)
            };

            // Act
            var result = geofence.IsPolygon();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPolygon_PolygonGeofence_ReturnsTrue()
        {
            // Arrange
            var geofence = new GeofenceDto
            {
                Type = nameof(GeofenceType.Polygon)
            };

            // Act
            var result = geofence.IsPolygon();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPolygon_NullGeofence_ThrowsArgumentNullException()
        {
            // Arrange
            GeofenceDto? geofence = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geofence!.IsPolygon());
        }
    }
}