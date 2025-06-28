// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Middleware;

using System.Text;

/// <summary>
/// Middleware for comprehensive HTTP request/response logging.
/// Logs request details (method, path, headers, body) and response details (status, body, timing).
/// Helps with debugging and monitoring API usage patterns.
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    /// <summary>
    /// Initializes the logging middleware with the next middleware in the pipeline.
    /// </summary>
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes HTTP requests and responses with comprehensive logging.
    /// Captures request/response body, timing, and headers for debugging.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestStartTime = DateTime.UtcNow;
        var traceId = context.TraceIdentifier;

        // Skip logging for health check endpoints to reduce noise
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Log incoming request
        await LogRequest(context, traceId);

        // Store original response stream to capture response body
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Log successful response
                var duration = (DateTime.UtcNow - requestStartTime).TotalMilliseconds;
                await LogResponse(context, responseBody, traceId, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in request pipeline. TraceId: {TraceId}", traceId);
                throw;
            }
            finally
            {
                // Copy response body to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    /// <summary>
    /// Logs incoming HTTP request details including headers and body.
    /// </summary>
    private async Task LogRequest(HttpContext context, string traceId)
    {
        var request = context.Request;
        var requestBody = await ReadRequestBody(request);

        var logMessage = new StringBuilder();
        logMessage.AppendLine($"[HTTP REQUEST] TraceId: {traceId}");
        logMessage.AppendLine($"  Method: {request.Method} {request.Path}{request.QueryString}");
        logMessage.AppendLine($"  Host: {request.Host}");
        logMessage.AppendLine($"  ContentType: {request.ContentType}");
        logMessage.AppendLine($"  ContentLength: {request.ContentLength}");

        // Log important headers (skip sensitive ones like Authorization)
        foreach (var header in request.Headers)
        {
            if (!ShouldHideHeader(header.Key))
            {
                logMessage.AppendLine($"  {header.Key}: {header.Value}");
            }
        }

        // Log request body if it's JSON or form data
        if (!string.IsNullOrEmpty(requestBody) && IsSafeToLogBody(request.ContentType))
        {
            var truncatedBody = requestBody.Length > 500
                ? requestBody.Substring(0, 500) + "..."
                : requestBody;
            logMessage.AppendLine($"  Body: {truncatedBody}");
        }

        _logger.LogInformation(logMessage.ToString());
    }

    /// <summary>
    /// Logs HTTP response details including status code and response body.
    /// </summary>
    private async Task LogResponse(HttpContext context, MemoryStream responseBody, string traceId, double durationMs)
    {
        var response = context.Response;
        responseBody.Position = 0;
        var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Position = 0;

        var logMessage = new StringBuilder();
        logMessage.AppendLine($"[HTTP RESPONSE] TraceId: {traceId}");
        logMessage.AppendLine($"  StatusCode: {response.StatusCode}");
        logMessage.AppendLine($"  ContentType: {response.ContentType}");
        logMessage.AppendLine($"  ContentLength: {response.ContentLength}");
        logMessage.AppendLine($"  Duration: {durationMs:F2}ms");

        // Log response body if it's small enough and JSON
        if (!string.IsNullOrEmpty(responseBodyText) && IsSafeToLogBody(response.ContentType))
        {
            var truncatedBody = responseBodyText.Length > 500
                ? responseBodyText.Substring(0, 500) + "..."
                : responseBodyText;
            logMessage.AppendLine($"  Body: {truncatedBody}");
        }

        // Log as warning if response indicates an error
        if (response.StatusCode >= 400)
        {
            _logger.LogWarning(logMessage.ToString());
        }
        else
        {
            _logger.LogInformation(logMessage.ToString());
        }
    }

    /// <summary>
    /// Reads the request body from the stream without losing it for downstream processing.
    /// Resets the stream position after reading.
    /// </summary>
    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.ContentLength == 0 || request.Body == null)
            return string.Empty;

        request.EnableBuffering();
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    /// <summary>
    /// Determines if a request path should be skipped from logging (e.g., health checks).
    /// </summary>
    private bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return pathValue.StartsWith("/health") || pathValue.StartsWith("/swagger");
    }

    /// <summary>
    /// Determines if a header should be logged (skips sensitive headers).
    /// </summary>
    private bool ShouldHideHeader(string headerName)
    {
        var sensitiveHeaders = new[] { "Authorization", "Cookie", "Set-Cookie", "X-API-Key" };
        return sensitiveHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if it's safe to log the response/request body based on content type.
    /// Only logs text-based content types to avoid logging binary data.
    /// </summary>
    private bool IsSafeToLogBody(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return true;

        var safeTypes = new[] { "application/json", "application/x-www-form-urlencoded", "text/plain" };
        return safeTypes.Any(t => contentType.Contains(t, StringComparison.OrdinalIgnoreCase));
    }
}
