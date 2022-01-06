// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Configuration options for caching behavior throughout the application.
/// These options are loaded from appsettings.json under the "Caching" section.
/// Allows tuning cache expiration times and enabling/disabling caching per feature.
/// </summary>
public class CachingOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Caching";

    /// <summary>
    /// Enable or disable caching globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default cache duration in seconds for items without specific expiration.
    /// </summary>
    public int DefaultDurationSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Cache duration for vehicle location data in seconds.
    /// Locations update frequently, so shorter duration is typical.
    /// </summary>
    public int LocationCacheDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Cache duration for vehicle information in seconds.
    /// Vehicle metadata changes less frequently than location data.
    /// </summary>
    public int VehicleCacheDurationSeconds { get; set; } = 600; // 10 minutes

    /// <summary>
    /// Cache duration for route data in seconds.
    /// Routes are relatively static once created.
    /// </summary>
    public int RouteCacheDurationSeconds { get; set; } = 1800; // 30 minutes

    /// <summary>
    /// Cache duration for asset information in seconds.
    /// Assets are static unless explicitly updated.
    /// </summary>
    public int AssetCacheDurationSeconds { get; set; } = 1800; // 30 minutes

    /// <summary>
    /// Cache duration for user/session information in seconds.
    /// User data should be refreshed regularly for security.
    /// </summary>
    public int SessionCacheDurationSeconds { get; set; } = 600; // 10 minutes

    /// <summary>
    /// Enable distributed caching (Redis/MemoryCache).
    /// Set to true for production environments with multiple instances.
    /// </summary>
    public bool UseDistributedCache { get; set; } = false;

    /// <summary>
    /// Connection string for distributed cache (Redis).
    /// Only used if UseDistributedCache is true.
    /// </summary>
    public string? DistributedCacheConnectionString { get; set; }

    /// <summary>
    /// Absolute expiration time for refresh token cache in minutes.
    /// </summary>
    public int RefreshTokenAbsoluteExpirationMinutes { get; set; } = 1440; // 24 hours

    /// <summary>
    /// Sliding expiration time for user session cache in minutes.
    /// </summary>
    public int SessionSlidingExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Maximum number of items in local memory cache.
    /// </summary>
    public int MaxMemoryCacheEntries { get; set; } = 10000;
}
