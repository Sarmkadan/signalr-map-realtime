// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Data transfer object for asset information.
/// </summary>
public class AssetDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Asset name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Serial number.</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Asset type.</summary>
    public AssetType AssetType { get; set; }

    /// <summary>Asset value.</summary>
    public decimal? Value { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Assigned vehicle ID.</summary>
    public int? VehicleId { get; set; }

    /// <summary>Condition status.</summary>
    public string? Condition { get; set; }

    /// <summary>Requires special handling.</summary>
    public bool RequiresSpecialHandling { get; set; }

    /// <summary>Last tracked timestamp.</summary>
    public DateTime? LastTrackedAt { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new asset.
/// </summary>
public class CreateAssetDto
{
    /// <summary>Asset name (required).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Serial number (required).</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Asset type.</summary>
    public AssetType AssetType { get; set; }

    /// <summary>Asset value.</summary>
    public decimal? Value { get; set; }

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Assigned vehicle ID.</summary>
    public int? VehicleId { get; set; }
}

/// <summary>
/// Request DTO for updating asset information.
/// </summary>
public class UpdateAssetDto
{
    /// <summary>Asset name.</summary>
    public string? Name { get; set; }

    /// <summary>Condition status.</summary>
    public string? Condition { get; set; }

    /// <summary>Asset value.</summary>
    public decimal? Value { get; set; }

    /// <summary>Assigned vehicle ID.</summary>
    public int? VehicleId { get; set; }
}
