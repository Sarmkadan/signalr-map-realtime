#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides unified JSON serialization for SignalRMapRealtime exceptions.
/// Ensures consistent JSON structure across all exception types in the hierarchy.
/// </summary>
public static class ExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes an exception to a JSON string with a unified structure.
    /// </summary>
    /// <param name="exception">The exception to serialize.</param>
    /// <param name="includeTypeInfo">Whether to include type discriminator for polymorphic deserialization.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static string ToJson(this Exception exception, bool includeTypeInfo = true, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        // Add type discriminator for polymorphic exceptions
        if (includeTypeInfo && exception is not SignalrMapRealtimeException)
        {
            // For non-SignalR exceptions, add type info based on exception type
            var typeName = GetExceptionTypeName(exception);
            var json = JsonSerializer.Serialize(exception, options);
            return json.Insert(json.Length - 1, $",\"$type\":\"{typeName}\"");
        }

        return JsonSerializer.Serialize(exception, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an exception with type discriminator support.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized exception if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    public static Exception? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Check if this is a polymorphic exception with type discriminator
            if (element.TryGetProperty("$type", out var typeProperty))
            {
                var typeName = typeProperty.GetString();
                return typeName switch
                {
                    "ValidationException" => JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions),
                    "ConfigurationException" => JsonSerializer.Deserialize<ConfigurationException>(json, _jsonOptions),
                    "LocationTrackingException" => JsonSerializer.Deserialize<LocationTrackingException>(json, _jsonOptions),
                    "VehicleNotFoundException" => JsonSerializer.Deserialize<VehicleNotFoundException>(json, _jsonOptions),
                    "AssetNotFoundException" => JsonSerializer.Deserialize<AssetNotFoundException>(json, _jsonOptions),
                    "TrackingSessionNotFoundException" => JsonSerializer.Deserialize<TrackingSessionNotFoundException>(json, _jsonOptions),
                    "InvalidLocationException" => JsonSerializer.Deserialize<InvalidLocationException>(json, _jsonOptions),
                    _ => JsonSerializer.Deserialize<Exception>(json, _jsonOptions)
                };
            }

            // Try to deserialize as ValidationException first (has 'errors' property)
            if (element.TryGetProperty("errors", out _))
            {
                return JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
            }

            // Default to base Exception
            return JsonSerializer.Deserialize<Exception>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an exception.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized exception if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out Exception? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets a consistent type name for exception serialization.
    /// </summary>
    private static string GetExceptionTypeName(Exception exception)
    {
        return exception.GetType().Name;
    }
}
