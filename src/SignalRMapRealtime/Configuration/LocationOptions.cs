#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Options that control location‑related thresholds and cleanup intervals.
/// Values are bound from configuration (appsettings.json) and fall back to the
/// defaults defined here, which match the previous constant values.
/// </summary>
public class LocationOptions
{
    public const string SectionName = "LocationOptions";

    /// <summary>Minimum distance difference in km to consider a location as moved.</summary>
    [Range(0, double.MaxValue)]
    public double MinLocationChangeKm { get; set; } = 0.05;

    /// <summary>Minimum accuracy threshold in meters for accepting a location.</summary>
    [Range(0, double.MaxValue)]
    public double MinAccuracyMeters { get; set; } = 50.0;

    /// <summary>Maximum allowed speed on a route in km/h.</summary>
    [Range(0, double.MaxValue)]
    public double MaxAllowedSpeed { get; set; } = 200.0;

    /// <summary>Default geofence radius in kilometers.</summary>
    [Range(0, double.MaxValue)]
    public double DefaultGeofenceRadiusKm { get; set; } = 0.5;

    /// <summary>Number of days to retain location history.</summary>
    [Range(1, int.MaxValue)]
    public int LocationRetentionDays { get; set; } = 90;

    /// <summary>Minimum points required for accurate route analysis.</summary>
    [Range(1, int.MaxValue)]
    public int MinPointsForAnalysis { get; set; } = 5;
}
