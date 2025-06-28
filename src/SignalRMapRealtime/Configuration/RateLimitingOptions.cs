// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Configuration options for rate limiting protection against abuse and DoS attacks.
/// These options are loaded from appsettings.json under the "RateLimiting" section.
/// Implements per-IP and per-user rate limiting strategies.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Enable or disable rate limiting globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// General rate limit: requests per minute for standard endpoints.
    /// </summary>
    public int RequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// Rate limit for location update endpoints (more permissive due to real-time requirements).
    /// </summary>
    public int LocationUpdatesPerMinute { get; set; } = 120;

    /// <summary>
    /// Rate limit for authentication endpoints (more restrictive to prevent brute force).
    /// </summary>
    public int AuthenticationAttemptsPerMinute { get; set; } = 10;

    /// <summary>
    /// Rate limit for list/search endpoints (typically used for pagination).
    /// </summary>
    public int ListEndpointsPerMinute { get; set; } = 40;

    /// <summary>
    /// Rate limit for webhook endpoints that receive external data.
    /// </summary>
    public int WebhookRequestsPerMinute { get; set; } = 30;

    /// <summary>
    /// Time window in seconds for rate limit calculation.
    /// For example, 60 means the rate limits above are per-minute.
    /// </summary>
    public int WindowSizeSeconds { get; set; } = 60;

    /// <summary>
    /// Enable per-IP rate limiting.
    /// </summary>
    public bool EnableIpBasedLimiting { get; set; } = true;

    /// <summary>
    /// Enable per-user rate limiting (requires authentication).
    /// </summary>
    public bool EnableUserBasedLimiting { get; set; } = true;

    /// <summary>
    /// Requests to skip rate limiting check (e.g., health check, status endpoints).
    /// </summary>
    public List<string> ExemptedEndpoints { get; set; } = new()
    {
        "/health",
        "/api/info",
        "/swagger"
    };

    /// <summary>
    /// Whitelisted IP addresses that bypass rate limiting.
    /// </summary>
    public List<string> WhitelistedIps { get; set; } = new();

    /// <summary>
    /// HTTP status code to return when rate limit is exceeded.
    /// Standard is 429 (Too Many Requests).
    /// </summary>
    public int TooManyRequestsStatusCode { get; set; } = 429;

    /// <summary>
    /// Include rate limit information in response headers.
    /// Helps clients understand their quota and remaining requests.
    /// </summary>
    public bool IncludeRateLimitHeaders { get; set; } = true;

    /// <summary>
    /// Enable distributed rate limiting for multi-instance deployments.
    /// Requires Redis connection for state sharing.
    /// </summary>
    public bool UseDistributedRateLimiting { get; set; } = false;

    /// <summary>
    /// Time to live for distributed rate limit counters in seconds.
    /// </summary>
    public int DistributedCounterTtlSeconds { get; set; } = 120;
}
