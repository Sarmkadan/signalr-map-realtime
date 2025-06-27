// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Middleware;

using System.Diagnostics;

/// <summary>
/// Middleware for monitoring API performance metrics.
/// Tracks request duration, response size, and identifies slow endpoints.
/// Logs performance warnings for endpoints that exceed expected thresholds.
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    /// <summary>
    /// Threshold in milliseconds for warning about slow requests.
    /// </summary>
    private const int SlowRequestThresholdMs = 1000;

    /// <summary>
    /// Initializes the performance monitoring middleware.
    /// </summary>
    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Monitors the performance of HTTP requests and responses.
    /// Records timing information and logs performance metrics.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Record performance metrics
                var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                var responseSize = responseBody.Length;
                var statusCode = context.Response.StatusCode;

                // Log performance information
                LogPerformanceMetrics(context, elapsedMs, responseSize, statusCode);

                // Check for slow requests
                if (elapsedMs > SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} completed in {ElapsedMs:F2}ms with status {StatusCode}",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMs,
                        statusCode);
                }

                // Copy response body to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    /// <summary>
    /// Logs detailed performance metrics for the request.
    /// Helps identify performance trends and bottlenecks.
    /// </summary>
    private void LogPerformanceMetrics(HttpContext context, double elapsedMs, long responseSize, int statusCode)
    {
        var request = context.Request;
        var throughput = elapsedMs > 0 ? (responseSize / (elapsedMs / 1000.0)) : 0;

        var performanceLevel = elapsedMs switch
        {
            < 100 => "EXCELLENT",
            < 500 => "GOOD",
            < 1000 => "ACCEPTABLE",
            < 5000 => "SLOW",
            _ => "VERY_SLOW"
        };

        _logger.LogInformation(
            "Performance: {Method} {Path} | Duration: {ElapsedMs:F2}ms | Size: {ResponseSize} bytes | " +
            "Throughput: {Throughput:F0} B/s | Status: {StatusCode} | Level: {PerformanceLevel}",
            request.Method,
            request.Path,
            elapsedMs,
            responseSize,
            throughput,
            statusCode,
            performanceLevel);
    }
}

/// <summary>
/// Extension methods for registering performance monitoring middleware.
/// </summary>
public static class PerformanceMonitoringMiddlewareExtensions
{
    /// <summary>
    /// Adds performance monitoring middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}
