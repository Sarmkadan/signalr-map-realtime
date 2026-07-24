using Xunit;
using SignalRMapRealtime.DTOs;

namespace SignalRMapRealtime.Tests
{
    public class AssetDtoJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var assetDto = new AssetDto { Id = 1, Name = "Test Asset" };

            // Act
            var json = assetDto.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("1", json);
            Assert.Contains("Test Asset", json);
        }

        [Fact]
        public void ToJson_NullAssetDto_ThrowsArgumentNullException()
        {
            // Arrange
            AssetDto assetDto = null!;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => assetDto.ToJson());
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsAssetDto()
        {
            // Arrange
            var json = "{\"Id\": 1, \"Name\": \"Test Asset\"}";

            // Act
            var assetDto = AssetDtoJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(assetDto);
            Assert.Equal(1, assetDto.Id);
            Assert.Equal("Test Asset", assetDto.Name);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentException()
        {
            // Arrange
            string json = null!;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AssetDtoJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Arrange
            string json = "";

            // Act
            var assetDto = AssetDtoJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(assetDto);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"Id\": 1, \"Name\": \"Test Asset\"}";

            // Act
            var success = AssetDtoJsonExtensions.TryFromJson(json, out var assetDto);

            // Assert
            Assert.True(success);
            Assert.NotNull(assetDto);
            Assert.Equal(1, assetDto.Id);
            Assert.Equal("Test Asset", assetDto.Name);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalse()
        {
            // Arrange
            string json = null!;

            // Act
            var success = AssetDtoJsonExtensions.TryFromJson(json, out var assetDto);

            // Assert
            Assert.False(success);
            Assert.Null(assetDto);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Arrange
            string json = "";

            // Act
            var success = AssetDtoJsonExtensions.TryFromJson(json, out var assetDto);

            // Assert
            Assert.False(success);
            Assert.Null(assetDto);
        }
    }
}
