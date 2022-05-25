using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SignalRMapRealtime.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="PaginatedResponse{T}"/> types.
/// </summary>
public static class PaginatedResponseJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    private static JsonSerializerOptions GetJsonOptions(bool indented) => indented
        ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
        : _jsonOptions;

    /// <summary>
    /// Serializes a <see cref="PaginatedResponse{T}"/> instance to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated response.</typeparam>
    /// <param name="value">The paginated response to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the paginated response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson<T>(this PaginatedResponse<T> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, GetJsonOptions(indented));
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="PaginatedResponse{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated response.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized paginated response, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PaginatedResponse<T>? FromJson<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<PaginatedResponse<T>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="PaginatedResponse{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated response.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized paginated response if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson<T>(string json, out PaginatedResponse<T>? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<PaginatedResponse<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}