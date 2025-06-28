// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Models;

/// <summary>
/// Generic paginated response wrapper that includes metadata about pagination.
/// Used for list endpoints to enable client-side pagination of large datasets.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The collection of items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

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
    /// Total number of pages available.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether there is a next page available.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Indicates whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Constructor that calculates pagination metadata automatically.
    /// </summary>
    public PaginatedResponse(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = PageNumber < TotalPages;
        HasPreviousPage = PageNumber > 1;
    }

    /// <summary>
    /// Factory method to create an empty paginated response.
    /// </summary>
    public static PaginatedResponse<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedResponse<T>([], pageNumber, pageSize, 0);
    }

    /// <summary>
    /// Factory method to create a paginated response from a complete list.
    /// Automatically handles slicing based on page number and size.
    /// </summary>
    public static PaginatedResponse<T> FromList(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var enumerable = source.ToList();
        var totalCount = enumerable.Count;
        var items = enumerable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResponse<T>(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Factory method to create a paginated response from an IQueryable.
    /// More efficient than FromList for LINQ-to-SQL queries.
    /// </summary>
    public static async Task<PaginatedResponse<T>> FromQueryableAsync(
        IQueryable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = source.Count();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<T>(items, pageNumber, pageSize, totalCount);
    }
}
