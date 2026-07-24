using System;
using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class GeofenceDtoJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsValidJson()
        {
            // Arrange
            var geofence = new GeofenceDto(); // assume parameterless ctor exists

            // Act
            var json = geofence.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_NullArgument_ThrowsArgumentNullException()
        {
            // Arrange
            GeofenceDto? geofence = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geofence!.ToJson());
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsObject()
        {
            // Arrange
            var original = new GeofenceDto(); // minimal instance
            var json = original.ToJson();

            // Act
            var deserialized = GeofenceDtoJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            // Basic sanity: the deserialized instance should be of the correct type
            Assert.IsType<GeofenceDto>(deserialized);
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => GeofenceDtoJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndObject()
        {
            // Arrange
            var original = new GeofenceDto();
            var json = original.ToJson();

            // Act
            var result = GeofenceDtoJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserialized);
            Assert.IsType<GeofenceDto>(deserialized);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var malformedJson = "{ this is not valid json }";

            // Act
            var result = GeofenceDtoJsonExtensions.TryFromJson(malformedJson, out var deserialized);

            // Assert
            Assert.False(result);
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_NullOrEmpty_ThrowsArgumentException()
        {
            // Act & Assert for null
            Assert.Throws<ArgumentException>(() => GeofenceDtoJsonExtensions.TryFromJson(null!, out _));

            // Act & Assert for empty string
            Assert.Throws<ArgumentException>(() => GeofenceDtoJsonExtensions.TryFromJson(string.Empty, out _));
        }
    }
}
