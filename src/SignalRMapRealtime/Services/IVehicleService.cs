// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.DTOs;

/// <summary>
/// Service interface for vehicle management and tracking.
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Creates a new vehicle in the system.
    /// </summary>
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto vehicleDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicle by ID with complete information.
    /// </summary>
    Task<VehicleDto?> GetVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vehicles with their current status.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles by operational status.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetVehiclesByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all online vehicles.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetOnlineVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles assigned to a specific driver.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetVehiclesByDriverAsync(int driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates vehicle information.
    /// </summary>
    Task<VehicleDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the online status of a vehicle.
    /// </summary>
    Task<bool> SetVehicleOnlineStatusAsync(int vehicleId, bool isOnline, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the operational status of a vehicle.
    /// </summary>
    Task<bool> UpdateVehicleStatusAsync(int vehicleId, VehicleStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles with low fuel level.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetLowFuelVehiclesAsync(double fuelThreshold = 20.0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles currently exceeding speed limits.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetSpeedingVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of online vehicles.
    /// </summary>
    Task<int> GetOnlineVehicleCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if vehicle exists.
    /// </summary>
    Task<bool> VehicleExistsAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vehicle from the system.
    /// </summary>
    Task<bool> DeleteVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles by asset type.
    /// </summary>
    Task<IEnumerable<VehicleDto>> GetVehiclesByAssetTypeAsync(AssetType assetType, CancellationToken cancellationToken = default);
}
