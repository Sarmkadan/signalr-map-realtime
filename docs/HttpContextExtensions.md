# HttpContextExtensions

Extension methods for `Microsoft.AspNetCore.Http.HttpContext` that provide common HTTP request inspection and manipulation utilities, particularly useful for real-time SignalR applications that need to inspect client metadata, modify responses, or handle different content types.

## API

### `GetClientIpAddress(HttpContext context)`
Returns the client's IP address as a string. The address is extracted from the `X-Forwarded-For` header if present and trusted; otherwise, it falls back to `context.Connection.RemoteIpAddress`. The result is normalized to exclude port numbers and loopback addresses are returned as `127.0.0.1`.

### `GetUserAgent(HttpContext context)`
Returns the value of the `User-Agent` header from the request. If the header is missing or empty, returns `null`.

### `GetReferer(HttpContext context)`
Returns the value of the `Referer` header from the request. If the header is missing or empty, returns `null`.

### `GetOrigin(HttpContext context)`
Returns the value of the `Origin` header from the request. If the header is missing or empty, returns `null`.

### `IsSecure(HttpContext context)`
Returns `true` if the request was made over HTTPS; otherwise, returns `false`.

### `GetFullUrl(HttpContext context)`
Returns the full URL of the request including scheme, host, path, and query string (e.g., `https://example.com/path?query=value`). The scheme is determined by `IsSecure`.

### `GetBaseUrl(HttpContext context)`
Returns the base URL of the request including scheme and host (e.g., `https://example.com`). The scheme is determined by `IsSecure`.

### `HasHeader(HttpContext context, string name)`
Returns `true` if the request contains a header with the specified `name` (case-insensitive); otherwise, returns `false`.

### `GetHeader(HttpContext context, string name)`
Returns the value of the header with the specified `name` (case-insensitive) from the request. If the header is missing, returns `null`.

### `IsAjaxRequest(HttpContext context)`
Returns `true` if the request includes the `X-Requested-With` header with value `XMLHttpRequest`; otherwise, returns `false`.

### `GetContentType(HttpContext context)`
Returns the value of the `Content-Type` header from the request. If the header is missing or empty, returns `null`.

### `IsJsonRequest(HttpContext context)`
Returns `true` if the `Content-Type` header indicates a JSON payload (e.g., `application/json`); otherwise, returns `false`.

### `IsFormRequest(HttpContext context)`
Returns `true` if the `Content-Type` header indicates a form payload (e.g., `application/x-www-form-urlencoded` or `multipart/form-data`); otherwise, returns `false`.

### `GetMethod(HttpContext context)`
Returns the HTTP method of the request (e.g., `GET`, `POST`, `PUT`, `DELETE`).

### `SetHeader(HttpContext context, string name, string value)`
Sets a response header with the specified `name` and `value`. The header is added if it does not exist; otherwise, it is overwritten.

### `SetStatusCode(HttpContext context, int statusCode)`
Sets the HTTP response status code to the specified `statusCode`.

### `SetContentType(HttpContext context, string contentType)`
Sets the `Content-Type` response header to the specified `contentType`.

### `DisableCaching(HttpContext context)`
Disables caching by setting the `Cache-Control` response header to `no-store, no-cache, must-revalidate, max-age=0`.

### `SetCacheControl(HttpContext context, string cacheControl)`
Sets the `Cache-Control` response header to the specified `cacheControl` value.

### `GetParameters(HttpContext context)`
Returns a `Dictionary<string, string>` containing all query parameters from the request. Keys are parameter names (case-sensitive), and values are the first occurrence of each parameter's value. If no parameters exist, returns an empty dictionary.

## Usage
