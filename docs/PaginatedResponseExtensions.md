# PaginatedResponseExtensions

The `PaginatedResponseExtensions` class provides a set of fluent extension methods for manipulating and querying `PaginatedResponse<T>` objects, enabling seamless transformation, filtering, and retrieval of paginated data sets without directly modifying the underlying source collections.

## API

### WithPageSize<T>(this PaginatedResponse<T> source, int pageSize)
Updates the page size of the provided `PaginatedResponse<T>`.
- **Parameters:**
  - `source`: The `PaginatedResponse<T>` instance to modify.
  - `pageSize`: The new page size.
- **Returns:** A new `PaginatedResponse<T>` instance with the specified page size.
- **Exceptions:** `ArgumentOutOfRangeException` if `pageSize` is less than or equal to zero.

### Where<T>(this PaginatedResponse<T> source, Func<T, bool> predicate)
Filters the items within the `PaginatedResponse<T>` based on the provided predicate.
- **Parameters:**
  - `source`: The `PaginatedResponse<T>` instance to filter.
  - `predicate`: A function to test each item for a condition.
- **Returns:** A new `PaginatedResponse<T>` containing only items that satisfy the predicate.
- **Exceptions:** `ArgumentNullException` if `source` or `predicate` is null.

### Select<T, TResult>(this PaginatedResponse<T> source, Func<T, TResult> selector)
Projects each element of the `PaginatedResponse<T>` into a new form.
- **Parameters:**
  - `source`: The `PaginatedResponse<T>` instance to project.
  - `selector`: A transform function to apply to each element.
- **Returns:** A new `PaginatedResponse<TResult>`.
- **Exceptions:** `ArgumentNullException` if `source` or `selector` is null.

### GetCurrentPage<T>(this PaginatedResponse<T> source)
Retrieves the items belonging to the current page defined in the `PaginatedResponse<T>`.
- **Parameters:**
  - `source`: The `PaginatedResponse<T>` instance.
- **Returns:** An `IReadOnlyList<T>` containing the items of the current page.
- **Exceptions:** `ArgumentNullException` if `source` is null.

## Usage

```csharp
// Example 1: Filtering and setting page size
var response = new PaginatedResponse<User>(users);
var filteredResponse = response
    .Where(u => u.IsActive)
    .WithPageSize(10);

// Example 2: Projecting data and accessing the current page
var userNames = response
    .Select(u => u.FullName)
    .GetCurrentPage();
```

## Notes

- **Immutability:** These extension methods are designed to return new instances of `PaginatedResponse<T>` rather than modifying the original object in place, adhering to functional programming patterns.
- **Thread Safety:** The methods themselves are thread-safe as they perform read-only operations on the source object and return new instances. However, thread safety of the underlying data within `PaginatedResponse<T>` depends on the implementation of the `PaginatedResponse<T>` type itself.
- **Null Arguments:** Passing `null` as the `source` argument to any of these methods will result in an `ArgumentNullException`.
