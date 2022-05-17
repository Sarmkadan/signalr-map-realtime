# ICacheService

A lightweight, asynchronous caching abstraction built on `MemoryCache` for SignalR real-time map applications. It provides a consistent interface for storing, retrieving, and invalidating cached data with support for scoped cache keys, pattern-based removal, and bulk operations.

## API

### `MemoryCacheService`
A concrete implementation of `ICacheService` that uses `Microsoft.Extensions.Caching.Memory.MemoryCache` as the underlying storage mechanism. This class is thread-safe and designed for dependency injection.

### `async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? absoluteExpiration = null)`
Retrieves the value associated with the specified `key` from the cache. If the key does not exist, invokes `valueFactory` to compute the value, stores it in the cache with an optional `absoluteExpiration`, and returns the result.

- **Parameters**:
  - `key`: A unique identifier for the cached item.
  - `valueFactory`: An asynchronous function that computes the value if the key is not found.
  - `absoluteExpiration` (optional): The duration after which the cache entry expires. If `null`, the default cache expiration is used.
- **Return Value**: The cached or newly computed value of type `T`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `key` or `valueFactory` is `null`.
  - `InvalidOperationException`: Thrown if `valueFactory` throws an exception.

### `Task<T?> GetAsync<T>(string key)`
Asynchronously retrieves the value associated with the specified `key` from the cache.

- **Parameters**:
  - `key`: A unique identifier for the cached item.
- **Return Value**: The cached value of type `T`, or `null` if the key does not exist or the value is not of type `T`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `key` is `null`.

### `Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)`
Asynchronously stores the specified `value` in the cache under the given `key`.

- **Parameters**:
  - `key`: A unique identifier for the cached item.
  - `value`: The value to cache.
  - `absoluteExpiration` (optional): The duration after which the cache entry expires. If `null`, the default cache expiration is used.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `key` or `value` is `null`.

### `Task RemoveAsync(string key)`
Asynchronously removes the cache entry associated with the specified `key`.

- **Parameters**:
  - `key`: A unique identifier for the cached item.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `key` is `null`.

### `Task RemoveByPatternAsync(string pattern)`
Asynchronously removes all cache entries whose keys match the specified `pattern`. The pattern syntax is `Microsoft.Extensions.Caching.Memory.CacheExtensions.GetKeys` compatible (e.g., `user:*`).

- **Parameters**:
  - `pattern`: A string pattern used to match cache keys for removal.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `pattern` is `null`.

### `Task ClearAsync()`
Asynchronously removes all entries from the cache.

### `Task<bool> ExistsAsync(string key)`
Asynchronously checks whether a cache entry exists for the specified `key`.

- **Parameters**:
  - `key`: A unique identifier for the cached item.
- **Return Value**: `true` if the key exists; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `key` is `null`.

## Usage

### Example 1: Basic Caching
