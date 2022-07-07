#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

using System.Text.RegularExpressions;

/// <summary>
/// Extension methods for data validation.
/// Provides common validation checks for emails, phone numbers, URLs, and other data types.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates if a string is a valid email address.
    /// Uses a practical regex pattern that covers most real-world cases.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
    public static bool IsValidEmail(this string? email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is a valid phone number (basic format).
    /// Accepts formats: +1234567890, 1234567890, (123) 456-7890, etc.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if the phone number is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="phoneNumber"/> is null.</exception>
    public static bool IsValidPhoneNumber(this string? phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove common separators and whitespace
        var cleaned = Regex.Replace(phoneNumber, @"[\s\-().]", "");

        // Check if it's between 10-15 digits with optional leading +
        if (cleaned.StartsWith("+"))
            cleaned = cleaned[1..];

        return cleaned.Length >= 10 && cleaned.Length <= 15 && Regex.IsMatch(cleaned, @"^\d+$");
    }

    /// <summary>
    /// Validates if a string is a valid URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is null.</exception>
    public static bool IsValidUrl(this string? url)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates if a string is a valid IPv4 address.
    /// </summary>
    /// <param name="ipAddress">The IP address to validate.</param>
    /// <returns>True if the IP address is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ipAddress"/> is null.</exception>
    public static bool IsValidIpAddress(this string? ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return System.Net.IPAddress.TryParse(ipAddress, out var addr) &&
            addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    /// <summary>
    /// Validates if a string is a valid GUID/UUID.
    /// </summary>
    /// <param name="guid">The GUID to validate.</param>
    /// <returns>True if the GUID is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="guid"/> is null.</exception>
    public static bool IsValidGuid(this string? guid)
    {
        ArgumentNullException.ThrowIfNull(guid);

        if (string.IsNullOrWhiteSpace(guid))
            return false;

        return Guid.TryParse(guid, out _);
    }

    /// <summary>
    /// Validates if a string contains only alphanumeric characters.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string contains only alphanumeric characters; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsAlphanumeric(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$");
    }

    /// <summary>
    /// Validates if a string is a strong password.
    /// Requirements: at least 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password meets strength requirements; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
    public static bool IsStrongPassword(this string? password)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}\[\]|<>;']");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// Validates if a numeric value is within a specified range (inclusive).
    /// </summary>
    /// <typeparam name="T">The type of value being compared.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>True if the value is within range; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="min"/> or <paramref name="max"/> is null.</exception>
    public static bool IsInRange<T>(this T value, T min, T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    /// <summary>
    /// Validates if a string length is within a specified range.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="minLength">The minimum length (inclusive).</param>
    /// <param name="maxLength">The maximum length (inclusive).</param>
    /// <returns>True if the string length is within range; otherwise, false.</returns>
    public static bool IsLengthInRange(this string? value, int minLength, int maxLength)
    {
        if (value is null)
            return minLength <= 0;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    /// <summary>
    /// Validates if a collection contains any elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <returns>True if the collection contains elements; otherwise, false.</returns>
    public static bool HasElements<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() ?? false;
    }

    /// <summary>
    /// Validates if a collection has exactly the specified count of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="count">The exact count to match.</param>
    /// <returns>True if the collection has exactly the specified count; otherwise, false.</returns>
    public static bool HasExactly<T>(this IEnumerable<T>? collection, int count)
    {
        return collection?.Count() == count;
    }

    /// <summary>
    /// Validates if a collection has at least the specified count of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="count">The minimum count to match.</param>
    /// <returns>True if the collection has at least the specified count; otherwise, false.</returns>
    public static bool HasAtLeast<T>(this IEnumerable<T>? collection, int count)
    {
        return (collection?.Count() ?? 0) >= count;
    }

    /// <summary>
    /// Validates if a numeric value is positive (greater than zero).
    /// </summary>
    /// <typeparam name="T">The type of numeric value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is positive; otherwise, false.</returns>
    public static bool IsPositive<T>(this T value) where T : IComparable<T>
    {
        return value.CompareTo(default!) > 0;
    }

    /// <summary>
    /// Validates if a numeric value is negative (less than zero).
    /// </summary>
    /// <typeparam name="T">The type of numeric value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is negative; otherwise, false.</returns>
    public static bool IsNegative<T>(this T value) where T : IComparable<T>
    {
        return value.CompareTo(default!) < 0;
    }

    /// <summary>
    /// Validates if a double value is Not a Number (NaN).
    /// </summary>
    /// <param name="value">The double value to validate.</param>
    /// <returns>True if the value is NaN; otherwise, false.</returns>
    public static bool IsNaN(this double value)
    {
        return double.IsNaN(value);
    }

    /// <summary>
    /// Validates if a double value is infinite.
    /// </summary>
    /// <param name="value">The double value to validate.</param>
    /// <returns>True if the value is infinite; otherwise, false.</returns>
    public static bool IsInfinite(this double value)
    {
        return double.IsInfinity(value);
    }

    /// <summary>
    /// Validates if a decimal value is between 0 and 100 (percentage).
    /// </summary>
    /// <param name="value">The decimal value to validate.</param>
    /// <returns>True if the value is a valid percentage; otherwise, false.</returns>
    public static bool IsValidPercentage(this decimal value)
    {
        return value >= 0 && value <= 100;
    }

    /// <summary>
    /// Validates if a string matches a specific format pattern.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">The regex pattern to match against.</param>
    /// <returns>True if the string matches the pattern; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is null.</exception>
    public static bool MatchesPattern(this string? value, string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            return Regex.IsMatch(value, pattern, RegexOptions.None);
        }
        catch
        {
            return false;
        }
    }
}
