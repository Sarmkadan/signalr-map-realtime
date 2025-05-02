// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Middleware;

using System.Net;
using System.Text.Json;
using SignalRMapRealtime.Models;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions and converts them to appropriate HTTP responses.
/// Ensures consistent error response format across the API.
/// In production, provides safe error messages without exposing internal details.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes the error handling middleware.
    /// </summary>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Intercepts all requests and catches any unhandled exceptions.
    /// Converts exceptions to appropriate HTTP error responses.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles an exception and writes an appropriate error response.
    /// Different responses for development vs production environments.
    /// </summary>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ArgumentNullException ex => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                $"Required parameter missing: {ex.ParamName}",
                "ARGUMENT_NULL",
                ex),

            ArgumentException ex => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                $"Invalid argument: {ex.Message}",
                "ARGUMENT_INVALID",
                ex),

            InvalidOperationException ex => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "The requested operation is not valid in the current state",
                "INVALID_OPERATION",
                ex),

            KeyNotFoundException ex => CreateErrorResponse(
                HttpStatusCode.NotFound,
                "The requested resource was not found",
                "RESOURCE_NOT_FOUND",
                ex),

            UnauthorizedAccessException ex => CreateErrorResponse(
                HttpStatusCode.Unauthorized,
                "You do not have permission to access this resource",
                "UNAUTHORIZED",
                ex),

            NotImplementedException ex => CreateErrorResponse(
                HttpStatusCode.NotImplemented,
                "This feature is not implemented yet",
                "NOT_IMPLEMENTED",
                ex),

            _ => CreateErrorResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.",
                "INTERNAL_SERVER_ERROR",
                exception)
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }

    /// <summary>
    /// Creates an error response object with appropriate level of detail.
    /// In development, includes stack trace; in production, only safe messages.
    /// </summary>
    private ErrorResponse CreateErrorResponse(HttpStatusCode statusCode, string message, string errorCode, Exception exception)
    {
        var response = new ErrorResponse
        {
            Message = message,
            ErrorCode = errorCode,
            StatusCode = (int)statusCode,
            Timestamp = DateTime.UtcNow
        };

        // Include detailed error information in development environment only
        if (_environment.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
            response.InnerException = exception.InnerException?.Message;
        }

        return response;
    }
}

/// <summary>
/// Extension methods for registering error handling middleware.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds error handling middleware to the pipeline.
    /// Should be registered early in the middleware chain to catch all exceptions.
    /// </summary>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
