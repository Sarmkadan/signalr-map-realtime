using System;
using SignalRMapRealtime.Exceptions;
using Xunit;

namespace signalr_map_realtime.Tests
{
    public class SignalrMapRealtimeExceptionTests
    {
        [Fact]
        public void DefaultConstructor_CreatesExceptionWithDefaultMessage()
        {
            // Act
            var ex = new SignalrMapRealtimeException();

            // Assert
            Assert.Equal("Exception of type 'SignalRMapRealtime.Exceptions.SignalrMapRealtimeException' was thrown.", ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageConstructor_SetsMessage()
        {
            // Arrange
            var message = "Custom error message for SignalR Map Realtime";

            // Act
            var ex = new SignalrMapRealtimeException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageConstructor_WithNullMessage_SetsMessageToNull()
        {
            // Act
            var ex = new SignalrMapRealtimeException(null);

            // Assert
            Assert.Null(ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageConstructor_WithEmptyMessage_SetsMessageToEmptyString()
        {
            // Act
            var ex = new SignalrMapRealtimeException(string.Empty);

            // Assert
            Assert.Equal(string.Empty, ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void MessageAndInnerExceptionConstructor_SetsMessageAndInnerException()
        {
            // Arrange
            var message = "SignalR Map Realtime error occurred";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var ex = new SignalrMapRealtimeException(message, innerException);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Same(innerException, ex.InnerException);
        }

        [Fact]
        public void MessageAndInnerExceptionConstructor_WithNullMessage_SetsMessageAndInnerException()
        {
            // Arrange
            var innerException = new ArgumentNullException("paramName");

            // Act
            var ex = new SignalrMapRealtimeException(null, innerException);

            // Assert
            Assert.Null(ex.Message);
            Assert.Same(innerException, ex.InnerException);
        }

        [Fact]
        public void MessageAndInnerExceptionConstructor_WithNullInnerException_SetsMessageOnly()
        {
            // Arrange
            var message = "Error without inner exception";

            // Act
            var ex = new SignalrMapRealtimeException(message, null);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void ExceptionInheritance_IsCorrect()
        {
            // Arrange
            var message = "Test exception";

            // Act
            var ex = new SignalrMapRealtimeException(message);

            // Assert
            Assert.IsType<Exception>(ex);
            Assert.IsType<SignalrMapRealtimeException>(ex);
        }
    }
}