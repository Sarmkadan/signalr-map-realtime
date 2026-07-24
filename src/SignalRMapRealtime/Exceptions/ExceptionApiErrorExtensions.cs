#nullable enable

using System;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides standardized API error serialization for any <see cref="Exception"/> type.
/// Implements <see cref="IApiErrorSerializable"/> to ensure consistent envelope structure
/// for exceptions that don't have specific serialization extensions.
/// </summary>
public static class ExceptionApiErrorExtensions
{
    /// <summary>
    /// Converts any exception to a standardized error response JSON string
    /// with consistent envelope structure (errorCode, message, details, statusCode).
    /// </summary>
    /// <param name="exception">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string with consistent error envelope structure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static string ToErrorResponse(this Exception exception, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var error = new GenericError(exception);
        return error.ToErrorResponse(indented);
    }

    /// <summary>
    /// Creates an <see cref="IApiErrorSerializable"/> wrapper for any exception.
    /// </summary>
    /// <param name="exception">The exception to wrap.</param>
    /// <returns>Wrapper implementing <see cref="IApiErrorSerializable"/> interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static IApiErrorSerializable ToApiError(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new GenericError(exception);
    }

    /// <summary>
    /// Implementation of <see cref="IApiErrorSerializable"/> for generic exceptions.
    /// Provides standardized error envelope structure for API responses.
    /// </summary>
    private sealed class GenericError : ApiErrorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericError"/> class.
        /// </summary>
        /// <param name="exception">The exception to wrap.</param>
        public GenericError(Exception exception) : base(exception) { }

        /// <summary>
        /// Gets the error code for programmatic error handling.
        /// </summary>
        public override string GetErrorCode()
        {
            var exceptionType = Exception.GetType().Name;

            return exceptionType switch
            {
                "ArgumentException" or "ArgumentNullException" or "ArgumentOutOfRangeException" => "INVALID_INPUT",
                "UnauthorizedAccessException" => "UNAUTHORIZED",
                "NotSupportedException" => "NOT_SUPPORTED",
                "InvalidOperationException" => "INVALID_OPERATION",
                _ => "INTERNAL_SERVER_ERROR"
            };
        }

        /// <summary>
        /// Gets additional error details for generic exceptions.
        /// </summary>
        public override object? GetDetails()
        {
            return null; // Generic exceptions don't have specific details structure
        }

        /// <summary>
        /// Gets the HTTP status code based on the exception type.
        /// </summary>
        public override int GetHttpStatusCode()
        {
            if (Exception is ArgumentException)
                return 400;

            if (Exception is UnauthorizedAccessException)
                return 401;

            if (Exception is InvalidOperationException or NotSupportedException)
                return 400;

            return 500;
        }
    }
}