using System;
using System.Collections.Generic;
using System.Linq;
using SignalRMapRealtime.Exceptions;
using Xunit;

namespace signalr_map_realtime.Tests
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void DefaultConstructor_SetsMessageAndEmptyErrors()
        {
            // Act
            var ex = new ValidationException();

            // Assert
            Assert.Equal("Validation failed.", ex.Message);
            Assert.NotNull(ex.Errors);
            Assert.Empty(ex.Errors);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageConstructor_SetsMessageAndEmptyErrors()
        {
            // Arrange
            var message = "Custom validation message";

            // Act
            var ex = new ValidationException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.NotNull(ex.Errors);
            Assert.Empty(ex.Errors);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageAndErrorsConstructor_SetsMessageAndProvidedErrors()
        {
            // Arrange
            var message = "Invalid input";
            var errors = new List<string> { "Error A", "Error B" };

            // Act
            var ex = new ValidationException(message, errors);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.NotNull(ex.Errors);
            Assert.Equal(errors, ex.Errors);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageAndNullErrorsConstructor_UsesEmptyErrors()
        {
            // Arrange
            var message = "No errors collection";

            // Act
            var ex = new ValidationException(message, (IEnumerable<string>)null);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.NotNull(ex.Errors);
            Assert.Empty(ex.Errors);
        }

        [Fact]
        public void MessageAndInnerExceptionConstructor_SetsMessageAndInnerException()
        {
            // Arrange
            var message = "Validation failed with inner";
            var inner = new InvalidOperationException("Inner exception");

            // Act
            var ex = new ValidationException(message, inner);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Same(inner, ex.InnerException);
            Assert.NotNull(ex.Errors);
            Assert.Empty(ex.Errors);
        }

        [Fact]
        public void ErrorsCollection_IsReadOnlyReference()
        {
            // Arrange
            var errors = new List<string> { "First", "Second" };
            var ex = new ValidationException("msg", errors);

            // Act
            // Mutate the original list after construction
            errors.Add("Third");

            // Assert
            // The exception should reflect the mutated collection because it stores the reference.
            // This behavior is intentional per current implementation.
            Assert.Contains("Third", ex.Errors);
        }

        [Fact]
        public void ErrorsProperty_NullSafeEnumeration()
        {
            // Arrange
            var ex = new ValidationException("msg", (IEnumerable<string>)null);

            // Act & Assert
            // Ensure that iterating over Errors never throws.
            foreach (var err in ex.Errors)
            {
                // No iteration expected.
                Assert.True(false, "Should not iterate over empty collection.");
            }

            Assert.Empty(ex.Errors);
        }
    }
}
