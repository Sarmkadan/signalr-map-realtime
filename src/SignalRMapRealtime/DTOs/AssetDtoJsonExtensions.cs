#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.DTOs;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="AssetDto"/>.
/// </summary>
/// <remarks>
/// This static class cannot be inherited.
/// </remarks>
public static class AssetDtoJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    /// <summary>
    /// Serializes the <see cref="AssetDto"/> instance to a JSON string using camelCase property naming.
    /// </summary>
    /// <param name="value">The asset DTO to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the asset DTO in camelCase format.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this AssetDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="AssetDto"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <returns>The deserialized <see cref="AssetDto"/> instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
    public static AssetDto? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AssetDto>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="AssetDto"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <param name="value">Receives the deserialized <see cref="AssetDto"/> instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    /// <remarks>
    /// The <paramref name="value"/> parameter will be set to null if deserialization fails.
    /// </remarks>
    public static bool TryFromJson(string json, out AssetDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = default;

        try
        {
            value = JsonSerializer.Deserialize<AssetDto>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}