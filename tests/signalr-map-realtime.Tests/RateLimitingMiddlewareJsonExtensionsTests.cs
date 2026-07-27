using System;
using System.Runtime.Serialization;
using System.Text.Json;
using Xunit;
using SignalRMapRealtime.Middleware;

namespace SignalRMapRealtime.Tests
{
    public class RateLimitingMiddlewareJsonExtensionsTests
    {
        private static RateLimitingMiddleware CreateMiddlewareInstance()
        {
            // Create an instance without invoking any constructor (in case the class
            // does not have a public parameterless constructor). This yields an
            // object with default values for all fields/properties, which is sufficient
            // for serialization tests.
            return (RateLimitingMiddleware)FormatterServices.GetUninitializedObject(typeof(RateLimitingMiddleware));
        }

        [Fact]
        public void ToJson_HappyPath_ReturnsNonEmptyString()
        {
            // Arrange
            var middleware = CreateMiddlewareInstance();

            // Act
            var json = middleware.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        [Fact]
        public void ToJson_Indented_ReturnsIndentedJson()
        {
            // Arrange
            var middleware = CreateMiddlewareInstance();

            // Act
            var json = middleware.ToJson(indented: true);

            // Assert
            // Indented JSON should contain line breaks.
            Assert.Contains("\n", json);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsObject()
        {
            // Arrange
            var middleware = CreateMiddlewareInstance();
            var json = middleware.ToJson();

            // Act
            var deserialized = RateLimitingMiddlewareJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RateLimitingMiddlewareJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyOrWhiteSpaceInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => RateLimitingMiddlewareJsonExtensions.FromJson(string.Empty));
            Assert.Throws<ArgumentException>(() => RateLimitingMiddlewareJsonExtensions.FromJson("   "));
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var result = RateLimitingMiddlewareJsonExtensions.FromJson(invalidJson);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndObject()
        {
            // Arrange
            var middleware = CreateMiddlewareInstance();
            var json = middleware.ToJson();

            // Act
            var success = RateLimitingMiddlewareJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{ not: valid }";

            // Act
            var success = RateLimitingMiddlewareJsonExtensions.TryFromJson(invalidJson, out var deserialized);

            // Assert
            Assert.False(success);
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RateLimitingMiddlewareJsonExtensions.TryFromJson(null!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyOrWhiteSpaceInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => RateLimitingMiddlewareJsonExtensions.TryFromJson(string.Empty, out _));
            Assert.Throws<ArgumentException>(() => RateLimitingMiddlewareJsonExtensions.TryFromJson("   ", out _));
        }
    }
}
