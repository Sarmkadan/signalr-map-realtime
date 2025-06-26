// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Represents the operational status of a vehicle in the tracking system.
/// </summary>
public enum VehicleStatus
{
    /// <summary>Vehicle is idle and not in active use.</summary>
    Idle = 0,

    /// <summary>Vehicle is currently in transit with active route.</summary>
    InTransit = 1,

    /// <summary>Vehicle has completed its route and returned to depot.</summary>
    AtDepot = 2,

    /// <summary>Vehicle is temporarily stopped during transit.</summary>
    Stopped = 3,

    /// <summary>Vehicle is unavailable for service.</summary>
    Maintenance = 4,

    /// <summary>Vehicle has been deactivated from tracking.</summary>
    Deactivated = 5
}
