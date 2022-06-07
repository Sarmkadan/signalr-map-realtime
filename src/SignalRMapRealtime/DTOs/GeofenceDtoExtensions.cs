#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

using System.Globalization;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Utilities;

/// <summary>Provides extension methods for <see cref="GeofenceDto"/> to facilitate common geofence operations.</summary>
public static class GeofenceDtoExtensions
{
    /// <summary>Determines whether the specified coordinate falls inside this geofence zone.</summary>
    /// <param name="dto">The geofence DTO.</param>
    /// <param name="latitude">Latitude of the point to test, in decimal degrees.</param>
    /// <param name="longitude">Longitude of the point to test, in decimal degrees.</param>
    /// <returns><c>true</c> if the point lies within the zone boundary; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <c>null</c>.</exception>
    public static bool ContainsPoint(this GeofenceDto dto, double latitude, double longitude)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.Type switch
        {
            nameof(GeofenceType.Circle) when dto.CenterLatitude.HasValue && dto.CenterLongitude.HasValue && dto.RadiusKm.HasValue
                => GeoLocationExtensions.DistanceBetween(
                    dto.CenterLatitude.Value, dto.CenterLongitude.Value, latitude, longitude) <= dto.RadiusKm.Value,
            nameof(GeofenceType.Polygon) when !string.IsNullOrWhiteSpace(dto.PolygonCoordinates)
                => ContainsPolygon(dto.PolygonCoordinates, latitude, longitude),
            _ => false
        };
    }

    /// <summary>Parses the polygon vertices from <see cref="GeofenceDto.PolygonCoordinates"/>.</summary>
    /// <param name="dto">The geofence DTO.</param>
    /// <returns>Ordered list of (Latitude, Longitude) tuples; empty when inapplicable or malformed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <c>null</c>.</exception>
    public static IReadOnlyList<(double Latitude, double Longitude)> GetPolygonPoints(this GeofenceDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.PolygonCoordinates))
            return Array.Empty<(double, double)>();

        var points = new List<(double, double)>();
        foreach (var pair in dto.PolygonCoordinates.Split(';', StringSplitOptions.RemoveEmptyEntries))
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

    /// <summary>Calculates the distance from the geofence center to the specified coordinate in kilometers.</summary>
    /// <param name="dto">The geofence DTO.</param>
    /// <param name="latitude">Latitude of the point, in decimal degrees.</param>
    /// <param name="longitude">Longitude of the point, in decimal degrees.</param>
    /// <returns>Distance in kilometers; <c>null</c> if the geofence is not a circle or lacks required coordinates.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <c>null</c>.</exception>
    public static double? DistanceTo(this GeofenceDto dto, double latitude, double longitude)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.CenterLatitude is null || dto.CenterLongitude is null || dto.RadiusKm is null)
            return null;

        return GeoLocationExtensions.DistanceBetween(
            dto.CenterLatitude.Value, dto.CenterLongitude.Value, latitude, longitude);
    }

    /// <summary>Determines whether this geofence is a circular zone.</summary>
    /// <param name="dto">The geofence DTO.</param>
    /// <returns><c>true</c> if the geofence type is circle; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <c>null</c>.</exception>
    public static bool IsCircle(this GeofenceDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return string.Equals(dto.Type, nameof(GeofenceType.Circle), StringComparison.Ordinal);
    }

    /// <summary>Determines whether this geofence is a polygonal zone.</summary>
    /// <param name="dto">The geofence DTO.</param>
    /// <returns><c>true</c> if the geofence type is polygon; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <c>null</c>.</exception>
    public static bool IsPolygon(this GeofenceDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return string.Equals(dto.Type, nameof(GeofenceType.Polygon), StringComparison.Ordinal);
    }

    private static bool ContainsPolygon(string? polygonCoordinates, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(polygonCoordinates))
            return false;

        var points = new List<(double Latitude, double Longitude)>();
        foreach (var pair in polygonCoordinates.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(',');
            if (parts.Length == 2
                && double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                && double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
            {
                points.Add((lat, lng));
            }
        }

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