using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ValidationException"/>.
/// Uses unified exception JSON serialization for consistent format across all SignalRMapRealtime exceptions.
/// </summary>
public static class ValidationExceptionJsonExtensions
{
    /// <summary>
    /// Serializes a <see cref="ValidationException"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The validation exception to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the validation exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
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
}