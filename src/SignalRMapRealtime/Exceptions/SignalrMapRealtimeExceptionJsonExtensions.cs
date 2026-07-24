#nullable enable

using System;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides standardized API error serialization for <see cref="SignalrMapRealtimeException"/> and all derived exception types.
/// Implements <see cref="IApiErrorSerializable"/> to ensure consistent envelope structure
/// across all exception types in the SignalRMapRealtime system.
/// </summary>
public static class SignalrMapRealtimeExceptionJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="SignalrMapRealtimeException"/> to a standardized error response JSON string
    /// with consistent envelope structure (errorCode, message, details, statusCode).
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string with consistent error envelope structure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToErrorResponse(this SignalrMapRealtimeException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var error = new SignalrMapRealtimeError(value);
        return error.ToErrorResponse(indented);
    }

    /// <summary>
    /// Creates an <see cref="IApiErrorSerializable"/> wrapper for a <see cref="SignalrMapRealtimeException"/>.
    /// </summary>
    /// <param name="exception">The SignalR Map Realtime exception to wrap.</param>
    /// <returns>Wrapper implementing <see cref="IApiErrorSerializable"/> interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static IApiErrorSerializable ToApiError(this SignalrMapRealtimeException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new SignalrMapRealtimeError(exception);
    }

    /// <summary>
    /// Implementation of <see cref="IApiErrorSerializable"/> for <see cref="SignalrMapRealtimeException"/>.
    /// Provides standardized error envelope structure for API responses.
    /// </summary>
    private sealed class SignalrMapRealtimeError : ApiErrorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalrMapRealtimeError"/> class.
        /// </summary>
        /// <param name="exception">The SignalR Map Realtime exception to wrap.</param>
        public SignalrMapRealtimeError(SignalrMapRealtimeException exception) : base(exception) { }

        /// <summary>
        /// Gets the error code for programmatic error handling.
        /// </summary>
        public override string GetErrorCode()
        {
            var exceptionType = Exception.GetType().Name;

            return exceptionType switch
            {
                "ConfigurationException" => "CONFIGURATION_ERROR",
                _ => "SIGNAL_R_MAP_REALTIME_ERROR"
            };
        }

        /// <summary>
        /// Gets additional error details specific to SignalR Map Realtime exceptions.
        /// </summary>
        public override object? GetDetails()
        {
            return null; // Base class provides generic error handling
        }

        /// <summary>
        /// Gets the HTTP status code based on the specific exception type.
        /// </summary>
        public override int GetHttpStatusCode()
        {
            return 500; // Most SignalR Map Realtime errors are server errors
        }
    }
}