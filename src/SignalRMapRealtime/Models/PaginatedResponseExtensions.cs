#nullable enable

using System.Linq.Expressions;

namespace SignalRMapRealtime.Models;

/// <summary>
/// Extension methods for <see cref="PaginatedResponse{T}"/> providing common pagination operations.
/// </summary>
public static class PaginatedResponseExtensions
{
    private const int DefaultPageSize = 10;

    /// <summary>
    /// Creates a new paginated response with the same items but different page size.
    /// Useful for changing pagination granularity without re-querying the data source.
    /// </summary>
    /// <typeparam name="T">The type of items in the response.</typeparam>
    /// <param name="source">The source paginated response.</param>
    /// <param name="newPageSize">The new page size to use.</param>
    /// <returns>A new paginated response with the same items but recalculated pagination metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newPageSize"/> is less than 1.</exception>
    public static PaginatedResponse<T> WithPageSize<T>(this PaginatedResponse<T> source, int newPageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(newPageSize, 1);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    public static PaginatedResponse<T> Where<T>(this PaginatedResponse<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static PaginatedResponse<TResult> Select<T, TResult>(
        this PaginatedResponse<T> source,
        Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<T> GetCurrentPage<T>(this PaginatedResponse<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Items;
    }
}