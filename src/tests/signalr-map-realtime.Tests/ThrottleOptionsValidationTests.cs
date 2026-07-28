// SPDX-License-Identifier: MIT
// ---------------------------------------------------------------
// Tests for ThrottleOptionsValidation
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using SignalRMapRealtime.Configuration;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class ThrottleOptionsValidationTests
{
    private static ThrottleOptions CreateValidOptions()
    {
        return new ThrottleOptions
        {
            Enabled = true,
            DeliveryVanIntervalSeconds = 1,
            CourierIntervalSeconds = 10,
            BicycleIntervalSeconds = 15,
            MotorcycleIntervalSeconds = 5,
            PortableIntervalSeconds = 30,
            FixedAssetIntervalSeconds = 300,
            DroneIntervalSeconds = 1,
            CoalesceFlushIntervalMilliseconds = 300,
            MaxBufferSizePerVehicle = 100
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        IReadOnlyList<string> errors = ThrottleOptionsValidation.Validate(options);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        bool isValid = ThrottleOptionsValidation.IsValid(options);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act & Assert
        var exception = Record.Exception(() => ThrottleOptionsValidation.EnsureValid(options));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenDisabled_ReturnsEmptyListEvenIfInvalid()
    {
        // Arrange
        var options = CreateValidOptions();
        options.Enabled = false;
        // make an obviously invalid value
        options.DeliveryVanIntervalSeconds = 0;

        // Act
        IReadOnlyList<string> errors = ThrottleOptionsValidation.Validate(options);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ThrottleOptionsValidation.Validate(null!));
    }

    [Fact]
    public void EnsureValid_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ThrottleOptionsValidation.EnsureValid(null!));
    }

    [Fact]
    public void EnsureValid_Invalid_ThrowsArgumentException_WithErrorDetails()
    {
        // Arrange
        var options = CreateValidOptions();
        options.CourierIntervalSeconds = 0; // below minimum

        // Act
        var ex = Assert.Throws<ArgumentException>(() => ThrottleOptionsValidation.EnsureValid(options));

        // Assert
        Assert.Contains(nameof(options.CourierIntervalSeconds), ex.Message);
        Assert.Contains("less than the minimum allowed value", ex.Message);
    }

    [Fact]
    public void IsValid_Invalid_ReturnsFalse()
    {
        // Arrange
        var options = CreateValidOptions();
        options.BicycleIntervalSeconds = 0; // invalid

        // Act
        bool isValid = ThrottleOptionsValidation.IsValid(options);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(1)]          // Minimum allowed
    [InlineData(86400)]      // Maximum allowed (24h)
    public void Validate_BoundaryValues_AreConsideredValid(int boundaryValue)
    {
        // Arrange
        var options = CreateValidOptions();
        options.DeliveryVanIntervalSeconds = boundaryValue;
        options.CourierIntervalSeconds = boundaryValue;
        options.BicycleIntervalSeconds = boundaryValue;
        options.MotorcycleIntervalSeconds = boundaryValue;
        options.PortableIntervalSeconds = boundaryValue;
        options.FixedAssetIntervalSeconds = boundaryValue;
        options.DroneIntervalSeconds = boundaryValue;

        // Act
        var errors = ThrottleOptionsValidation.Validate(options);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ValueExceedsMaximum_ProducesError()
    {
        // Arrange
        var options = CreateValidOptions();
        options.DeliveryVanIntervalSeconds = 86401; // just above max

        // Act
        var errors = ThrottleOptionsValidation.Validate(options);

        // Assert
        Assert.Single(errors);
        Assert.Contains(nameof(options.DeliveryVanIntervalSeconds), errors[0]);
        Assert.Contains("exceeds the maximum allowed value", errors[0]);
    }
}
