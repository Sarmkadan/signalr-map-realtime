// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Constants;

/// <summary>
/// Constants for API endpoints and configuration.
/// </summary>
public static class ApiConstants
{
    /// <summary>Base API route prefix.</summary>
    public const string ApiRoutePrefix = "api/v1";

    /// <summary>SignalR hub endpoint.</summary>
    public const string SignalRHubEndpoint = "/hubs/location";

    /// <summary>Default API response page size.</summary>
    public const int DefaultPageSize = 50;

    /// <summary>Maximum allowed page size.</summary>
    public const int MaxPageSize = 500;

    /// <summary>Cache duration in minutes for vehicle data.</summary>
    public const int VehicleCacheDurationMinutes = 5;

    /// <summary>Cache duration in minutes for location data.</summary>
    public const int LocationCacheDurationMinutes = 1;

    /// <summary>Request timeout in seconds.</summary>
    public const int RequestTimeoutSeconds = 30;

    /// <summary>Maximum requests per minute per IP address.</summary>
    public const int RateLimitPerMinute = 1000;

    /// <summary>API version number.</summary>
    public const string ApiVersion = "1.0.0";

    /// <summary>API documentation title.</summary>
    public const string ApiTitle = "SignalR Map Realtime API";

    /// <summary>API documentation description.</summary>
    public const string ApiDescription = "Real-time location tracking and mapping API using SignalR and Leaflet";
}

/// <summary>
/// Constants for error codes and messages.
/// </summary>
public static class ErrorConstants
{
    /// <summary>Generic error code.</summary>
    public const string ErrorCodeGeneral = "ERROR_GENERAL";

    /// <summary>Resource not found error code.</summary>
    public const string ErrorCodeNotFound = "ERROR_NOT_FOUND";

    /// <summary>Invalid request error code.</summary>
    public const string ErrorCodeInvalidRequest = "ERROR_INVALID_REQUEST";

    /// <summary>Unauthorized error code.</summary>
    public const string ErrorCodeUnauthorized = "ERROR_UNAUTHORIZED";

    /// <summary>Forbidden error code.</summary>
    public const string ErrorCodeForbidden = "ERROR_FORBIDDEN";

    /// <summary>Conflict error code.</summary>
    public const string ErrorCodeConflict = "ERROR_CONFLICT";

    /// <summary>Internal server error code.</summary>
    public const string ErrorCodeInternalError = "ERROR_INTERNAL";
}
