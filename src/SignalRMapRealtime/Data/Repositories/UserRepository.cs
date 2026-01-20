// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Repository for user data access.
/// </summary>
public class UserRepository : BaseRepository<User>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of UserRepository.
    /// </summary>
    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets user by email address.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return await _context.Users
            .Include(u => u.AssignedVehicles)
            .Include(u => u.AssignedRoutes)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Gets user by employee ID.
    /// </summary>
    public async Task<User?> GetByEmployeeIdAsync(string employeeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeId);
        return await _context.Users
            .Include(u => u.AssignedVehicles)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
    }

    /// <summary>
    /// Gets online users.
    /// </summary>
    public async Task<IEnumerable<User>> GetOnlineUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsOnline && u.IsActive)
            .Include(u => u.LastLocation)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active users.
    /// </summary>
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets users by department.
    /// </summary>
    public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(department);
        return await _context.Users
            .Where(u => u.Department == department && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets users with assigned vehicles.
    /// </summary>
    public async Task<IEnumerable<User>> GetDriversWithVehiclesAsync()
    {
        return await _context.Users
            .Include(u => u.AssignedVehicles)
            .Where(u => u.AssignedVehicles.Any() && u.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Gets user login statistics.
    /// </summary>
    public async Task<IEnumerable<User>> GetRecentlyLoggedInUsersAsync(int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.Users
            .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt >= cutoffDate)
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync();
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;
        user.Deactivate();
        await SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets users by job title.
    /// </summary>
    public async Task<IEnumerable<User>> GetUsersByJobTitleAsync(string jobTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobTitle);
        return await _context.Users
            .Where(u => u.JobTitle == jobTitle && u.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Counts online users.
    /// </summary>
    public async Task<int> GetOnlineUserCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsOnline && u.IsActive);
    }
}
