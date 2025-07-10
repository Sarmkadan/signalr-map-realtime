// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

using System.Globalization;
using SignalRMapRealtime.Utilities;

/// <summary>Defines the boundary shape of a geofence zone.</summary>
public enum GeofenceType
{
    /// <summary>Circular zone defined by a centre point and radius in kilometres.</summary>
    Circle = 0,

    /// <summary>Polygonal zone defined by an ordered series of coordinate vertices.</summary>
    Polygon = 1,
}

/// <summary>
/// Represents a named geographic boundary zone used for proximity-based alerts and event triggers.
/// </summary>
public class Geofence
{
    /// <summary>Unique identifier for this zone.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Human-readable name of the zone.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description or operational note.</summary>
    public string? Description { get; set; }

    /// <summary>Boundary shape type.</summary>
    public GeofenceType Type { get; set; }

    /// <summary>Whether this zone is evaluated during location checks.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Centre latitude for <see cref="GeofenceType.Circle"/> zones.</summary>
    public double? CenterLatitude { get; set; }

    /// <summary>Centre longitude for <see cref="GeofenceType.Circle"/> zones.</summary>
    public double? CenterLongitude { get; set; }

    /// <summary>Trigger radius in kilometres for <see cref="GeofenceType.Circle"/> zones.</summary>
    public double? RadiusKm { get; set; }

    /// <summary>
    /// Semicolon-separated vertex pairs for <see cref="GeofenceType.Polygon"/> zones,
    /// each formatted as <c>latitude,longitude</c> using invariant-culture decimals.
    /// Example: <c>51.50,0.12;51.51,0.13;51.50,0.14</c>.
    /// </summary>
    public string? PolygonCoordinates { get; set; }

    /// <summary>UTC timestamp when this zone was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last modification.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Identifier of the user who created this zone.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Determines whether the specified coordinate falls inside this geofence zone.
    /// </summary>
    /// <param name="latitude">Latitude of the point to test, in decimal degrees.</param>
    /// <param name="longitude">Longitude of the point to test, in decimal degrees.</param>
    /// <returns><c>true</c> if the point lies within the zone boundary; otherwise <c>false</c>.</returns>
    public bool ContainsPoint(double latitude, double longitude) => Type switch
    {
        GeofenceType.Circle => ContainsCircle(latitude, longitude),
        GeofenceType.Polygon => ContainsPolygon(latitude, longitude),
        _ => false,
    };

    /// <summary>
    /// Parses the polygon vertices from <see cref="PolygonCoordinates"/>.
    /// </summary>
    /// <returns>Ordered list of (Latitude, Longitude) tuples; empty when inapplicable or malformed.</returns>
    public IReadOnlyList<(double Latitude, double Longitude)> GetPolygonPoints()
    {
        if (string.IsNullOrWhiteSpace(PolygonCoordinates))
            return Array.Empty<(double, double)>();

        var points = new List<(double, double)>();
        foreach (var pair in PolygonCoordinates.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(',');
            if (parts.Length == 2
                && double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                && double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
            {
                points.Add((lat, lng));
            }
        }

        return points;
    }

    private bool ContainsCircle(double latitude, double longitude)
    {
        if (CenterLatitude is null || CenterLongitude is null || RadiusKm is null)
            return false;

        var distanceKm = GeoLocationExtensions.DistanceBetween(
            CenterLatitude.Value, CenterLongitude.Value, latitude, longitude);
        return distanceKm <= RadiusKm.Value;
    }

    private bool ContainsPolygon(double latitude, double longitude)
    {
        var points = GetPolygonPoints();
        if (points.Count < 3)
            return false;

        // Ray-casting: count how many polygon edges a horizontal ray from the point crosses.
        var inside = false;
        var j = points.Count - 1;

        for (var i = 0; i < points.Count; i++)
        {
            var (iLat, iLng) = points[i];
            var (jLat, jLng) = points[j];

            if (iLng > longitude != jLng > longitude
                && latitude < (jLat - iLat) * (longitude - iLng) / (jLng - iLng) + iLat)
            {
                inside = !inside;
            }

            j = i;
        }

        return inside;
    }
}
