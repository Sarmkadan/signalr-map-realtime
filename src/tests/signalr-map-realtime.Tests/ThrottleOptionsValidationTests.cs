// SPDX-License-Identifier: MIT
// ---------------------------------------------------------------
// Tests for ThrottleOptionsValidation
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using SignalRMapRealtime.Configuration;
using Xunit;

namespace SignalRMapRealtime.Tests;

/// <summary>
/// Unit tests for <c>ThrottleOptionsValidation</c>. They verify that a valid throttle configuration
/// passes, that out-of-range interval values are reported, that disabled throttling skips validation,
/// and that null input and invalid input are handled by the validation methods.
/// </summary>
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

    /// <summary>
    /// Verifies that <c>Validate</c> returns an empty error list for a fully valid configuration.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <c>true</c> for a fully valid configuration.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>EnsureValid</c> does not throw for a fully valid configuration.
    /// </summary>
    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act & Assert
        var exception = Record.Exception(() => ThrottleOptionsValidation.EnsureValid(options));
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that when <c>Enabled</c> is <c>false</c>, <c>Validate</c> returns an empty error list
    /// even if a value is invalid (here <c>DeliveryVanIntervalSeconds</c> is 0).
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>Validate</c> throws <see cref="ArgumentNullException"/> when passed <c>null</c>.
    /// </summary>
    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ThrottleOptionsValidation.Validate(null!));
    }

    /// <summary>
    /// Verifies that <c>EnsureValid</c> throws <see cref="ArgumentNullException"/> when passed <c>null</c>.
    /// </summary>
    [Fact]
    public void EnsureValid_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ThrottleOptionsValidation.EnsureValid(null!));
    }

    /// <summary>
    /// Verifies that <c>EnsureValid</c> throws <see cref="ArgumentException"/> for an invalid configuration
    /// (<c>CourierIntervalSeconds</c> set to 0), and that the message names the property and states
    /// that the value is less than the minimum allowed value.
    /// </summary>
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

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <c>false</c> when <c>BicycleIntervalSeconds</c> is 0.
    /// </summary>
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

    /// <summary>
    /// Verifies that the interval bounds are inclusive: when every interval property is set to the
    /// given value, <c>Validate</c> returns no errors. Tested with 1 (minimum) and 86400 (maximum, 24 hours).
    /// </summary>
    /// <param name="boundaryValue">The interval value, in seconds, assigned to every interval property.</param>
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

    /// <summary>
    /// Verifies that an interval one second above the maximum (86401, for <c>DeliveryVanIntervalSeconds</c>)
    /// produces exactly one error that names the property and states that it exceeds the maximum allowed value.
    /// </summary>
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
