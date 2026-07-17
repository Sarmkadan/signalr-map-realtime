// ... (rest of the file remains unchanged)

## CachingOptions

The `CachingOptions` class configures caching behavior throughout the application, allowing tuning of cache expiration times and enabling/disabling caching per feature. This helps improve performance by reducing database queries and computations.

### Usage Example

```csharp
using SignalRMapRealtime.Configuration;

// Access and configure caching options
var cachingOptions = new CachingOptions
{
    Enabled = true,
    DefaultDurationSeconds = 300, // 5 minutes
    LocationCacheDurationSeconds = 30,
    VehicleCacheDurationSeconds = 600, // 10 minutes
    RouteCacheDurationSeconds = 1800, // 30 minutes
    AssetCacheDurationSeconds = 1800, // 30 minutes
    SessionCacheDurationSeconds = 600, // 10 minutes
    UseDistributedCache = false,
    DistributedCacheConnectionString = null,
    RefreshTokenAbsoluteExpirationMinutes = 1440, // 24 hours
    SessionSlidingExpirationMinutes = 30,
    MaxMemoryCacheEntries = 10000
};
```

// ... (rest of the file remains unchanged)
