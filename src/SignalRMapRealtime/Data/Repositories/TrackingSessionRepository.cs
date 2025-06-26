// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Repository for tracking session data access.
/// </summary>
public class TrackingSessionRepository : BaseRepository<TrackingSession>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of TrackingSessionRepository.
    /// </summary>
    public TrackingSessionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the active tracking session for a vehicle.
    /// </summary>
    public async Task<TrackingSession?> GetActiveSessionByVehicleAsync(int vehicleId)
    {
        return await _context.TrackingSessions
            .Where(ts => ts.VehicleId == vehicleId && ts.Status == SessionStatus.Active)
            .Include(ts => ts.Locations)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all sessions for a vehicle.
    /// </summary>
    public async Task<IEnumerable<TrackingSession>> GetSessionsByVehicleAsync(int vehicleId)
    {
        return await _context.TrackingSessions
            .Where(ts => ts.VehicleId == vehicleId)
            .Include(ts => ts.Locations)
            .OrderByDescending(ts => ts.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets sessions by status.
    /// </summary>
    public async Task<IEnumerable<TrackingSession>> GetSessionsByStatusAsync(SessionStatus status)
    {
        return await _context.TrackingSessions
            .Where(ts => ts.Status == status)
            .Include(ts => ts.Vehicle)
            .OrderByDescending(ts => ts.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets sessions within a time range.
    /// </summary>
    public async Task<IEnumerable<TrackingSession>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TrackingSessions
            .Where(ts => ts.StartTime >= startDate && ts.StartTime <= endDate)
            .Include(ts => ts.Vehicle)
            .OrderByDescending(ts => ts.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets session with complete data including locations.
    /// </summary>
    public async Task<TrackingSession?> GetSessionWithDetailsAsync(int sessionId)
    {
        return await _context.TrackingSessions
            .Include(ts => ts.Vehicle)
            .Include(ts => ts.Locations)
            .Include(ts => ts.Route)
            .FirstOrDefaultAsync(ts => ts.Id == sessionId);
    }

    /// <summary>
    /// Gets sessions with high average speed.
    /// </summary>
    public async Task<IEnumerable<TrackingSession>> GetHighSpeedSessionsAsync(double speedThreshold = 80.0)
    {
        return await _context.TrackingSessions
            .Where(ts => ts.AverageSpeed >= speedThreshold && ts.Status == SessionStatus.Completed)
            .Include(ts => ts.Vehicle)
            .OrderByDescending(ts => ts.AverageSpeed)
            .ToListAsync();
    }

    /// <summary>
    /// Gets recently expired sessions.
    /// </summary>
    public async Task<IEnumerable<TrackingSession>> GetExpiredSessionsAsync()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        return await _context.TrackingSessions
            .Where(ts => ts.Status == SessionStatus.Active && ts.StartTime < oneHourAgo)
            .Include(ts => ts.Vehicle)
            .ToListAsync();
    }

    /// <summary>
    /// Gets total distance traveled across all sessions.
    /// </summary>
    public async Task<double> GetTotalDistanceTraveledAsync(int vehicleId)
    {
        return await _context.TrackingSessions
            .Where(ts => ts.VehicleId == vehicleId && ts.Status == SessionStatus.Completed)
            .SumAsync(ts => ts.TotalDistance);
    }

    /// <summary>
    /// Counts active sessions globally.
    /// </summary>
    public async Task<int> GetActiveSessionCountAsync()
    {
        return await _context.TrackingSessions
            .CountAsync(ts => ts.Status == SessionStatus.Active);
    }
}
