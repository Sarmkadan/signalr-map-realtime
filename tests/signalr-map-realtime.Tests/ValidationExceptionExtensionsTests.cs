using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using SignalRMapRealtime.Exceptions;

namespace signalr_map_realtime.Tests
{
    public class ValidationExceptionExtensionsTests
    {
        [Fact]
        public void HasErrors_HasErrors_ReturnsTrue()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var result = exception.HasErrors();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasErrors_NoErrors_ReturnsFalse()
        {
            // Arrange
            var exception = new ValidationException("No errors");

            // Act
            var result = exception.HasErrors();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasErrors_NullErrorsCollection_ReturnsFalse()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: (IEnumerable<string>)null);

            // Act
            var result = exception.HasErrors();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasErrors_EmptyErrorsCollection_ReturnsFalse()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: Enumerable.Empty<string>());

            // Act
            var result = exception.HasErrors();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasErrors_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ValidationExceptionExtensions.HasErrors(null));
        }

        [Fact]
        public void GetCombinedMessage_HasErrors_ReturnsCombinedMessage()
        {
            // Arrange
            var errors = new List<string> { "First error", "Second error", "Third error" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var result = exception.GetCombinedMessage();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("First error; Second error; Third error", result);
        }

        [Fact]
        public void GetCombinedMessage_SingleError_ReturnsSingleMessage()
        {
            // Arrange
            var errors = new List<string> { "Single error message" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var result = exception.GetCombinedMessage();

            // Assert
            Assert.Equal("Single error message", result);
        }

        [Fact]
        public void GetCombinedMessage_NoErrors_ReturnsEmptyString()
        {
            // Arrange
            var exception = new ValidationException("No errors");

            // Act
            var result = exception.GetCombinedMessage();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetCombinedMessage_NullErrorsCollection_ReturnsEmptyString()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: (IEnumerable<string>)null);

            // Act
            var result = exception.GetCombinedMessage();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetCombinedMessage_EmptyErrorsCollection_ReturnsEmptyString()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: Enumerable.Empty<string>());

            // Act
            var result = exception.GetCombinedMessage();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetCombinedMessage_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ValidationExceptionExtensions.GetCombinedMessage(null));
        }

        [Fact]
        public void GetDistinctErrors_HasErrors_ReturnsDistinctErrors()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2", "Error 1", "Error 3", "Error 2" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var result = exception.GetDistinctErrors();

            // Assert
            Assert.NotNull(result);
            var distinctErrors = result.ToList();
            Assert.Equal(3, distinctErrors.Count);
            Assert.Contains("Error 1", distinctErrors);
            Assert.Contains("Error 2", distinctErrors);
            Assert.Contains("Error 3", distinctErrors);
        }

        [Fact]
        public void GetDistinctErrors_SingleError_ReturnsSingleError()
        {
            // Arrange
            var errors = new List<string> { "Single error" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var result = exception.GetDistinctErrors();

            // Assert
            var distinctErrors = result.ToList();
            Assert.Single(distinctErrors);
            Assert.Equal("Single error", distinctErrors[0]);
        }

        [Fact]
        public void GetDistinctErrors_NoErrors_ReturnsEmptyCollection()
        {
            // Arrange
            var exception = new ValidationException("No errors");

            // Act
            var result = exception.GetDistinctErrors();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetDistinctErrors_NullErrorsCollection_ReturnsEmptyCollection()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: (IEnumerable<string>)null);

            // Act
            var result = exception.GetDistinctErrors();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetDistinctErrors_EmptyErrorsCollection_ReturnsEmptyCollection()
        {
            // Arrange
            var exception = new ValidationException("Test message", errors: Enumerable.Empty<string>());

            // Act
            var result = exception.GetDistinctErrors();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetDistinctErrors_NullException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ValidationExceptionExtensions.GetDistinctErrors(null));
        }

        [Fact]
        public void GetDistinctErrors_MultipleSameErrors_ReturnsOnlyUniqueErrors()
        {
            // Arrange
            var errors = new List<string> { "Duplicate", "Duplicate", "Duplicate" };
            var exception = new ValidationException("Validation failed", errors);

            // Act
            var result = exception.GetDistinctErrors();

            // Assert
            var distinctErrors = result.ToList();
            Assert.Single(distinctErrors);
            Assert.Equal("Duplicate", distinctErrors[0]);
        }
    }
}