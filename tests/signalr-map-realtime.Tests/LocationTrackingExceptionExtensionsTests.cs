using System;
using FluentAssertions;
using SignalRMapRealtime.Exceptions;
using Xunit;

namespace signalr_map_realtime.Tests
{
    public class LocationTrackingExceptionExtensionsTests
    {
        public class GetVehicleId
        {
            [Fact]
            public void ReturnsVehicleId_WhenExceptionIsVehicleNotFoundException()
            {
                // Arrange
                var expectedVehicleId = 123;
                var exception = new VehicleNotFoundException(expectedVehicleId);

                // Act
                var result = exception.GetVehicleId();

                // Assert
                result.Should().Be(expectedVehicleId);
            }

            [Fact]
            public void ReturnsNull_WhenExceptionIsNotVehicleNotFoundException()
            {
                // Arrange
                var exception = new AssetNotFoundException(456);

                // Act
                var result = exception.GetVehicleId();

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void ReturnsNull_WhenExceptionIsInvalidLocationException()
            {
                // Arrange
                var exception = new InvalidLocationException(10.5, 20.3);

                // Act
                var result = exception.GetVehicleId();

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.GetVehicleId());
            }
        }

        public class GetAssetId
        {
            [Fact]
            public void ReturnsAssetId_WhenExceptionIsAssetNotFoundException()
            {
                // Arrange
                var expectedAssetId = 456;
                var exception = new AssetNotFoundException(expectedAssetId);

                // Act
                var result = exception.GetAssetId();

                // Assert
                result.Should().Be(expectedAssetId);
            }

            [Fact]
            public void ReturnsNull_WhenExceptionIsNotAssetNotFoundException()
            {
                // Arrange
                var exception = new VehicleNotFoundException(123);

                // Act
                var result = exception.GetAssetId();

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void ReturnsNull_WhenExceptionIsTrackingSessionNotFoundException()
            {
                // Arrange
                var exception = new TrackingSessionNotFoundException(789);

                // Act
                var result = exception.GetAssetId();

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.GetAssetId());
            }
        }

        public class GetSessionId
        {
            [Fact]
            public void ReturnsSessionId_WhenExceptionIsTrackingSessionNotFoundException()
            {
                // Arrange
                var expectedSessionId = 789;
                var exception = new TrackingSessionNotFoundException(expectedSessionId);

                // Act
                var result = exception.GetSessionId();

                // Assert
                result.Should().Be(expectedSessionId);
            }

            [Fact]
            public void ReturnsNull_WhenExceptionIsNotTrackingSessionNotFoundException()
            {
                // Arrange
                var exception = new VehicleNotFoundException(123);

                // Act
                var result = exception.GetSessionId();

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void ReturnsNull_WhenExceptionIsInvalidLocationException()
            {
                // Arrange
                var exception = new InvalidLocationException(10.5, 20.3);

                // Act
                var result = exception.GetSessionId();

                // Assert
                result.Should().BeNull();
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.GetSessionId());
            }
        }

        public class GetCoordinates
        {
            [Fact]
            public void ReturnsCoordinates_WhenExceptionIsInvalidLocationException()
            {
                // Arrange
                var expectedLatitude = 10.5;
                var expectedLongitude = 20.3;
                var exception = new InvalidLocationException(expectedLatitude, expectedLongitude);

                // Act
                exception.GetCoordinates(out var latitude, out var longitude);

                // Assert
                latitude.Should().Be(expectedLatitude);
                longitude.Should().Be(expectedLongitude);
            }

            [Fact]
            public void ReturnsNullCoordinates_WhenExceptionIsNotInvalidLocationException()
            {
                // Arrange
                var exception = new VehicleNotFoundException(123);

                // Act
                exception.GetCoordinates(out var latitude, out var longitude);

                // Assert
                latitude.Should().BeNull();
                longitude.Should().BeNull();
            }

            [Fact]
            public void ReturnsNullCoordinates_WhenExceptionIsAssetNotFoundException()
            {
                // Arrange
                var exception = new AssetNotFoundException(456);

                // Act
                exception.GetCoordinates(out var latitude, out var longitude);

                // Assert
                latitude.Should().BeNull();
                longitude.Should().BeNull();
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.GetCoordinates(out _, out _));
            }

            [Fact]
            public void ReturnsNullCoordinates_WhenInvalidLocationExceptionHasNullCoordinates()
            {
                // Arrange
                var exception = new InvalidLocationException("Custom message");

                // Act
                exception.GetCoordinates(out var latitude, out var longitude);

                // Assert
                latitude.Should().BeNull();
                longitude.Should().BeNull();
            }
        }

