#nullable enable

using System;
using System.Text.Json;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Interface for API error serialization that provides a consistent envelope structure
/// for converting exceptions to standardized error responses.
/// </summary>
public interface IApiErrorSerializable
{
    /// <summary>
    /// Converts the exception to a standardized error response JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string with consistent error envelope structure containing errorCode, message, and details.</returns>
    string ToErrorResponse(bool indented = false);

    /// <summary>
    /// Gets the HTTP status code that should be returned for this error type.
    /// </summary>
    int GetHttpStatusCode();

    /// <summary>
    /// Gets the error code for programmatic error handling.
    /// </summary>
    string GetErrorCode();

    /// <summary>
    /// Gets the base exception message.
    /// </summary>
    string GetMessage();

    /// <summary>
    /// Gets additional error details (e.g., validation errors, field-specific data).
    /// </summary>
    object? GetDetails();
}