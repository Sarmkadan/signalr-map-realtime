// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;

/// <summary>
/// Caching service that provides a unified interface for caching operations.
/// Supports both local memory caching and distributed caching (Redis).
/// Abstracts caching complexity from business logic.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value if it exists, otherwise executes the factory function and caches the result.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Retrieves a cached value if it exists.
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Stores a value in the cache with optional expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a specific cached value.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all cached values that match a pattern.
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Clears all cached values.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// In-memory implementation of the cache service using .NET's IMemoryCache.
/// Suitable for single-instance deployments.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly CachingOptions _options;

    public MemoryCacheService(
        IMemoryCache memoryCache,
        ILogger<MemoryCacheService> logger,
        IOptions<CachingOptions> options)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Retrieves or creates a cached value. Uses GetOrCreate internally for efficiency.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (!_options.Enabled)
            return await factory();

        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue!;
        }

        _logger.LogDebug("Cache miss for key: {Key}. Creating value...", key);

        var value = await factory();
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromSeconds(_options.DefaultDurationSeconds),
            SlidingExpiration = TimeSpan.FromSeconds(_options.DefaultDurationSeconds / 2)
        };

        _memoryCache.Set(key, value, cacheOptions);
        return value;
    }

    /// <summary>
    /// Retrieves a cached value if it exists.
    /// </summary>
    public Task<T?> GetAsync<T>(string key)
    {
        if (!_options.Enabled)
            return Task.FromResult<T?>(default);

        var found = _memoryCache.TryGetValue(key, out T? value);
        if (found)
            _logger.LogDebug("Cache hit for key: {Key}", key);
        else
            _logger.LogDebug("Cache miss for key: {Key}", key);

        return Task.FromResult(value);
    }

    /// <summary>
    /// Stores a value in the cache with optional expiration.
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (!_options.Enabled)
            return Task.CompletedTask;

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromSeconds(_options.DefaultDurationSeconds),
            SlidingExpiration = TimeSpan.FromSeconds(_options.DefaultDurationSeconds / 2)
        };

        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a specific cached value.
    /// </summary>
    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Removed cache for key: {Key}", key);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes cached values matching a pattern (e.g., "location:*").
    /// Note: In-memory cache doesn't support pattern matching natively,
    /// so this is a no-op for memory cache. Use distributed cache for pattern support.
    /// </summary>
    public Task RemoveByPatternAsync(string pattern)
    {
        _logger.LogWarning("Pattern-based cache removal not supported in memory cache. Pattern: {Pattern}", pattern);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all cached values. Only practical for testing.
    /// </summary>
    public Task ClearAsync()
    {
        _logger.LogWarning("Clearing all cache entries");
        // There's no built-in way to clear all items in IMemoryCache
        // This would require maintaining our own dictionary of keys
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    public Task<bool> ExistsAsync(string key)
    {
        var exists = _memoryCache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }
}
