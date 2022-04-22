#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Root configuration options for the SignalR Map Realtime application.
/// This class serves as the main entry point for all application configuration,
/// consolidating all individual option classes into a single root configuration object.
/// </summary>
/// <remarks>
/// This class is designed to be registered with the IOptions pattern in Program.cs:
///
/// <code>
/// builder.Services.Configure&lt;SignalrMapRealtimeOptions&gt;(
///     builder.Configuration.GetSection(SignalrMapRealtimeOptions.SectionName));
/// </code>
///
/// All configuration values are loaded from the "SignalRMapRealtime" section
/// in appsettings.json and related configuration sources.
/// </remarks>
/// <example>
/// <code>
/// {
///   "SignalRMapRealtime": {
///     "AppInfo": {
///       "ApiVersion": "2.0.0",
///       "ApiTitle": "SignalR Map Realtime API",
///       "Environment": "Production",
///       "EnableSwagger": true,
///       "EnableCors": true,
///       "RequestTimeoutSeconds": 30,
///       "LocationUpdateIntervalSeconds": 30
///     },
///     "HealthChecks": {
///       "Enabled": true,
///       "TimeoutSeconds": 5,
///       "MinimumStatus": "Healthy"
///     },
///     "ApiKeyAuthentication": {
///       "Enabled": true,
///       "HeaderName": "X-API-Key",
///       "Required": true
///     },
///     "Performance": {
///       "EnableDetailedMetrics": true,
///       "MaxConcurrentConnections": 10000,
///       "RequestQueueLimit": 1000
///     }
///   }
/// }
/// </code>
/// </example>
public class SignalrMapRealtimeOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "SignalRMapRealtime";

    /// <summary>
    /// Application information and metadata configuration.
    /// </summary>
    [Required]
    public AppInfoOptions AppInfo { get; set; } = new();

    /// <summary>
    /// Health check configuration.
    /// </summary>
    [Required]
    public HealthCheckOptions HealthChecks { get; set; } = new();

    /// <summary>
    /// API key authentication configuration.
    /// </summary>
    [Required]
    public ApiKeyAuthenticationOptions ApiKeyAuthentication { get; set; } = new();

    /// <summary>
    /// Performance monitoring and optimization configuration.
    /// </summary>
    [Required]
    public PerformanceOptions Performance { get; set; } = new();

    /// <summary>
    /// SignalR specific configuration.
    /// </summary>
    [Required]
    public SignalRHubOptions SignalRHubs { get; set; } = new();

    /// <summary>
    /// WebSocket and real-time communication configuration.
    /// </summary>
    [Required]
    public WebSocketOptions WebSockets { get; set; } = new();

    /// <summary>
    /// Background job and worker configuration.
    /// </summary>
    [Required]
    public BackgroundJobsOptions BackgroundJobs { get; set; } = new();

    /// <summary>
    /// Security configuration.
    /// </summary>
    [Required]
    public SecurityOptions Security { get; set; } = new();

    /// <summary>
    /// Validation helper to ensure all required sections are properly configured.
    /// </summary>
    /// <returns>True if all required sections are configured, false otherwise.</returns>
    public bool Validate(out List<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        return Validator.TryValidateObject(this, validationContext, validationResults, true);
    }

    /// <summary>
    /// Application information and metadata configuration.
    /// </summary>
    public class AppInfoOptions
    {
        /// <summary>
        /// API version number. Should follow semantic versioning (MAJOR.MINOR.PATCH).
        /// </summary>
        /// <example>"2.0.0"</example>
        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^\d+\.\d+\.\d+$")]
        public string ApiVersion { get; set; } = "2.0.0";

        /// <summary>
        /// API title/name displayed in Swagger and API responses.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string ApiTitle { get; set; } = "SignalR Map Realtime API";

        /// <summary>
        /// Current environment (Development, Staging, Production).
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^(Development|Staging|Production)$", ErrorMessage = "Environment must be Development, Staging, or Production")]
        public string Environment { get; set; } = "Production";

        /// <summary>
        /// Enable Swagger/OpenAPI documentation endpoint.
        /// </summary>
        public bool EnableSwagger { get; set; } = true;

        /// <summary>
        /// Enable CORS policy for cross-origin requests.
        /// </summary>
        public bool EnableCors { get; set; } = true;

        /// <summary>
        /// Request timeout in seconds for API endpoints.
        /// </summary>
        [Range(5, 300, ErrorMessage = "RequestTimeoutSeconds must be between 5 and 300")]
        public int RequestTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Default interval in seconds between location updates.
        /// </summary>
        [Range(1, 600, ErrorMessage = "LocationUpdateIntervalSeconds must be between 1 and 600")]
        public int LocationUpdateIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum allowed payload size in kilobytes for API requests.
        /// </summary>
        [Range(1, 10240, ErrorMessage = "MaxPayloadSizeKb must be between 1 and 10240")]
        public int MaxPayloadSizeKb { get; set; } = 1024; // 1MB
    }

    /// <summary>
    /// Health check configuration.
    /// </summary>
    public class HealthCheckOptions
    {
        /// <summary>
        /// Enable health check endpoints.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Health check timeout in seconds.
        /// </summary>
        [Range(1, 60, ErrorMessage = "TimeoutSeconds must be between 1 and 60")]
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Minimum acceptable health status (Healthy, Degraded, Unhealthy).
        /// </summary>
        [RegularExpression(@"^(Healthy|Degraded|Unhealthy)$", ErrorMessage = "MinimumStatus must be Healthy, Degraded, or Unhealthy")]
        public string MinimumStatus { get; set; } = "Healthy";

        /// <summary>
        /// Enable detailed health check information in responses.
        /// </summary>
        public bool IncludeDetails { get; set; } = true;
    }

    /// <summary>
    /// API key authentication configuration.
    /// </summary>
    public class ApiKeyAuthenticationOptions
    {
        /// <summary>
        /// Enable API key authentication for SignalR hubs and API endpoints.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// HTTP header name for API key.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
        public string HeaderName { get; set; } = "X-API-Key";

        /// <summary>
        /// API key value (should be provided via secrets or environment variables in production).
        /// </summary>
        [StringLength(1000)]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Require API key for all endpoints (including health checks).
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Allow unauthenticated access to specific endpoints.
        /// </summary>
        public List<string> ExemptedEndpoints { get; set; } = new()
        {
            "/health",
            "/api/info",
            "/swagger",
            "/swagger/",
            "/swagger/v1/swagger.json"
        };
    }

    /// <summary>
    /// Performance monitoring and optimization configuration.
    /// </summary>
    public class PerformanceOptions
    {
        /// <summary>
        /// Enable detailed performance metrics collection.
        /// </summary>
        public bool EnableDetailedMetrics { get; set; } = true;

        /// <summary>
        /// Maximum number of concurrent WebSocket connections.
        /// </summary>
        [Range(100, 100000, ErrorMessage = "MaxConcurrentConnections must be between 100 and 100000")]
        public int MaxConcurrentConnections { get; set; } = 10000;

        /// <summary>
        /// Maximum number of requests in the queue before rejecting new requests.
        /// </summary>
        [Range(10, 10000, ErrorMessage = "RequestQueueLimit must be between 10 and 10000")]
        public int RequestQueueLimit { get; set; } = 1000;

        /// <summary>
        /// Enable automatic request rate limiting based on system load.
        /// </summary>
        public bool EnableAdaptiveRateLimiting { get; set; } = false;

        /// <summary>
        /// Maximum allowed request processing time in seconds before timeout.
        /// </summary>
        [Range(5, 300, ErrorMessage = "MaxProcessingTimeSeconds must be between 5 and 300")]
        public int MaxProcessingTimeSeconds { get; set; } = 30;

        /// <summary>
        /// Enable caching for frequently accessed data.
        /// </summary>
        public bool EnableCaching { get; set; } = true;
    }

    /// <summary>
    /// SignalR specific configuration.
    /// </summary>
    public class SignalRHubOptions
    {
        /// <summary>
        /// Enable SignalR real-time communication.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum number of concurrent SignalR connections per hub.
        /// </summary>
        [Range(100, 50000, ErrorMessage = "MaxConnectionsPerHub must be between 100 and 50000")]
        public int MaxConnectionsPerHub { get; set; } = 10000;

        /// <summary>
        /// Enable SignalR backplane for multi-server deployments (Redis required).
        /// </summary>
        public bool UseBackplane { get; set; } = false;

        /// <summary>
        /// Connection string for SignalR backplane (Redis).
        /// Only used if UseBackplane is true.
        /// </summary>
        [StringLength(500)]
        public string? BackplaneConnectionString { get; set; }

        /// <summary>
        /// Enable automatic reconnection for clients.
        /// </summary>
        public bool EnableAutomaticReconnect { get; set; } = true;

        /// <summary>
        /// Reconnection timeout in seconds.
        /// </summary>
        [Range(5, 120, ErrorMessage = "ReconnectTimeoutSeconds must be between 5 and 120")]
        public int ReconnectTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum message size in kilobytes for SignalR messages.
        /// </summary>
        [Range(1, 1024, ErrorMessage = "MaxMessageSizeKb must be between 1 and 1024")]
        public int MaxMessageSizeKb { get; set; } = 256;

        /// <summary>
        /// Enable message compression for SignalR communication.
        /// </summary>
        public bool EnableMessageCompression { get; set; } = true;
    }

    /// <summary>
    /// WebSocket and real-time communication configuration.
    /// </summary>
    public class WebSocketOptions
    {
        /// <summary>
        /// Enable WebSocket support.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// WebSocket keep-alive interval in seconds.
        /// </summary>
        [Range(10, 300, ErrorMessage = "KeepAliveIntervalSeconds must be between 10 and 300")]
        public int KeepAliveIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum WebSocket message size in kilobytes.
        /// </summary>
        [Range(1, 1024, ErrorMessage = "MaxMessageSizeKb must be between 1 and 1024")]
        public int MaxMessageSizeKb { get; set; } = 256;

        /// <summary>
        /// Enable WebSocket ping/pong for connection health monitoring.
        /// </summary>
        public bool EnablePingPong { get; set; } = true;

        /// <summary>
        /// Ping interval in seconds when EnablePingPong is true.
        /// </summary>
        [Range(15, 120, ErrorMessage = "PingIntervalSeconds must be between 15 and 120")]
        public int PingIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum allowed WebSocket connection duration in hours.
        /// </summary>
        [Range(1, 24, ErrorMessage = "MaxConnectionDurationHours must be between 1 and 24")]
        public int MaxConnectionDurationHours { get; set; } = 8;
    }

    /// <summary>
    /// Background job and worker configuration.
    /// </summary>
    public class BackgroundJobsOptions
    {
        /// <summary>
        /// Enable background job processing.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum number of concurrent background workers.
        /// </summary>
        [Range(1, 20, ErrorMessage = "MaxConcurrentWorkers must be between 1 and 20")]
        public int MaxConcurrentWorkers { get; set; } = 5;

        /// <summary>
        /// Session cleanup job interval in minutes.
        /// </summary>
        [Range(1, 1440, ErrorMessage = "SessionCleanupIntervalMinutes must be between 1 and 1440")]
        public int SessionCleanupIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Enable periodic cache cleanup.
        /// </summary>
        public bool EnableCacheCleanup { get; set; } = true;

        /// <summary>
        /// Cache cleanup interval in minutes.
        /// </summary>
        [Range(5, 1440, ErrorMessage = "CacheCleanupIntervalMinutes must be between 5 and 1440")]
        public int CacheCleanupIntervalMinutes { get; set; } = 30;

        /// <summary>
        /// Enable database maintenance jobs.
        /// </summary>
        public bool EnableDatabaseMaintenance { get; set; } = true;

        /// <summary>
        /// Database maintenance interval in hours.
        /// </summary>
        [Range(1, 168, ErrorMessage = "DatabaseMaintenanceIntervalHours must be between 1 and 168")]
        public int DatabaseMaintenanceIntervalHours { get; set; } = 24;
    }

    /// <summary>
    /// Security configuration.
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>
        /// Enable HTTPS redirection.
        /// </summary>
        public bool EnableHttpsRedirection { get; set; } = true;

        /// <summary>
        /// Enable request logging for security auditing.
        /// </summary>
        public bool EnableRequestLogging { get; set; } = true;

        /// <summary>
        /// Enable CORS security headers.
        /// </summary>
        public bool EnableCorsSecurityHeaders { get; set; } = true;

        /// <summary>
        /// Enable rate limiting for security protection.
        /// </summary>
        public bool EnableRateLimiting { get; set; } = true;

        /// <summary>
        /// Enable input validation for all API endpoints.
        /// </summary>
        public bool EnableInputValidation { get; set; } = true;

        /// <summary>
        /// Enable security headers in HTTP responses.
        /// </summary>
        public bool EnableSecurityHeaders { get; set; } = true;

        /// <summary>
        /// Content Security Policy directives.
        /// </summary>
        [StringLength(1000)]
        public string? ContentSecurityPolicy { get; set; } = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:";
    }
}