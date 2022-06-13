#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Models;

/// <summary>
/// Validation helpers for <see cref="ErrorResponse"/> instances.
/// Provides comprehensive validation for error response data integrity.
/// </summary>
public static class ErrorResponseValidation
{
    /// <summary>
    /// Validates an <see cref="ErrorResponse"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The error response to validate.</param>
    /// <returns>An immutable list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ErrorResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Message
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        // Validate ErrorCode
        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode cannot be null, empty, or whitespace.");
        }

        // Validate Errors dictionary
        if (value.Errors is null)
        {
            problems.Add("Errors dictionary cannot be null.");
        }
        else
        {
            if (value.Errors.Values.Any(errors => errors is null))
            {
                problems.Add("Errors dictionary contains null error arrays.");
            }

            foreach (var kvp in value.Errors)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Error dictionary contains entries with null or empty keys.");
                    break;
                }

                if (kvp.Value is null || kvp.Value.Length == 0)
                {
                    problems.Add($"Error dictionary contains null or empty error array for key '{kvp.Key}'.");
                }
                else
                {
                    foreach (var error in kvp.Value)
                    {
                        if (string.IsNullOrWhiteSpace(error))
                        {
                            problems.Add($"Error dictionary contains empty error message for key '{kvp.Key}'.");
                            break;
                        }
                    }
                }
            }
        }

        // Validate StatusCode
        if (value.StatusCode < 400 || value.StatusCode > 599)
        {
            problems.Add("StatusCode must be a valid HTTP status code (400-599).");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            problems.Add("Timestamp cannot be default (Unix epoch).");
        }
        else if (value.Timestamp.Kind != DateTimeKind.Utc)
        {
            problems.Add("Timestamp must be in UTC kind.");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Timestamp cannot be in the future (beyond 5 minutes tolerance).");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("Timestamp appears to be too old (more than 1 year in the past).");
        }

        // Validate TraceId if present
        if (value.TraceId is not null && string.IsNullOrWhiteSpace(value.TraceId))
        {
            problems.Add("TraceId cannot be whitespace if set.");
        }

        // Validate StackTrace if present
        if (value.StackTrace is not null && string.IsNullOrWhiteSpace(value.StackTrace))
        {
            problems.Add("StackTrace cannot be whitespace if set.");
        }

        // Validate InnerException if present
        if (value.InnerException is not null && string.IsNullOrWhiteSpace(value.InnerException))
        {
            problems.Add("InnerException cannot be whitespace if set.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ErrorResponse"/> is valid.
    /// </summary>
    /// <param name="value">The error response to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ErrorResponse value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ErrorResponse"/> is valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if invalid.
    /// </summary>
    /// <param name="value">The error response to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ErrorResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ErrorResponse is invalid. Problems: {string.Join(" ", problems)}",
            nameof(value));
    }
}