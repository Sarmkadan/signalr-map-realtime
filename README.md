# SignalRMapRealtime

A real-time mapping application using SignalR for live updates and tracking.

## ApiResponseExtensions

The `ApiResponseExtensions` class provides convenient extension methods for working with `ApiResponse` and `ApiResponse<T>` types. These methods simplify common operations like checking success status, retrieving messages, ensuring successful responses, and transforming non-generic responses into generic ones.

### Usage Example

```csharp
// Example API response from a service call
var apiResponse = new ApiResponse
{
    Success = true,
    Message = "Asset retrieved successfully",
    StatusCode = 200,
    Timestamp = DateTime.UtcNow,
    TraceId = "abc123"
};

// Check if response is successful
bool isSuccessful = apiResponse.IsSuccessful(); // Returns true

// Get message with null safety
string message = apiResponse.GetMessageOrDefault(); // Returns "Asset retrieved successfully"

// Ensure response is successful (throws InvalidOperationException on failure)
apiResponse.EnsureSuccess();

// Transform non-generic response to generic with data
var asset = new Asset { Id = 1, Name = "GPS Tracker" };
var genericResponse = apiResponse.WithData(asset);

// Work with generic response
bool genericIsSuccessful = genericResponse.IsSuccessful<Asset>(); // Returns true
string genericMessage = genericResponse.GetMessageOrDefault<Asset>(); // Returns "Asset retrieved successfully"
genericResponse.EnsureSuccess<Asset>();
```

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