        public class ToErrorMessage
        {
            [Fact]
            public void IncludesMessageAndVehicleId_ForVehicleNotFoundException()
            {
                // Arrange
                var vehicleId = 123;
                var exception = new VehicleNotFoundException(vehicleId);

                // Act
                var result = exception.ToErrorMessage();

                // Assert
                result.Should().Contain("Vehicle with ID 123 was not found.");
                result.Should().Contain($"Vehicle ID: {vehicleId}");
            }

            [Fact]
            public void IncludesMessageAndAssetId_ForAssetNotFoundException()
            {
                // Arrange
                var assetId = 456;
                var exception = new AssetNotFoundException(assetId);

                // Act
                var result = exception.ToErrorMessage();

                // Assert
                result.Should().Contain("Asset with ID 456 was not found.");
                result.Should().Contain($"Asset ID: {assetId}");
            }

            [Fact]
            public void IncludesMessageAndSessionId_ForTrackingSessionNotFoundException()
            {
                // Arrange
                var sessionId = 789;
                var exception = new TrackingSessionNotFoundException(sessionId);

                // Act
                var result = exception.ToErrorMessage();

                // Assert
                result.Should().Contain("Tracking session with ID 789 was not found.");
                result.Should().Contain($"Session ID: {sessionId}");
            }

            [Fact]
            public void IncludesMessageAndCoordinates_ForInvalidLocationException()
            {
                // Arrange
                var exception = new InvalidLocationException(10.5, 20.3);

                // Act
                var result = exception.ToErrorMessage();

                // Assert
                result.Should().Contain("Invalid location coordinates");
                result.Should().Contain("Latitude=10.500000");
                result.Should().Contain("Longitude=20.300000");
            }

            [Fact]
            public void IncludesOnlyMessage_ForBaseLocationTrackingException()
            {
                // Arrange
                var exception = new LocationTrackingException("Base exception message");

                // Act
                var result = exception.ToErrorMessage();

                // Assert
                result.Should().Be("Base exception message");
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.ToErrorMessage());
            }
        }

        public class IsNotFoundError
        {
            [Fact]
            public void ReturnsTrue_ForVehicleNotFoundException()
            {
                // Arrange
                var exception = new VehicleNotFoundException(123);

                // Act
                var result = exception.IsNotFoundError();

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void ReturnsTrue_ForAssetNotFoundException()
            {
                // Arrange
                var exception = new AssetNotFoundException(456);

                // Act
                var result = exception.IsNotFoundError();

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void ReturnsTrue_ForTrackingSessionNotFoundException()
            {
                // Arrange
                var exception = new TrackingSessionNotFoundException(789);

                // Act
                var result = exception.IsNotFoundError();

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void ReturnsFalse_ForInvalidLocationException()
            {
                // Arrange
                var exception = new InvalidLocationException(10.5, 20.3);

                // Act
                var result = exception.IsNotFoundError();

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalse_ForBaseLocationTrackingException()
            {
                // Arrange
                var exception = new LocationTrackingException("Base exception");

                // Act
                var result = exception.IsNotFoundError();

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.IsNotFoundError());
            }
        }

        public class IsInvalidLocationError
        {
            [Fact]
            public void ReturnsTrue_ForInvalidLocationException()
            {
                // Arrange
                var exception = new InvalidLocationException(10.5, 20.3);

                // Act
                var result = exception.IsInvalidLocationError();

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void ReturnsFalse_ForVehicleNotFoundException()
            {
                // Arrange
                var exception = new VehicleNotFoundException(123);

                // Act
                var result = exception.IsInvalidLocationError();

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalse_ForAssetNotFoundException()
            {
                // Arrange
                var exception = new AssetNotFoundException(456);

                // Act
                var result = exception.IsInvalidLocationError();

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalse_ForTrackingSessionNotFoundException()
            {
                // Arrange
                var exception = new TrackingSessionNotFoundException(789);

                // Act
                var result = exception.IsInvalidLocationError();

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalse_ForBaseLocationTrackingException()
            {
                // Arrange
                var exception = new LocationTrackingException("Base exception");

                // Act
                var result = exception.IsInvalidLocationError();

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void ThrowsArgumentNullException_WhenExceptionIsNull()
            {
                // Arrange
                LocationTrackingException exception = null!;

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => exception.IsInvalidLocationError());
            }
        }
    }
}