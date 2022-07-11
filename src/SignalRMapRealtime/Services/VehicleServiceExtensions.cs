#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Globalization;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.DTOs;

/// <summary>
/// Extension methods for VehicleService providing additional functionality.
/// </summary>
public static class VehicleServiceExtensions
{
    /// <summary>
    /// Gets a vehicle by its registration number.
    /// </summary>
    /// <param name="service">The vehicle service instance.</param>
    /// <param name="registrationNumber">The vehicle registration number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The vehicle DTO if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when registration number is null or empty.</exception>
    public static async Task<VehicleDto?> GetVehicleByRegistrationAsync(
        this VehicleService service,
        string registrationNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(registrationNumber);

        var allVehicles = await service.GetAllVehiclesAsync(cancellationToken).ConfigureAwait(false);
        return allVehicles.FirstOrDefault(v =>
            string.Equals(v.RegistrationNumber, registrationNumber, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets vehicles by multiple asset types.
    /// </summary>
    /// <param name="service">The vehicle service instance.</param>
    /// <param name="assetTypes">Collection of asset types to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of vehicles matching any of the specified asset types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when assetTypes is null.</exception>
    public static async Task<IEnumerable<VehicleDto>> GetVehiclesByAssetTypesAsync(
        this VehicleService service,
        IEnumerable<AssetType> assetTypes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assetTypes);

        var allVehicles = await service.GetAllVehiclesAsync(cancellationToken).ConfigureAwait(false);
        var assetTypeSet = assetTypes.ToHashSet();
        return allVehicles.Where(v => assetTypeSet.Contains(v.AssetType));
    }

    /// <summary>
    /// Gets vehicles by multiple statuses.
    /// </summary>
    /// <param name="service">The vehicle service instance.</param>
    /// <param name="statuses">Collection of statuses to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of vehicles matching any of the specified statuses.</returns>
    /// <exception cref="ArgumentNullException">Thrown when statuses is null.</exception>
    public static async Task<IEnumerable<VehicleDto>> GetVehiclesByStatusesAsync(
        this VehicleService service,
        IEnumerable<VehicleStatus> statuses,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(statuses);

        var allVehicles = await service.GetAllVehiclesAsync(cancellationToken).ConfigureAwait(false);
        var statusSet = statuses.ToHashSet();
        return allVehicles.Where(v => statusSet.Contains(v.Status));
    }

    /// <summary>
    /// Gets vehicles that are currently online and have a specific status.
    /// </summary>
    /// <param name="service">The vehicle service instance.</param>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of online vehicles with the specified status.</returns>
    public static async Task<IEnumerable<VehicleDto>> GetOnlineVehiclesByStatusAsync(
        this VehicleService service,
        VehicleStatus status,
        CancellationToken cancellationToken = default)
    {
        var onlineVehicles = await service.GetOnlineVehiclesAsync(cancellationToken).ConfigureAwait(false);
        return onlineVehicles.Where(v => v.Status == status);
    }
}