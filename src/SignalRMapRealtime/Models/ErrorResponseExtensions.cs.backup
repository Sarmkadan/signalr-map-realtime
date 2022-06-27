#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;

namespace SignalRMapRealtime.Models;

/// <summary>
/// Extension methods for ErrorResponse providing additional functionality for error handling,
/// formatting, and conversion scenarios.
/// </summary>
public static class ErrorResponseExtensions
{
    /// <summary>
    /// Creates a new ErrorResponse with the same properties but with updated message and timestamp.
    /// Useful for error transformation scenarios.
    /// </summary>
    /// <param name="errorResponse">The source error response</param>
    /// <param name="newMessage">The new error message</param>
    /// <returns>A new ErrorResponse instance with updated properties</returns>
    public static ErrorResponse WithMessage(this ErrorResponse errorResponse, string newMessage)
    {
        if (errorResponse == null)
        {
            throw new ArgumentNullException(nameof(errorResponse));
        }

        return new ErrorResponse
        {
            Message = newMessage,
            ErrorCode = errorResponse.ErrorCode,
            Errors = new Dictionary<string, string[]>(errorResponse.Errors),
            StatusCode = errorResponse.StatusCode,
            Timestamp = DateTime.UtcNow,
            TraceId = errorResponse.TraceId,
            StackTrace = errorResponse.StackTrace,
            InnerException = errorResponse.InnerException
        };
    }

    /// <summary>
    /// Creates a new ErrorResponse with the same properties but with updated status code.
    /// Useful for error escalation scenarios.
    /// </summary>
    /// <param name="errorResponse">The source error response</param>
    /// <param name="newStatusCode">The new HTTP status code</param>
    /// <returns>A new ErrorResponse instance with updated status code</returns>
    public static ErrorResponse WithStatusCode(this ErrorResponse errorResponse, int newStatusCode)
    {
        if (errorResponse == null)
        {
            throw new ArgumentNullException(nameof(errorResponse));
        }

        return new ErrorResponse
        {
            Message = errorResponse.Message,
            ErrorCode = errorResponse.ErrorCode,
            Errors = new Dictionary<string, string[]>(errorResponse.Errors),
            StatusCode = newStatusCode,
            Timestamp = DateTime.UtcNow,
            TraceId = errorResponse.TraceId,
            StackTrace = errorResponse.StackTrace,
            InnerException = errorResponse.InnerException
        };
    }

    /// <summary>
    /// Formats the error response as a JSON string suitable for API responses.
    /// Includes all relevant fields in a structured format.
    /// </summary>
    /// <param name="errorResponse">The error response to format</param>
    /// <param name="includeStackTrace">Whether to include stack trace in output (defaults to false in production)</param>
    /// <returns>Formatted JSON string</returns>
    public static string ToJson(this ErrorResponse errorResponse, bool includeStackTrace = false)
    {
        if (errorResponse == null)
        {
            throw new ArgumentNullException(nameof(errorResponse));
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        var result = new Dictionary<string, object>
        {
            ["message"] = errorResponse.Message,
            ["errorCode"] = errorResponse.ErrorCode,
            ["statusCode"] = errorResponse.StatusCode,
            ["timestamp"] = errorResponse.Timestamp.ToString("o"),
            ["traceId"] = errorResponse.TraceId ?? string.Empty
        };

        if (errorResponse.Errors != null && errorResponse.Errors.Count > 0)
        {
            result["errors"] = errorResponse.Errors;
        }

        if (includeStackTrace && !string.IsNullOrEmpty(errorResponse.StackTrace))
        {
            result["stackTrace"] = errorResponse.StackTrace.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        if (!string.IsNullOrEmpty(errorResponse.InnerException))
        {
            result["innerException"] = errorResponse.InnerException;
        }

        return System.Text.Json.JsonSerializer.Serialize(result, options);
    }

    /// <summary>
    /// Creates a human-readable error message combining all error details.
    /// Useful for logging and user-friendly error displays.
    /// </summary>
    /// <param name="errorResponse">The error response to format</param>
    /// <returns>Formatted error message string</returns>
    public static string ToFriendlyMessage(this ErrorResponse errorResponse)
    {
        if (errorResponse == null)
        {
            throw new ArgumentNullException(nameof(errorResponse));
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Error: {errorResponse.Message}");
        builder.AppendLine($"Code: {errorResponse.ErrorCode} (Status: {errorResponse.StatusCode})");

        if (!string.IsNullOrEmpty(errorResponse.TraceId))
        {
            builder.AppendLine($"Trace ID: {errorResponse.TraceId}");
        }

        if (errorResponse.Errors != null && errorResponse.Errors.Count > 0)
        {
            builder.AppendLine("Validation Errors:");
            foreach (var fieldError in errorResponse.Errors)
            {
                builder.AppendLine($"  {fieldError.Key}:");
                foreach (var error in fieldError.Value)
                {
                    builder.AppendLine($"    - {error}");
                }
            }
        }

        if (!string.IsNullOrEmpty(errorResponse.InnerException))
        {
            builder.AppendLine($"Inner Exception: {errorResponse.InnerException}");
        }

        if (!string.IsNullOrEmpty(errorResponse.StackTrace))
        {
            builder.AppendLine("Stack Trace:");
            foreach (var line in errorResponse.StackTrace.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                builder.AppendLine(line);
            }
        }

        return builder.ToString().Trim();
    }
}