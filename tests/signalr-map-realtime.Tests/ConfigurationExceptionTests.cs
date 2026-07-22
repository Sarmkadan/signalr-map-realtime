using Xunit;
using SignalRMapRealtime.Exceptions;

namespace signalr_map_realtime.Tests
{
    public class ConfigurationExceptionTests
    {
        [Fact]
        public void Constructor_NoMessage_NoInnerException_ThrowsConfigurationException()
        {
            // Act
            var exception = new ConfigurationException();

            // Assert
            Assert.IsType<ConfigurationException>(exception);
        }

        [Fact]
        public void Constructor_Message_NoInnerException_ThrowsConfigurationException()
        {
            // Arrange
            var message = "Test message";

            // Act
            var exception = new ConfigurationException(message);

            // Assert
            Assert.IsType<ConfigurationException>(exception);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void Constructor_Message_WithInnerException_ThrowsConfigurationException()
        {
            // Arrange
            var message = "Test message";
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new ConfigurationException(message, innerException);

            // Assert
            Assert.IsType<ConfigurationException>(exception);
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_NullMessage_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ConfigurationException(null));
        }

        [Fact]
        public void Constructor_NullInnerException_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ConfigurationException("Test message", null));
        }
    }
}
