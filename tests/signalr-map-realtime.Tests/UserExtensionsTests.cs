using System;
using System.Collections.Generic;
using SignalRMapRealtime.Domain.Models;
using Xunit;
using FluentAssertions;

namespace SignalRMapRealtime.Tests;

public class UserExtensionsTests
{
    [Fact]
    public void IsEligibleForVehicleAssignment_DriverInTransportationDepartment_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            JobTitle = "Driver",
            Department = "Transportation"
        };

        // Act
        var result = user.IsEligibleForVehicleAssignment();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEligibleForVehicleAssignment_ManagerInLogisticsDepartment_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            JobTitle = "Manager",
            Department = "Logistics"
        };

        // Act
        var result = user.IsEligibleForVehicleAssignment();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEligibleForVehicleAssignment_CaseInsensitiveJobTitle_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            JobTitle = "driver",
            Department = "transportation"
        };

        // Act
        var result = user.IsEligibleForVehicleAssignment();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEligibleForVehicleAssignment_NonEligibleJobTitle_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            JobTitle = "Courier",
            Department = "Transportation"
        };

        // Act
        var result = user.IsEligibleForVehicleAssignment();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForVehicleAssignment_NonEligibleDepartment_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            JobTitle = "Driver",
            Department = "Sales"
        };

        // Act
        var result = user.IsEligibleForVehicleAssignment();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForVehicleAssignment_NullUser_ThrowsArgumentNullException()
    {
        // Arrange
        User user = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.IsEligibleForVehicleAssignment());
    }

    [Fact]
    public void GetFullDetails_UserWithAllProperties_ReturnsFormattedString()
    {
        // Arrange
        var location = new Location
        {
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        var vehicle = new Vehicle
        {
            Name = "Truck-001"
        };

        var user = new User
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "555-1234",
            ProfileImageUrl = "https://example.com/avatar.jpg",
            LastLocation = location,
            AssignedVehicles = new List<Vehicle> { vehicle }
        };

        // Act
        var result = user.GetFullDetails();

        // Assert
        result.Should().Be("John Doe - john.doe@example.com - Last location: 40.712800, -74.006000 - Assigned vehicles: Truck-001");
    }

    [Fact]
    public void GetFullDetails_UserWithNullLastLocation_ReturnsNoLastLocation()
    {
        // Arrange
        var user = new User
        {
            FullName = "Jane Smith",
            Email = "jane.smith@example.com",
            LastLocation = null,
            AssignedVehicles = new List<Vehicle>()
        };

        // Act
        var result = user.GetFullDetails();

        // Assert
        result.Should().Be("Jane Smith - jane.smith@example.com - No last location - No assigned vehicles");
    }

    [Fact]
    public void GetFullDetails_NullUser_ThrowsArgumentNullException()
    {
        // Arrange
        User user = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.GetFullDetails());
    }

    [Fact]
    public void HasValidContactInfo_UserWithPhoneAndProfileImage_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            PhoneNumber = "555-9876",
            ProfileImageUrl = "https://example.com/profile.jpg"
        };

        // Act
        var result = user.HasValidContactInfo();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasValidContactInfo_UserWithNullPhoneNumber_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            PhoneNumber = null,
            ProfileImageUrl = "https://example.com/profile.jpg"
        };

        // Act
        var result = user.HasValidContactInfo();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasValidContactInfo_UserWithEmptyProfileImageUrl_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            PhoneNumber = "555-1234",
            ProfileImageUrl = ""
        };

        // Act
        var result = user.HasValidContactInfo();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasValidContactInfo_NullUser_ThrowsArgumentNullException()
    {
        // Arrange
        User user = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.HasValidContactInfo());
    }
}