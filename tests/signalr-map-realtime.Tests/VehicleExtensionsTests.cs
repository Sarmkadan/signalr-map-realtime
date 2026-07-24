using System;
using System.Collections.Generic;
using SignalRMapRealtime.Domain.Models;
using Xunit;
using FluentAssertions;

namespace SignalRMapRealtime.Tests;

public class VehicleExtensionsTests
{
    [Fact]
    public void HasDriver_VehicleWithDriverIdAndDriver_ReturnsTrue()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            DriverId = 1,
            Driver = new User { Id = 1, FullName = "John Doe" }
        };

        // Act
        var result = vehicle.HasDriver();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasDriver_VehicleWithoutDriver_ReturnsFalse()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            DriverId = null,
            Driver = null
        };

        // Act
        var result = vehicle.HasDriver();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasDriver_NullVehicle_ThrowsArgumentNullException()
    {
        // Arrange
        Vehicle vehicle = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => vehicle.HasDriver());
    }

    [Fact]
    public void GetFullDescription_VehicleWithAllProperties_ReturnsFormattedDescription()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Make = "Toyota",
            Model = "Camry",
            ModelYear = 2022,
            Name = "Company Car",
            RegistrationNumber = "ABC123"
        };

        // Act
        var result = vehicle.GetFullDescription();

        // Assert
        result.Should().Be("Toyota Camry 2022 - Company Car (ABC123)");
    }

    [Fact]
    public void GetFullDescription_VehicleWithNullMake_ReturnsUnknownMake()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Make = null,
            Model = "Model S",
            ModelYear = 2023,
            Name = "Electric Car",
            RegistrationNumber = "ELEC001"
        };

        // Act
        var result = vehicle.GetFullDescription();

        // Assert
        result.Should().Be("Unknown Model S 2023 - Electric Car (ELEC001)");
    }

    [Fact]
    public void GetFullDescription_VehicleWithNullYearAndModelYear_ReturnsNAYear()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Make = "Chevrolet",
            Model = "Malibu",
            ModelYear = null,
            Year = null,
            Name = "Sedan",
            RegistrationNumber = "CHV001"
        };

        // Act
        var result = vehicle.GetFullDescription();

        // Assert
        result.Should().Be("Chevrolet Malibu N/A - Sedan (CHV001)");
    }

    [Fact]
    public void GetFullDescription_NullVehicle_ThrowsArgumentNullException()
    {
        // Arrange
        Vehicle vehicle = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => vehicle.GetFullDescription());
    }

    [Fact]
    public void GetTrackingSessionCount_VehicleWithNullTrackingSessions_ReturnsZero()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            TrackingSessions = null!
        };

        // Act
        var result = vehicle.GetTrackingSessionCount();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetTrackingSessionCount_VehicleWithOneTrackingSession_ReturnsOne()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            TrackingSessions = new List<TrackingSession>
            {
                new TrackingSession { Id = 1 }
            }
        };

        // Act
        var result = vehicle.GetTrackingSessionCount();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetTrackingSessionCount_NullVehicle_ThrowsArgumentNullException()
    {
        // Arrange
        Vehicle vehicle = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => vehicle.GetTrackingSessionCount());
    }

    [Fact]
    public void GetVehicleAge_VehicleWithYear_ReturnsCorrectAge()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var vehicle = new Vehicle
        {
            Year = currentYear - 5
        };

        // Act
        var result = vehicle.GetVehicleAge();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetVehicleAge_VehicleWithNullYearAndModelYear_ReturnsNull()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Year = null,
            ModelYear = null
        };

        // Act
        var result = vehicle.GetVehicleAge();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetVehicleAge_NullVehicle_ThrowsArgumentNullException()
    {
        // Arrange
        Vehicle vehicle = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => vehicle.GetVehicleAge());
    }
}