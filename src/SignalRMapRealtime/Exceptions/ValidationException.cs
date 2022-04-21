using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalRMapRealtime.Exceptions
{
    /// <summary>
    /// Exception thrown when input validation fails.
    /// </summary>
    public class ValidationException : SignalrMapRealtimeException
    {
        public IEnumerable<string> Errors { get; }

        public ValidationException()
            : base("Validation failed.")
        {
            Errors = Enumerable.Empty<string>();
        }

        public ValidationException(string message)
            : base(message)
        {
            Errors = Enumerable.Empty<string>();
        }

        public ValidationException(string message, IEnumerable<string> errors)
            : base(message)
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = Enumerable.Empty<string>();
        }
    }
}
