using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides standardized API error serialization for <see cref="ValidationException"/>.
/// Implements <see cref="IApiErrorSerializable"/> to ensure consistent envelope structure
/// across all exception types in the SignalRMapRealtime system.
/// </summary>
public static class ValidationExceptionJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="ValidationException"/> to a standardized error response JSON string
    /// with consistent envelope structure (errorCode, message, details, statusCode).
    /// </summary>
    /// <param name="value">The validation exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string with consistent error envelope structure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToErrorResponse(this ValidationException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var error = new ValidationError(value);
        return error.ToErrorResponse(indented);
    }

    /// <summary>
    /// Creates an <see cref="IApiErrorSerializable"/> wrapper for a <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="exception">The validation exception to wrap.</param>
    /// <returns>Wrapper implementing <see cref="IApiErrorSerializable"/> interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static IApiErrorSerializable ToApiError(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new ValidationError(exception);
    }

    /// <summary>
    /// Serializes a <see cref="ValidationException"/> instance to a JSON string.
    /// Uses unified exception JSON serialization for backward compatibility.
    /// </summary>
    /// <param name="value">The validation exception to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the validation exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    [Obsolete("Use ToErrorResponse() for standardized error envelope structure.")]
    public static string ToJson(this ValidationException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return ExceptionJsonExtensions.ToJson(value, includeTypeInfo: true, indented);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="ValidationException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized validation exception, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    [Obsolete("Use FromJson from ExceptionJsonExtensions instead.")]
    public static ValidationException? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return ExceptionJsonExtensions.FromJson(json) as ValidationException;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="ValidationException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized validation exception if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    [Obsolete("Use TryFromJson from ExceptionJsonExtensions instead.")]
    public static bool TryFromJson(string? json, out ValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = FromJson(json);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Implementation of <see cref="IApiErrorSerializable"/> for <see cref="ValidationException"/>.
    /// Provides standardized error envelope structure for API responses.
    /// </summary>
    private sealed class ValidationError : ApiErrorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="exception">The validation exception to wrap.</param>
        public ValidationError(ValidationException exception) : base(exception) { }

        /// <summary>
        /// Gets the error code for programmatic error handling.
        /// </summary>
        public override string GetErrorCode()
        {
            return "VALIDATION_ERROR";
        }

        /// <summary>
        /// Gets additional error details including validation errors.
        /// </summary>
        public override object? GetDetails()
        {
            var validationException = Exception as ValidationException;
            if (validationException?.Errors != null && validationException.Errors.Any())
            {
                return new Dictionary<string, string[]>
                {
                    ["validationErrors"] = validationException.Errors.ToArray()
                };
            }

            return null;
        }

        /// <summary>
        /// Gets the HTTP status code for validation errors (always 400 Bad Request).
        /// </summary>
        public override int GetHttpStatusCode()
        {
            return 400; // Validation errors are always 400 Bad Request
        }
    }
}