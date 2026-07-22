using Xunit;
using System;
using System.Collections.Generic;
using SignalRMapRealtime.Exceptions;

namespace signalr_map_realtime.Tests
{
    public class LocationTrackingExceptionValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var exception = new LocationTrackingException();

            // Act
            var result = LocationTrackingExceptionValidation.Validate(exception);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => LocationTrackingExceptionValidation.Validate(null));
        }

        [Fact]
        public void Validate_EmptyException_ReturnsEmptyList()
        {
            // Arrange
            var exception = new LocationTrackingException();

            // Act
            var result = LocationTrackingExceptionValidation.Validate(exception);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_VehicleNotFoundException_ReturnsValidationProblem()
        {
            // Arrange
            var exception = new VehicleNotFoundException(1);

            // Act
            var result = LocationTrackingExceptionValidation.Validate(exception);

            // Assert
            Assert.Single(result);
            Assert.Equal("VehicleNotFoundException.VehicleId must be positive, but was 1.", result[0]);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var exception = new LocationTrackingException();

            // Act
            var result = LocationTrackingExceptionValidation.IsValid(exception);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => LocationTrackingExceptionValidation.IsValid(null));
        }

        [Fact]
        public void IsValid_EmptyException_ReturnsTrue()
        {
            // Arrange
            var exception = new LocationTrackingException();

            // Act
            var result = LocationTrackingExceptionValidation.IsValid(exception);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_VehicleNotFoundException_ReturnsFalse()
        {
            // Arrange
            var exception = new VehicleNotFoundException(1);

            // Act
            var result = LocationTrackingExceptionValidation.IsValid(exception);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var exception = new LocationTrackingException();

            // Act and Assert
            LocationTrackingExceptionValidation.EnsureValid(exception);
        }

        [Fact]
        public void EnsureValid_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => LocationTrackingExceptionValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_EmptyException_DoesNotThrow()
        {
            // Arrange
            var exception = new LocationTrackingException();

            // Act and Assert
            LocationTrackingExceptionValidation.EnsureValid(exception);
        }

        [Fact]
        public void EnsureValid_VehicleNotFoundException_ThrowsArgumentException()
        {
            // Arrange
            var exception = new VehicleNotFoundException(1);

            // Act and Assert
            Assert.Throws<ArgumentException>(() => LocationTrackingExceptionValidation.EnsureValid(exception));
        }
    }
}
