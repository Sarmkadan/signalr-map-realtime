#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

using Microsoft.Net.Http.Headers;

/// <summary>
/// Extension methods that convert database row versions to and from HTTP entity tags.
/// </summary>
public static class RowVersionExtensions
{
    /// <summary>
    /// Converts a row version into a strong entity tag: the base64 form of the bytes, wrapped in double quotes.
    /// </summary>
    /// <param name="rowVersion">The row version bytes to convert.</param>
    /// <returns>A strong ETag value suitable for the ETag and If-Match headers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rowVersion"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="rowVersion"/> is empty.</exception>
    public static string ToETag(this byte[] rowVersion)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);
        if (rowVersion.Length == 0)
            throw new ArgumentException("Row version must not be empty.", nameof(rowVersion));

        return $"\"{Convert.ToBase64String(rowVersion)}\"";
    }

    /// <summary>
    /// Parses a single strong entity tag produced by <see cref="ToETag"/> back into row version bytes.
    /// Weak tags, lists of tags and values that are not valid base64 are rejected.
    /// </summary>
    /// <param name="etag">The entity tag header value to parse.</param>
    /// <param name="rowVersion">When this method returns true, the decoded row version bytes; otherwise an empty array.</param>
    /// <returns>True if <paramref name="etag"/> is a single strong entity tag with a valid base64 payload; otherwise false.</returns>
    public static bool TryParseETag(string? etag, out byte[] rowVersion)
    {
        rowVersion = [];

        if (!EntityTagHeaderValue.TryParse(etag, out var entityTag) || entityTag is null || entityTag.IsWeak)
            return false;

        var payload = entityTag.Tag.Value?.Trim('"');
        if (string.IsNullOrEmpty(payload))
            return false;

        var buffer = new byte[payload.Length];
        if (!Convert.TryFromBase64String(payload, buffer, out var written) || written == 0)
            return false;

        rowVersion = buffer[..written];
        return true;
    }
}
