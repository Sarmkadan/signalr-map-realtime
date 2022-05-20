# CollectionExtensions

The `CollectionExtensions` class provides a comprehensive set of static extension methods designed to enhance standard .NET collection operations with common utility patterns. It addresses frequent requirements such as safe element retrieval, conditional insertion, batch processing, and data transformation, thereby reducing boilerplate code and improving readability when working with `IEnumerable<T>`, `ICollection<T>`, and `IList<T>` instances within the `signalr-map-realtime` project.

## API

### `AddIfNotExists<T>`
Adds an item to the collection only if an equivalent item does not already exist.
*   **Parameters**: `this ICollection<T> collection`, `T item`.
*   **Return Value**: `void`.
*   **Exceptions**: Throws `ArgumentNullException` if the collection is null.

### `AddRangeIfNotExists<T>`
Iterates through a sequence of items and adds each one to the collection only if it is not already present.
*   **Parameters**: `this ICollection<T> collection`, `IEnumerable<T> items`.
*   **Return Value**: `void`.
*   **Exceptions**: Throws `ArgumentNullException` if the collection or the items sequence is null.

### `RemoveWhere<T>`
Removes all elements from the collection that match a specified predicate.
*   **Parameters**: `this ICollection<T> collection`, `Func<T, bool> predicate`.
*   **Return Value**: `void`.
*   **Exceptions**: Throws `ArgumentNullException` if the collection or predicate is null.

### `GetFirstOrNull<T>`
Returns the first element of a sequence, or the default value (`null` for reference types, `default` for value types) if the sequence contains no elements.
*   **Parameters**: `this IEnumerable<T> source`.
*   **Return Value**: `T?`.
*   **Exceptions**: Throws `ArgumentNullException` if the source is null.

### `GetLastOrNull<T>`
Returns the last element of a sequence, or the default value if the sequence contains no elements.
*   **Parameters**: `this IEnumerable<T> source`.
*   **Return Value**: `T?`.
*   **Exceptions**: Throws `ArgumentNullException` if the source is null.

### `IsNullOrEmpty<T>`
Determines whether a collection is null or contains no elements.
*   **Parameters**: `this IEnumerable<T>? source`.
*   **Return Value**: `bool` (`true` if null or empty; otherwise `false`).
*   **Exceptions**: None.

### `HasItems<T>`
Determines whether a collection is not null and contains at least one element.
*   **Parameters**: `this IEnumerable<T>? source`.
*   **Return Value**: `bool` (`true` if not null and has items; otherwise `false`).
*   **Exceptions**: None.

### `DistinctBy<T, TKey>`
Returns a sequence of elements where duplicates are removed based on a specified key selector function.
*   **Parameters**: `this IEnumerable<T> source`, `Func<T, TKey> keySelector`.
*   **Return Value**: `IEnumerable<T>`.
*   **Exceptions**: Throws `ArgumentNullException` if source or keySelector is null.

### `ChunkBy<T>`
Splits the input sequence into chunks of lists. (Note: Based on signature `IEnumerable<List<T>> ChunkBy<T>`, this likely splits into fixed-size chunks or logical groups, returning an enumerable of lists).
*   **Parameters**: `this IEnumerable<T> source`.
*   **Return Value**: `IEnumerable<List<T>>`.
*   **Exceptions**: Throws `ArgumentNullException` if the source is null.

### `Flatten<T>`
Flattens a hierarchical or nested structure into a single linear sequence.
*   **Parameters**: `this IEnumerable<T> source`.
*   **Return Value**: `IEnumerable<T>`.
*   **Exceptions**: Throws `ArgumentNullException` if the source is null. Behavior depends on internal logic regarding how nesting is detected (e.g., if T implements IEnumerable).

### `Partition<T>`
Splits a collection into two separate sequences based on a predicate: one containing elements that match the predicate (True) and one containing those that do not (False).
*   **Parameters**: `this IEnumerable<T> source`, `Func<T, bool> predicate`.
*   **Return Value**: `(IEnumerable<T> True, IEnumerable<T> False)`.
*   **Exceptions**: Throws `ArgumentNullException` if source or predicate is null.

### `ForEach<T>`
Executes a specified action for each element in the collection.
*   **Parameters**: `this IEnumerable<T> source`, `Action<T> action`.
*   **Return Value**: `void`.
*   **Exceptions**: Throws `ArgumentNullException` if source or action is null.

### `ForEachAsync<T>`
Asynchronously executes a specified function for each element in the collection.
*   **Parameters**: `this IEnumerable<T> source`, `Func<T, Task> asyncAction`.
*   **Return Value**: `Task`.
*   **Exceptions**: Throws `ArgumentNullException` if source or asyncAction is null. Propagates exceptions thrown by the async action.

