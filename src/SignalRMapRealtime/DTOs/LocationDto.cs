// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Data transfer object for location information.
/// </summary>
public class LocationDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Latitude coordinate.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude coordinate.</summary>
    public double Longitude { get; set; }

    /// <summary>Altitude in meters.</summary>
    public double? Altitude { get; set; }

    /// <summary>GPS accuracy in meters.</summary>
    public double? Accuracy { get; set; }

    /// <summary>Current speed in km/h.</summary>
    public double? Speed { get; set; }

    /// <summary>Direction bearing in degrees.</summary>
    public double? Bearing { get; set; }

    /// <summary>Location type classification.</summary>
    public LocationType LocationType { get; set; }

    /// <summary>Address or description.</summary>
    public string? Address { get; set; }

    /// <summary>Additional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Associated vehicle ID.</summary>
    public int VehicleId { get; set; }

    /// <summary>Recording timestamp.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new location record.
/// </summary>
public class CreateLocationDto
{
    /// <summary>Latitude coordinate (required).</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude coordinate (required).</summary>
    public double Longitude { get; set; }

    /// <summary>Altitude in meters.</summary>
    public double? Altitude { get; set; }

    /// <summary>GPS accuracy in meters.</summary>
    public double? Accuracy { get; set; }

    /// <summary>Current speed in km/h.</summary>
    public double? Speed { get; set; }

    /// <summary>Direction bearing in degrees.</summary>
    public double? Bearing { get; set; }

    /// <summary>Location type classification.</summary>
    public LocationType LocationType { get; set; }

    /// <summary>Address or description.</summary>
    public string? Address { get; set; }

    /// <summary>Additional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Associated vehicle ID (required).</summary>
    public int VehicleId { get; set; }
}

/// <summary>
/// Request DTO for updating location information.
/// </summary>
public class UpdateLocationDto
{
    /// <summary>Current speed in km/h.</summary>
    public double? Speed { get; set; }

    /// <summary>Direction bearing in degrees.</summary>
    public double? Bearing { get; set; }

    /// <summary>Address or description.</summary>
    public string? Address { get; set; }

    /// <summary>Additional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Location type classification.</summary>
    public LocationType? LocationType { get; set; }
}
