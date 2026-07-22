using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using SignalRMapRealtime.Exceptions;

namespace signalr_map_realtime.Tests
{
    public class LocationTrackingExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var exception = new LocationTrackingException("Test message");

            // Act
            var json = LocationTrackingExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => LocationTrackingExceptionJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsDeserializedException()
        {
            // Arrange
            var json = "{\"Message\":\"Test message\"}";
            var expectedException = new LocationTrackingException("Test message");

            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"Message\":\"Test message\"}";
            var expectedException = new LocationTrackingException("Test message");

            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(null, out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson("", out _);

            // Assert
            Assert.False(success);
        }
    }
}
