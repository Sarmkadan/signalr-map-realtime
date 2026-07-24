using System;

namespace SignalRMapRealtime.Exceptions
{
    /// <summary>
    /// Exception thrown when there is an error in configuration.
    /// </summary>
    public class ConfigurationException : SignalrMapRealtimeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        public ConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public ConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
