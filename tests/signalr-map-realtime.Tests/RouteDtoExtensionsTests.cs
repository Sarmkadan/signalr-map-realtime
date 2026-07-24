using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class RouteDtoExtensionsTests
    {
        [Fact]
        public void IsRouteActive_HappyPath_ActiveAndNotCompleted_ReturnsTrue()
        {
            // Arrange
            var route = new RouteDto
            {
                IsActive = true,
                IsCompleted = false
            };

            // Act
            var result = route.IsRouteActive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsRouteActive_HappyPath_ActiveAndCompleted_ReturnsFalse()
        {
            // Arrange
            var route = new RouteDto
            {
                IsActive = true,
                IsCompleted = true
            };

            // Act
            var result = route.IsRouteActive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsRouteActive_HappyPath_InactiveAndNotCompleted_ReturnsFalse()
        {
            // Arrange
            var route = new RouteDto
            {
                IsActive = false,
                IsCompleted = false
            };

            // Act
            var result = route.IsRouteActive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsRouteActive_NullRoute_ThrowsArgumentNullException()
        {
            // Arrange
            RouteDto route = null!;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => route.IsRouteActive());
        }

        [Fact]
        public void CalculateDurationInMinutes_HappyPath_ReturnsCorrectDuration()
        {
            // Arrange
            var plannedDeparture = new DateTime(2024, 1, 1, 10, 0, 0);
            var estimatedArrival = new DateTime(2024, 1, 1, 12, 30, 0);
            var route = new RouteDto
            {
                PlannedDepartureTime = plannedDeparture,
                EstimatedArrivalTime = estimatedArrival
            };

            // Act
            var duration = route.CalculateDurationInMinutes();

            // Assert
            Assert.Equal(150, duration); // 2 hours 30 minutes = 150 minutes
        }

        [Fact]
        public void CalculateDurationInMinutes_HappyPath_OneMinuteDuration_ReturnsOne()
        {
            // Arrange
            var plannedDeparture = new DateTime(2024, 1, 1, 10, 0, 0);
            var estimatedArrival = new DateTime(2024, 1, 1, 10, 1, 0);
            var route = new RouteDto
            {
                PlannedDepartureTime = plannedDeparture,
                EstimatedArrivalTime = estimatedArrival
            };

            // Act
            var duration = route.CalculateDurationInMinutes();

            // Assert
            Assert.Equal(1, duration);
        }

        [Fact]
        public void CalculateDurationInMinutes_NullRoute_ThrowsArgumentNullException()
        {
            // Arrange
            RouteDto route = null!;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => route.CalculateDurationInMinutes());
        }

        [Fact]
        public void CalculateDurationInMinutes_DefaultPlannedDepartureTime_ThrowsArgumentException()
        {
            // Arrange
            var route = new RouteDto
            {
                PlannedDepartureTime = default,
                EstimatedArrivalTime = new DateTime(2024, 1, 1, 12, 0, 0)
            };

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => route.CalculateDurationInMinutes());
            Assert.Equal("PlannedDepartureTime must be set (Parameter 'route')", exception.Message);
        }

        [Fact]
        public void CalculateDurationInMinutes_DefaultEstimatedArrivalTime_ThrowsArgumentException()
        {
            // Arrange
            var route = new RouteDto
            {
                PlannedDepartureTime = new DateTime(2024, 1, 1, 10, 0, 0),
                EstimatedArrivalTime = default
            };

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => route.CalculateDurationInMinutes());
            Assert.Equal("EstimatedArrivalTime must be set (Parameter 'route')", exception.Message);
        }

        [Fact]
        public void HasValidVehicle_HappyPath_ValidVehicleIdAndVehicle_ReturnsTrue()
        {
            // Arrange
            var route = new RouteDto
            {
                VehicleId = 5,
                Vehicle = new VehicleDto()
            };

            // Act
            var result = route.HasValidVehicle();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasValidVehicle_HappyPath_VehicleIdGreaterThanZero_ReturnsTrue()
        {
            // Arrange
            var route = new RouteDto
            {
                VehicleId = 1,
                Vehicle = null
            };

            // Act
            var result = route.HasValidVehicle();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasValidVehicle_HappyPath_VehicleIdZero_ReturnsFalse()
        {
            // Arrange
            var route = new RouteDto
            {
                VehicleId = 0,
                Vehicle = new VehicleDto()
            };

            // Act
            var result = route.HasValidVehicle();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasValidVehicle_NullRoute_ThrowsArgumentNullException()
        {
            // Arrange
            RouteDto route = null!;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => route.HasValidVehicle());
        }

        [Fact]
        public void HasValidVehicle_HappyPath_NegativeVehicleId_ReturnsFalse()
        {
            // Arrange
            var route = new RouteDto
            {
                VehicleId = -1,
                Vehicle = new VehicleDto()
            };

            // Act
            var result = route.HasValidVehicle();

            // Assert
            Assert.False(result);
        }
    }
}