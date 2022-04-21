#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.Logging;
using AutoMapper;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Exceptions;

/// <summary>
/// Service for managing vehicle tracking and operations.
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly VehicleRepository _vehicleRepository;
    private readonly UserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VehicleService> _logger;

    /// <summary>
    /// Initializes a new instance of VehicleService.
    /// </summary>
    public VehicleService(
        VehicleRepository vehicleRepository,
        UserRepository userRepository,
        IMapper mapper,
        ILogger<VehicleService> logger)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(logger);
        _vehicleRepository = vehicleRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new vehicle in the system with validation.
    /// </summary>
    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto vehicleDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating new vehicle: Name={Name}, Registration={RegistrationNumber}, Type={AssetType}",
            vehicleDto.Name,
            vehicleDto.RegistrationNumber,
            vehicleDto.AssetType);

        ArgumentNullException.ThrowIfNull(vehicleDto);
        if (string.IsNullOrWhiteSpace(vehicleDto.Name))
            throw new ArgumentException("Vehicle name is required.");
        if (string.IsNullOrWhiteSpace(vehicleDto.RegistrationNumber))
            throw new ArgumentException("Registration number is required.");

        if (vehicleDto.DriverId.HasValue)
        {
            var driver = await _userRepository.GetByIdAsync(vehicleDto.DriverId.Value, cancellationToken).ConfigureAwait(false);
            if (driver is null)
            {
                _logger.LogWarning("Driver with ID {DriverId} not found for vehicle {VehicleName}", vehicleDto.DriverId, vehicleDto.Name);
                throw new InvalidOperationException($"Driver with ID {vehicleDto.DriverId} not found.");
            }
        }

        var vehicle = new Vehicle
        {
            Name = vehicleDto.Name,
            RegistrationNumber = vehicleDto.RegistrationNumber,
            AssetType = vehicleDto.AssetType,
            DriverId = vehicleDto.DriverId,
            Manufacturer = vehicleDto.Manufacturer,
            ModelYear = vehicleDto.ModelYear,
            VIN = vehicleDto.VIN,
            MaxSpeed = vehicleDto.MaxSpeed,
            IsOnline = false,
            Status = VehicleStatus.Idle,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _vehicleRepository.AddAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created vehicle {VehicleId}: {Name} ({RegistrationNumber})", vehicle.Id, vehicle.Name, vehicle.RegistrationNumber);
        return _mapper.Map<VehicleDto>(vehicle);
    }

    /// <summary>
    /// Gets vehicle by ID with complete information.
    /// </summary>
    public async Task<VehicleDto?> GetVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving vehicle {VehicleId}", vehicleId);
        var vehicle = await _vehicleRepository.GetVehicleWithTrackingDataAsync(vehicleId).ConfigureAwait(false);
        return vehicle is null ? null : _mapper.Map<VehicleDto>(vehicle);
    }

    /// <summary>
    /// Gets all vehicles with their current status.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving all vehicles");
        var vehicles = await _vehicleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets vehicles by operational status.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving vehicles with status {Status}", status);
        var vehicles = await _vehicleRepository.GetVehiclesByStatusAsync(status).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets all online vehicles.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetOnlineVehiclesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving online vehicles");
        var vehicles = await _vehicleRepository.GetOnlineVehiclesAsync().ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets vehicles assigned to a specific driver.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByDriverAsync(int driverId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving vehicles for driver {DriverId}", driverId);
        var vehicles = await _vehicleRepository.GetVehiclesByDriverAsync(driverId).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Updates vehicle information.
    /// </summary>
    public async Task<VehicleDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating vehicle {VehicleId}", vehicleId);

        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for update", vehicleId);
            throw new VehicleNotFoundException(vehicleId);
        }

        if (!string.IsNullOrWhiteSpace(vehicleDto.Name))
            vehicle.Name = vehicleDto.Name;
        if (vehicleDto.Status.HasValue)
            vehicle.UpdateStatus(vehicleDto.Status.Value);
        if (vehicleDto.DriverId.HasValue)
            vehicle.DriverId = vehicleDto.DriverId;
        if (vehicleDto.FuelLevel.HasValue)
            vehicle.FuelLevel = vehicleDto.FuelLevel;
        if (vehicleDto.MaxSpeed.HasValue)
            vehicle.MaxSpeed = vehicleDto.MaxSpeed;

        vehicle.UpdatedAt = DateTime.UtcNow;
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Updated vehicle {VehicleId}: {Name}", vehicleId, vehicle.Name);
        return _mapper.Map<VehicleDto>(vehicle);
    }

    /// <summary>
    /// Updates the online status of a vehicle.
    /// </summary>
    public async Task<bool> SetVehicleOnlineStatusAsync(int vehicleId, bool isOnline, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting vehicle {VehicleId} online status to {IsOnline}", vehicleId, isOnline);

        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found when setting online status", vehicleId);
            return false;
        }

        vehicle.SetOnlineStatus(isOnline);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Vehicle {VehicleId} online status set to {IsOnline}", vehicleId, isOnline);
        return true;
    }

    /// <summary>
    /// Updates the operational status of a vehicle.
    /// </summary>
    public async Task<bool> UpdateVehicleStatusAsync(int vehicleId, VehicleStatus newStatus, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating vehicle {VehicleId} status to {NewStatus}", vehicleId, newStatus);

        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found when updating status", vehicleId);
            return false;
        }

        vehicle.UpdateStatus(newStatus);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Vehicle {VehicleId} status updated to {NewStatus}", vehicleId, newStatus);
        return true;
    }

    /// <summary>
    /// Gets vehicles with low fuel level.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetLowFuelVehiclesAsync(double fuelThreshold = 20.0, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving vehicles with fuel level below {FuelThreshold}%", fuelThreshold);
        var vehicles = await _vehicleRepository.GetLowFuelVehiclesAsync(fuelThreshold).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets vehicles currently exceeding speed limits.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetSpeedingVehiclesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving speeding vehicles");
        var vehicles = await _vehicleRepository.GetSpeedingVehiclesAsync().ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets the count of online vehicles.
    /// </summary>
    public async Task<int> GetOnlineVehicleCountAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving online vehicle count");
        return await _vehicleRepository.GetOnlineVehicleCountAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if vehicle exists.
    /// </summary>
    public async Task<bool> VehicleExistsAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if vehicle {VehicleId} exists", vehicleId);
        return await _vehicleRepository.ExistsAsync(vehicleId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a vehicle from the system.
    /// </summary>
    public async Task<bool> DeleteVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting vehicle {VehicleId}", vehicleId);

        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for deletion", vehicleId);
            return false;
        }

        await _vehicleRepository.RemoveAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted vehicle {VehicleId}: {Name}", vehicleId, vehicle.Name);
        return true;
    }

    /// <summary>
    /// Gets vehicles by asset type.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByAssetTypeAsync(AssetType assetType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving vehicles with asset type {AssetType}", assetType);
        var vehicles = await _vehicleRepository.GetVehiclesByAssetTypeAsync(assetType).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }
}
