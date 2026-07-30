#nullable enable
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

    // ------------------------------------------------------------------------
    // NOTE: Tunable values have been moved to LocationOptions (bound from
    // configuration). The following constants are kept here only for backward
    // compatibility and will be removed in a future major version.
    // ------------------------------------------------------------------------
}
