// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Data transfer object for route information.
/// </summary>
public class RouteDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Route name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Assigned vehicle ID.</summary>
    public int VehicleId { get; set; }

    /// <summary>Assigned vehicle DTO.</summary>
    public VehicleDto? Vehicle { get; set; }

    /// <summary>Assigned user ID.</summary>
    public int? AssignedUserId { get; set; }

    /// <summary>Assigned user DTO.</summary>
    public UserDto? AssignedUser { get; set; }

    /// <summary>Planned departure time.</summary>
    public DateTime PlannedDepartureTime { get; set; }

    /// <summary>Estimated arrival time.</summary>
    public DateTime EstimatedArrivalTime { get; set; }

    /// <summary>Planned distance in km.</summary>
    public double? TotalDistance { get; set; }

    /// <summary>Actual distance in km.</summary>
    public double? ActualDistance { get; set; }

    /// <summary>Route active status.</summary>
    public bool IsActive { get; set; }

    /// <summary>Route completed status.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Waypoints on route.</summary>
    public ICollection<WaypointDto> Waypoints { get; set; } = new List<WaypointDto>();

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new route.
/// </summary>
public class CreateRouteDto
{
    /// <summary>Route name (required).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Vehicle ID (required).</summary>
    public int VehicleId { get; set; }

    /// <summary>Assigned user ID.</summary>
    public int? AssignedUserId { get; set; }

    /// <summary>Planned departure time.</summary>
    public DateTime PlannedDepartureTime { get; set; }

    /// <summary>Estimated arrival time.</summary>
    public DateTime EstimatedArrivalTime { get; set; }

    /// <summary>Planned distance in km.</summary>
    public double? TotalDistance { get; set; }
}

/// <summary>
/// Request DTO for updating route information.
/// </summary>
public class UpdateRouteDto
{
    /// <summary>Route name.</summary>
    public string? Name { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Assigned user ID.</summary>
    public int? AssignedUserId { get; set; }

    /// <summary>Estimated arrival time.</summary>
    public DateTime? EstimatedArrivalTime { get; set; }
}

/// <summary>
/// Data transfer object for waypoint information.
/// </summary>
public class WaypointDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Sequence order.</summary>
    public int Sequence { get; set; }

    /// <summary>Waypoint name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Latitude.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude.</summary>
    public double Longitude { get; set; }

    /// <summary>Address.</summary>
    public string? Address { get; set; }

    /// <summary>Completion status.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Contact name.</summary>
    public string? ContactName { get; set; }

    /// <summary>Contact phone.</summary>
    public string? ContactPhone { get; set; }
}

/// <summary>
/// Request DTO for creating a waypoint.
/// </summary>
public class CreateWaypointDto
{
    /// <summary>Waypoint name (required).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Latitude (required).</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude (required).</summary>
    public double Longitude { get; set; }

    /// <summary>Address.</summary>
    public string? Address { get; set; }

    /// <summary>Instructions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Contact name.</summary>
    public string? ContactName { get; set; }

    /// <summary>Contact phone.</summary>
    public string? ContactPhone { get; set; }
}

/// <summary>
/// Request DTO for updating waypoint information.
/// </summary>
public class UpdateWaypointDto
{
    /// <summary>Waypoint name.</summary>
    public string? Name { get; set; }

    /// <summary>Address.</summary>
    public string? Address { get; set; }

    /// <summary>Contact name.</summary>
    public string? ContactName { get; set; }

    /// <summary>Contact phone.</summary>
    public string? ContactPhone { get; set; }
}
