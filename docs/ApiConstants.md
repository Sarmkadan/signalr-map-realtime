# ApiConstants

Source: `src/SignalRMapRealtime/Constants/ApiConstants.cs`

The file lives in the `SignalRMapRealtime.Constants` namespace and defines two static classes: `ApiConstants` (API routes, paging, caching, timeouts, rate limit, and API metadata) and `ErrorConstants` (error code strings). Both contain only `const` fields, so they cannot be instantiated and need no configuration.

## ApiConstants

Static class with API endpoint and configuration values.

### Routing

#### `ApiRoutePrefix`
Base API route prefix.  
**Type:** `string`  
**Value:** `"api/v1"`

#### `SignalRHubEndpoint`
SignalR hub endpoint.  
**Type:** `string`  
**Value:** `"/hubs/location"`

### Paging

#### `DefaultPageSize`
Default API response page size.  
**Type:** `int`  
**Value:** `50`

#### `MaxPageSize`
Maximum allowed page size.  
**Type:** `int`  
**Value:** `500`

### Caching

#### `VehicleCacheDurationMinutes`
Cache duration in minutes for vehicle data.  
**Type:** `int`  
**Value:** `5`

#### `LocationCacheDurationMinutes`
Cache duration in minutes for location data.  
**Type:** `int`  
**Value:** `1`

### Requests

#### `RequestTimeoutSeconds`
Request timeout in seconds.  
**Type:** `int`  
**Value:** `30`

#### `RateLimitPerMinute`
Maximum requests per minute per IP address.  
**Type:** `int`  
**Value:** `1000`

### API metadata

#### `ApiVersion`
API version number.  
**Type:** `string`  
**Value:** `"1.0.0"`

#### `ApiTitle`
API documentation title.  
**Type:** `string`  
**Value:** `"SignalR Map Realtime API"`

#### `ApiDescription`
API documentation description.  
**Type:** `string`  
**Value:** `"Real-time location tracking and mapping API using SignalR and Leaflet"`

## ErrorConstants

Static class with error code strings. Every value is a `string` with the `ERROR_` prefix.

| Constant | Value | Meaning |
|---|---|---|
| `ErrorCodeGeneral` | `"ERROR_GENERAL"` | Generic error |
| `ErrorCodeNotFound` | `"ERROR_NOT_FOUND"` | Resource not found |
| `ErrorCodeInvalidRequest` | `"ERROR_INVALID_REQUEST"` | Invalid request |
| `ErrorCodeUnauthorized` | `"ERROR_UNAUTHORIZED"` | Unauthorized |
| `ErrorCodeForbidden` | `"ERROR_FORBIDDEN"` | Forbidden |
| `ErrorCodeConflict` | `"ERROR_CONFLICT"` | Conflict |
| `ErrorCodeInternalError` | `"ERROR_INTERNAL"` | Internal server error |

## Usage notes

- All members are `const`, so their values are fixed at compile time and are inlined by the compiler.
- `DefaultPageSize` is the default and `MaxPageSize` is the upper bound, so `DefaultPageSize` never exceeds `MaxPageSize`.
- The cache durations are given in minutes and the request timeout in seconds. Check the unit when using them.
- `RateLimitPerMinute` is documented as a per-IP limit.
