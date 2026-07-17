// ... (rest of the file remains unchanged)

## RateLimitingOptions

The `RateLimitingOptions` class provides configuration options for rate limiting protection against abuse and DoS attacks. It allows you to customize rate limits for different types of requests, such as standard endpoints, location updates, authentication attempts, and more. By adjusting these settings, you can effectively prevent excessive usage and protect your application.

### Usage Example

```csharp
using SignalRMapRealtime.Configuration;

// Access and configure RateLimitingOptions
var rateLimitingOptions = new RateLimitingOptions
{
    Enabled = true,
    RequestsPerMinute = 60,
    LocationUpdatesPerMinute = 120,
    AuthenticationAttemptsPerMinute = 10,
    ListEndpointsPerMinute = 40,
    WebhookRequestsPerMinute = 30,
    WindowSizeSeconds = 60,
    EnableIpBasedLimiting = true,
    EnableUserBasedLimiting = true,
    ExemptedEndpoints = new List<string> { "/health", "/api/info" },
    WhitelistedIps = new List<string> { "192.168.1.100" },
    TooManyRequestsStatusCode = 429,
    IncludeRateLimitHeaders = true,
    UseDistributedRateLimiting = false,
    DistributedCounterTtlSeconds = 120
};

// Use rateLimitingOptions in your application configuration
``
// ... (rest of the file remains unchanged)
