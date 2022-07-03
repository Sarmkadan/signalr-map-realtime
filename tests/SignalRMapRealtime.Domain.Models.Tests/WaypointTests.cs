using FluentAssertions;
using NSubstitute;
using SignalRMapRealtime.Domain.Models;
using System;

namespace SignalRMapRealtime.Domain.Models.Tests
{
    public class WaypointTests
    {
        [Fact]
        public void CompleteWaypoint_ValidInput_MarksAsCompleted()
        {
            // Arrange
            var waypoint = new Waypoint();
            var arrivalTime = DateTime.UtcNow;
            var departureTime = arrivalTime.AddMinutes(30);

            // Act
            waypoint.CompleteWaypoint(arrivalTime, departureTime);

            // Assert
            waypoint.IsCompleted.Should().BeTrue();
            waypoint.ActualArrivalTime.Should().Be(arrivalTime);
            waypoint.ActualDepartureTime.Should().Be(departureTime);
        }

        [Fact]
        public void CompleteWaypoint_NoDepartureTimeProvided_MarksAsCompletedWithCurrentTime()
        {
            // Arrange
            var waypoint = new Waypoint();
            var arrivalTime = DateTime.UtcNow;

            // Act
            waypoint.CompleteWaypoint(arrivalTime);

            // Assert
            waypoint.IsCompleted.Should().BeTrue();
            waypoint.ActualArrivalTime.Should().Be(arrivalTime);
            waypoint.ActualDepartureTime.Should().NotBeNull();
            waypoint.ActualDepartureTime.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Reset_ResetsWaypointToIncompleteState()
        {
            // Arrange
            var waypoint = new Waypoint
            {
                IsCompleted = true,
                ActualArrivalTime = DateTime.UtcNow,
                ActualDepartureTime = DateTime.UtcNow
            };

            // Act
            waypoint.Reset();

            // Assert
            waypoint.IsCompleted.Should().BeFalse();
            waypoint.ActualArrivalTime.Should().BeNull();
            waypoint.ActualDepartureTime.Should().BeNull();
        }

        [Fact]
        public void HasValidCoordinates_ValidCoordinates_ReturnsTrue()
        {
            // Arrange
            var waypoint = new Waypoint
            {
                Latitude = 37.7749,
                Longitude = -122.4194
            };

            // Act
            var result = waypoint.HasValidCoordinates();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasValidCoordinates_InvalidCoordinates_ReturnsFalse()
        {
            // Arrange
            var waypoint = new Waypoint
            {
                Latitude = 100,
                Longitude = 200
            };

            // Act
            var result = waypoint.HasValidCoordinates();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasTimeWindow_ValidTimeWindow_ReturnsTrue()
        {
            // Arrange
            var waypoint = new Waypoint
            {
                ArrivalTimeStart = "2022-01-01T08:00:00",
                ArrivalTimeEnd = "2022-01-01T10:00:00"
            };

            // Act
            var result = waypoint.HasTimeWindow();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasTimeWindow_NoTimeWindow_ReturnsFalse()
        {
            // Arrange
            var waypoint = new Waypoint();

            // Act
            var result = waypoint.HasTimeWindow();

            // Assert
            result.Should().BeFalse();
        }
    }
}
