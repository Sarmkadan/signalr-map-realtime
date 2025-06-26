// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

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

    /// <summary>
    /// Initializes a new instance of VehicleService.
    /// </summary>
    public VehicleService(VehicleRepository vehicleRepository, UserRepository userRepository, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        _vehicleRepository = vehicleRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new vehicle in the system with validation.
    /// </summary>
    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto vehicleDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vehicleDto);
        if (string.IsNullOrWhiteSpace(vehicleDto.Name))
            throw new ArgumentException("Vehicle name is required.");
        if (string.IsNullOrWhiteSpace(vehicleDto.RegistrationNumber))
            throw new ArgumentException("Registration number is required.");

        if (vehicleDto.DriverId.HasValue)
        {
            var driver = await _userRepository.GetByIdAsync(vehicleDto.DriverId.Value, cancellationToken);
            if (driver == null)
                throw new InvalidOperationException($"Driver with ID {vehicleDto.DriverId} not found.");
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

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VehicleDto>(vehicle);
    }

    /// <summary>
    /// Gets vehicle by ID with complete information.
    /// </summary>
    public async Task<VehicleDto?> GetVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetVehicleWithTrackingDataAsync(vehicleId);
        return vehicle == null ? null : _mapper.Map<VehicleDto>(vehicle);
    }

    /// <summary>
    /// Gets all vehicles with their current status.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets vehicles by operational status.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetVehiclesByStatusAsync(status);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets all online vehicles.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetOnlineVehiclesAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetOnlineVehiclesAsync();
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets vehicles assigned to a specific driver.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByDriverAsync(int driverId, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetVehiclesByDriverAsync(driverId);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Updates vehicle information.
    /// </summary>
    public async Task<VehicleDto> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle == null)
            throw new VehicleNotFoundException(vehicleId);

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
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VehicleDto>(vehicle);
    }

    /// <summary>
    /// Updates the online status of a vehicle.
    /// </summary>
    public async Task<bool> SetVehicleOnlineStatusAsync(int vehicleId, bool isOnline, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle == null)
            return false;

        vehicle.SetOnlineStatus(isOnline);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Updates the operational status of a vehicle.
    /// </summary>
    public async Task<bool> UpdateVehicleStatusAsync(int vehicleId, VehicleStatus newStatus, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle == null)
            return false;

        vehicle.UpdateStatus(newStatus);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets vehicles with low fuel level.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetLowFuelVehiclesAsync(double fuelThreshold = 20.0, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetLowFuelVehiclesAsync(fuelThreshold);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets vehicles currently exceeding speed limits.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetSpeedingVehiclesAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetSpeedingVehiclesAsync();
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }

    /// <summary>
    /// Gets the count of online vehicles.
    /// </summary>
    public async Task<int> GetOnlineVehicleCountAsync(CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.GetOnlineVehicleCountAsync();
    }

    /// <summary>
    /// Checks if vehicle exists.
    /// </summary>
    public async Task<bool> VehicleExistsAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.ExistsAsync(vehicleId, cancellationToken);
    }

    /// <summary>
    /// Deletes a vehicle from the system.
    /// </summary>
    public async Task<bool> DeleteVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle == null)
            return false;

        await _vehicleRepository.RemoveAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets vehicles by asset type.
    /// </summary>
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByAssetTypeAsync(AssetType assetType, CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetVehiclesByAssetTypeAsync(assetType);
        return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
    }
}
