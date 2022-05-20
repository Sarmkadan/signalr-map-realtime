# CachingOptions

`CachingOptions` is a configuration class used to control caching behavior in the `signalr-map-realtime` project. It defines parameters for enabling/disabling caching, cache durations, and distributed cache settings to optimize performance for real-time map and location-based features.

## API

### `Enabled`
Gets or sets a value indicating whether caching is enabled. When `false`, all caching mechanisms are bypassed.

- **Type:** `bool`
- **Default:** `true`
- **Usage:** Set to `false` to disable caching entirely for debugging or during low-memory scenarios.

### `DefaultDurationSeconds`
Gets or sets the default duration (in seconds) for cached items when no specific duration is provided.

- **Type:** `int`
- **Default:** `300` (5 minutes)
- **Usage:** Used as a fallback when a more specific cache duration is not defined.

### `LocationCacheDurationSeconds`
Gets or sets the duration (in seconds) for caching location data.

- **Type:** `int`
- **Default:** `60` (1 minute)
- **Usage:** Optimized for frequently updated location data to balance freshness and performance.

### `VehicleCacheDurationSeconds`
Gets or sets the duration (in seconds) for caching vehicle-related data.

- **Type:** `int`
- **Default:** `300` (5 minutes)
- **Usage:** Suitable for vehicle state or metadata that changes less frequently than location data.

### `RouteCacheDurationSeconds`
Gets or sets the duration (in seconds) for caching route calculations or pathfinding results.

- **Type:** `int`
- **Default:** `600` (10 minutes)
- **Usage:** Longer duration to avoid recomputing complex routes frequently.

### `AssetCacheDurationSeconds`
Gets or sets the duration (in seconds) for caching asset-related data (e.g., equipment, markers).

- **Type:** `int`
- **Default:** `300` (5 minutes)
- **Usage:** Balances between real-time updates and computational overhead.

### `SessionCacheDurationSeconds`
Gets or sets the duration (in seconds) for caching user session data.

- **Type:** `int`
- **Default:** `1800` (30 minutes)
- **Usage:** Longer-lived sessions for persistent connections or user preferences.

### `UseDistributedCache`
Gets or sets a value indicating whether to use a distributed cache (e.g., Redis) instead of in-memory caching.

- **Type:** `bool`
- **Default:** `false`
- **Usage:** Enable in distributed environments to share cache across multiple application instances.

### `DistributedCacheConnectionString`
Gets or sets the connection string for the distributed cache provider.

- **Type:** `string?`
- **Default:** `null`
- **Usage:** Required if `UseDistributedCache` is `true`. Typically points to a Redis or similar service.

### `RefreshTokenAbsoluteExpirationMinutes`
Gets or sets the absolute expiration time (in minutes) for refresh tokens stored in cache.

- **Type:** `int`
- **Default:** `43200` (30 days)
- **Usage:** Ensures tokens expire even if not actively used, enhancing security.

### `SessionSlidingExpirationMinutes`
Gets or sets the sliding expiration time (in minutes) for user session data in cache.

- **Type:** `int`
- **Default:** `30`
- **Usage:** Extends session lifetime as long as the user remains active.

### `MaxMemoryCacheEntries`
Gets or sets the maximum number of entries allowed in the in-memory cache.

- **Type:** `int`
- **Default:** `10000`
- **Usage:** Prevents unbounded memory growth by enforcing a hard limit on cached items.

## Usage

### Example 1: Basic Configuration
