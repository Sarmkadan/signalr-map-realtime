// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

using System.Security.Claims;

/// <summary>
/// Extension methods for working with security claims.
/// Provides utilities for extracting user information from ClaimsPrincipal.
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// Gets the user ID from claims.
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the user name from claims.
    /// </summary>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the user email from claims.
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets all user roles from claims.
    /// </summary>
    public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal principal)
    {
        return principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Checks if user has a specific role.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal?.FindAll(ClaimTypes.Role).Any(c => c.Value == role) ?? false;
    }

    /// <summary>
    /// Checks if user has any of the specified roles.
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        var userRoles = principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
        return roles.Any(role => userRoles.Contains(role));
    }

    /// <summary>
    /// Gets a custom claim value by type.
    /// </summary>
    public static string? GetClaimValue(this ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Gets all values for a specific claim type.
    /// </summary>
    public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindAll(claimType).Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Checks if a claim exists.
    /// </summary>
    public static bool HasClaim(this ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindFirst(claimType) != null;
    }

    /// <summary>
    /// Gets user's organization from claims.
    /// </summary>
    public static string? GetOrganization(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst("organization")?.Value;
    }

    /// <summary>
    /// Gets user's department from claims.
    /// </summary>
    public static string? GetDepartment(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst("department")?.Value;
    }

    /// <summary>
    /// Checks if user is authenticated.
    /// </summary>
    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        return principal?.Identity?.IsAuthenticated ?? false;
    }
}
