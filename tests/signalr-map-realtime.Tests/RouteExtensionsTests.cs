using FluentAssertions;
using SignalRMapRealtime.Domain.Models;
using System;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class RouteExtensionsTests
{
    [Fact]
    public void GetEstimatedDurationMinutes_WithValidPlannedTimes_ReturnsDurationInMinutes()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 30, 0)
        };

        // Act
        var result = route.GetEstimatedDurationMinutes();

        // Assert
        result.Should().Be(150);
    }

    [Fact]
    public void GetEstimatedDurationMinutes_WithSameDepartureAndArrival_ReturnsZero()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };

        // Act
        var result = route.GetEstimatedDurationMinutes();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetEstimatedDurationMinutes_WithNullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        Route route = null!;

        // Act
        Action act = () => route.GetEstimatedDurationMinutes();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetEstimatedDurationMinutes_WithDefaultPlannedDepartureTime_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = default,
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0)
        };

        // Act
        var result = route.GetEstimatedDurationMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetEstimatedDurationMinutes_WithDefaultEstimatedArrivalTime_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = default
        };

        // Act
        var result = route.GetEstimatedDurationMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetActualDurationMinutes_WithValidActualTimes_ReturnsDurationInMinutes()
    {
        // Arrange
        var route = new Route
        {
            ActualDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 11, 45, 0)
        };

        // Act
        var result = route.GetActualDurationMinutes();

        // Assert
        result.Should().Be(225);
    }

    [Fact]
    public void GetActualDurationMinutes_WithSameActualDepartureAndArrival_ReturnsZero()
    {
        // Arrange
        var route = new Route
        {
            ActualDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };

        // Act
        var result = route.GetActualDurationMinutes();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetActualDurationMinutes_WithNullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        Route route = null!;

        // Act
        Action act = () => route.GetActualDurationMinutes();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetActualDurationMinutes_WithNullActualDepartureTime_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            ActualDepartureTime = null,
            ActualArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0)
        };

        // Act
        var result = route.GetActualDurationMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetActualDurationMinutes_WithNullActualArrivalTime_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            ActualDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            ActualArrivalTime = null
        };

        // Act
        var result = route.GetActualDurationMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsDelayed_WithActualArrivalAfterEstimatedArrival_ReturnsTrue()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 10, 30, 0)
        };

        // Act
        var result = route.IsDelayed();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDelayed_WithActualArrivalBeforeEstimatedArrival_ReturnsFalse()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 9, 30, 0)
        };

        // Act
        var result = route.IsDelayed();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDelayed_WithActualArrivalEqualToEstimatedArrival_ReturnsFalse()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0)
        };

        // Act
        var result = route.IsDelayed();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDelayed_WithNullActualArrivalTime_ReturnsFalse()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = null
        };

        // Act
        var result = route.IsDelayed();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDelayed_WithNullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        Route route = null!;

        // Act
        Action act = () => route.IsDelayed();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDelayMinutes_WithDelayedRoute_ReturnsDelayInMinutes()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 10, 45, 0)
        };

        // Act
        var result = route.GetDelayMinutes();

        // Assert
        result.Should().Be(45);
    }

    [Fact]
    public void GetDelayMinutes_WithNotDelayedRoute_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 9, 30, 0)
        };

        // Act
        var result = route.GetDelayMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetDelayMinutes_WithNullRoute_ThrowsArgumentNullException()
    {
        // Arrange
        Route route = null!;

        // Act
        Action act = () => route.GetDelayMinutes();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDelayMinutes_WithNullActualArrivalTime_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = null
        };

        // Act
        var result = route.GetDelayMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetDelayMinutes_WithDefaultEstimatedArrivalTime_ReturnsNull()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = default,
            ActualArrivalTime = new DateTime(2024, 1, 1, 10, 30, 0)
        };

        // Act
        var result = route.GetDelayMinutes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetDelayMinutes_WithZeroDelay_ReturnsZero()
    {
        // Arrange
        var route = new Route
        {
            PlannedDepartureTime = new DateTime(2024, 1, 1, 8, 0, 0),
            EstimatedArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualArrivalTime = new DateTime(2024, 1, 1, 10, 0, 0)
        };

        // Act
        var result = route.GetDelayMinutes();

        // Assert
        result.Should().Be(0);
    }
}