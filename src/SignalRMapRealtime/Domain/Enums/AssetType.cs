// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Defines the classification of tracked assets in the system.
/// </summary>
public enum AssetType
{
    /// <summary>Delivery van or cargo vehicle.</summary>
    DeliveryVan = 0,

    /// <summary>Courier or personal transport.</summary>
    Courier = 1,

    /// <summary>Bicycle for urban delivery.</summary>
    Bicycle = 2,

    /// <summary>Motorcycle or scooter.</summary>
    Motorcycle = 3,

    /// <summary>Portable device or equipment.</summary>
    Portable = 4,

    /// <summary>Fixed asset or container at location.</summary>
    FixedAsset = 5,

    /// <summary>Drone or aerial vehicle.</summary>
    Drone = 6
}
