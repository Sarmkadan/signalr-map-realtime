// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Middleware;

using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;

/// <summary>
/// Rate limiting middleware that protects the API from abuse and DoS attacks.
/// Tracks requests per IP address and optionally per user (if authenticated).
/// Returns 429 (Too Many Requests) when limits are exceeded.
/// Uses in-memory storage for simplicity; use distributed cache for multi-instance deployments.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    // In-memory store for tracking requests: Key = "ip:address" or "user:id", Value = list of request timestamps
    private static readonly Dictionary<string, List<DateTime>> RequestTracker = new();
    private static readonly object LockObject = new();

    /// <summary>
    /// Initializes the rate limiting middleware.
    /// </summary>
    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Processes each HTTP request and enforces rate limits.
    /// Checks request count within the configured time window.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting if disabled globally
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var request = context.Request;

        // Skip rate limiting for exempted endpoints
        if (IsExemptedEndpoint(request.Path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);

        // Skip rate limiting for whitelisted IPs
        if (IsWhitelistedIp(clientId))
        {
            await _next(context);
            return;
        }

        // Get the rate limit for this endpoint
        var requestsPerMinute = GetRateLimitForEndpoint(request.Path);

        // Check if client has exceeded the rate limit
        if (HasExceededRateLimit(clientId, requestsPerMinute))
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Path: {Path}", clientId, request.Path);

            context.Response.StatusCode = _options.TooManyRequestsStatusCode;
            context.Response.ContentType = "application/json";

            // Add rate limit headers if enabled
            if (_options.IncludeRateLimitHeaders)
            {
                context.Response.Headers["X-RateLimit-Limit"] = requestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = DateTime.UtcNow.AddSeconds(_options.WindowSizeSeconds).ToUnixTimestamp().ToString();
            }

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfterSeconds = _options.WindowSizeSeconds
            });

            return;
        }

        // Record this request
        RecordRequest(clientId);

        // Add rate limit headers to response if enabled
        if (_options.IncludeRateLimitHeaders)
        {
            var remaining = GetRemainingRequests(clientId, requestsPerMinute);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit"] = requestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = DateTime.UtcNow.AddSeconds(_options.WindowSizeSeconds).ToUnixTimestamp().ToString();
                return Task.CompletedTask;
            });
        }

        await _next(context);
    }

    /// <summary>
    /// Gets a unique identifier for the client (IP address or user ID).
    /// Prioritizes authenticated user ID over IP address.
    /// </summary>
    private string GetClientIdentifier(HttpContext context)
    {
        // If user is authenticated, use user ID for per-user rate limiting
        if (_options.EnableUserBasedLimiting && context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
                return $"user:{userId}";
        }

        // Fall back to IP-based rate limiting
        if (_options.EnableIpBasedLimiting)
        {
            var ipAddress = GetClientIpAddress(context);
            return $"ip:{ipAddress}";
        }

        return "unknown";
    }

    /// <summary>
    /// Extracts the client's IP address from the request.
    /// Handles X-Forwarded-For header for proxy scenarios.
    /// </summary>
    private string GetClientIpAddress(HttpContext context)
    {
        // Check for X-Forwarded-For header (proxy scenarios)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',');
            return ips[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Determines the rate limit for a specific endpoint.
    /// Different endpoints have different limits based on their function.
    /// </summary>
    private int GetRateLimitForEndpoint(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;

        return pathValue switch
        {
            var p when p.StartsWith("/api/locations", StringComparison.OrdinalIgnoreCase) => _options.LocationUpdatesPerMinute,
            var p when p.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) => _options.AuthenticationAttemptsPerMinute,
            var p when p.Contains("/list", StringComparison.OrdinalIgnoreCase) || p.Contains("/search", StringComparison.OrdinalIgnoreCase) => _options.ListEndpointsPerMinute,
            var p when p.StartsWith("/webhooks", StringComparison.OrdinalIgnoreCase) => _options.WebhookRequestsPerMinute,
            _ => _options.RequestsPerMinute
        };
    }

    /// <summary>
    /// Checks if a client has exceeded their rate limit.
    /// </summary>
    private bool HasExceededRateLimit(string clientId, int requestsPerMinute)
    {
        lock (LockObject)
        {
            if (!RequestTracker.TryGetValue(clientId, out var timestamps))
                return false;

            var cutoffTime = DateTime.UtcNow.AddSeconds(-_options.WindowSizeSeconds);
            var recentRequests = timestamps.Count(t => t > cutoffTime);

            return recentRequests >= requestsPerMinute;
        }
    }

    /// <summary>
    /// Records a request for a client in the tracker.
    /// </summary>
    private void RecordRequest(string clientId)
    {
        lock (LockObject)
        {
            if (!RequestTracker.TryGetValue(clientId, out var timestamps))
            {
                timestamps = new List<DateTime>();
                RequestTracker[clientId] = timestamps;
            }

            timestamps.Add(DateTime.UtcNow);

            // Clean up old requests to prevent memory bloat
            if (timestamps.Count > 1000)
            {
                var cutoffTime = DateTime.UtcNow.AddSeconds(-_options.WindowSizeSeconds);
                timestamps.RemoveAll(t => t < cutoffTime);
            }
        }
    }

    /// <summary>
    /// Gets the number of remaining requests for a client.
    /// </summary>
    private int GetRemainingRequests(string clientId, int requestsPerMinute)
    {
        lock (LockObject)
        {
            if (!RequestTracker.TryGetValue(clientId, out var timestamps))
                return requestsPerMinute;

            var cutoffTime = DateTime.UtcNow.AddSeconds(-_options.WindowSizeSeconds);
            var recentRequests = timestamps.Count(t => t > cutoffTime);

            return Math.Max(0, requestsPerMinute - recentRequests);
        }
    }

    /// <summary>
    /// Checks if an endpoint is exempted from rate limiting.
    /// </summary>
    private bool IsExemptedEndpoint(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return _options.ExemptedEndpoints.Any(e => pathValue.StartsWith(e, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if an IP address is whitelisted and should bypass rate limiting.
    /// </summary>
    private bool IsWhitelistedIp(string clientId)
    {
        if (!clientId.StartsWith("ip:"))
            return false;

        var ipAddress = clientId.Substring(3);
        return _options.WhitelistedIps.Contains(ipAddress);
    }
}

/// <summary>
/// Extension methods for registering rate limiting middleware.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}

/// <summary>
/// Unix timestamp helper extension.
/// </summary>
internal static class UnixTimestampExtensions
{
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)dateTime.ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1))
            .TotalSeconds;
    }
}
