#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides standardized API error serialization for <see cref="LocationTrackingException"/> and its derived types.
/// Implements <see cref="IApiErrorSerializable"/> to ensure consistent envelope structure
/// across all exception types in the SignalRMapRealtime system.
/// </summary>
public static class LocationTrackingExceptionJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="LocationTrackingException"/> to a standardized error response JSON string
    /// with consistent envelope structure (errorCode, message, details, statusCode).
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string with consistent error envelope structure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToErrorResponse(this LocationTrackingException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var error = new LocationTrackingError(value);
        return error.ToErrorResponse(indented);
    }

    /// <summary>
    /// Creates an <see cref="IApiErrorSerializable"/> wrapper for a <see cref="LocationTrackingException"/>.
    /// </summary>
    /// <param name="exception">The location tracking exception to wrap.</param>
    /// <returns>Wrapper implementing <see cref="IApiErrorSerializable"/> interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static IApiErrorSerializable ToApiError(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new LocationTrackingError(exception);
    }

    /// <summary>
    /// Converts a <see cref="LocationTrackingException"/> to a JSON string.
    /// Uses unified exception JSON serialization for backward compatibility.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    [Obsolete("Use ToErrorResponse() for standardized error envelope structure.")]
    public static string ToJson(this LocationTrackingException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return ExceptionJsonExtensions.ToJson(value, includeTypeInfo: true, indented);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="LocationTrackingException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized exception if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    [Obsolete("Use FromJson from ExceptionJsonExtensions instead.")]
    public static LocationTrackingException? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return ExceptionJsonExtensions.FromJson(json) as LocationTrackingException;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="LocationTrackingException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized exception if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false. When false, <paramref name="value"/> will be null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    [Obsolete("Use TryFromJson from ExceptionJsonExtensions instead.")]
    public static bool TryFromJson(string json, out LocationTrackingException? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = ExceptionJsonExtensions.FromJson(json) as LocationTrackingException;
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Implementation of <see cref="IApiErrorSerializable"/> for <see cref="LocationTrackingException"/>.
    /// Provides standardized error envelope structure for API responses.
    /// </summary>
    private sealed class LocationTrackingError : ApiErrorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationTrackingError"/> class.
        /// </summary>
        /// <param name="exception">The location tracking exception to wrap.</param>
        public LocationTrackingError(LocationTrackingException exception) : base(exception) { }

        /// <summary>
        /// Gets the error code for programmatic error handling based on exception type.
        /// </summary>
        public override string GetErrorCode()
        {
            return Exception switch
            {
                VehicleNotFoundException => "RESOURCE_NOT_FOUND",
                AssetNotFoundException => "RESOURCE_NOT_FOUND",
                TrackingSessionNotFoundException => "RESOURCE_NOT_FOUND",
                InvalidLocationException => "INVALID_INPUT",
                _ => "LOCATION_TRACKING_ERROR"
            };
        }

        /// <summary>
        /// Gets additional error details specific to location tracking exceptions.
        /// </summary>
        public override object? GetDetails()
        {
            return Exception switch
            {
                VehicleNotFoundException vehicleEx => new { vehicleId = vehicleEx.VehicleId },
                AssetNotFoundException assetEx => new { assetId = assetEx.AssetId },
                TrackingSessionNotFoundException sessionEx => new { sessionId = sessionEx.SessionId },
                InvalidLocationException locationEx => new { latitude = locationEx.Latitude, longitude = locationEx.Longitude },
                _ => null
            };
        }

        /// <summary>
        /// Gets the HTTP status code based on the specific exception type.
        /// </summary>
        public override int GetHttpStatusCode()
        {
            return Exception switch
            {
                VehicleNotFoundException => 404,
                AssetNotFoundException => 404,
                TrackingSessionNotFoundException => 404,
                InvalidLocationException => 400,
                _ => 500 // Default to server error for unexpected location tracking errors
            };
        }
    }
}