#nullable enable

using System.Linq.Expressions;

namespace SignalRMapRealtime.Models;

/// <summary>
/// Extension methods for <see cref="PaginatedResponse{T}"/> providing common pagination operations.
/// </summary>
public static class PaginatedResponseExtensions
{
    /// <summary>
    /// Creates a new paginated response with the same items but different page size.
    /// Useful for changing pagination granularity without re-querying the data source.
    /// </summary>
    /// <typeparam name="T">The type of items in the response.</typeparam>
    /// <param name="source">The source paginated response.</param>
    /// <param name="newPageSize">The new page size to use.</param>
    /// <returns>A new paginated response with the same items but recalculated pagination metadata.</returns>
    public static PaginatedResponse<T> WithPageSize<T>(this PaginatedResponse<T> source, int newPageSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (newPageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(newPageSize), "Page size must be positive.");

        var newPageNumber = Math.Min(source.PageNumber, (int)Math.Ceiling(source.TotalCount / (double)newPageSize));

        return new PaginatedResponse<T>(
            source.Items,
            newPageNumber,
            newPageSize,
            source.TotalCount
        );
    }

    /// <summary>
    /// Creates a new paginated response with the same pagination but filtered items.
    /// Useful for applying additional filtering to already-paginated results without re-querying.
    /// </summary>
    /// <typeparam name="T">The type of items in the response.</typeparam>
    /// <param name="source">The source paginated response.</param>
    /// <param name="predicate">The filter predicate to apply.</param>
    /// <returns>A new paginated response with filtered items but same pagination metadata.</returns>
    public static PaginatedResponse<T> Where<T>(this PaginatedResponse<T> source, Func<T, bool> predicate)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var filteredItems = source.Items.Where(predicate).ToList();

        return new PaginatedResponse<T>(
            filteredItems.AsReadOnly(),
            source.PageNumber,
            source.PageSize,
            source.TotalCount
        );
    }

    /// <summary>
    /// Creates a new paginated response with items projected to a different type.
    /// Useful for transforming paginated results without losing pagination metadata.
    /// </summary>
    /// <typeparam name="T">The source item type.</typeparam>
    /// <typeparam name="TResult">The result item type.</typeparam>
    /// <param name="source">The source paginated response.</param>
    /// <param name="selector">The projection function.</param>
    /// <returns>A new paginated response with projected items.</returns>
    public static PaginatedResponse<TResult> Select<T, TResult>(
        this PaginatedResponse<T> source,
        Func<T, TResult> selector)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        var projectedItems = source.Items.Select(selector).ToList();

        return new PaginatedResponse<TResult>(
            projectedItems.AsReadOnly(),
            source.PageNumber,
            source.PageSize,
            source.TotalCount
        );
    }

    /// <summary>
    /// Gets the current page's items as a read-only collection.
    /// Provides a convenient way to access items without null checks on the Items property.
    /// </summary>
    /// <typeparam name="T">The type of items in the response.</typeparam>
    /// <param name="source">The source paginated response.</param>
    /// <returns>The items on the current page.</returns>
    public static IReadOnlyList<T> GetCurrentPage<T>(this PaginatedResponse<T> source)
    {
        return source?.Items ?? throw new ArgumentNullException(nameof(source));
    }
}