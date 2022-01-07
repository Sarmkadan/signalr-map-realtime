// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Repository for route data access.
/// </summary>
public class RouteRepository : BaseRepository<Route>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of RouteRepository.
    /// </summary>
    public RouteRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets active routes for a vehicle.
    /// </summary>
    public async Task<IEnumerable<Route>> GetActiveRoutesByVehicleAsync(int vehicleId)
    {
        return await _context.Routes
            .Where(r => r.VehicleId == vehicleId && r.IsActive)
            .Include(r => r.Waypoints)
            .Include(r => r.AssignedUser)
            .ToListAsync();
    }

    /// <summary>
    /// Gets routes assigned to a user.
    /// </summary>
    public async Task<IEnumerable<Route>> GetRoutesByUserAsync(int userId)
    {
        return await _context.Routes
            .Where(r => r.AssignedUserId == userId)
            .Include(r => r.Vehicle)
            .Include(r => r.Waypoints)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets routes by completion status.
    /// </summary>
    public async Task<IEnumerable<Route>> GetRoutesByCompletionAsync(bool isCompleted)
    {
        return await _context.Routes
            .Where(r => r.IsCompleted == isCompleted)
            .Include(r => r.Vehicle)
            .Include(r => r.AssignedUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets route with complete waypoint data.
    /// </summary>
    public async Task<Route?> GetRouteWithDetailsAsync(int routeId)
    {
        return await _context.Routes
            .Include(r => r.Vehicle)
            .Include(r => r.AssignedUser)
            .Include(r => r.Waypoints.OrderBy(w => w.Sequence))
            .Include(r => r.TrackingSession)
            .FirstOrDefaultAsync(r => r.Id == routeId);
    }

    /// <summary>
    /// Gets routes created within a date range.
    /// </summary>
    public async Task<IEnumerable<Route>> GetRoutesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Routes
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .Include(r => r.Vehicle)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets routes with longest estimated time.
    /// </summary>
    public async Task<IEnumerable<Route>> GetLongestRoutesAsync(int topCount = 10)
    {
        return await _context.Routes
            .AsEnumerable()
            .OrderByDescending(r => (r.EstimatedArrivalTime - r.PlannedDepartureTime).TotalHours)
            .Include(r => r.Vehicle)
            .Take(topCount)
            .ToListAsync();
    }

    /// <summary>
    /// Gets average completion time for routes.
    /// </summary>
    public async Task<double?> GetAverageCompletionTimeAsync(int vehicleId)
    {
        var completedRoutes = await _context.Routes
            .Where(r => r.VehicleId == vehicleId && r.IsCompleted && r.ActualArrivalTime.HasValue && r.ActualDepartureTime.HasValue)
            .ToListAsync();

        if (!completedRoutes.Any())
            return null;

        return completedRoutes.Average(r => (r.ActualArrivalTime!.Value - r.ActualDepartureTime!.Value).TotalHours);
    }

    /// <summary>
    /// Gets incomplete routes that should be started.
    /// </summary>
    public async Task<IEnumerable<Route>> GetPendingRoutesAsync()
    {
        return await _context.Routes
            .Where(r => !r.IsActive && !r.IsCompleted && r.PlannedDepartureTime <= DateTime.UtcNow)
            .Include(r => r.Vehicle)
            .Include(r => r.AssignedUser)
            .OrderBy(r => r.PlannedDepartureTime)
            .ToListAsync();
    }
}