### `GetAt<T>`
Retrieves the element at a specific index, returning the default value if the index is out of range instead of throwing an exception.
*   **Parameters**: `this IEnumerable<T> source`, `int index`.
*   **Return Value**: `T` (or default if out of range).
*   **Exceptions**: Throws `ArgumentNullException` if the source is null.

### `ToDictionaryDistinct<TSource, TKey, TValue>`
Creates a `Dictionary<TKey, TValue>` from a sequence, ensuring that keys are distinct. If duplicate keys are encountered, the behavior typically involves taking the first occurrence or skipping subsequent ones to prevent `ArgumentException`.
*   **Parameters**: `this IEnumerable<TSource> source`, `Func<TSource, TKey> keySelector`, `Func<TSource, TValue> valueSelector`.
*   **Return Value**: `Dictionary<TKey, TValue>`.
*   **Exceptions**: Throws `ArgumentNullException` if source or selectors are null.

### `Shuffle<T>`
Randomizes the order of elements in the collection and returns the result as a new list.
*   **Parameters**: `this IEnumerable<T> source`.
*   **Return Value**: `List<T>`.
*   **Exceptions**: Throws `ArgumentNullException` if the source is null.

## Usage

### Example 1: Safe Retrieval and Conditional Updates
This example demonstrates retrieving elements safely without exception handling overhead and updating a collection only with unique items.

```csharp
using System;
using System.Collections.Generic;
using SignalRMapRealtime.Extensions; // Namespace assumption

public class ConnectionManager
{
    private readonly ICollection<string> _activeConnections = new List<string>();

    public void ProcessConnections(IEnumerable<string> incomingIds)
    {
        // Safely get the first connection without try-catch
        var primaryConnection = incomingIds.GetFirstOrNull();
        
        if (primaryConnection != null)
        {
            Console.WriteLine($"Primary: {primaryConnection}");
        }

        // Add incoming IDs only if they don't already exist in the active set
        _activeConnections.AddRangeIfNotExists(incomingIds);

        // Remove connections that start with "temp_"
        _activeConnections.RemoveWhere(id => id.StartsWith("temp_"));
    }
}
```

### Example 2: Data Transformation and Partitioning
This example illustrates partitioning data based on a condition and converting a sequence to a dictionary while handling potential key collisions gracefully.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using SignalRMapRealtime.Extensions;

public class DataProcessor
{
    public void ProcessMetrics(IEnumerable<Metric> metrics)
    {
        // Split metrics into valid and invalid sets
        var (validMetrics, invalidMetrics) = metrics.Partition(m => m.IsValid);

        // Create a dictionary ensuring distinct keys (e.g., by MetricId)
        // If duplicate IDs exist, ToDictionaryDistinct handles them without throwing
        var metricMap = validMetrics.ToDictionaryDistinct(
            m => m.Id,
            m => m.Value
        );

        // Perform async operations on valid items
        // Note: Awaiting the task ensures all operations complete
        validMetrics.ForEachAsync(async m => 
        {
            await SaveToDatabaseAsync(m);
        }).Wait();
    }

    private Task SaveToDatabaseAsync(Metric m) => Task.CompletedTask;
}

public class Metric 
{ 
    public int Id { get; set; } 
    public string Value { get; set; } 
    public bool IsValid { get; set; } 
}
```

## Notes

*   **Deferred Execution**: Methods returning `IEnumerable<T>` (such as `DistinctBy`, `ChunkBy`, `Flatten`, `Partition`, and `GetAt` depending on implementation details) utilize deferred execution. The query is not evaluated until the result is enumerated (e.g., via `foreach` or `.ToList()`).
*   **Thread Safety**: These extension methods are **not** inherently thread-safe. If the underlying collection (e.g., `List<T>`) is modified by another thread while `ForEach`, `RemoveWhere`, or enumeration-based methods are executing, a `InvalidOperationException` may occur. Callers must ensure external synchronization when accessing shared collections from multiple threads.
*   **Null Handling**: Most methods throw `ArgumentNullException` immediately if the source collection or provided delegates (`Func`, `Action`) are null. However, `IsNullOrEmpty` and `HasItems` explicitly accept null sources as valid input states.
*   **Shuffle Behavior**: The `Shuffle` method returns a new `List<T>` and does not modify the original source sequence. The randomness relies on the internal random number generator implementation; rapid successive calls in tight loops may require caution depending on the seeding strategy used internally.
*   **Distinct Operations**: `ToDictionaryDistinct` and `DistinctBy` rely on the default equality comparer for the key type unless specified otherwise. Ensure that the key type implements appropriate `GetHashCode` and `Equals` logic for accurate deduplication.
