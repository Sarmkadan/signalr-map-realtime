#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Extension methods for the Location class providing additional functionality for geographic calculations,
/// coordinate validation, and location metadata operations.
/// </summary>
public static class LocationExtensions
{
    /// <summary>
    /// Calculates the distance between this location and another in meters using the Haversine formula.
    /// </summary>
    /// <param name="location">The source location.</param>
    /// <param name="other">The target location to calculate distance to.</param>
    /// <returns>Distance in meters.</returns>
    public static double CalculateDistanceToMeters(this Location location, Location other)
    {
        const double EarthRadiusMeters = 6371000.0;
        double dLat = (other.Latitude - location.Latitude) * Math.PI / 180.0;
        double dLon = (other.Longitude - location.Longitude) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(location.Latitude * Math.PI / 180.0) *
                   Math.Cos(other.Latitude * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Determines if this location has valid coordinates (latitude between -90 and 90, longitude between -180 and 180).
    /// </summary>
    /// <param name="location">The location to validate.</param>
    /// <param name="includeNullCheck">If true, also checks if Latitude and Longitude are not null.</param>
    /// <returns>True if coordinates are valid.</returns>
    public static bool HasValidCoordinates(this Location location, bool includeNullCheck = false)
    {
        if (includeNullCheck && location.Latitude == 0 && location.Longitude == 0)
        {
            return false;
        }

        return location.Latitude >= -90 && location.Latitude <= 90 &&
               location.Longitude >= -180 && location.Longitude <= 180;
    }

    /// <summary>
    /// Gets the bearing/heading from this location to another location in degrees (0-360).
    /// Returns null if either location has invalid coordinates.
    /// </summary>
    /// <param name="location">The source location.</param>
    /// <param name="target">The target location to calculate bearing to.</param>
    /// <returns>Bearing in degrees from 0 to 360, or null if coordinates are invalid.</returns>
    public static double? GetBearingTo(this Location location, Location target)
    {
        // Check if coordinates are valid
        if (!location.HasValidCoordinates() || !target.HasValidCoordinates())
        {
            return null;
        }

        double lat1 = location.Latitude * Math.PI / 180.0;
        double lon1 = location.Longitude * Math.PI / 180.0;
        double lat2 = target.Latitude * Math.PI / 180.0;
        double lon2 = target.Longitude * Math.PI / 180.0;

        double y = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) -
                   Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);

        double bearing = Math.Atan2(y, x) * 180.0 / Math.PI;
        bearing = (bearing + 360) % 360;

        return bearing;
    }

    /// <summary>
    /// Determines if this location is within a specified bounding box defined by two corner locations.
    /// </summary>
    /// <param name="location">The location to check.</param>
    /// <param name="southWest">South-west corner of the bounding box.</param>
    /// <param name="northEast">North-east corner of the bounding box.</param>
    /// <returns>True if the location is within the bounding box.</returns>
    public static bool IsWithinBoundingBox(this Location location, Location southWest, Location northEast)
    {
        if (!location.HasValidCoordinates() || !southWest.HasValidCoordinates() || !northEast.HasValidCoordinates())
        {
            return false;
        }

        return location.Latitude >= southWest.Latitude &&
               location.Latitude <= northEast.Latitude &&
               location.Longitude >= southWest.Longitude &&
               location.Longitude <= northEast.Longitude;
    }
}