#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
    /// The cursor for the current page. This is an opaque token that represents the position in the dataset.
    /// Used for cursor-based pagination to provide stable pagination when data changes.
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// The cursor for the next page. This is an opaque token that represents the position after the current page.
    /// Used for cursor-based pagination to provide stable pagination when data changes.
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// Indicates whether cursor-based pagination is being used (true) or offset-based pagination (false).
    /// </summary>
    public bool IsCursorPagination { get; set; }

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

    /// <summary>
    /// Creates a cursor-based paginated response from a sorted list.
    /// Uses cursor-based pagination for stable pagination when data changes.
    /// </summary>
    /// <param name="source">The sorted source collection.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cursor">The cursor token for the current page (null for first page).</param>
    /// <returns>A paginated response with cursor information.</returns>
    public static PaginatedResponse<T> FromCursorList(
        IEnumerable<T> source,
        int pageSize,
        string? cursor = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (pageSize < 1) pageSize = 10;

        var enumerable = source.ToList();
        var totalCount = enumerable.Count;

        // Decode cursor to get the cursor value
        var (_, lastCursor) = DecodeCursor(cursor);

        // Filter items based on cursor
        IEnumerable<T> filteredItems = enumerable;
        if (!string.IsNullOrEmpty(lastCursor))
        {
            // Get the cursor value to compare against
            var cursorParts = DecodeCursorValue(lastCursor);

            if (cursorParts.Id > 0)
            {
                // Filter to items with Id > cursorId
                // This ensures we don't get duplicates and maintain stable pagination
                filteredItems = enumerable.Where(item =>
                {
                    var itemIdProperty = typeof(T).GetProperty("Id");
                    if (itemIdProperty == null) return true;
                    var itemId = itemIdProperty.GetValue(item) as int?;
                    return itemId > cursorParts.Id;
                });
            }
        }

        // Get items for current page
        var items = filteredItems
            .Take(pageSize)
            .ToList();

        // Calculate next cursor
        string? nextCursor = null;
        if (items.Count == pageSize)
        {
            // Get the last item's cursor value
            var lastItem = items[^1];
            nextCursor = CreateCursorFromItem(lastItem);
        }

        return new PaginatedResponse<T>(items, 1, pageSize, totalCount)
        {
            Cursor = cursor,
            NextCursor = nextCursor,
            IsCursorPagination = true
        };
    }

    /// <summary>
    /// Creates a cursor-based paginated response from a sorted IQueryable.
    /// More efficient for database queries.
    /// </summary>
    /// <param name="source">The sorted source query.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cursor">The cursor token for the current page (null for first page).</param>
    /// <returns>A paginated response with cursor information.</returns>
    public static async Task<PaginatedResponse<T>> FromCursorQueryableAsync(
        IQueryable<T> source,
        int pageSize,
        string? cursor = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (pageSize < 1) pageSize = 10;

        // Decode cursor to get the cursor value
        var (_, lastCursor) = DecodeCursor(cursor);

        // Apply cursor filtering if provided
        var filteredSource = source;
        if (!string.IsNullOrEmpty(lastCursor))
        {
            var cursorParts = DecodeCursorValue(lastCursor);

            if (cursorParts.Id > 0)
            {
                // Filter to items with Id > cursorId
                // This ensures stable pagination even when data changes
                filteredSource = filteredSource.Where(item => EF.Property<int>(item, "Id") > cursorParts.Id);
            }
        }

        var totalCountTask = filteredSource.CountAsync();

        // Get items for current page
        var items = await filteredSource
            .Take(pageSize)
            .ToListAsync();

        // Calculate next cursor
        string? nextCursor = null;
        if (items.Count == pageSize)
        {
            // Get the last item's cursor value
            var lastItem = items[^1];
            nextCursor = CreateCursorFromItem(lastItem);
        }

        return new PaginatedResponse<T>(items, 1, pageSize, await totalCountTask)
        {
            Cursor = cursor,
            NextCursor = nextCursor,
            IsCursorPagination = true
        };
    }

    /// <summary>
    /// Creates a cursor from an item. The cursor encodes the item's position key.
    /// </summary>
    /// <param name="item">The item to create a cursor from.</param>
    /// <returns>A base64-encoded cursor string.</returns>
    private static string CreateCursorFromItem(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Use reflection to get Id and UpdatedAt properties
        var idProperty = typeof(T).GetProperty("Id");
        var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");

        if (idProperty == null)
        {
            throw new InvalidOperationException("Item type must have an Id property for cursor pagination");
        }

        var idValue = idProperty.GetValue(item);
        var updatedAtValue = updatedAtProperty?.GetValue(item) as DateTime?;

        if (idValue == null)
        {
            throw new InvalidOperationException("Id property cannot be null");
        }

        // Create cursor data: "id:updatedAtTicks" format
        // Using ticks for better precision and to avoid date formatting issues
        var cursorData = $"{idValue}:{updatedAtValue?.Ticks ?? 0}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(cursorData));
    }

    /// <summary>
    /// Decodes a cursor token into its components.
    /// </summary>
    /// <param name="cursor">The cursor token to decode.</param>
    /// <returns>A tuple containing the cursor Id and UpdatedAt ticks.</returns>
    private static (int Id, long UpdatedAtTicks) DecodeCursorValue(string cursor)
    {
        try
        {
            var cursorData = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = cursorData.Split(':');

            if (parts.Length >= 2 && int.TryParse(parts[0], out var id) && long.TryParse(parts[1], out var ticks))
            {
                return (id, ticks);
            }
        }
        catch
        {
            // If cursor is invalid, return default values
        }

        return (0, 0);
    }

    /// <summary>
    /// Decodes a cursor token into the starting position for pagination.
    /// </summary>
    /// <param name="cursor">The cursor token to decode.</param>
    /// <returns>A tuple containing the number of items to skip and the last cursor value.</returns>
    private static (int SkipCount, string? LastCursor) DecodeCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
        {
            return (0, null);
        }

        try
        {
            var cursorData = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = cursorData.Split(':');

            if (parts.Length >= 2 && long.TryParse(parts[1], out var ticks))
            {
                // The cursor represents the position AFTER the last item we saw
                // So we need to skip past all items with Id <= cursorId
                return (0, cursor);
            }
        }
        catch
        {
            // If cursor is invalid, start from beginning
        }

        return (0, null);
    }
}