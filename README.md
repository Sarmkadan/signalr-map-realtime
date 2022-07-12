// ... existing content ...

## PagedRequestExtensions

The `PagedRequestExtensions` class provides utility methods for handling pagination, sorting, and filtering in paged requests. It includes helpers to calculate skip/take values, validate page parameters, and apply query parameters to `PagedRequest` instances.

### Usage Example

```csharp
var baseRequest = new PagedRequest();
var processedRequest = baseRequest
    .WithPagination(2, 20) // Page 2, 20 items per page
    .WithSortBy("name", true) // Sort by name (descending)
    .WithSearchQuery("active=true"); // Filter by active status

int pageSize = PagedRequestExtensions.GetValidPageSize(20, 10, 100); // Returns 20 (clamped between 10-100)
int pageNumber = PagedRequestExtensions.GetValidPageNumber(0, 1, 1000); // Returns 1 (minimum 1)
int skip = PagedRequestExtensions.CalculateSkip(pageNumber, pageSize); // Returns 0 (1-1)*20
int take = PagedRequestExtensions.CalculateTake(pageNumber, pageSize); // Returns 20
bool hasFilters = PagedRequestExtensions.HasFilters(processedRequest); // Returns true
```

// ... existing content ...
