using Xunit;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Tests
{
    public class WaypointExtensionsTests
    {
        [Fact]
        public void CalculateDistanceTo_HappyPath_ReturnsDistance()
        {
            // Arrange
            var source = new Waypoint { Latitude = 1.0, Longitude = 1.0 };
            var destination = new Waypoint { Latitude = 2.0, Longitude = 2.0 };

            // Act
            var distance = WaypointExtensions.CalculateDistanceTo(source, destination);

            // Assert
            Assert.NotEqual(0.0, distance);
        }

        [Fact]
        public void CalculateDistanceTo_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            var destination = new Waypoint { Latitude = 2.0, Longitude = 2.0 };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => WaypointExtensions.CalculateDistanceTo(null, destination));
        }

        [Fact]
        public void CalculateDistanceTo_NullDestination_ThrowsArgumentNullException()
        {
            // Arrange
            var source = new Waypoint { Latitude = 1.0, Longitude = 1.0 };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => WaypointExtensions.CalculateDistanceTo(source, null));
        }

        [Fact]
        public void GetDisplayName_HappyPath_ReturnsDisplayName()
        {
            // Arrange
            var waypoint = new Waypoint { Name = "Test Waypoint" };

            // Act
            var displayName = WaypointExtensions.GetDisplayName(waypoint);

            // Assert
            Assert.Equal("Test Waypoint", displayName);
        }

        [Fact]
        public void GetDisplayName_EmptyName_ReturnsCoordinates()
        {
            // Arrange
            var waypoint = new Waypoint { Name = string.Empty, Latitude = 1.0, Longitude = 1.0 };

            // Act
            var displayName = WaypointExtensions.GetDisplayName(waypoint);

            // Assert
            Assert.Equal("1.0000, 1.0000", displayName);
        }

        [Fact]
        public void IsInProgress_HappyPath_ReturnsTrue()
        {
            // Arrange
            var waypoint = new Waypoint { ActualArrivalTime = DateTime.Now, ActualDepartureTime = null };

            // Act
            var isInProgress = WaypointExtensions.IsInProgress(waypoint);

            // Assert
            Assert.True(isInProgress);
        }

        [Fact]
        public void IsInProgress_Completed_ReturnsFalse()
        {
            // Arrange
            var waypoint = new Waypoint { ActualArrivalTime = DateTime.Now, ActualDepartureTime = DateTime.Now };

            // Act
            var isInProgress = WaypointExtensions.IsInProgress(waypoint);

            // Assert
            Assert.False(isInProgress);
        }
    }
}
