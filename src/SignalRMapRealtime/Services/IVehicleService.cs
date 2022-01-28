#nullable enable
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
    /// <param name="vehicleDto">The data for the new vehicle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the created vehicle DTO.</returns>
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto vehicleDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicle by ID with complete information.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the vehicle DTO, or null if not found.</returns>
    Task<VehicleDto?> GetVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all vehicles with their current status.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of all vehicle DTOs.</returns>
    Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles by operational status.
    /// </summary>
    /// <param name="status">The operational status to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of vehicle DTOs matching the status.</returns>
    Task<IEnumerable<VehicleDto>> GetVehiclesByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all online vehicles.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of online vehicle DTOs.</returns>
    Task<IEnumerable<VehicleDto>> GetOnlineVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles assigned to a specific driver.
    /// </summary>
    /// <param name="driverId">The ID of the driver.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of vehicle DTOs assigned to the driver.</returns>
    Task<IEnumerable<VehicleDto>> GetVehiclesByDriverAsync(int driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates vehicle information.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to update.</param>
    /// <param name="vehicleDto">The new vehicle data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the updated vehicle DTO.</returns>
    Task<VehicleDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the online status of a vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="isOnline">The new online status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, true if the status was updated successfully, otherwise false.</returns>
    Task<bool> SetVehicleOnlineStatusAsync(int vehicleId, bool isOnline, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the operational status of a vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="newStatus">The new operational status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, true if the status was updated successfully, otherwise false.</returns>
    Task<bool> UpdateVehicleStatusAsync(int vehicleId, VehicleStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles with low fuel level.
    /// </summary>
    /// <param name="fuelThreshold">The fuel percentage threshold.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of vehicles with low fuel.</returns>
    Task<IEnumerable<VehicleDto>> GetLowFuelVehiclesAsync(double fuelThreshold = 20.0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles currently exceeding speed limits.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of speeding vehicle DTOs.</returns>
    Task<IEnumerable<VehicleDto>> GetSpeedingVehiclesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of online vehicles.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the count of online vehicles.</returns>
    Task<int> GetOnlineVehicleCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if vehicle exists.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, true if the vehicle exists, otherwise false.</returns>
    Task<bool> VehicleExistsAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vehicle from the system.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, true if deleted successfully, otherwise false.</returns>
    Task<bool> DeleteVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vehicles by asset type.
    /// </summary>
    /// <param name="assetType">The asset type to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of vehicles of the specified asset type.</returns>
    Task<IEnumerable<VehicleDto>> GetVehiclesByAssetTypeAsync(AssetType assetType, CancellationToken cancellationToken = default);
}
