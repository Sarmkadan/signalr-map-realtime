// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public class UserDto
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Employee ID.</summary>
    public string? EmployeeId { get; set; }

    /// <summary>Job title.</summary>
    public string? JobTitle { get; set; }

    /// <summary>Department.</summary>
    public string? Department { get; set; }

    /// <summary>Online status.</summary>
    public bool IsOnline { get; set; }

    /// <summary>Account active status.</summary>
    public bool IsActive { get; set; }

    /// <summary>Last login time.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public class CreateUserDto
{
    /// <summary>Full name (required).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address (required).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Employee ID.</summary>
    public string? EmployeeId { get; set; }

    /// <summary>Job title.</summary>
    public string? JobTitle { get; set; }

    /// <summary>Department.</summary>
    public string? Department { get; set; }
}

/// <summary>
/// Request DTO for updating user information.
/// </summary>
public class UpdateUserDto
{
    /// <summary>Full name.</summary>
    public string? FullName { get; set; }

    /// <summary>Phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Job title.</summary>
    public string? JobTitle { get; set; }

    /// <summary>Department.</summary>
    public string? Department { get; set; }
}
