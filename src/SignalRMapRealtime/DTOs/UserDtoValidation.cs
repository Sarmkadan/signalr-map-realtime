#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text.RegularExpressions;

namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="UserDto"/> instances.
/// </summary>
public static class UserDtoValidation
{
    /// <summary>
    /// Validates a <see cref="UserDto"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The user DTO to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UserDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add("Id must be a positive integer.");
        }

        // Validate FullName
        if (string.IsNullOrWhiteSpace(value.FullName))
        {
            errors.Add("FullName cannot be null or whitespace.");
        }
        else if (value.FullName.Length > 200)
        {
            errors.Add("FullName cannot exceed 200 characters.");
        }

        // Validate Email
        if (string.IsNullOrWhiteSpace(value.Email))
        {
            errors.Add("Email cannot be null or whitespace.");
        }
        else if (value.Email.Length > 254)
        {
            errors.Add("Email cannot exceed 254 characters.");
        }
        else if (!IsValidEmail(value.Email))
        {
            errors.Add("Email must be a valid email address.");
        }

        // Validate PhoneNumber (if provided)
        if (value.PhoneNumber is not null)
        {
            if (value.PhoneNumber.Length > 20)
            {
                errors.Add("PhoneNumber cannot exceed 20 characters.");
            }
            else if (!IsValidPhoneNumber(value.PhoneNumber))
            {
                errors.Add("PhoneNumber must be a valid phone number.");
            }
        }

        // Validate EmployeeId (if provided)
        if (value.EmployeeId is not null && value.EmployeeId.Length > 50)
        {
            errors.Add("EmployeeId cannot exceed 50 characters.");
        }

        // Validate JobTitle (if provided)
        if (value.JobTitle is not null && value.JobTitle.Length > 100)
        {
            errors.Add("JobTitle cannot exceed 100 characters.");
        }

        // Validate Department (if provided)
        if (value.Department is not null && value.Department.Length > 100)
        {
            errors.Add("Department cannot exceed 100 characters.");
        }

        // Validate LastLoginAt (if provided)
        if (value.LastLoginAt.HasValue)
        {
            var lastLogin = value.LastLoginAt.Value;
            if (lastLogin > DateTime.UtcNow)
            {
                errors.Add("LastLoginAt cannot be in the future.");
            }
            else if (lastLogin < value.CreatedAt)
            {
                errors.Add("LastLoginAt cannot be before CreatedAt.");
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow)
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="UserDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The user DTO to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UserDto? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="UserDto"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The user DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this UserDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UserDto validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates an email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="email"/> is null.</exception>
    private static bool IsValidEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // RFC 5322 compliant email validation regex
            // Local part: 1-64 chars, supports standard special characters
            // @ symbol
            // Domain: 1-255 chars, supports standard domain characters and structure
            var emailRegex = new Regex(
                @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]{1,64}@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

            return emailRegex.IsMatch(email);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a phone number format using E.164 standard.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if the phone number is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="phoneNumber"/> is null.</exception>
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        // Remove all non-digit characters except leading + for international numbers
        var digitsOnly = new string(phoneNumber
            .Where(c => char.IsDigit(c) || c == '+')
            .ToArray());

        // Basic validation: at least 8 digits, max 15 digits (E.164 standard)
        // Allow leading + followed by 8-15 digits
        if (digitsOnly.StartsWith('+'))
        {
            return digitsOnly.Length is >= 9 and <= 16;
        }

        return digitsOnly.Length is >= 8 and <= 15;
    }
}