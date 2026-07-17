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
```

## CollectionExtensions

`CollectionExtensions` provides a set of handy LINQ‑style extension methods for working with `IEnumerable<T>` and `List<T>` collections. It includes helpers for adding items without duplicates, chunking, partitioning, safe retrieval, and other common collection tasks.

**Usage example**

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SignalRMapRealtime.Utilities;

class Program
{
    static async Task Main()
    {
        var numbers = new List<int> { 1, 2, 2, 3, 4, 5 };

        // Add items if they don't already exist
        numbers.AddIfNotExists(6);
        numbers.AddRangeIfNotExists(new[] { 3, 7 });

        // Remove even numbers
        numbers.RemoveWhere(n => n % 2 == 0);

        // Safe first/last retrieval
        int? first = numbers.GetFirstOrNull();
        int? last = numbers.GetLastOrNull();

        // Null/empty checks
        bool isEmpty = numbers.IsNullOrEmpty();
        bool hasItems = numbers.HasItems();

        // Distinct by key
        var distinct = numbers.DistinctBy(n => n);

        // Chunk into groups of 2
        var chunks = numbers.ChunkBy(2);

        // Flatten nested collections
        var nested = new List<List<int>> { new() { 1, 2 }, new() { 3, 4 } };
        var flat = nested.Flatten();

        // Partition into even and odd numbers
        var (evens, odds) = numbers.Partition(n => n % 2 == 0);

        // Iterate with actions
        numbers.ForEach(n => Console.WriteLine(n));
        await numbers.ForEachAsync(async n => await Task.Delay(10));

        // Get items at specific indices
        var at = numbers.GetAt(0, 2);

        // Create a dictionary, keeping the first occurrence of each key
        var dict = numbers.ToDictionaryDistinct(n => n, n => n.ToString());

        // Shuffle the collection
        var shuffled = numbers.Shuffle();
    }
}
```

## PaginationExtensions

`PaginationExtensions` offers a comprehensive set of helpers for validating pagination parameters, applying pagination to `IEnumerable<T>` and `IQueryable<T>` collections, calculating pagination metadata, and retrieving pagination information such as total pages, skip count, and page boundaries.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using SignalRMapRealtime.Utilities;

var numbers = Enumerable.Range(1, 25).ToList();

// Validate parameters (throws if invalid)
PaginationExtensions.ValidatePaginationParameters(2, 10);

// Normalize parameters (returns a tuple with safe values)
var (pageNumber, pageSize) = PaginationExtensions.NormalizePaginationParameters(2, 10);

// Apply pagination to an IEnumerable<T>
IEnumerable<int> pageItems = numbers.ApplyPagination(pageNumber, pageSize);
Console.WriteLine($"Page {pageNumber} items: {string.Join(", ", pageItems)}");

// Apply pagination to an IQueryable<T>
IQueryable<int> query = numbers.AsQueryable();
IQueryable<int> pagedQuery = query.ApplyPagination(pageNumber, pageSize);
Console.WriteLine($"Queryable count on page: {pagedQuery.Count()}");

// Apply pagination with sorting to an IQueryable<T>
IQueryable<int> sortedPaged = query.ApplyPaginationWithSort(
    pageNumber,
    pageSize,
    q => q.OrderByDescending(x => x));
Console.WriteLine($"First item of sorted page: {sortedPaged.First()}");

// Get paged results together with total count for IEnumerable<T>
var (items, totalCount) = numbers.GetPagedResults(pageNumber, pageSize);
Console.WriteLine($"Total items: {totalCount}, items on page: {string.Join(", ", items)}");

// Get paged results together with total count for IQueryable<T>
var (queryItems, queryTotal) = query.GetPagedQueryableResults(pageNumber, pageSize);
Console.WriteLine($"Total query items: {queryTotal}, page count: {queryItems.Count()}");

// Helper calculations
int skip = PaginationExtensions.CalculateSkip(pageNumber, pageSize);
int totalPages = PaginationExtensions.CalculateTotalPages(totalCount, pageSize);
bool isValidPage = PaginationExtensions.IsValidPageNumber(pageNumber, totalCount, pageSize);
PaginationInfo info = PaginationExtensions.GetPaginationInfo(pageNumber, pageSize, totalCount);

Console.WriteLine($"Skip: {skip}, TotalPages: {totalPages}, IsValidPage: {isValidPage}");
Console.WriteLine($"Info – IsFirstPage: {info.IsFirstPage}, HasNextPage: {info.HasNextPage}");
```

// ... (rest of the file remains unchanged)
