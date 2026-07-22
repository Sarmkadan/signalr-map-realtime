using Xunit;
using System.Text.Json;
using SignalRMapRealtime.Exceptions;

namespace signalr_map_realtime.Tests
{
    public class ValidationExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var json = ValidationExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
            Assert.Contains("Validation failed", json);
        }

        [Fact]
        public void ToJson_WithIndentedFlag_ReturnsPrettyJson()
        {
            // Arrange
            var errors = new List<string> { "Error 1" };
            var exception = new ValidationException("Test message", errors);

            // Act
            var json = ValidationExceptionJsonExtensions.ToJson(exception, indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("{\n", json);
            Assert.Contains("Test message", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ValidationExceptionJsonExtensions.ToJson(null));
        }

        [Fact]
        public void ToJson_NullErrorsCollection_SerializesCorrectly()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: null);

            // Act
            var json = ValidationExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            var deserialized = ValidationExceptionJsonExtensions.FromJson(json);
            Assert.NotNull(deserialized);
            Assert.Equal("Test message", deserialized.Message);
            Assert.Empty(deserialized.Errors);
        }

        [Fact]
        public void ToJson_EmptyErrorsCollection_SerializesCorrectly()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: Enumerable.Empty<string>());

            // Act
            var json = ValidationExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            var deserialized = ValidationExceptionJsonExtensions.FromJson(json);
            Assert.NotNull(deserialized);
            Assert.Equal("Test message", deserialized.Message);
            Assert.Empty(deserialized.Errors);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Validation failed\",\"errors\":[\"Error 1\",\"Error 2\"]}";
            var expectedErrors = new List<string> { "Error 1", "Error 2" };
            var expectedException = new ValidationException("Validation failed", expectedErrors);

            // Act
            var exception = ValidationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(expectedException.Message, exception.Message);
            Assert.Equal(expectedException.Errors, exception.Errors);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var exception = ValidationExceptionJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            // Act
            var exception = ValidationExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_WhitespaceString_ReturnsNull()
        {
            // Act
            var exception = ValidationExceptionJsonExtensions.FromJson("   ");

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{invalid json}";

            // Act and Assert
            Assert.Throws<JsonException>(() => ValidationExceptionJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedException()
        {
            // Arrange
            var json = "{\"message\":\"Validation failed\",\"errors\":[\"Error 1\"]}";
            var expectedErrors = new List<string> { "Error 1" };
            var expectedException = new ValidationException("Validation failed", expectedErrors);

            // Act
            var success = ValidationExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            Assert.True(success);
            Assert.NotNull(exception);
            Assert.Equal(expectedException.Message, exception.Message);
            Assert.Equal(expectedException.Errors, exception.Errors);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = ValidationExceptionJsonExtensions.TryFromJson(null, out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void TryFromJson_EmptyString_ReturnsFalse()
        {
            // Act
            var success = ValidationExceptionJsonExtensions.TryFromJson("", out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var invalidJson = "{invalid json}";

            // Act
            var success = ValidationExceptionJsonExtensions.TryFromJson(invalidJson, out var exception);

            // Assert
            Assert.False(success);
            Assert.Null(exception);
        }

        [Fact]
        public void RoundTrip_SerializationDeserialization_PreservesData()
        {
            // Arrange
            var originalErrors = new List<string> { "Error 1", "Error 2", "Error 3" };
            var originalException = new ValidationException("Round trip test", originalErrors);

            // Act - serialize and deserialize
            var json = ValidationExceptionJsonExtensions.ToJson(originalException);
            var deserializedException = ValidationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserializedException);
            Assert.Equal(originalException.Message, deserializedException.Message);
            Assert.Equal(originalException.Errors.Count(), deserializedException.Errors.Count());
            Assert.Equal(originalException.Errors, deserializedException.Errors);
        }
    }
}