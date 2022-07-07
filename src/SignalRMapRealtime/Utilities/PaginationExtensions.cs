#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

/// <summary>
/// Extension methods for pagination operations on IEnumerable and IQueryable collections.
/// Provides utilities for implementing server-side pagination with validation.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Validates and normalizes pagination parameters.
    /// Ensures pageNumber is at least 1 and pageSize is within acceptable bounds.
    /// </summary>
    /// <param name="pageNumber">The requested page number (1-based).</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="maxPageSize">The maximum allowed page size. Defaults to 100.</param>
    /// <returns>A tuple containing the validated page number and page size.</returns>
    public static (int PageNumber, int PageSize) NormalizePaginationParameters(
        int pageNumber, int pageSize, int maxPageSize = 100)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1");
        if (maxPageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPageSize), "Maximum page size must be at least 1");

        var validPageNumber = pageNumber;
        var validPageSize = Math.Min(pageSize, maxPageSize);

        return (validPageNumber, validPageSize);
    }

    /// <summary>
    /// Validates that pagination parameters are within acceptable ranges.
    /// Throws ArgumentException if parameters are invalid.
    /// </summary>
    /// <param name="pageNumber">The page number to validate.</param>
    /// <param name="pageSize">The page size to validate.</param>
    /// <param name="maxPageSize">The maximum allowed page size. Defaults to 100.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1, pageSize is less than 1, or pageSize exceeds maxPageSize.
    /// </exception>
    public static void ValidatePaginationParameters(int pageNumber, int pageSize, int maxPageSize = 100)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1");

        if (pageSize > maxPageSize)
            throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size cannot exceed {maxPageSize}");
    }

    /// <summary>
    /// Applies pagination to an IEnumerable collection.
    /// Returns items on the specified page after sorting.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to paginate.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An enumerable containing only the items for the requested page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1 or pageSize is less than 1.
    /// </exception>
    public static IEnumerable<T> ApplyPagination<T>(
        this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);

        return source
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);
    }

    /// <summary>
    /// Applies pagination to an IQueryable collection (LINQ-to-SQL).
    /// More efficient than ApplyPagination for database queries.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="source">The source query to paginate.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A queryable containing only the items for the requested page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1 or pageSize is less than 1.
    /// </exception>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);

        return source
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);
    }

    /// <summary>
    /// Applies pagination with sorting to an IQueryable collection.
    /// Combines sorting and pagination for efficient database queries.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="source">The source query to paginate.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="orderBy">A function that applies ordering to the query.</param>
    /// <returns>A queryable containing only the items for the requested page, in the specified order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or orderBy is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1 or pageSize is less than 1.
    /// </exception>
    public static IQueryable<T> ApplyPaginationWithSort<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(orderBy);

        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);

        return orderBy(source)
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);
    }

    /// <summary>
    /// Applies pagination and returns total count separately.
    /// Useful when you need both the paginated items and total count.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to paginate.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A tuple containing the paginated items and the total count of all items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1 or pageSize is less than 1.
    /// </exception>
    public static (IEnumerable<T> Items, int TotalCount) GetPagedResults<T>(
        this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);
        var enumerable = source.ToList();
        var totalCount = enumerable.Count;

        var items = enumerable
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);

        return (items, totalCount);
    }

    /// <summary>
    /// Applies pagination to IQueryable and returns total count separately.
    /// More efficient than GetPagedResults for database queries.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="source">The source query to paginate.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A tuple containing the paginated query and the total count of all items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1 or pageSize is less than 1.
    /// </exception>
    public static (IQueryable<T> Items, int TotalCount) GetPagedQueryableResults<T>(
        this IQueryable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);
        var totalCount = source.Count();

        var items = source
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);

        return (items, totalCount);
    }

    /// <summary>
    /// Calculates the skip count for a given page number and size.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The number of items to skip before the requested page.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1 or pageSize is less than 1.
    /// </exception>
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1");

        return (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Calculates the total number of pages needed to display all items.
    /// </summary>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The total number of pages required.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageSize is less than or equal to 0.
    /// </exception>
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0");

        return (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Checks if a page number is valid for the given total count and page size.
    /// </summary>
    /// <param name="pageNumber">The page number to check.</param>
    /// <param name="totalCount">The total number of items.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>True if the page number is valid; otherwise, false.</returns>
    public static bool IsValidPageNumber(int pageNumber, int totalCount, int pageSize)
    {
        if (pageNumber < 1) return false;
        if (pageSize <= 0) return false;

        var totalPages = CalculateTotalPages(totalCount, pageSize);
        return pageNumber <= totalPages;
    }

    /// <summary>
    /// Gets information about pagination boundaries (first page, last page, etc.).
    /// </summary>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A PaginationInfo object containing pagination metadata.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pageNumber is less than 1, pageSize is less than 1, or totalCount is negative.
    /// </exception>
    public static PaginationInfo GetPaginationInfo(
        int pageNumber, int pageSize, int totalCount)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1");
        if (totalCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative");

        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);
        var totalPages = CalculateTotalPages(totalCount, validPageSize);
        var skip = CalculateSkip(validPageNumber, validPageSize);

        return new PaginationInfo
        {
            PageNumber = validPageNumber,
            PageSize = validPageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Skip = skip,
            IsFirstPage = validPageNumber == 1,
            IsLastPage = validPageNumber >= totalPages,
            HasPreviousPage = validPageNumber > 1,
            HasNextPage = validPageNumber < totalPages,
            ItemsOnPage = Math.Min(validPageSize, Math.Max(0, totalCount - skip))
        };
    }
}

/// <summary>
/// Contains information about pagination state.
/// </summary>
public sealed class PaginationInfo
{
    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total count of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Number of items to skip from the beginning.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Whether this is the first page.
    /// </summary>
    public bool IsFirstPage { get; set; }

    /// <summary>
    /// Whether this is the last page.
    /// </summary>
    public bool IsLastPage { get; set; }

    /// <summary>
    /// Whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Whether a next page exists.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Number of items on the current page.
    /// </summary>
    public int ItemsOnPage { get; set; }
}