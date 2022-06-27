#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Models;

/// <summary>
/// Extension methods for <see cref="PagedRequest"/> and derived classes.
/// Provides common utility methods for pagination and filtering scenarios.
/// </summary>
public static class PagedRequestExtensions
{
    /// <summary>
    /// Calculates the skip count for pagination based on PageNumber and PageSize.
    /// </summary>
    /// <param name="request">The paged request. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns>The number of items to skip.</returns>
    public static int CalculateSkip(this PagedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return (request.GetValidPageNumber() - 1) * request.GetValidPageSize();
    }

    /// <summary>
    /// Calculates the take count for pagination based on PageSize.
    /// Ensures the take count does not exceed the maximum allowed value.
    /// </summary>
    /// <param name="request">The paged request. Cannot be <see langword="null"/>.</param>
    /// <param name="maxTake">Maximum allowed take count. Defaults to 100.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxTake"/> is less than 1.</exception>
    /// <returns>The number of items to take.</returns>
    public static int CalculateTake(this PagedRequest request, int maxTake = 100)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTake, 1);
        return Math.Min(request.GetValidPageSize(), maxTake);
    }

    /// <summary>
    /// Determines if the request has any filter criteria set (search, sort, filter, etc.).
    /// </summary>
    /// <param name="request">The paged request. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns><see langword="true"/> if any filter criteria is set; otherwise <see langword="false"/>.</returns>
    public static bool HasFilters(this PagedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return !string.IsNullOrWhiteSpace(request.SearchQuery) ||
               !string.IsNullOrWhiteSpace(request.SortBy) ||
               !string.IsNullOrWhiteSpace(request.Filter);
    }

    /// <summary>
    /// Gets the effective page size, ensuring it's within valid bounds [1, 100].
    /// </summary>
    /// <param name="request">The paged request. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns>Validated page size in the range [1, 100].</returns>
    public static int GetValidPageSize(this PagedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Math.Clamp(request.PageSize, 1, 100);
    }

    /// <summary>
    /// Gets the effective page number, ensuring it's at least 1.
    /// </summary>
    /// <param name="request">The paged request. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns>Validated page number (1-indexed).</returns>
    public static int GetValidPageNumber(this PagedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Math.Max(request.PageNumber, 1);
    }

    /// <summary>
    /// Creates a shallow copy of the request with updated pagination parameters.
    /// </summary>
    /// <param name="request">The original paged request. Cannot be <see langword="null"/>.</param>
    /// <param name="pageNumber">New page number.</param>
    /// <param name="pageSize">New page size.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns>A new instance with updated pagination.</returns>
    public static PagedRequest WithPagination(this PagedRequest request, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchQuery = request.SearchQuery,
            SortBy = request.SortBy,
            Filter = request.Filter
        };
    }

    /// <summary>
    /// Creates a shallow copy of the request with updated search query.
    /// </summary>
    /// <param name="request">The original paged request. Cannot be <see langword="null"/>.</param>
    /// <param name="searchQuery">New search query.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns>A new instance with updated search query.</returns>
    public static PagedRequest WithSearchQuery(this PagedRequest request, string? searchQuery)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PagedRequest
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchQuery = searchQuery,
            SortBy = request.SortBy,
            Filter = request.Filter
        };
    }

    /// <summary>
    /// Creates a shallow copy of the request with updated sort criteria.
    /// </summary>
    /// <param name="request">The original paged request. Cannot be <see langword="null"/>.</param>
    /// <param name="sortBy">New sort field.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <returns>A new instance with updated sort criteria.</returns>
    public static PagedRequest WithSortBy(this PagedRequest request, string? sortBy)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PagedRequest
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchQuery = request.SearchQuery,
            SortBy = sortBy,
            Filter = request.Filter
        };
    }

}