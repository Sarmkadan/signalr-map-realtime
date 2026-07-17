using System;

namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Provides extension methods for <see cref="LocationDto"/> objects to perform location-based calculations and comparisons.
/// </summary>
public static class LocationDtoExtensions
{
    /// <summary>
    /// Calculates the distance between two locations using the Haversine formula.
    /// </summary>
    /// <param name="location">The location to calculate from. Cannot be <see langword="null"/>.</param>
    /// <param name="otherLocation">The other location. Cannot be <see langword="null"/>.</param>
    /// <returns>The distance between the two locations in kilometers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="location"/> or <paramref name="otherLocation"/> is <see langword="null"/>.</exception>
    public static double CalculateDistanceTo(this LocationDto location, LocationDto otherLocation)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(otherLocation);

        var lat1 = location.Latitude * Math.PI / 180;
        var lon1 = location.Longitude * Math.PI / 180;
        var lat2 = otherLocation.Latitude * Math.PI / 180;
        var lon2 = otherLocation.Longitude * Math.PI / 180;

        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;

        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Pow(Math.Sin(dLon / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var radius = 6371; // Radius of the Earth in kilometers

        return radius * c;
    }

    /// <summary>
    /// Checks if two locations are within a certain distance of each other.
    /// </summary>
    /// <param name="location">The location to check. Cannot be <see langword="null"/>.</param>
    /// <param name="otherLocation">The other location. Cannot be <see langword="null"/>.</param>
    /// <param name="maxDistanceKm">The maximum distance in kilometers. Must be non-negative.</param>
    /// <returns>True if the locations are within the maximum distance, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="location"/> or <paramref name="otherLocation"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="maxDistanceKm"/> is negative.</exception>
    public static bool IsWithinDistance(this LocationDto location, LocationDto otherLocation, double maxDistanceKm)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(otherLocation);

        if (maxDistanceKm < 0)
        {
            throw new ArgumentException("Max distance must be non-negative", nameof(maxDistanceKm));
        }

        return location.CalculateDistanceTo(otherLocation) <= maxDistanceKm;
    }
}