using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class LocationDtoJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var location = new LocationDto { Latitude = 10.0, Longitude = 20.0 };

            // Act
            var json = location.ToJson();

            // Assert
            Assert.NotEmpty(json);
        }

        [Fact]
        public void ToJson_NullLocation_ThrowsArgumentNullException()
        {
            // Arrange
            LocationDto? location = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => location!.ToJson());
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsLocationDto()
        {
            // Arrange
            var json = "{\"latitude\":10.0,\"longitude\":20.0}";

            // Act
            var location = LocationDtoJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(location);
            Assert.Equal(10.0, location!.Latitude);
            Assert.Equal(20.0, location.Longitude);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentException()
        {
            // Arrange
            string? json = null;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => LocationDtoJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            var json = "Invalid json";

            // Act
            var location = LocationDtoJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(location);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndLocationDto()
        {
            // Arrange
            var json = "{\"latitude\":10.0,\"longitude\":20.0}";

            // Act
            var success = LocationDtoJsonExtensions.TryFromJson(json, out var location);

            // Assert
            Assert.True(success);
            Assert.NotNull(location);
            Assert.Equal(10.0, location!.Latitude);
            Assert.Equal(20.0, location.Longitude);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalse()
        {
            // Arrange
            string? json = null;

            // Act
            var success = LocationDtoJsonExtensions.TryFromJson(json!, out _);

            // Assert
            Assert.False(success);
        }
    }
}
