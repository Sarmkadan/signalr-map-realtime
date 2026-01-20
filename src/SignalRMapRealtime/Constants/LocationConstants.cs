// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Constants;

/// <summary>
/// Constants related to location tracking and coordinates.
/// </summary>
public static class LocationConstants
{
    /// <summary>Minimum valid latitude (-90 degrees).</summary>
    public const double MinLatitude = -90.0;

    /// <summary>Maximum valid latitude (90 degrees).</summary>
    public const double MaxLatitude = 90.0;

    /// <summary>Minimum valid longitude (-180 degrees).</summary>
    public const double MinLongitude = -180.0;

    /// <summary>Maximum valid longitude (180 degrees).</summary>
    public const double MaxLongitude = 180.0;

    /// <summary>Earth's radius in kilometers (used for distance calculations).</summary>
    public const double EarthRadiusKm = 6371.0;

    /// <summary>Minimum distance difference in km to consider a location as moved.</summary>
    public const double MinLocationChangeKm = 0.05;

    /// <summary>Minimum accuracy threshold in meters for accepting a location.</summary>
    public const double MinAccuracyMeters = 50.0;

    /// <summary>Maximum allowed speed on a route in km/h.</summary>
    public const double MaxAllowedSpeed = 200.0;

    /// <summary>Default geofence radius in kilometers.</summary>
    public const double DefaultGeofenceRadiusKm = 0.5;

    /// <summary>Number of days to retain location history.</summary>
    public const int LocationRetentionDays = 90;

    /// <summary>Minimum points required for accurate route analysis.</summary>
    public const int MinPointsForAnalysis = 5;
}

/// <summary>
/// Constants for route operations.
/// </summary>
public static class RouteConstants
{
    /// <summary>Maximum number of waypoints per route.</summary>
    public const int MaxWaypointsPerRoute = 100;

    /// <summary>Minimum time window in minutes for a waypoint.</summary>
    public const int MinTimeWindowMinutes = 15;

    /// <summary>Default estimated duration at a waypoint in minutes.</summary>
    public const int DefaultWaypointDurationMinutes = 30;

    /// <summary>Maximum time allowed to complete a route in hours.</summary>
    public const int MaxRouteCompletionHours = 12;
}

/// <summary>
/// Constants for vehicle tracking.
/// </summary>
public static class VehicleConstants
{
    /// <summary>Default location update interval in seconds.</summary>
    public const int DefaultUpdateIntervalSeconds = 30;

    /// <summary>Session timeout in hours if no location update is received.</summary>
    public const int SessionTimeoutHours = 1;

    /// <summary>Default fuel threshold percentage for alerts.</summary>
    public const double FuelAlertThresholdPercent = 20.0;

    /// <summary>Speed violation threshold percentage above max speed.</summary>
    public const double SpeedViolationThresholdPercent = 10.0;
}
