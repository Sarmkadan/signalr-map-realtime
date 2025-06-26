// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Categorizes different types of locations in the tracking system.
/// </summary>
public enum LocationType
{
    /// <summary>Location point recorded during vehicle transit.</summary>
    TrackPoint = 0,

    /// <summary>Delivery or service location.</summary>
    DeliveryPoint = 1,

    /// <summary>Vehicle depot or base station.</summary>
    Depot = 2,

    /// <summary>Customer or client location.</summary>
    CustomerLocation = 3,

    /// <summary>Waypoint on the planned route.</summary>
    Waypoint = 4,

    /// <summary>Geofence boundary or area.</summary>
    Geofence = 5
}
