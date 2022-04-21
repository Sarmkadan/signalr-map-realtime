using System;

namespace SignalRMapRealtime.Exceptions
{
    /// <summary>
    /// Exception thrown when there is an error in configuration.
    /// </summary>
    public class ConfigurationException : SignalrMapRealtimeException
    {
        public ConfigurationException()
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
