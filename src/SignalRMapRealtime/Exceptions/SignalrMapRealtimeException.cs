using System;

namespace SignalRMapRealtime.Exceptions
{
    /// <summary>
    /// Base exception for all SignalRMapRealtime specific errors.
    /// </summary>
    public class SignalrMapRealtimeException : Exception
    {
        public SignalrMapRealtimeException()
        {
        }

        public SignalrMapRealtimeException(string message) : base(message)
        {
        }

        public SignalrMapRealtimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
