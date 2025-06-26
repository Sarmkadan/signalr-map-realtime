// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

/// <summary>
/// Represents a driver, courier, or system user who operates tracked vehicles and assets.
/// </summary>
public class User
{
    /// <summary>Unique identifier for the user.</summary>
    public int Id { get; set; }

    /// <summary>Full name of the user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address of the user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number for contact.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Employee ID or badge number.</summary>
    public string? EmployeeId { get; set; }

    /// <summary>Job title or role (e.g., "Delivery Driver", "Courier").</summary>
    public string? JobTitle { get; set; }

    /// <summary>Department or team assignment.</summary>
    public string? Department { get; set; }

    /// <summary>Profile image or avatar URL.</summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>Indicates if the user account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Indicates if the user is currently logged in and online.</summary>
    public bool IsOnline { get; set; }

    /// <summary>Last recorded geographic location of the user.</summary>
    public Location? LastLocation { get; set; }

    /// <summary>Collection of vehicles assigned to or driven by this user.</summary>
    public ICollection<Vehicle> AssignedVehicles { get; set; } = new List<Vehicle>();

    /// <summary>Collection of routes assigned to this user.</summary>
    public ICollection<Route> AssignedRoutes { get; set; } = new List<Route>();

    /// <summary>Timestamp when the user account was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of the last update to user information.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Timestamp of last login activity.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Updates the user's online status and records the timestamp.
    /// </summary>
    public void SetOnlineStatus(bool isOnline)
    {
        IsOnline = isOnline;
        if (isOnline)
            LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates email format using basic regex pattern.
    /// </summary>
    public bool IsEmailValid()
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(Email);
            return address.Address == Email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Updates the user's current location information.
    /// </summary>
    public void UpdateLocation(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        LastLocation = location;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account and clears online status.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        IsOnline = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates a previously deactivated user account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
