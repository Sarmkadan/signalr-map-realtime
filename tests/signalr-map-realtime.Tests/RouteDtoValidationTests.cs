using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class RouteDtoValidationTests
    {
        [Fact]
        public void Validate_HappyPath_NoErrors_ReturnsEmptyList()
        {
            // Arrange
            var route = new RouteDto
            {
                Name = "Test Route",
                VehicleId = 1,
                PlannedDepartureTime = DateTime.UtcNow,
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1),
                TotalDistance = 100,
                ActualDistance = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Waypoints = new List<WaypointDto>()
            };

            // Act
            var errors = RouteDtoValidation.Validate(route);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_HappyPath_EmptyName_ReturnsError()
        {
            // Arrange
            var route = new RouteDto
            {
                Name = "",
                VehicleId = 1,
                PlannedDepartureTime = DateTime.UtcNow,
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1),
                TotalDistance = 100,
                ActualDistance = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Waypoints = new List<WaypointDto>()
            };

            // Act
            var errors = RouteDtoValidation.Validate(route);

            // Assert
            Assert.Single(errors);
            Assert.Equal("Name is required and cannot be empty or whitespace.", errors[0]);
        }

        [Fact]
        public void Validate_HappyPath_NullRoute_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => RouteDtoValidation.Validate(null));
        }

        [Fact]
        public void IsValid_HappyPath_ValidRoute_ReturnsTrue()
        {
            // Arrange
            var route = new RouteDto
            {
                Name = "Test Route",
                VehicleId = 1,
                PlannedDepartureTime = DateTime.UtcNow,
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1),
                TotalDistance = 100,
                ActualDistance = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Waypoints = new List<WaypointDto>()
            };

            // Act
            var isValid = RouteDtoValidation.IsValid(route);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_HappyPath_InvalidRoute_ReturnsFalse()
        {
            // Arrange
            var route = new RouteDto
            {
                Name = "",
                VehicleId = 1,
                PlannedDepartureTime = DateTime.UtcNow,
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1),
                TotalDistance = 100,
                ActualDistance = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Waypoints = new List<WaypointDto>()
            };

            // Act
            var isValid = RouteDtoValidation.IsValid(route);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EnsureValid_HappyPath_ValidRoute_DoesNothing()
        {
            // Arrange
            var route = new RouteDto
            {
                Name = "Test Route",
                VehicleId = 1,
                PlannedDepartureTime = DateTime.UtcNow,
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1),
                TotalDistance = 100,
                ActualDistance = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Waypoints = new List<WaypointDto>()
            };

            // Act and Assert
            RouteDtoValidation.EnsureValid(route);
        }

        [Fact]
        public void EnsureValid_HappyPath_InvalidRoute_ThrowsArgumentException()
        {
            // Arrange
            var route = new RouteDto
            {
                Name = "",
                VehicleId = 1,
                PlannedDepartureTime = DateTime.UtcNow,
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1),
                TotalDistance = 100,
                ActualDistance = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Waypoints = new List<WaypointDto>()
            };

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => RouteDtoValidation.EnsureValid(route));
            Assert.Equal("RouteDto validation failed:Name is required and cannot be empty or whitespace.", exception.Message);
        }
    }
}
