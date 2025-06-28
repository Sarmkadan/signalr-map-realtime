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
    public static bool IsValidEmail(this string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

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
    public static bool IsValidPhoneNumber(this string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

        // Remove common separators and whitespace
        var cleaned = Regex.Replace(phoneNumber, @"[\s\-().]", "");

        // Check if it's between 10-15 digits with optional leading +
        if (cleaned.StartsWith("+"))
            cleaned = cleaned.Substring(1);

        return cleaned.Length >= 10 && cleaned.Length <= 15 && Regex.IsMatch(cleaned, @"^\d+$");
    }

    /// <summary>
    /// Validates if a string is a valid URL.
    /// </summary>
    public static bool IsValidUrl(this string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates if a string is a valid IPv4 address.
    /// </summary>
    public static bool IsValidIpAddress(this string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return false;

        return System.Net.IPAddress.TryParse(ipAddress, out var addr) &&
               addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    /// <summary>
    /// Validates if a string is a valid GUID/UUID.
    /// </summary>
    public static bool IsValidGuid(this string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid)) return false;

        return Guid.TryParse(guid, out _);
    }

    /// <summary>
    /// Validates if a string contains only alphanumeric characters.
    /// </summary>
    public static bool IsAlphanumeric(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$");
    }

    /// <summary>
    /// Validates if a string is a strong password.
    /// Requirements: at least 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char.
    /// </summary>
    public static bool IsStrongPassword(this string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password!.Length < 8) return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}\[\]|<>;']");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// Validates if a numeric value is within a specified range (inclusive).
    /// </summary>
    public static bool IsInRange<T>(this T value, T min, T max) where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    /// <summary>
    /// Validates if a string length is within a specified range.
    /// </summary>
    public static bool IsLengthInRange(this string? value, int minLength, int maxLength)
    {
        if (value == null) return minLength <= 0;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    /// <summary>
    /// Validates if a collection contains any elements.
    /// </summary>
    public static bool HasElements<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() ?? false;
    }

    /// <summary>
    /// Validates if a collection has exactly the specified count of elements.
    /// </summary>
    public static bool HasExactly<T>(this IEnumerable<T>? collection, int count)
    {
        return collection?.Count() == count;
    }

    /// <summary>
    /// Validates if a collection has at least the specified count of elements.
    /// </summary>
    public static bool HasAtLeast<T>(this IEnumerable<T>? collection, int count)
    {
        return (collection?.Count() ?? 0) >= count;
    }

    /// <summary>
    /// Validates if a numeric value is positive (greater than zero).
    /// </summary>
    public static bool IsPositive<T>(this T value) where T : IComparable<T>
    {
        return value.CompareTo(default!) > 0;
    }

    /// <summary>
    /// Validates if a numeric value is negative (less than zero).
    /// </summary>
    public static bool IsNegative<T>(this T value) where T : IComparable<T>
    {
        return value.CompareTo(default!) < 0;
    }

    /// <summary>
    /// Validates if a double value is Not a Number (NaN).
    /// </summary>
    public static bool IsNaN(this double value)
    {
        return double.IsNaN(value);
    }

    /// <summary>
    /// Validates if a double value is infinite.
    /// </summary>
    public static bool IsInfinite(this double value)
    {
        return double.IsInfinity(value);
    }

    /// <summary>
    /// Validates if a decimal value is between 0 and 100 (percentage).
    /// </summary>
    public static bool IsValidPercentage(this decimal value)
    {
        return value >= 0 && value <= 100;
    }

    /// <summary>
    /// Validates if a string matches a specific format pattern.
    /// </summary>
    public static bool MatchesPattern(this string? value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

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
