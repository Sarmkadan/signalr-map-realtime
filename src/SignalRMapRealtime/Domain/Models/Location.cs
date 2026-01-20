// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Represents a geographic location point with metadata for tracking and mapping.
/// </summary>
public class Location
{
    /// <summary>Unique identifier for the location record.</summary>
    public int Id { get; set; }

    /// <summary>Latitude coordinate in decimal degrees.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude coordinate in decimal degrees.</summary>
    public double Longitude { get; set; }

    /// <summary>Altitude in meters above sea level.</summary>
    public double? Altitude { get; set; }

    /// <summary>Accuracy of the GPS reading in meters.</summary>
    public double? Accuracy { get; set; }

    /// <summary>Speed of movement in km/h.</summary>
    public double? Speed { get; set; }

    /// <summary>Bearing or heading direction in degrees (0-360).</summary>
    public double? Bearing { get; set; }

    /// <summary>Type classification of this location.</summary>
    public LocationType LocationType { get; set; }

    /// <summary>Address or description of the location.</summary>
    public string? Address { get; set; }

    /// <summary>Additional metadata or notes about the location.</summary>
    public string? Notes { get; set; }

    /// <summary>Identifier of the vehicle this location belongs to.</summary>
    public int VehicleId { get; set; }

    /// <summary>Reference to the associated vehicle.</summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>Identifier of the tracking session this location is part of.</summary>
    public int TrackingSessionId { get; set; }

    /// <summary>Reference to the associated tracking session.</summary>
    public TrackingSession? TrackingSession { get; set; }

    /// <summary>Timestamp when this location was recorded.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Timestamp when this record was created in the database.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Calculates the distance between this location and another in kilometers using the Haversine formula.
    /// </summary>
    public double CalculateDistanceTo(Location other)
    {
        const double EarthRadiusKm = 6371.0;
        double dLat = (other.Latitude - Latitude) * Math.PI / 180.0;
        double dLon = (other.Longitude - Longitude) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Latitude * Math.PI / 180.0) * Math.Cos(other.Latitude * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Validates that the coordinates are within valid geographic bounds.
    /// </summary>
    public bool IsValidCoordinate()
    {
        return Latitude >= -90 && Latitude <= 90 && Longitude >= -180 && Longitude <= 180;
    }

    /// <summary>
    /// Determines if this location is significantly different from another based on distance threshold.
    /// </summary>
    public bool IsDifferentFrom(Location other, double minDistanceKm = 0.05)
    {
        return CalculateDistanceTo(other) >= minDistanceKm;
    }
}
