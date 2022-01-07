// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

/// <summary>
/// Extension methods for collection manipulation and querying.
/// Provides utilities for common operations on IEnumerable and List collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Adds an item to the collection if it doesn't already exist.
    /// Uses equality comparison to determine duplicates.
    /// </summary>
    public static void AddIfNotExists<T>(this List<T> collection, T item)
    {
        if (!collection.Contains(item))
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Adds multiple items to the collection, ignoring duplicates.
    /// </summary>
    public static void AddRangeIfNotExists<T>(this List<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.AddIfNotExists(item);
        }
    }

    /// <summary>
    /// Removes all items matching a condition.
    /// More readable than using RemoveAll with a lambda.
    /// </summary>
    public static void RemoveWhere<T>(this List<T> collection, Func<T, bool> predicate)
    {
        collection.RemoveAll(predicate);
    }

    /// <summary>
    /// Gets the first item or a default value if collection is empty.
    /// Returns null if empty (safer than FirstOrDefault).
    /// </summary>
    public static T? GetFirstOrNull<T>(this IEnumerable<T> source) where T : class
    {
        return source?.FirstOrDefault();
    }

    /// <summary>
    /// Gets the last item in the collection.
    /// </summary>
    public static T? GetLastOrNull<T>(this IEnumerable<T> source) where T : class
    {
        return source?.LastOrDefault();
    }

    /// <summary>
    /// Checks if collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() != true;
    }

    /// <summary>
    /// Checks if collection has items.
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() ?? false;
    }

    /// <summary>
    /// Distinct items by a specific property.
    /// Useful for removing duplicates based on a key.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seen.Add(key))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Chunks a collection into smaller groups.
    /// Useful for batch processing.
    /// </summary>
    public static IEnumerable<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
    {
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var chunk = new List<T> { enumerator.Current };
            while (chunk.Count < chunkSize && enumerator.MoveNext())
            {
                chunk.Add(enumerator.Current);
            }
            yield return chunk;
        }
    }

    /// <summary>
    /// Flattens nested enumerables into a single sequence.
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Returns items where the condition is true, grouped with items where it's false.
    /// </summary>
    public static (IEnumerable<T> True, IEnumerable<T> False) Partition<T>(
        this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var trueList = new List<T>();
        var falseList = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                trueList.Add(item);
            else
                falseList.Add(item);
        }

        return (trueList, falseList);
    }

    /// <summary>
    /// Executes an action on each item in the collection.
    /// Similar to ForEach but works with IEnumerable.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an async action on each item in the collection.
    /// </summary>
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
        }
    }

    /// <summary>
    /// Gets items at specified indices from the collection.
    /// </summary>
    public static IEnumerable<T> GetAt<T>(this IEnumerable<T> source, params int[] indices)
    {
        var list = source.ToList();
        foreach (var index in indices)
        {
            if (index >= 0 && index < list.Count)
            {
                yield return list[index];
            }
        }
    }

    /// <summary>
    /// Converts collection to a dictionary with duplicate key handling.
    /// Takes the first item when duplicate keys are encountered.
    /// </summary>
    public static Dictionary<TKey, TValue> ToDictionaryDistinct<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!result.ContainsKey(key))
            {
                result[key] = valueSelector(item);
            }
        }
        return result;
    }

    /// <summary>
    /// Shuffles the collection randomly.
    /// </summary>
    public static List<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        var random = new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
        return list;
    }
}
