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
        /// <summary>
        /// Gets the collection of validation error messages.
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        public ValidationException()
        : base("Validation failed.")
        {
            Errors = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public ValidationException(string message)
        : base(message)
        {
            Errors = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and validation errors.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errors">Collection of validation error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public ValidationException(string message, IEnumerable<string> errors)
        : base(message)
        {
            Errors = errors ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public ValidationException(string message, Exception innerException)
        : base(message, innerException)
        {
            Errors = Enumerable.Empty<string>();
        }
    }
}
