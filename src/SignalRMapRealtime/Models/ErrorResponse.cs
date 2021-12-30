// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Models;

/// <summary>
/// Detailed error response structure for providing comprehensive error information to clients.
/// Includes validation errors, field-level details, and developer-friendly debugging information.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// General error message describing the issue.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code for programmatic handling by clients.
    /// Examples: INVALID_INPUT, RESOURCE_NOT_FOUND, UNAUTHORIZED
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Field-level validation errors mapping property names to error messages.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    /// <summary>
    /// HTTP status code of the error response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp when the error occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Unique request identifier for tracking errors in logs.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Stack trace information (only included in development environment).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception message if applicable.
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// Constructor initializing with default UTC timestamp.
    /// </summary>
    public ErrorResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a validation error response.
    /// </summary>
    public static ErrorResponse ValidationError(
        Dictionary<string, string[]> errors,
        string message = "Validation failed",
        string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "VALIDATION_ERROR",
            Errors = errors,
            StatusCode = 400,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a not found error response.
    /// </summary>
    public static ErrorResponse NotFoundError(string message, string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "NOT_FOUND",
            StatusCode = 404,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create an unauthorized error response.
    /// </summary>
    public static ErrorResponse UnauthorizedError(string message = "Unauthorized", string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "UNAUTHORIZED",
            StatusCode = 401,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a forbidden error response.
    /// </summary>
    public static ErrorResponse ForbiddenError(string message = "Access forbidden", string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "FORBIDDEN",
            StatusCode = 403,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a generic server error response.
    /// </summary>
    public static ErrorResponse ServerError(
        string message = "Internal server error",
        string? traceId = null,
        string? stackTrace = null,
        string? innerException = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "INTERNAL_SERVER_ERROR",
            StatusCode = 500,
            TraceId = traceId,
            StackTrace = stackTrace,
            InnerException = innerException,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a conflict error response (e.g., duplicate resource).
    /// </summary>
    public static ErrorResponse ConflictError(string message, string? traceId = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = "CONFLICT",
            StatusCode = 409,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }
}
