using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalRMapRealtime.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ValidationException"/> to facilitate
    /// error handling and inspection of validation failures.
    /// </summary>
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception contains any validation errors.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns><c>true</c> if there is at least one error; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static bool HasErrors(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Errors?.Any() ?? false;
        }

        /// <summary>
        /// Returns a single string that concatenates all validation error messages,
        /// separated by a semicolon and a space.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns>A combined error message string, or <see cref="string.Empty"/> if no errors exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static string GetCombinedMessage(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return string.Join("; ", exception.Errors ?? Enumerable.Empty<string>());
        }

        /// <summary>
        /// Retrieves the distinct set of validation error messages.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{String}"/> containing unique error messages.
        /// Returns an empty sequence if no errors exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static IEnumerable<string> GetDistinctErrors(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Errors?.Distinct() ?? Enumerable.Empty<string>();
        }
    }
}
