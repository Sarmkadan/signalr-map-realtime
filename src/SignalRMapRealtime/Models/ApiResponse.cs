// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Models;

/// <summary>
/// Standard API response wrapper for consistent response formatting across all endpoints.
/// Allows clients to parse responses with a predictable structure.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the API operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data payload. Null if operation failed or no data to return.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Human-readable message about the operation result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp when the response was generated (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Unique request identifier for tracking and debugging purposes.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Initializes a new instance of the ApiResponse class with default UTC timestamp.
    /// </summary>
    public ApiResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a successful response.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful", int statusCode = 200, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a failed response.
    /// </summary>
    public static ApiResponse<T> FailureResponse(string message, int statusCode = 400, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            StatusCode = statusCode,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Non-generic version of API response for operations that don't return data.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates whether the API operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message about the operation result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp when the response was generated (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Unique request identifier for tracking and debugging purposes.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Initializes a new instance of the ApiResponse class with default UTC timestamp.
    /// </summary>
    public ApiResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a successful response.
    /// </summary>
    public static ApiResponse SuccessResponse(string message = "Operation successful", int statusCode = 200, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            StatusCode = statusCode,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a failed response.
    /// </summary>
    public static ApiResponse FailureResponse(string message, int statusCode = 400, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };
    }
}
