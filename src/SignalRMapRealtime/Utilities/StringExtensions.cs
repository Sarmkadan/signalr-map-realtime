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
    /// Checks if a string is null, empty, or contains only whitespace.
    /// More intuitive than string.IsNullOrWhiteSpace for fluent APIs.
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Checks if a string is NOT null, empty, or whitespace.
    /// Logical inverse of IsNullOrEmpty.
    /// </summary>
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Truncates a string to a maximum length and optionally appends ellipsis.
    /// Useful for display purposes and log messages.
    /// </summary>
    public static string Truncate(this string? value, int maxLength, bool addEllipsis = true)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        if (value!.Length <= maxLength) return value;

        var suffix = addEllipsis ? "..." : string.Empty;
        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Converts a string to title case (e.g., "hello world" -> "Hello World").
    /// Handles null/empty strings gracefully.
    /// </summary>
    public static string ToTitleCase(this string? value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value!.ToLower());
    }

    /// <summary>
    /// Converts a string to kebab-case (e.g., "HelloWorld" -> "hello-world").
    /// Useful for URL slugs and identifiers.
    /// </summary>
    public static string ToKebabCase(this string? value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            value!,
            "(?<!^)(?=[A-Z])",
            "-"
        ).ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "HelloWorld" -> "hello_world").
    /// Common in database columns and configuration keys.
    /// </summary>
    public static string ToSnakeCase(this string? value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            value!,
            "(?<!^)(?=[A-Z])",
            "_"
        ).ToLowerInvariant();
    }

    /// <summary>
    /// Safely gets a substring without throwing exceptions on invalid indices.
    /// Returns empty string if indices are out of range.
    /// </summary>
    public static string SubstringSafe(this string? value, int startIndex, int length)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        if (startIndex < 0 || startIndex >= value!.Length) return string.Empty;

        var availableLength = value.Length - startIndex;
        var actualLength = Math.Min(length, availableLength);

        return value.Substring(startIndex, actualLength);
    }

    /// <summary>
    /// Removes all occurrences of any character in a given set from a string.
    /// Useful for sanitization (e.g., removing special characters).
    /// </summary>
    public static string RemoveCharacters(this string? value, params char[] charsToRemove)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        return new string(value!.Where(c => !charsToRemove.Contains(c)).ToArray());
    }

    /// <summary>
    /// Counts occurrences of a substring in a string.
    /// Case-sensitive by default.
    /// </summary>
    public static int CountOccurrences(this string? value, string substring)
    {
        if (value.IsNullOrEmpty() || substring.IsNullOrEmpty()) return 0;

        return (value!.Length - value.Replace(substring, string.Empty).Length) / substring!.Length;
    }

    /// <summary>
    /// Reverses the order of characters in a string.
    /// </summary>
    public static string Reverse(this string? value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        return new string(value!.Reverse().ToArray());
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// Returns empty string if count is less than 1.
    /// </summary>
    public static string Repeat(this string? value, int count)
    {
        if (value.IsNullOrEmpty() || count < 1) return string.Empty;

        return string.Concat(Enumerable.Repeat(value, count));
    }

    /// <summary>
    /// Validates if a string matches a regex pattern.
    /// </summary>
    public static bool Matches(this string? value, string pattern)
    {
        if (value.IsNullOrEmpty()) return false;

        return System.Text.RegularExpressions.Regex.IsMatch(value!, pattern);
    }

    /// <summary>
    /// Masks a string by replacing middle characters with a mask character.
    /// Useful for securely displaying sensitive data (e.g., email addresses, phone numbers).
    /// </summary>
    public static string Mask(this string? value, char maskChar = '*', int visibleStart = 2, int visibleEnd = 2)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        if (value!.Length <= visibleStart + visibleEnd) return value;

        var start = value.Substring(0, visibleStart);
        var end = value.Substring(value.Length - visibleEnd);
        var mask = new string(maskChar, value.Length - visibleStart - visibleEnd);

        return $"{start}{mask}{end}";
    }
}
