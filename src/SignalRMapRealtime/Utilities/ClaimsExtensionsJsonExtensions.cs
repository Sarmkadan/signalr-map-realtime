#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Utilities;

using System.Security.Claims;
using System.Text.Json;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for ClaimsPrincipal
/// using the extension methods from ClaimsExtensions.
/// </summary>
public static class ClaimsExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a ClaimsPrincipal to a JSON string using ClaimsExtensions extension methods.
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ClaimsPrincipal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> is null.</exception>
    public static string ToJson(this ClaimsPrincipal principal, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return JsonSerializer.Serialize(principal, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a ClaimsPrincipal from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A ClaimsPrincipal instance, or null if the JSON is null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ClaimsPrincipal? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ClaimsPrincipal>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a ClaimsPrincipal from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="principal">Receives the deserialized ClaimsPrincipal instance, or null on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string? json, out ClaimsPrincipal? principal)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            principal = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            principal = null;
            return false;
        }
    }
}