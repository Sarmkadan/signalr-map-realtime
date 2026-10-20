# CacheService

Documentation for `src/SignalRMapRealtime/Services/CacheService.cs`.

## ICacheService

Public interface defining the caching contract used throughout the application.

### Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `GetOrCreateAsync<T>` | `Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)` | Retrieves a cached value if it exists; otherwise executes the factory function, caches the result and returns it. |
| `GetAsync<T>` | `Task<T?> GetAsync<T>(string key)` | Retrieves a cached value by key, or `default` if not found. |
| `SetAsync<T>` | `Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)` | Stores a value in the cache with an optional expiration duration. |
| `RemoveAsync` | `Task RemoveAsync(string key)` | Removes a single cached entry by its key. |
| `RemoveByPatternAsync` | `Task RemoveByPatternAsync(string pattern)` | Removes all cached entries whose keys match the given pattern. |
| `ClearAsync` | `Task ClearAsync()` | Clears all cached values. |
| `ExistsAsync` | `Task<bool> ExistsAsync(string key)` | Returns `true` if the specified key is present in the cache. |

## MemoryCacheService

In-memory implementation of `ICacheService` backed by `IMemoryCache`. Suitable for single-instance deployments.

### Constructor

```csharp
public MemoryCacheService(
    IMemoryCache memoryCache,
    ILogger<MemoryCacheService> logger,
    IOptions<CachingOptions> options)
```

All parameters are required. The `CachingOptions` instance controls whether caching is enabled and the default duration.

### Behaviour

- When `CachingOptions.Enabled` is `false`, caching is bypassed: `GetOrCreateAsync` always invokes the factory, `GetAsync` returns `default`, and `SetAsync` is a no-op.
- `GetOrCreateAsync` sets both `AbsoluteExpirationRelativeToNow` (the supplied expiration or the configured default) and `SlidingExpiration` (half the absolute duration).
- `RemoveByPatternAsync` logs a warning and returns without removing anything because `IMemoryCache` does not natively support pattern-based eviction.
- `ClearAsync` is a no-op for the same reason; it logs a warning.

### Usage Example

```csharp
// Resolve via DI
var cache = serviceProvider.GetRequiredService<ICacheService>();

// Get or create
var vehicles = await cache.GetOrCreateAsync(
    "vehicles:active",
    () => repo.GetActiveVehiclesAsync(),
    TimeSpan.FromMinutes(5));

// Manual set / get
await cache.SetAsync("session:123", sessionDto, TimeSpan.FromMinutes(10));
var cached = await cache.GetAsync<SessionDto>("session:123");

// Check existence and remove
if (await cache.ExistsAsync("session:123"))
    await cache.RemoveAsync("session:123");
```
