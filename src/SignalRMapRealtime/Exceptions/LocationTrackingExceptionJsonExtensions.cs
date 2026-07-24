#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="LocationTrackingException"/> and its derived types.
/// Uses unified exception JSON serialization for consistent format across all SignalRMapRealtime exceptions.
/// </summary>
public static class LocationTrackingExceptionJsonExtensions
{
    /// <summary>
    /// Converts a <see cref="LocationTrackingException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
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
}