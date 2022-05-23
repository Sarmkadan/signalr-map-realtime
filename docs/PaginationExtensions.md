# PaginationExtensions

Provides utility methods for normalizing, validating, and applying pagination parameters to collections and queryables, including support for sorting and total count calculations.

## API

### `NormalizePaginationParameters(int pageNumber, int pageSize)`
Normalizes pagination parameters to ensure they fall within valid ranges.
- **Parameters**:
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: A tuple `(PageNumber, PageSize)` where `PageNumber` is at least 1 and `PageSize` is at least 1 and at most 1000.
- **Throws**: `ArgumentOutOfRangeException` if `pageSize` is less than 1 or greater than 1000.

### `ValidatePaginationParameters(int pageNumber, int pageSize)`
Validates pagination parameters without normalization.
- **Parameters**:
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Throws**:
  - `ArgumentOutOfRangeException` if `pageNumber` is less than 1.
  - `ArgumentOutOfRangeException` if `pageSize` is less than 1 or greater than 1000.

### `ApplyPagination<T>(IEnumerable<T> source, int pageNumber, int pageSize)`
Applies pagination to an in-memory collection.
- **Parameters**:
  - `source`: The source collection to paginate.
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: A new `IEnumerable<T>` containing the paginated subset of `source`.
- **Throws**: `ArgumentNullException` if `source` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1 or greater than 1000.

### `ApplyPagination<T>(IQueryable<T> source, int pageNumber, int pageSize)`
Applies pagination to a queryable source.
- **Parameters**:
  - `source`: The source queryable to paginate.
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: A new `IQueryable<T>` representing the paginated subset of `source`.
- **Throws**: `ArgumentNullException` if `source` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1 or greater than 1000.

### `ApplyPaginationWithSort<T>(IQueryable<T> source, int pageNumber, int pageSize, string sortBy, bool descending = false)`
Applies pagination with sorting to a queryable source.
- **Parameters**:
  - `source`: The source queryable to paginate and sort.
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
  - `sortBy`: The property name to sort by.
  - `descending`: Whether to sort in descending order.
- **Returns**: A new `IQueryable<T>` representing the paginated and sorted subset of `source`.
- **Throws**: `ArgumentNullException` if `source` is `null` or `sortBy` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1 or greater than 1000.
- **Throws**: `ArgumentException` if `sortBy` is empty or whitespace.

### `GetPagedResults<T>(IEnumerable<T> source, int pageNumber, int pageSize)`
Retrieves a paginated subset of an in-memory collection along with the total count of items.
- **Parameters**:
  - `source`: The source collection to paginate.
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: A tuple `(Items, TotalCount)` where `Items` is the paginated subset and `TotalCount` is the total number of items in `source`.
- **Throws**: `ArgumentNullException` if `source` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1 or greater than 1000.

### `GetPagedQueryableResults<T>(IQueryable<T> source, int pageNumber, int pageSize)`
Retrieves a paginated subset of a queryable source along with the total count of items.
- **Parameters**:
  - `source`: The source queryable to paginate.
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: A tuple `(Items, TotalCount)` where `Items` is the paginated queryable subset and `TotalCount` is the total number of items in `source`.
- **Throws**: `ArgumentNullException` if `source` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1 or greater than 1000.

### `CalculateSkip(int pageNumber, int pageSize)`
Calculates the number of items to skip for a given page.
- **Parameters**:
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: The number of items to skip.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1.

### `CalculateTotalPages(int totalCount, int pageSize)`
Calculates the total number of pages given a total count and page size.
- **Parameters**:
  - `totalCount`: The total number of items.
  - `pageSize`: The requested page size.
- **Returns**: The total number of pages.
- **Throws**: `ArgumentOutOfRangeException` if `pageSize` is less than 1.

### `IsValidPageNumber(int pageNumber)`
Determines whether a page number is valid.
- **Parameters**:
  - `pageNumber`: The page number to validate.
- **Returns**: `true` if `pageNumber` is greater than or equal to 1; otherwise, `false`.

### `GetPaginationInfo(int totalCount, int pageNumber, int pageSize)`
Creates a `PaginationInfo` object with pagination metadata.
- **Parameters**:
  - `totalCount`: The total number of items.
  - `pageNumber`: The current page number (1-based).
  - `pageSize`: The requested page size.
- **Returns**: A `PaginationInfo` instance populated with pagination metadata.
- **Throws**: `ArgumentOutOfRangeException` if `pageNumber` is less than 1 or `pageSize` is less than 1.

### `PaginationInfo.PageNumber`
Gets the current page number (1-based).
- **Type**: `int`

### `PaginationInfo.PageSize`
Gets the requested page size.
- **Type**: `int`

### `PaginationInfo.TotalCount`
Gets the total number of items across all pages.
- **Type**: `int`

### `PaginationInfo.TotalPages`
Gets the total number of pages.
- **Type**: `int`

### `PaginationInfo.Skip`
Gets the number of items to skip to reach the current page.
- **Type**: `int`

### `PaginationInfo.IsFirstPage`
Gets a value indicating whether the current page is the first page.
- **Type**: `bool`

### `PaginationInfo.IsLastPage`
Gets a value indicating whether the current page is the last page.
- **Type**: `bool`

### `PaginationInfo.HasPreviousPage`
Gets a value indicating whether there is a previous page.
- **Type**: `bool`

### `PaginationInfo.HasNextPage`
Gets a value indicating whether there is a next page.
- **Type**: `bool`

## Usage

### Example 1: Basic Pagination with In-Memory Collection
