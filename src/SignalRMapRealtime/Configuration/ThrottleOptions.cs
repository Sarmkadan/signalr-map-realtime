#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Per-asset-type location update throttle configuration.
/// Controls the minimum interval between accepted location broadcasts for each asset type,
/// reducing unnecessary SignalR traffic for slow-moving or stationary assets.
/// These options are loaded from appsettings.json under the "LocationThrottle" section.
/// </summary>
public class ThrottleOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "LocationThrottle";

    /// <summary>
    /// Enable or disable per-asset-type throttling globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Minimum seconds between accepted updates for DeliveryVan assets.
    /// </summary>
    public int DeliveryVanIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// Minimum seconds between accepted updates for Courier (on-foot) assets.
    /// </summary>
    public int CourierIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Minimum seconds between accepted updates for Bicycle assets.
    /// </summary>
    public int BicycleIntervalSeconds { get; set; } = 15;

    /// <summary>
    /// Minimum seconds between accepted updates for Motorcycle assets.
    /// </summary>
    public int MotorcycleIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Minimum seconds between accepted updates for Portable device assets.
    /// </summary>
    public int PortableIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Minimum seconds between accepted updates for FixedAsset (rarely-moving) assets.
    /// </summary>
    public int FixedAssetIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Minimum seconds between accepted updates for Drone assets.
    /// </summary>
    public int DroneIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// Returns the configured minimum update interval for the given asset type.
    /// </summary>
    public TimeSpan GetIntervalForAssetType(AssetType assetType) => assetType switch
    {
        AssetType.DeliveryVan  => TimeSpan.FromSeconds(DeliveryVanIntervalSeconds),
        AssetType.Courier      => TimeSpan.FromSeconds(CourierIntervalSeconds),
        AssetType.Bicycle      => TimeSpan.FromSeconds(BicycleIntervalSeconds),
        AssetType.Motorcycle   => TimeSpan.FromSeconds(MotorcycleIntervalSeconds),
        AssetType.Portable     => TimeSpan.FromSeconds(PortableIntervalSeconds),
        AssetType.FixedAsset   => TimeSpan.FromSeconds(FixedAssetIntervalSeconds),
        AssetType.Drone        => TimeSpan.FromSeconds(DroneIntervalSeconds),
        _                      => TimeSpan.FromSeconds(DeliveryVanIntervalSeconds)
    };
}
