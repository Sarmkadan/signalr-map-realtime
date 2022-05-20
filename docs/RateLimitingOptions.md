# RateLimitingOptions

`RateLimitingOptions` configures the rate-limiting middleware for the SignalR map real-time application. It controls global enablement, per-endpoint request thresholds, time windows, IP and user-based limiting strategies, exemption lists, distributed counter behavior, and HTTP response customization. All values are settable at application startup and remain immutable for the lifetime of the rate limiter once the middleware is initialized.

## API

### `public bool Enabled`

Gets or sets whether rate limiting is active. When `false`, all requests pass through without any limit checks, regardless of other configured values. Default is `false`.

### `public int RequestsPerMinute`

The maximum number of general-purpose requests allowed per minute, aggregated across all non-specialized endpoints. Applies to endpoints not covered by the more specific counters below. Must be a positive integer; values less than 1 disable the general limit.

### `public int LocationUpdatesPerMinute`

The maximum number of location-update requests allowed per minute. Used to throttle the frequency of geospatial data submissions from clients. Must be a positive integer; values less than 1 disable this specific limit.

### `public int AuthenticationAttemptsPerMinute`

The maximum number of authentication attempts allowed per minute. Protects login and token endpoints from brute-force attacks. Must be a positive integer; values less than 1 disable this specific limit.

### `public int ListEndpointsPerMinute`

The maximum number of requests to list-style endpoints (e.g., entity enumeration, search results) allowed per minute. Must be a positive integer; values less than 1 disable this specific limit.

### `public int WebhookRequestsPerMinute`

The maximum number of incoming webhook requests allowed per minute. Applies to externally triggered callbacks. Must be a positive integer; values less than 1 disable this specific limit.

### `public int WindowSizeSeconds`

The sliding time window, in seconds, over which request counts are evaluated. A value of 60 means limits are enforced over the trailing 60-second period. Must be a positive integer; typical values range from 30 to 300.

### `public bool EnableIpBasedLimiting`

When `true`, rate limits are enforced per client IP address. Requests from different IPs are counted independently. If both IP-based and user-based limiting are enabled, both constraints apply simultaneously.

### `public bool EnableUserBasedLimiting`

When `true`, rate limits are enforced per authenticated user identity. Anonymous requests are treated as a single aggregate identity unless IP-based limiting is also active. If both IP-based and user-based limiting are enabled, both constraints apply simultaneously.

### `public List<string> ExemptedEndpoints`

A list of endpoint path prefixes or exact paths that are excluded from all rate limiting. Matching requests bypass counters entirely. Paths are compared case-insensitively. An empty list means no exemptions.

### `public List<string> WhitelistedIps`

A list of IP addresses or CIDR ranges that are exempt from IP-based limiting. Requests originating from these addresses are not counted against IP limits. User-based limits may still apply if enabled. An empty list means no IP whitelisting.

### `public int TooManyRequestsStatusCode`

The HTTP status code returned when a rate limit is exceeded. Typically set to `429` (Too Many Requests), but can be customized for integration requirements. Must be a valid HTTP status code integer.

### `public bool IncludeRateLimitHeaders`

When `true`, the middleware adds standard rate-limit headers (`X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`, and optionally `Retry-After`) to responses. Enables clients to programmatically adapt their request pacing.

### `public bool UseDistributedRateLimiting`

When `true`, rate-limit counters are stored in a distributed cache (e.g., Redis) rather than in-process memory. Required for horizontally scaled deployments where multiple server instances must share limit state. When `false`, counters are held in local memory and are not synchronized across instances.

### `public int DistributedCounterTtlSeconds`

The time-to-live, in seconds, for distributed counter entries. After this duration, an inactive counter is evicted from the distributed store. Must be greater than `WindowSizeSeconds` to avoid premature eviction while the window is still relevant. Ignored when `UseDistributedRateLimiting` is `false`.

## Usage

### Example 1: Basic in-memory configuration with IP limiting

```csharp
var options = new RateLimitingOptions
{
    Enabled = true,
    EnableIpBasedLimiting = true,
    EnableUserBasedLimiting = false,
    RequestsPerMinute = 300,
    LocationUpdatesPerMinute = 60,
    AuthenticationAttemptsPerMinute = 10,
    WindowSizeSeconds = 60,
    TooManyRequestsStatusCode = 429,
    IncludeRateLimitHeaders = true,
    UseDistributedRateLimiting = false,
    ExemptedEndpoints = new List<string> { "/health", "/metrics" },
    WhitelistedIps = new List<string> { "10.0.0.0/8" }
};

// Apply to the middleware during startup
app.UseRateLimiting(options);
```

### Example 2: Distributed limiting for a multi-instance deployment

```csharp
var options = new RateLimitingOptions
{
    Enabled = true,
    EnableIpBasedLimiting = true,
    EnableUserBasedLimiting = true,
    RequestsPerMinute = 500,
    LocationUpdatesPerMinute = 100,
    AuthenticationAttemptsPerMinute = 5,
    ListEndpointsPerMinute = 200,
    WebhookRequestsPerMinute = 50,
    WindowSizeSeconds = 120,
    TooManyRequestsStatusCode = 429,
    IncludeRateLimitHeaders = true,
    UseDistributedRateLimiting = true,
    DistributedCounterTtlSeconds = 180,
    ExemptedEndpoints = new List<string> { "/api/status" },
    WhitelistedIps = new List<string>()
};

// Requires a distributed cache (e.g., Redis) registered in the service container
services.AddStackExchangeRedisCache(...);
app.UseRateLimiting(options);
```

## Notes

- **Order of precedence**: `ExemptedEndpoints` and `WhitelistedIps` are evaluated before any counter logic. A request matching an exempted path bypasses all limits. A request from a whitelisted IP bypasses IP-based limits but is still subject to user-based limits if `EnableUserBasedLimiting` is `true`.
- **Combined limiting**: When both `EnableIpBasedLimiting` and `EnableUserBasedLimiting` are `true`, a single request increments both counters. Exceeding either limit results in a rejection.
- **Window semantics**: The sliding window is not a fixed-interval bucket. Counts are evaluated over the trailing `WindowSizeSeconds` from the current request time, providing smooth throttling without burst edge effects at interval boundaries.
- **Thread safety**: The options object itself is not thread-safe for mutation after the middleware has started. Configuration should be fully populated before calling `UseRateLimiting`. The internal counters (in-memory or distributed) are thread-safe and designed for concurrent access.
- **Distributed counter TTL**: `DistributedCounterTtlSeconds` must exceed `WindowSizeSeconds` by a margin sufficient to account for clock skew and replication lag in the distributed cache. A value of at least `WindowSizeSeconds + 30` is recommended. If a counter expires before the window slides past its entries, under-counting and unintended request admission may occur.
- **Status code**: Setting `TooManyRequestsStatusCode` to a non-standard value (e.g., `403`) may confuse clients expecting `429`. Ensure downstream consumers are aligned with the chosen status code.
- **Header output**: `IncludeRateLimitHeaders` adds headers only when `Enabled` is `true`. Headers reflect the most restrictive applicable limit for the request when multiple limit types are active.
- **Empty lists**: `ExemptedEndpoints` and `WhitelistedIps` default to empty lists. An empty list means no exemptions or whitelisting, not a failure state.
- **Zero or negative limits**: Setting a per-minute limit to zero or a negative value effectively disables that specific limit category, even if `Enabled` is `true`. The general `RequestsPerMinute` may still apply.
