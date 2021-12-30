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
    public static (int PageNumber, int PageSize) NormalizePaginationParameters(
        int pageNumber, int pageSize, int maxPageSize = 100)
    {
        var validPageNumber = Math.Max(1, pageNumber);
        var validPageSize = Math.Min(Math.Max(1, pageSize), maxPageSize);

        return (validPageNumber, validPageSize);
    }

    /// <summary>
    /// Validates that pagination parameters are within acceptable ranges.
    /// Throws ArgumentException if parameters are invalid.
    /// </summary>
    public static void ValidatePaginationParameters(int pageNumber, int pageSize, int maxPageSize = 100)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1", nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentException("Page size must be at least 1", nameof(pageSize));

        if (pageSize > maxPageSize)
            throw new ArgumentException($"Page size cannot exceed {maxPageSize}", nameof(pageSize));
    }

    /// <summary>
    /// Applies pagination to an IEnumerable collection.
    /// Returns items on the specified page after sorting.
    /// </summary>
    public static IEnumerable<T> ApplyPagination<T>(
        this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);

        return source
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);
    }

    /// <summary>
    /// Applies pagination to an IQueryable collection (LINQ-to-SQL).
    /// More efficient than ApplyPagination for database queries.
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> source, int pageNumber, int pageSize)
    {
        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);

        return source
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);
    }

    /// <summary>
    /// Applies pagination with sorting to an IQueryable collection.
    /// Combines sorting and pagination for efficient database queries.
    /// </summary>
    public static IQueryable<T> ApplyPaginationWithSort<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
    {
        var (validPageNumber, validPageSize) = NormalizePaginationParameters(pageNumber, pageSize);

        return orderBy(source)
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize);
    }

    /// <summary>
    /// Applies pagination and returns total count separately.
    /// Useful when you need both the paginated items and total count.
    /// </summary>
    public static (IEnumerable<T> Items, int TotalCount) GetPagedResults<T>(
        this IEnumerable<T> source, int pageNumber, int pageSize)
    {
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
    public static (IQueryable<T> Items, int TotalCount) GetPagedQueryableResults<T>(
        this IQueryable<T> source, int pageNumber, int pageSize)
    {
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
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        var validPageNumber = Math.Max(1, pageNumber);
        var validPageSize = Math.Max(1, pageSize);
        return (validPageNumber - 1) * validPageSize;
    }

    /// <summary>
    /// Calculates the total number of pages needed to display all items.
    /// </summary>
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0) return 0;
        return (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Checks if a page number is valid for the given total count and page size.
    /// </summary>
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
    public static PaginationInfo GetPaginationInfo(
        int pageNumber, int pageSize, int totalCount)
    {
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
public class PaginationInfo
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
