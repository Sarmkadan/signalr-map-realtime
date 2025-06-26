// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Data transfer object for vehicle information.
/// </summary>
public class VehicleDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Vehicle name or identifier.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>License plate or registration number.</summary>
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>Operational status.</summary>
    public VehicleStatus Status { get; set; }

    /// <summary>Asset type classification.</summary>
    public AssetType AssetType { get; set; }

    /// <summary>Assigned driver ID.</summary>
    public int? DriverId { get; set; }

    /// <summary>Manufacturer name.</summary>
    public string? Manufacturer { get; set; }

    /// <summary>Model year.</summary>
    public int? ModelYear { get; set; }

    /// <summary>Maximum speed limit in km/h.</summary>
    public double? MaxSpeed { get; set; }

    /// <summary>Current fuel level percentage.</summary>
    public double? FuelLevel { get; set; }

    /// <summary>Online status.</summary>
    public bool IsOnline { get; set; }

    /// <summary>Last recorded location.</summary>
    public LocationDto? LastLocation { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new vehicle.
/// </summary>
public class CreateVehicleDto
{
    /// <summary>Vehicle name or identifier (required).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>License plate or registration number (required).</summary>
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>Asset type classification.</summary>
    public AssetType AssetType { get; set; }

    /// <summary>Assigned driver ID.</summary>
    public int? DriverId { get; set; }

    /// <summary>Manufacturer name.</summary>
    public string? Manufacturer { get; set; }

    /// <summary>Model year.</summary>
    public int? ModelYear { get; set; }

    /// <summary>VIN or chassis number.</summary>
    public string? VIN { get; set; }

    /// <summary>Maximum speed limit in km/h.</summary>
    public double? MaxSpeed { get; set; }
}

/// <summary>
/// Request DTO for updating vehicle information.
/// </summary>
public class UpdateVehicleDto
{
    /// <summary>Vehicle name or identifier.</summary>
    public string? Name { get; set; }

    /// <summary>Operational status.</summary>
    public VehicleStatus? Status { get; set; }

    /// <summary>Assigned driver ID.</summary>
    public int? DriverId { get; set; }

    /// <summary>Current fuel level percentage.</summary>
    public double? FuelLevel { get; set; }

    /// <summary>Maximum speed limit in km/h.</summary>
    public double? MaxSpeed { get; set; }
}
