# PaginatedResponse

`PaginatedResponse<T>` is a generic container class designed to encapsulate a subset of data along with its corresponding pagination metadata. It facilitates the transfer of paginated datasets between service layers and clients, ensuring consistent responses for collection-based queries by providing necessary details such as the current page, total item count, and navigation flags.

## API

### Properties

*   **`IReadOnlyList<T> Items`**: Gets the collection of items contained within the current page.
*   **`int PageNumber`**: Gets the 1-based index of the current page.
*   **`int PageSize`**: Gets the maximum number of items allowed per page.
*   **`int TotalCount`**: Gets the total number of items across all pages.
*   **`int TotalPages`**: Gets the total number of pages available based on `TotalCount` and `PageSize`.
*   **`bool HasNextPage`**: Gets a value indicating whether there are pages following the current page.
*   **`bool HasPreviousPage`**: Gets a value indicating whether there are pages preceding the current page.

### Methods & Members

*   **`PaginatedResponse()`**: Initializes a new instance of the `PaginatedResponse<T>` class.
*   **`static PaginatedResponse<T> Empty`**: Gets a static instance representing an empty result set.
*   **`static PaginatedResponse<T> FromList(IList<T> items, int pageNumber, int pageSize, int totalCount)`**: Creates a new `PaginatedResponse<T>` instance from an existing list and pagination parameters.
*   **`static async Task<PaginatedResponse<T>> FromQueryableAsync(IQueryable<T> source, int pageNumber, int pageSize)`**: Asynchronously creates a `PaginatedResponse<T>` by executing the necessary count and pagination queries against the provided `IQueryable` source.

## Usage

### Creating a Response from IQueryable

```csharp
public async Task<PaginatedResponse<Product>> GetProductsAsync(int page, int size)
{
    IQueryable<Product> query = _dbContext.Products.OrderBy(p => p.Name);
    return await PaginatedResponse<Product>.FromQueryableAsync(query, page, size);
}
```

### Creating a Response from an Existing List

```csharp
public PaginatedResponse<string> GetUserNames(List<string> allNames, int page, int size)
{
    var totalCount = allNames.Count;
    var pagedItems = allNames.Skip((page - 1) * size).Take(size).ToList();
    return PaginatedResponse<string>.FromList(pagedItems, page, size, totalCount);
}
```

## Notes

*   **Thread Safety**: The `PaginatedResponse<T>` class is designed to be immutable once created. As such, it is thread-safe for read operations.
*   **Pagination Logic**: While the `FromQueryableAsync` method handles data source queries, ensure that the input `pageNumber` is always greater than or equal to 1, and `pageSize` is greater than 0, to avoid unexpected behavior or division by zero.
*   **Empty Sets**: When `TotalCount` is 0, both `HasNextPage` and `HasPreviousPage` will evaluate to `false`, and `Items` will be an empty list.
