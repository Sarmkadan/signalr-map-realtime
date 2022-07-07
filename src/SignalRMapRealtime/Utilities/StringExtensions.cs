#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

/// <summary>
/// Extension methods for string manipulation and validation.
/// Provides utilities for common string operations used throughout the application.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if a string is NOT null, empty, or whitespace.
    /// Logical inverse of <see cref="string.IsNullOrWhiteSpace"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Truncates a string to a maximum length and optionally appends ellipsis.
    /// Useful for display purposes and log messages.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum length of the resulting string.</param>
    /// <param name="addEllipsis">Whether to append ellipsis if truncation occurs.</param>
    /// <returns>The truncated string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string? value, int maxLength, bool addEllipsis = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        var suffix = addEllipsis ? "..." : string.Empty;
        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Converts a string to title case (e.g., "hello world" -> "Hello World").
    /// Handles null/empty strings gracefully.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The title-cased string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    public static string ToTitleCase(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    /// <summary>
    /// Converts a string to kebab-case (e.g., "HelloWorld" -> "hello-world").
    /// Useful for URL slugs and identifiers.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The kebab-cased string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    public static string ToKebabCase(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            value,
            "(?<!^)(?=[A-Z])",
            "-"
        ).ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "HelloWorld" -> "hello_world").
    /// Common in database columns and configuration keys.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The snake-cased string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    public static string ToSnakeCase(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            value,
            "(?<!^)(?=[A-Z])",
            "_"
        ).ToLowerInvariant();
    }

    /// <summary>
    /// Safely gets a substring without throwing exceptions on invalid indices.
    /// Returns empty string if indices are out of range.
    /// </summary>
    /// <param name="value">The source string.</param>
    /// <param name="startIndex">The starting index.</param>
    /// <param name="length">The length of substring to extract.</param>
    /// <returns>The substring, or empty string if input is null, whitespace, or indices are invalid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> or <paramref name="length"/> is negative.</exception>
    public static string SubstringSafe(this string? value, int startIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        if (startIndex >= value.Length) return string.Empty;

        var availableLength = value.Length - startIndex;
        var actualLength = Math.Min(length, availableLength);

        return value.Substring(startIndex, actualLength);
    }

    /// <summary>
    /// Removes all occurrences of any character in a given set from a string.
    /// Useful for sanitization (e.g., removing special characters).
    /// </summary>
    /// <param name="value">The source string.</param>
    /// <param name="charsToRemove">Characters to remove.</param>
    /// <returns>The sanitized string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="charsToRemove"/> is null.
    /// Thrown when the input is null.
    /// </exception>
    public static string RemoveCharacters(this string? value, params char[] charsToRemove)
    {
        ArgumentNullException.ThrowIfNull(charsToRemove);

        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return new string(value.Where(c => !charsToRemove.Contains(c)).ToArray());
    }

    /// <summary>
    /// Counts occurrences of a substring in a string.
    /// Case-sensitive by default.
    /// </summary>
    /// <param name="value">The source string.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>The number of occurrences, or 0 if either input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="substring"/> is null.</exception>
    public static int CountOccurrences(this string? value, string substring)
    {
        ArgumentNullException.ThrowIfNull(substring);

        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(substring)) return 0;

        return (value.Length - value.Replace(substring, string.Empty).Length) / substring.Length;
    }

    /// <summary>
    /// Reverses the order of characters in a string.
    /// </summary>
    /// <param name="value">The string to reverse.</param>
    /// <returns>The reversed string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    public static string Reverse(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return new string(value.Reverse().ToArray());
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// Returns empty string if count is less than 1.
    /// </summary>
    /// <param name="value">The string to repeat.</param>
    /// <param name="count">Number of repetitions.</param>
    /// <returns>The repeated string, or empty string if input is null, whitespace, or count is less than 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    public static string Repeat(this string? value, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (string.IsNullOrWhiteSpace(value) || count < 1) return string.Empty;

        return string.Concat(Enumerable.Repeat(value, count));
    }

    /// <summary>
    /// Validates if a string matches a regex pattern.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">The regex pattern to match against.</param>
    /// <returns>True if the string matches the pattern; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="pattern"/> is null.
    /// Thrown when the input is null.
    /// </exception>
    public static bool Matches(this string? value, string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (string.IsNullOrWhiteSpace(value)) return false;

        return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
    }

    /// <summary>
    /// Masks a string by replacing middle characters with a mask character.
    /// Useful for securely displaying sensitive data (e.g., email addresses, phone numbers).
    /// </summary>
    /// <param name="value">The string to mask.</param>
    /// <param name="maskChar">The character to use for masking. Defaults to '*'.</param>
    /// <param name="visibleStart">Number of characters to leave visible at the start. Defaults to 2.</param>
    /// <param name="visibleEnd">Number of characters to leave visible at the end. Defaults to 2.</param>
    /// <returns>The masked string, or empty string if input is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="visibleStart"/> is negative.
    /// Thrown when <paramref name="visibleEnd"/> is negative.
    /// Thrown when the sum of <paramref name="visibleStart"/> and <paramref name="visibleEnd"/>
    /// is greater than or equal to the length of the input string.
    /// </exception>
    public static string Mask(
        this string? value,
        char maskChar = '*',
        int visibleStart = 2,
        int visibleEnd = 2)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(visibleStart);
        ArgumentOutOfRangeException.ThrowIfNegative(visibleEnd);

        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        if (value.Length <= visibleStart + visibleEnd) return value;

        var start = value.Substring(0, visibleStart);
        var end = value.Substring(value.Length - visibleEnd);
        var mask = new string(maskChar, value.Length - visibleStart - visibleEnd);

        return $"{start}{mask}{end}";
    }
}
