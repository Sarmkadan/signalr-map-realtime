#nullable enable
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
    /// Approximate distance in kilometers represented by one degree of latitude.
    /// </summary>
    private const double KilometersPerDegreeLatitude = 111.32;

    private const double MilesPerKilometer = 0.621371;
    private const double MetersPerKilometer = 1000.0;
    private const double MaximumLatitudeDegrees = 90.0;
    private const double MaximumLongitudeDegrees = 180.0;
    private const double DegreesInHalfCircle = 180.0;
    private const double DegreesInFullCircle = 360.0;

    /// <summary>
    /// Angular width of one of the 16 compass sectors in degrees.
    /// </summary>
    private const double CompassSectorDegrees = 22.5;

    /// <summary>
    /// Half of a compass sector; the N sector is centred on 0° and extends this far on each side.
    /// </summary>
    private const double CompassHalfSectorDegrees = CompassSectorDegrees / 2;

    /// <summary>
    /// Calculates the distance between two GPS coordinates using Haversine formula.
    /// Returns distance in kilometers.
    /// </summary>
    /// <param name="from">Source location.</param>
    /// <param name="to">Target location.</param>
    /// <returns>Distance between locations in kilometers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either location is null.</exception>
    public static double DistanceTo(this Location from, Location to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        return CalculateDistance(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
    }

    /// <summary>
    /// Calculates the distance between two GPS coordinates using Haversine formula.
    /// Returns distance in kilometers.
    /// </summary>
    /// <param name="lat1">Latitude of first point in decimal degrees.</param>
    /// <param name="lon1">Longitude of first point in decimal degrees.</param>
    /// <param name="lat2">Latitude of second point in decimal degrees.</param>
    /// <param name="lon2">Longitude of second point in decimal degrees.</param>
    /// <returns>Distance between points in kilometers.</returns>
    /// <exception cref="ArgumentException">Thrown when coordinates are outside valid geographic bounds.</exception>
    public static double DistanceBetween(double lat1, double lon1, double lat2, double lon2)
    {
        if (!lat1.IsValidLatitude())
        {
            throw new ArgumentException($"Invalid latitude: {lat1}. Must be between -90 and 90.", nameof(lat1));
        }

        if (!lon1.IsValidLongitude())
        {
            throw new ArgumentException($"Invalid longitude: {lon1}. Must be between -180 and 180.", nameof(lon1));
        }

        if (!lat2.IsValidLatitude())
        {
            throw new ArgumentException($"Invalid latitude: {lat2}. Must be between -90 and 90.", nameof(lat2));
        }

        if (!lon2.IsValidLongitude())
        {
            throw new ArgumentException($"Invalid longitude: {lon2}. Must be between -180 and 180.", nameof(lon2));
        }

        return CalculateDistance(lat1, lon1, lat2, lon2);
    }

    /// <summary>
    /// Converts distance from kilometers to miles.
    /// </summary>
    /// <param name="kilometers">Distance in kilometers.</param>
    /// <returns>Distance in miles.</returns>
    public static double KilometersToMiles(this double kilometers) => kilometers * MilesPerKilometer;

    /// <summary>
    /// Converts distance from miles to kilometers.
    /// </summary>
    /// <param name="miles">Distance in miles.</param>
    /// <returns>Distance in kilometers.</returns>
    public static double MilesToKilometers(this double miles) => miles / MilesPerKilometer;

    /// <summary>
    /// Converts distance from kilometers to meters.
    /// </summary>
    /// <param name="kilometers">Distance in kilometers.</param>
    /// <returns>Distance in meters.</returns>
    public static double KilometersToMeters(this double kilometers) => kilometers * MetersPerKilometer;

    /// <summary>
    /// Validates if latitude is within valid range (-90 to 90).
    /// </summary>
    /// <param name="latitude">Latitude value to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValidLatitude(this double latitude)
    {
        return latitude >= -MaximumLatitudeDegrees && latitude <= MaximumLatitudeDegrees;
    }

    /// <summary>
    /// Validates if longitude is within valid range (-180 to 180).
    /// </summary>
    /// <param name="longitude">Longitude value to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValidLongitude(this double longitude)
    {
        return longitude >= -MaximumLongitudeDegrees && longitude <= MaximumLongitudeDegrees;
    }

    /// <summary>
    /// Validates if coordinates form a valid location (both latitude and longitude valid).
    /// </summary>
    /// <param name="latitude">Latitude value to validate.</param>
    /// <param name="longitude">Longitude value to validate.</param>
    /// <returns>True if both coordinates are valid, false otherwise.</returns>
    public static bool IsValidCoordinate(this double latitude, double longitude)
    {
        return latitude.IsValidLatitude() && longitude.IsValidLongitude();
    }

    /// <summary>
    /// Calculates the bearing (compass direction) from one location to another.
    /// Returns bearing in degrees (0-360) where 0/360 is North.
    /// </summary>
    /// <param name="from">Source location.</param>
    /// <param name="to">Target location.</param>
    /// <returns>Bearing in degrees from source to target.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either location is null.</exception>
    public static double BearingTo(this Location from, Location to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        return CalculateBearing(from.Latitude, from.Longitude, to.Latitude, to.Longitude);
    }

    /// <summary>
    /// Calculates the destination reached by travelling a distance along a bearing from a starting location.
    /// This is the inverse of <see cref="BearingTo"/> and <see cref="DistanceTo"/> on a spherical Earth model.
    /// </summary>
    /// <param name="from">Starting location.</param>
    /// <param name="bearingDegrees">Initial bearing in degrees clockwise from North (any value; normalized internally).</param>
    /// <param name="distanceKm">Distance to travel in kilometers.</param>
    /// <returns>Latitude and longitude of the destination in decimal degrees.</returns>
    /// <exception cref="ArgumentNullException">Thrown when from is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when distanceKm is negative.</exception>
    public static (double Latitude, double Longitude) DestinationPoint(
        this Location from, double bearingDegrees, double distanceKm)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceKm);

        var angularDistance = distanceKm / EarthRadiusKm;
        var bearing = DegreesToRadians(bearingDegrees);
        var lat1 = DegreesToRadians(from.Latitude);
        var lon1 = DegreesToRadians(from.Longitude);

        var sinLat2 = Math.Sin(lat1) * Math.Cos(angularDistance) +
                      Math.Cos(lat1) * Math.Sin(angularDistance) * Math.Cos(bearing);
        var lat2 = Math.Asin(Math.Clamp(sinLat2, -1.0, 1.0));
        var lon2 = lon1 + Math.Atan2(
            Math.Sin(bearing) * Math.Sin(angularDistance) * Math.Cos(lat1),
            Math.Cos(angularDistance) - Math.Sin(lat1) * Math.Sin(lat2));

        var latitude = RadiansToDegrees(lat2);
        var longitude = RadiansToDegrees(lon2);

        // Normalize longitude to -180 to 180
        longitude = ((longitude + DegreesInHalfCircle) % DegreesInFullCircle + DegreesInFullCircle)
                    % DegreesInFullCircle - DegreesInHalfCircle;

        return (latitude, longitude);
    }

    /// <summary>
    /// Determines cardinal direction from bearing angle.
    /// Returns: N, NE, E, SE, S, SW, W, NW, or the exact bearing if it doesn't fit.
    /// </summary>
    /// <param name="bearing">Bearing angle in degrees (0-360).</param>
    /// <returns>Cardinal direction abbreviation.</returns>
    public static string GetCardinalDirection(this double bearing)
    {
        // Normalize bearing to 0-360 range
        bearing = ((bearing % DegreesInFullCircle) + DegreesInFullCircle) % DegreesInFullCircle;

        // Sector boundaries sit at half-sector offsets: H + k * S, where H = CompassHalfSectorDegrees and S = CompassSectorDegrees
        return bearing switch
        {
            >= DegreesInFullCircle - CompassHalfSectorDegrees or < CompassHalfSectorDegrees => "N",
            >= CompassHalfSectorDegrees and < CompassHalfSectorDegrees + CompassSectorDegrees => "NE",
            >= CompassHalfSectorDegrees + CompassSectorDegrees and < CompassHalfSectorDegrees + 2 * CompassSectorDegrees => "E",
            >= CompassHalfSectorDegrees + 2 * CompassSectorDegrees and < CompassHalfSectorDegrees + 3 * CompassSectorDegrees => "SE",
            >= CompassHalfSectorDegrees + 3 * CompassSectorDegrees and < CompassHalfSectorDegrees + 4 * CompassSectorDegrees => "S",
            >= CompassHalfSectorDegrees + 4 * CompassSectorDegrees and < CompassHalfSectorDegrees + 5 * CompassSectorDegrees => "SW",
            >= CompassHalfSectorDegrees + 5 * CompassSectorDegrees and < CompassHalfSectorDegrees + 6 * CompassSectorDegrees => "W",
            >= CompassHalfSectorDegrees + 6 * CompassSectorDegrees and < CompassHalfSectorDegrees + 7 * CompassSectorDegrees => "NW",
            >= CompassHalfSectorDegrees + 7 * CompassSectorDegrees and < CompassHalfSectorDegrees + 8 * CompassSectorDegrees => "S",
            >= CompassHalfSectorDegrees + 8 * CompassSectorDegrees and < CompassHalfSectorDegrees + 9 * CompassSectorDegrees => "SW",
            >= CompassHalfSectorDegrees + 9 * CompassSectorDegrees and < CompassHalfSectorDegrees + 10 * CompassSectorDegrees => "W",
            >= CompassHalfSectorDegrees + 10 * CompassSectorDegrees and < CompassHalfSectorDegrees + 11 * CompassSectorDegrees => "NW",
            >= CompassHalfSectorDegrees + 11 * CompassSectorDegrees and < CompassHalfSectorDegrees + 12 * CompassSectorDegrees => "N",
            >= CompassHalfSectorDegrees + 12 * CompassSectorDegrees and < CompassHalfSectorDegrees + 13 * CompassSectorDegrees => "NE",
            >= CompassHalfSectorDegrees + 13 * CompassSectorDegrees and < CompassHalfSectorDegrees + 14 * CompassSectorDegrees => "E",
            >= CompassHalfSectorDegrees + 14 * CompassSectorDegrees and < CompassHalfSectorDegrees + 15 * CompassSectorDegrees => "SE",
            _ => bearing.ToString("F1") + "°"
        };
    }

    /// <summary>
    /// Checks if a location is within a specified radius from a center point.
    /// Useful for geofencing and proximity checks.
    /// </summary>
    /// <param name="location">Location to check.</param>
    /// <param name="centerPoint">Center point for radius calculation.</param>
    /// <param name="radiusKm">Maximum allowed distance in kilometers.</param>
    /// <returns>True if location is within radius, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when location or centerPoint is null.</exception>
    public static bool IsWithinRadius(this Location location, Location centerPoint, double radiusKm)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(centerPoint);

        var distance = DistanceBetween(
            location.Latitude,
            location.Longitude,
            centerPoint.Latitude,
            centerPoint.Longitude);

        return distance <= radiusKm;
    }

    /// <summary>
    /// Calculates the bounding box (rectangular area) around a center point.
    /// Returns tuple of (minLat, minLon, maxLat, maxLon).
    /// </summary>
    /// <param name="centerPoint">Center point for bounding box calculation.</param>
    /// <param name="radiusKm">Radius in kilometers to extend from center.</param>
    /// <returns>Tuple containing min/max latitude and longitude values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when centerPoint is null.</exception>
    /// <exception cref="ArgumentException">Thrown when radius is negative.</exception>
    public static (double MinLat, double MinLon, double MaxLat, double MaxLon) GetBoundingBox(
        this Location centerPoint, double radiusKm)
    {
        ArgumentNullException.ThrowIfNull(centerPoint);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radiusKm);

        var latChange = KilometersPerDegreeLatitude;
        var lonChange = KilometersPerDegreeLatitude *
                        Math.Cos(centerPoint.Latitude * Math.PI / DegreesInHalfCircle);

        var latOffset = radiusKm / latChange;
        var lonOffset = radiusKm / lonChange;

        return (
            centerPoint.Latitude - latOffset,
            centerPoint.Longitude - lonOffset,
            centerPoint.Latitude + latOffset,
            centerPoint.Longitude + lonOffset);
    }

    /// <summary>
    /// Formats coordinates as a readable string.
    /// Example: "40.7128° N, 74.0060° W"
    /// </summary>
    /// <param name="location">Location to format.</param>
    /// <param name="decimalPlaces">Number of decimal places for precision (default: 4).</param>
    /// <returns>Formatted coordinate string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when location is null.</exception>
    public static string FormatCoordinates(this Location location, int decimalPlaces = 4)
    {
        ArgumentNullException.ThrowIfNull(location);

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
        return (bearing + DegreesInFullCircle) % DegreesInFullCircle; // Normalize to 0-360
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double DegreesToRadians(double degrees) => degrees * Math.PI / DegreesInHalfCircle;

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    private static double RadiansToDegrees(double radians) => radians * DegreesInHalfCircle / Math.PI;
}
