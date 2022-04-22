using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalRMapRealtime.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="ValidationException"/>.
    /// </summary>
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception contains any validation errors.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns><c>true</c> if there is at least one error; otherwise, <c>false</c>.</returns>
        public static bool HasErrors(this ValidationException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            return exception.Errors?.Any() ?? false;
        }

        /// <summary>
        /// Returns a single string that concatenates all validation error messages,
        /// separated by a semicolon and a space.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns>A combined error message string.</returns>
        public static string GetCombinedMessage(this ValidationException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            return string.Join("; ", exception.Errors ?? Enumerable.Empty<string>());
        }

        /// <summary>
        /// Retrieves the distinct set of validation error messages.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{String}"/> containing unique error messages.</returns>
        public static IEnumerable<string> GetDistinctErrors(this ValidationException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            return exception.Errors?.Distinct() ?? Enumerable.Empty<string>();
        }
    }
}
