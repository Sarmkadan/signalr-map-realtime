// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

using SignalRMapRealtime.Domain.Models;

/// <summary>Read response representing a configured geofence zone.</summary>
public class GeofenceDto
{
    /// <summary>Unique identifier of the zone.</summary>
    public Guid Id { get; set; }

    /// <summary>Human-readable name of the zone.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description or operational note.</summary>
    public string? Description { get; set; }

    /// <summary>Boundary shape type as a display string (e.g. "Circle", "Polygon").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Whether the zone is currently evaluated during location checks.</summary>
    public bool IsActive { get; set; }

    /// <summary>Centre latitude for circle zones, in decimal degrees.</summary>
    public double? CenterLatitude { get; set; }

    /// <summary>Centre longitude for circle zones, in decimal degrees.</summary>
    public double? CenterLongitude { get; set; }

    /// <summary>Trigger radius in kilometres for circle zones.</summary>
    public double? RadiusKm { get; set; }

    /// <summary>Semicolon-separated vertex pairs for polygon zones.</summary>
    public string? PolygonCoordinates { get; set; }

    /// <summary>UTC timestamp when the zone was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the last modification.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Identifier of the user who created the zone.</summary>
    public string? CreatedBy { get; set; }
}

/// <summary>Payload for registering a new geofence zone.</summary>
public class CreateGeofenceDto
{
    /// <summary>Human-readable name for the zone.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description or operational note.</summary>
    public string? Description { get; set; }

    /// <summary>Boundary shape type.</summary>
    public GeofenceType Type { get; set; }

    /// <summary>Whether the zone should be active immediately upon creation.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Centre latitude in decimal degrees (required for <see cref="GeofenceType.Circle"/> zones).</summary>
    public double? CenterLatitude { get; set; }

    /// <summary>Centre longitude in decimal degrees (required for <see cref="GeofenceType.Circle"/> zones).</summary>
    public double? CenterLongitude { get; set; }

    /// <summary>Trigger radius in kilometres (required for <see cref="GeofenceType.Circle"/> zones).</summary>
    public double? RadiusKm { get; set; }

    /// <summary>
    /// Semicolon-separated vertex pairs (required for <see cref="GeofenceType.Polygon"/> zones),
    /// each formatted as <c>latitude,longitude</c> using invariant-culture decimals.
    /// </summary>
    public string? PolygonCoordinates { get; set; }

    /// <summary>Identifier of the user registering the zone.</summary>
    public string? CreatedBy { get; set; }
}

/// <summary>Alert payload emitted when a tracked vehicle crosses a geofence boundary.</summary>
public class GeofenceAlertDto
{
    /// <summary>Identifier of the geofence that was crossed.</summary>
    public Guid GeofenceId { get; set; }

    /// <summary>Name of the geofence that was crossed.</summary>
    public string GeofenceName { get; set; } = string.Empty;

    /// <summary>Identifier of the vehicle that triggered the alert.</summary>
    public Guid VehicleId { get; set; }

    /// <summary>Latitude where the boundary crossing was detected, in decimal degrees.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude where the boundary crossing was detected, in decimal degrees.</summary>
    public double Longitude { get; set; }

    /// <summary>Crossing direction: <c>"Entered"</c> when the vehicle entered the zone, <c>"Exited"</c> when it left.</summary>
    public string ViolationType { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the alert was generated.</summary>
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
}
