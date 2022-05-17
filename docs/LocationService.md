# LocationService

The `LocationService` manages geospatial data for real-time tracking and historical analysis in a SignalR-based mapping application. It provides methods to record, retrieve, update, and delete location records, as well as compute distances and statistics, with support for pagination and filtering by proximity or type.

## API

### `LocationService`

Constructor for the service, typically registered as a singleton or scoped dependency in the application's DI container.

### `async Task<LocationDto> RecordLocationAsync(LocationDto location)`

Records a new location entry in the system.

- **Parameters**:
  - `location`: The `LocationDto` containing latitude, longitude, timestamp, and optional metadata (e.g., type, device ID).
- **Return value**: The persisted `LocationDto` with any server-generated fields (e.g., ID, processed timestamp).
- **Exceptions**: Throws if validation fails or persistence fails.

### `async Task<LocationDto?> GetLatestLocationAsync(string? deviceId = null)`

Retrieves the most recent location record, optionally filtered by device.

- **Parameters**:
  - `deviceId`: Optional identifier to scope the query to a specific device.
- **Return value**: The latest `LocationDto`, or `null` if none exists.
- **Exceptions**: Throws on database or network failure.

### `async Task<IEnumerable<LocationDto>> GetLocationHistoryAsync(string deviceId, DateTime? from = null, DateTime? to = null)`

Fetches the full historical sequence of locations for a given device within a time window.

- **Parameters**:
  - `deviceId`: Required device identifier.
  - `from`: Optional start of the time window (inclusive).
  - `to`: Optional end of the time window (inclusive).
- **Return value**: Ordered sequence of `LocationDto` entries from oldest to newest.
- **Exceptions**: Throws if `deviceId` is null or empty, or on persistence failure.

### `async Task<IEnumerable<LocationDto>> GetRecentLocationsAsync(int hours = 1, string? deviceId = null)`

Returns the most recent locations within the specified time window, optionally filtered by device.

- **Parameters**:
  - `hours`: Number of hours to look back (default: 1).
  - `deviceId`: Optional device filter.
- **Return value**: Sequence of `LocationDto` entries ordered by timestamp descending.
- **Exceptions**: Throws on invalid `hours` (≤ 0) or persistence failure.

### `async Task<IEnumerable<LocationDto>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm, string? type = null)`

Finds locations within a geographic radius, optionally filtered by type.

- **Parameters**:
  - `latitude`: Center latitude in decimal degrees.
  - `longitude`: Center longitude in decimal degrees.
  - `radiusKm`: Search radius in kilometers.
  - `type`: Optional location type filter (e.g., "vehicle", "user").
- **Return value**: Sequence of `LocationDto` entries within the radius, ordered by distance ascending.
- **Exceptions**: Throws if coordinates are invalid or `radiusKm` ≤ 0.

### `async Task<LocationStatsDto> GetLocationStatsAsync(string? deviceId = null, DateTime? from = null, DateTime? to = null)`

Computes aggregate statistics (count, min/max distance, etc.) for locations matching the filters.

- **Parameters**:
  - `deviceId`: Optional device filter.
  - `from`: Optional start of the time window.
  - `to`: Optional end of the time window.
- **Return value**: `LocationStatsDto` containing computed metrics.
- **Exceptions**: Throws on invalid time window or persistence failure.

### `async Task<IEnumerable<LocationDto>> GetLocationsByTypeAsync(string type, DateTime? from = null, DateTime? to = null)`

Retrieves all locations of a specific type within an optional time window.

- **Parameters**:
  - `type`: Required location type (non-empty).
  - `from`: Optional start of the time window.
  - `to`: Optional end of the time window.
- **Return value**: Sequence of `LocationDto` entries ordered by timestamp descending.
- **Exceptions**: Throws if `type` is null or empty, or on persistence failure.

### `async Task<LocationDto> UpdateLocationAsync(Guid id, LocationDto updates)`

Updates an existing location record with new data.

- **Parameters**:
  - `id`: The unique identifier of the location to update.
  - `updates`: `LocationDto` containing fields to update (e.g., latitude, longitude, metadata).
- **Return value**: The updated `LocationDto`.
- **Exceptions**: Throws if `id` not found, validation fails, or persistence fails.

### `async Task<PaginatedResponse<LocationDto>> GetLocationsAsync(int page = 1, int pageSize = 10, string? type = null, DateTime? from = null, DateTime? to = null)`

Fetches a paginated list of locations with optional filtering.

- **Parameters**:
  - `page`: Page number (1-based, default: 1).
  - `pageSize`: Number of items per page (default: 10, max: 100).
  - `type`: Optional location type filter.
  - `from`: Optional start of the time window.
  - `to`: Optional end of the time window.
- **Return value**: `PaginatedResponse<LocationDto>` containing the page of results and total count.
- **Exceptions**: Throws if `page` < 1, `pageSize` invalid, or persistence failure.

### `async Task<LocationDto?> GetLocationByIdAsync(Guid id)`

Retrieves a single location by its unique identifier.

- **Parameters**:
  - `id`: The unique identifier of the location.
- **Return value**: The matching `LocationDto`, or `null` if not found.
- **Exceptions**: Throws on persistence failure.

### `async Task<bool> DeleteLocationAsync(Guid id)`

Removes a location record from the system.

- **Parameters**:
  - `id`: The unique identifier of the location to delete.
- **Return value**: `true` if the record was found and deleted; `false` otherwise.
- **Exceptions**: Throws on persistence failure.

### `bool ValidateCoordinates(double latitude, double longitude)`

Validates geographic coordinates for correctness.

- **Parameters**:
  - `latitude`: Latitude in decimal degrees.
  - `longitude`: Longitude in decimal degrees.
- **Return value**: `true` if valid; otherwise `false`.
- **Exceptions**: None.

### `double CalculateDistance(double lat1, double lon1, double lat2, double lon2)`

Computes the great-circle distance between two points in kilometers.

- **Parameters**:
  - `lat1`, `lon1`: Coordinates of the first point.
  - `lat2`, `lon2`: Coordinates of the second point.
- **Return value**: Distance in kilometers.
- **Exceptions**: None.

### `async Task<int> CleanupOldLocationsAsync(int days)`

Removes location records older than the specified number of days.

- **Parameters**:
  - `days`: Age threshold in days (records older than this are deleted).
- **Return value**: Number of records deleted.
- **Exceptions**: Throws on persistence failure.

## Usage
