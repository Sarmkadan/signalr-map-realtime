// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Extension methods for geographic calculations and location operations.
/// Provides utilities for distance calculations, coordinate validation, and geo-spatial operations.
/// </summary>
public static class GeoLocationExtensions
{
    /// <summary>
    /// Earth's radius in kilometers (mean radius for WGS84 ellipsoid).
    /// </summary>
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Calculates the distance between two GPS coordinates using Haversine formula.
    /// Returns distance in kilometers.
    /// </summary>
    public static double DistanceTo(this Location from, Location to)
    {
        if (from == null || to == null)
            throw new ArgumentNullException(from == null ? nameof(from) : nameof(to));

        return CalculateDistance(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
    }

    /// <summary>
    /// Calculates the distance between two GPS coordinates using Haversine formula.
    /// Returns distance in kilometers.
    /// </summary>
    public static double DistanceBetween(double lat1, double lon1, double lat2, double lon2)
    {
        return CalculateDistance(lat1, lon1, lat2, lon2);
    }

    /// <summary>
    /// Converts distance from kilometers to miles.
    /// </summary>
    public static double KilometersToMiles(this double kilometers)
    {
        return kilometers * 0.621371;
    }

    /// <summary>
    /// Converts distance from miles to kilometers.
    /// </summary>
    public static double MilesToKilometers(this double miles)
    {
        return miles / 0.621371;
    }

    /// <summary>
    /// Converts distance from kilometers to meters.
    /// </summary>
    public static double KilometersToMeters(this double kilometers)
    {
        return kilometers * 1000.0;
    }

    /// <summary>
    /// Validates if latitude is within valid range (-90 to 90).
    /// </summary>
    public static bool IsValidLatitude(this double latitude)
    {
        return latitude >= -90.0 && latitude <= 90.0;
    }

    /// <summary>
    /// Validates if longitude is within valid range (-180 to 180).
    /// </summary>
    public static bool IsValidLongitude(this double longitude)
    {
        return longitude >= -180.0 && longitude <= 180.0;
    }

    /// <summary>
    /// Validates if coordinates form a valid location (both latitude and longitude valid).
    /// </summary>
    public static bool IsValidCoordinate(this double latitude, double longitude)
    {
        return latitude.IsValidLatitude() && longitude.IsValidLongitude();
    }

    /// <summary>
    /// Calculates the bearing (compass direction) from one location to another.
    /// Returns bearing in degrees (0-360) where 0/360 is North.
    /// </summary>
    public static double BearingTo(this Location from, Location to)
    {
        if (from == null || to == null)
            throw new ArgumentNullException(from == null ? nameof(from) : nameof(to));

        return CalculateBearing(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
    }

    /// <summary>
    /// Determines cardinal direction from bearing angle.
    /// Returns: N, NE, E, SE, S, SW, W, NW, or the exact bearing if it doesn't fit.
    /// </summary>
    public static string GetCardinalDirection(this double bearing)
    {
        // Normalize bearing to 0-360 range
        bearing = ((bearing % 360) + 360) % 360;

        return bearing switch
        {
            >= 348.75 or < 11.25 => "N",
            >= 11.25 and < 33.75 => "NE",
            >= 33.75 and < 56.25 => "E",
            >= 56.25 and < 78.75 => "SE",
            >= 78.75 and < 101.25 => "S",
            >= 101.25 and < 123.75 => "SW",
            >= 123.75 and < 146.25 => "W",
            >= 146.25 and < 168.75 => "NW",
            >= 168.75 and < 191.25 => "S",
            >= 191.25 and < 213.75 => "SW",
            >= 213.75 and < 236.25 => "W",
            >= 236.25 and < 258.75 => "NW",
            >= 258.75 and < 281.25 => "N",
            >= 281.25 and < 303.75 => "NE",
            >= 303.75 and < 326.25 => "E",
            >= 326.25 and < 348.75 => "SE",
            _ => bearing.ToString("F1") + "°"
        };
    }

    /// <summary>
    /// Checks if a location is within a specified radius from a center point.
    /// Useful for geofencing and proximity checks.
    /// </summary>
    public static bool IsWithinRadius(this Location location, Location centerPoint, double radiusKm)
    {
        var distance = DistanceBetween(
            location.Latitude, location.Longitude,
            centerPoint.Latitude, centerPoint.Longitude
        );
        return distance <= radiusKm;
    }

    /// <summary>
    /// Calculates the bounding box (rectangular area) around a center point.
    /// Returns tuple of (minLat, minLon, maxLat, maxLon).
    /// </summary>
    public static (double MinLat, double MinLon, double MaxLat, double MaxLon) GetBoundingBox(
        this Location centerPoint, double radiusKm)
    {
        const double latChange = 111.32; // 1 degree latitude ≈ 111.32 km
        var lonChange = 111.32 * Math.Cos(centerPoint.Latitude * Math.PI / 180.0);

        var latOffset = radiusKm / latChange;
        var lonOffset = radiusKm / lonChange;

        return (
            centerPoint.Latitude - latOffset,
            centerPoint.Longitude - lonOffset,
            centerPoint.Latitude + latOffset,
            centerPoint.Longitude + lonOffset
        );
    }

    /// <summary>
    /// Formats coordinates as a readable string.
    /// Example: "40.7128° N, 74.0060° W"
    /// </summary>
    public static string FormatCoordinates(this Location location, int decimalPlaces = 4)
    {
        var latDirection = location.Latitude >= 0 ? "N" : "S";
        var lonDirection = location.Longitude >= 0 ? "E" : "W";

        return $"{Math.Abs(location.Latitude).ToString($"F{decimalPlaces}")}° {latDirection}, " +
               $"{Math.Abs(location.Longitude).ToString($"F{decimalPlaces}")}° {lonDirection}";
    }

    /// <summary>
    /// Calculates the actual distance using Haversine formula.
    /// Internal helper method used by other distance calculation methods.
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = EarthRadiusKm * c;

        return distance;
    }

    /// <summary>
    /// Calculates the bearing between two coordinates.
    /// </summary>
    private static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = DegreesToRadians(lon2 - lon1);
        var y = Math.Sin(dLon) * Math.Cos(DegreesToRadians(lat2));
        var x = Math.Cos(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) -
                Math.Sin(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(dLon);

        var bearing = RadiansToDegrees(Math.Atan2(y, x));
        return (bearing + 360) % 360; // Normalize to 0-360
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    private static double RadiansToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}
