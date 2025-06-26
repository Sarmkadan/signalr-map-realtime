// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Repository for vehicle data access with specialized queries.
/// </summary>
public class VehicleRepository : BaseRepository<Vehicle>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of VehicleRepository.
    /// </summary>
    public VehicleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves vehicles by their operational status.
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetVehiclesByStatusAsync(VehicleStatus status)
    {
        return await _context.Vehicles
            .Where(v => v.Status == status)
            .Include(v => v.LastLocation)
            .Include(v => v.Driver)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves vehicles that are currently online and tracking.
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetOnlineVehiclesAsync()
    {
        return await _context.Vehicles
            .Where(v => v.IsOnline)
            .Include(v => v.LastLocation)
            .Include(v => v.Driver)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves vehicles by registration number.
    /// </summary>
    public async Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationNumber);
        return await _context.Vehicles
            .Include(v => v.Driver)
            .Include(v => v.LastLocation)
            .FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber);
    }

    /// <summary>
    /// Retrieves vehicles assigned to a specific driver.
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetVehiclesByDriverAsync(int driverId)
    {
        return await _context.Vehicles
            .Where(v => v.DriverId == driverId)
            .Include(v => v.LastLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves vehicles by asset type.
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetVehiclesByAssetTypeAsync(AssetType assetType)
    {
        return await _context.Vehicles
            .Where(v => v.AssetType == assetType)
            .Include(v => v.LastLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Gets vehicles with low fuel level.
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetLowFuelVehiclesAsync(double fuelThreshold = 20.0)
    {
        return await _context.Vehicles
            .Where(v => v.FuelLevel.HasValue && v.FuelLevel.Value < fuelThreshold && v.IsOnline)
            .Include(v => v.Driver)
            .ToListAsync();
    }

    /// <summary>
    /// Gets vehicles speeding (exceeding their max speed).
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetSpeedingVehiclesAsync()
    {
        return await _context.Vehicles
            .Where(v => v.LastLocation != null &&
                        v.MaxSpeed.HasValue &&
                        v.LastLocation.Speed.HasValue &&
                        v.LastLocation.Speed > v.MaxSpeed)
            .Include(v => v.LastLocation)
            .Include(v => v.Driver)
            .ToListAsync();
    }

    /// <summary>
    /// Gets total count of online vehicles.
    /// </summary>
    public async Task<int> GetOnlineVehicleCountAsync()
    {
        return await _context.Vehicles.CountAsync(v => v.IsOnline);
    }

    /// <summary>
    /// Retrieves vehicles with their complete tracking data.
    /// </summary>
    public async Task<Vehicle?> GetVehicleWithTrackingDataAsync(int vehicleId)
    {
        return await _context.Vehicles
            .Include(v => v.Driver)
            .Include(v => v.LastLocation)
            .Include(v => v.Locations)
            .Include(v => v.TrackingSessions)
            .Include(v => v.Routes)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);
    }
}
