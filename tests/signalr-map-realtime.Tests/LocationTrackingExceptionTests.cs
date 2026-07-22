using System;
using SignalRMapRealtime.Exceptions;
using Xunit;

namespace signalr_map_realtime.Tests
{
    public class LocationTrackingExceptionTests
    {
        [Fact]
        public void LocationTrackingException_Constructors_SetsPropertiesCorrectly()
        {
            // Arrange & Act - Default Constructor
            var defaultEx = new LocationTrackingException();

            // Assert
            Assert.NotNull(defaultEx);

            // Arrange & Act - Message Constructor
            var message = "Location tracking failed";
            var messageEx = new LocationTrackingException(message);

            // Assert
            Assert.Equal(message, messageEx.Message);

            // Arrange & Act - Message and InnerException Constructor
            var innerEx = new InvalidOperationException("Inner fault");
            var fullEx = new LocationTrackingException(message, innerEx);

            // Assert
            Assert.Equal(message, fullEx.Message);
            Assert.Same(innerEx, fullEx.InnerException);
        }

        [Fact]
        public void VehicleNotFoundException_Constructor_SetsVehicleIdAndDefaultMessage()
        {
            // Arrange
            var vehicleId = 42;

            // Act
            var ex = new VehicleNotFoundException(vehicleId);

            // Assert
            Assert.Equal(vehicleId, ex.VehicleId);
            Assert.Contains(vehicleId.ToString(), ex.Message);
            Assert.Contains("was not found", ex.Message);
        }

        [Fact]
        public void VehicleNotFoundException_Constructor_WithCustomMessage_SetsVehicleIdAndMessage()
        {
            // Arrange
            var vehicleId = 99;
            var customMessage = "The specific vehicle could not be located in the database.";

            // Act
            var ex = new VehicleNotFoundException(vehicleId, customMessage);

            // Assert
            Assert.Equal(vehicleId, ex.VehicleId);
            Assert.Equal(customMessage, ex.Message);
        }

        [Fact]
        public void InvalidLocationException_Constructor_SetsCoordinatesAndDefaultMessage()
        {
            // Arrange
            var lat = 91.0; // Invalid latitude
            var lon = -200.0; // Invalid longitude

            // Act
            var ex = new InvalidLocationException(lat, lon);

            // Assert
            Assert.Equal(lat, ex.Latitude);
            Assert.Equal(lon, ex.Longitude);
            Assert.Contains(lat.ToString(), ex.Message);
            Assert.Contains(lon.ToString(), ex.Message);
        }

        [Fact]
        public void InvalidLocationException_Constructor_WithMessage_SetsMessageAndNullCoordinates()
        {
            // Arrange
            var message = "Coordinates are out of bounds";

            // Act
            var ex = new InvalidLocationException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.Latitude);
            Assert.Null(ex.Longitude);
        }

        [Fact]
        public void AssetNotFoundException_Constructor_SetsAssetIdAndMessage()
        {
            // Arrange
            var assetId = 123;

            // Act
            var ex = new AssetNotFoundException(assetId);

            // Assert
            Assert.Equal(assetId, ex.AssetId);
            Assert.Contains(assetId.ToString(), ex.Message);
            Assert.Contains("was not found", ex.Message);
        }

        [Fact]
        public void TrackingSessionNotFoundException_Constructor_SetsSessionIdAndMessage()
        {
            // Arrange
            var sessionId = 456;

            // Act
            var ex = new TrackingSessionNotFoundException(sessionId);

            // Assert
            Assert.Equal(sessionId, ex.SessionId);
            Assert.Contains(sessionId.ToString(), ex.Message);
            Assert.Contains("was not found", ex.Message);
        }

        [Fact]
        public void Exceptions_Inheritance_Chain_IsCorrect()
        {
            // Arrange & Act
            var vehicleEx = new VehicleNotFoundException(1);
            var locationEx = new InvalidLocationException(0, 0);
            var assetEx = new AssetNotFoundException(1);
            var sessionEx = new TrackingSessionNotFoundException(1);

            // Assert
            Assert.IsAssignableFrom<LocationTrackingException>(vehicleEx);
            Assert.IsAssignableFrom<LocationTrackingException>(locationEx);
            Assert.IsAssignableFrom<LocationTrackingException>(assetEx);
            Assert.IsAssignableFrom<LocationTrackingException>(sessionEx);

            Assert.IsAssignableFrom<Exception>(vehicleEx);
        }
    }
}
