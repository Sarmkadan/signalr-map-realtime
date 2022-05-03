# PagedRequest

The `PagedRequest` class is a data transfer object (DTO) used to encapsulate pagination, sorting, filtering, and query-specific criteria for retrieving paginated data from API endpoints. It streamlines requests by consolidating common query parameters, ensuring consistent handling of data retrieval across the `signalr-map-realtime` service.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `PageNumber` | `int` | The current page number to retrieve. |
| `PageSize` | `int` | The number of items to include in a single page. |
| `SearchQuery` | `string?` | An optional keyword string used to perform a search. |
| `SortBy` | `string?` | The field or property name to sort the result set by. |
| `Filter` | `string?` | A general-purpose filtering expression or identifier. |
| `Status` | `string?` | A filtering criterion based on the status of the entity. |
| `VehicleType` | `string?` | A filtering criterion based on the type of vehicle. |
| `AssignedToId` | `Guid?` | A filter for entities assigned to a specific user or resource identifier. |
| `VehicleId` | `Guid?` | A filter for entities associated with a specific vehicle identifier. |
| `StartDate` | `DateTime?` | The lower bound for date-based filtering ranges. |
| `EndDate` | `DateTime?` | The upper bound for date-based filtering ranges. |
| `LocationType` | `string?` | A filter based on the classification of the location. |
| `MinAccuracy` | `double?` | A numerical constraint representing the minimum required accuracy. |
| `CreatedAfter` | `DateTime?` | A filter to return items created after the specified time. |
| `CreatedBefore` | `DateTime?` | A filter to return items created before the specified time. |
| `AssetType` | `string?` | A filter based on the classification of the asset. |
| `CurrentVehicleId` | `Guid?` | A filter for entities currently associated with a specific vehicle identifier. |

## Usage

### Basic Pagination
```csharp
var request = new PagedRequest
{
    PageNumber = 1,
    PageSize = 20
};

// Pass request to the service layer for data retrieval
var results = await _mapService.GetEntitiesAsync(request);
```

### Complex Filtering
```csharp
var request = new PagedRequest
{
    PageNumber = 1,
    PageSize = 50,
    Status = "Active",
    VehicleId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
    CreatedAfter = DateTime.UtcNow.AddDays(-7),
    SortBy = "CreatedDate"
};

// Retrieve filtered results
var results = await _mapService.GetEntitiesAsync(request);
```

## Notes

*   **Thread-Safety:** `PagedRequest` is a plain data object with mutable properties. It is not thread-safe. If multiple threads need to access or modify a single instance, external synchronization mechanisms must be used.
*   **Validation:** The class does not implement internal validation. Consumers should ensure that `PageNumber` and `PageSize` contain non-negative, logically valid values before passing the object to service methods.
*   **Nullability:** Most properties are nullable. API consumers are responsible for handling `null` values within service logic to ensure appropriate filtering behavior (e.g., treating `null` as "no filter applied").
*   **Member Duplication:** Although `Status` and `VehicleId` appear in various contexts, they are defined as unified properties on this type. Ensure the application logic appropriately interprets these values based on the specific API endpoint being called.
