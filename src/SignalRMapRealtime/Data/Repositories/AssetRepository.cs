// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Repository for asset data access.
/// </summary>
public class AssetRepository : BaseRepository<Asset>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of AssetRepository.
    /// </summary>
    public AssetRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets asset by serial number.
    /// </summary>
    public async Task<Asset?> GetBySerialNumberAsync(string serialNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serialNumber);
        return await _context.Assets
            .Include(a => a.Vehicle)
            .Include(a => a.CurrentLocation)
            .FirstOrDefaultAsync(a => a.SerialNumber == serialNumber);
    }

    /// <summary>
    /// Gets assets by type.
    /// </summary>
    public async Task<IEnumerable<Asset>> GetAssetsByTypeAsync(AssetType assetType)
    {
        return await _context.Assets
            .Where(a => a.AssetType == assetType)
            .Include(a => a.Vehicle)
            .Include(a => a.CurrentLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Gets assets assigned to a vehicle.
    /// </summary>
    public async Task<IEnumerable<Asset>> GetAssetsByVehicleAsync(int vehicleId)
    {
        return await _context.Assets
            .Where(a => a.VehicleId == vehicleId)
            .Include(a => a.CurrentLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Gets unassigned assets not attached to any vehicle.
    /// </summary>
    public async Task<IEnumerable<Asset>> GetUnassignedAssetsAsync()
    {
        return await _context.Assets
            .Where(a => a.VehicleId == null)
            .Include(a => a.CurrentLocation)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets assets requiring special handling.
    /// </summary>
    public async Task<IEnumerable<Asset>> GetSpecialHandlingAssetsAsync()
    {
        return await _context.Assets
            .Where(a => a.RequiresSpecialHandling)
            .Include(a => a.Vehicle)
            .Include(a => a.CurrentLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Gets assets with tracking data.
    /// </summary>
    public async Task<Asset?> GetAssetWithHistoryAsync(int assetId)
    {
        return await _context.Assets
            .Include(a => a.Vehicle)
            .Include(a => a.CurrentLocation)
            .Include(a => a.LocationHistory)
            .FirstOrDefaultAsync(a => a.Id == assetId);
    }

    /// <summary>
    /// Gets recently tracked assets.
    /// </summary>
    public async Task<IEnumerable<Asset>> GetRecentlyTrackedAssetsAsync(int minutesBack = 60)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutesBack);
        return await _context.Assets
            .Where(a => a.LastTrackedAt.HasValue && a.LastTrackedAt >= cutoffTime)
            .Include(a => a.Vehicle)
            .Include(a => a.CurrentLocation)
            .OrderByDescending(a => a.LastTrackedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets total asset value by type.
    /// </summary>
    public async Task<decimal> GetTotalValueByTypeAsync(AssetType assetType)
    {
        return await _context.Assets
            .Where(a => a.AssetType == assetType && a.Value.HasValue)
            .SumAsync(a => a.Value ?? 0);
    }

    /// <summary>
    /// Gets assets not tracked recently.
    /// </summary>
    public async Task<IEnumerable<Asset>> GetNotRecentlyTrackedAsync(int hoursNoTrack = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hoursNoTrack);
        return await _context.Assets
            .Where(a => !a.LastTrackedAt.HasValue || a.LastTrackedAt < cutoffTime)
            .Include(a => a.Vehicle)
            .OrderBy(a => a.LastTrackedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Counts assets by condition.
    /// </summary>
    public async Task<int> CountByConditionAsync(string condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(condition);
        return await _context.Assets.CountAsync(a => a.Condition == condition);
    }
}
