#nullable enable
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
    /// <param name="collection">The collection to add to.</param>
    /// <param name="item">The item to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
    public static void AddIfNotExists<T>(this List<T> collection, T item)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (!collection.Contains(item))
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Adds multiple items to the collection, ignoring duplicates.
    /// </summary>
    /// <param name="collection">The collection to add to.</param>
    /// <param name="items">The items to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="items"/> is <see langword="null"/>.</exception>
    public static void AddRangeIfNotExists<T>(this List<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            collection.AddIfNotExists(item);
        }
    }

    /// <summary>
    /// Removes all items matching a condition.
    /// More readable than using RemoveAll with a lambda.
    /// </summary>
    /// <param name="collection">The collection to modify.</param>
    /// <param name="predicate">The condition to match items for removal.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static void RemoveWhere<T>(this List<T> collection, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(predicate);

        collection.RemoveAll(predicate.Invoke);
    }

    /// <summary>
    /// Gets the first item or a default value if collection is empty.
    /// Returns null if empty (safer than FirstOrDefault).
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <returns>The first item or null if collection is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static T? GetFirstOrNull<T>(this IEnumerable<T> source) where T : class
        => source?.FirstOrDefault();

    /// <summary>
    /// Gets the last item in the collection.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <returns>The last item or null if collection is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static T? GetLastOrNull<T>(this IEnumerable<T> source) where T : class
        => source?.LastOrDefault();

    /// <summary>
    /// Checks if collection is null or empty.
    /// </summary>
    /// <param name="collection">The collection to check.</param>
    /// <returns><see langword="true"/> if collection is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        => collection?.Any() != true;

    /// <summary>
    /// Checks if collection has items.
    /// </summary>
    /// <param name="collection">The collection to check.</param>
    /// <returns><see langword="true"/> if collection has items; otherwise, <see langword="false"/>.</returns>
    public static bool HasItems<T>(this IEnumerable<T>? collection)
        => collection?.Any() ?? false;

    /// <summary>
    /// Distinct items by a specific property.
    /// Useful for removing duplicates based on a key.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key to distinct by.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="keySelector">Function to extract the key from each element.</param>
    /// <returns>Collection with duplicates removed based on the key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

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
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="chunkSize">The size of each chunk.</param>
    /// <returns>Collection of chunks, each containing up to <paramref name="chunkSize"/> items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than 1.</exception>
    public static IEnumerable<List<T>> ChunkBy<T>(
        this IEnumerable<T> source,
        int chunkSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);

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
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection of collections.</param>
    /// <returns>Flattened sequence of all elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Returns items where the condition is true, grouped with items where it's false.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="predicate">The condition to partition by.</param>
    /// <returns>A tuple containing two collections: items where predicate is true, and items where predicate is false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static (IEnumerable<T> True, IEnumerable<T> False) Partition<T>(
        this IEnumerable<T> source,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var trueList = new List<T>();
        var falseList = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
            {
                trueList.Add(item);
            }
            else
            {
                falseList.Add(item);
            }
        }

        return (trueList, falseList);
    }

    /// <summary>
    /// Executes an action on each item in the collection.
    /// Similar to ForEach but works with IEnumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="action">The action to execute for each item.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an async action on each item in the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="action">The async action to execute for each item.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            await action(item).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets items at specified indices from the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="indices">The indices to retrieve.</param>
    /// <returns>Collection of items at the specified indices.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> GetAt<T>(this IEnumerable<T> source, params int[] indices)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(indices);

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
    /// <typeparam name="TSource">The type of source elements.</typeparam>
    /// <typeparam name="TKey">The type of keys.</typeparam>
    /// <typeparam name="TValue">The type of values.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="keySelector">Function to extract the key from each element.</param>
    /// <param name="valueSelector">Function to extract the value from each element.</param>
    /// <returns>Dictionary with unique keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="valueSelector"/> is <see langword="null"/>.</exception>
    public static Dictionary<TKey, TValue> ToDictionaryDistinct<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);

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
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to shuffle.</param>
    /// <returns>New list with items in random order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static List<T> Shuffle<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

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