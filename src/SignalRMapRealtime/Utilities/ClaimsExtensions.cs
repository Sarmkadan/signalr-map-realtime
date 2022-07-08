#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

/// <summary>
/// Extension methods for working with security claims.
/// Provides utilities for extracting user information from ClaimsPrincipal.
/// </summary>
public static class ClaimsExtensions
{
    private const string OrganizationClaimType = "organization";
    private const string DepartmentClaimType = "department";

    /// <summary>
    /// Gets the user ID from claims.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the user ID from.</param>
    /// <returns>The user ID if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the user name from claims.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the user name from.</param>
    /// <returns>The user name if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the user email from claims.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the user email from.</param>
    /// <returns>The user email if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets all user roles from claims.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract roles from.</param>
    /// <returns>An enumerable of role names; empty if no roles are found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    /// <summary>
    /// Checks if user has a specific role.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to check.</param>
    /// <param name="role">The role name to check for.</param>
    /// <returns><see langword="true"/> if the user has the specified role; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="role"/> is <see langword="null"/>.</exception>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(role);
        return principal.FindAll(ClaimTypes.Role).Any(c => c.Value == role);
    }

    /// <summary>
    /// Checks if user has any of the specified roles.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to check.</param>
    /// <param name="roles">The role names to check for.</param>
    /// <returns><see langword="true"/> if the user has any of the specified roles; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="roles"/> is <see langword="null"/>.</exception>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(roles);
        var userRoles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return roles.Any(role => userRoles.Contains(role));
    }

    /// <summary>
    /// Gets a custom claim value by type.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the claim from.</param>
    /// <param name="claimType">The type of claim to retrieve.</param>
    /// <returns>The claim value if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="claimType"/> is <see langword="null"/>.</exception>
    public static string? GetClaimValue(this ClaimsPrincipal principal, string claimType)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(claimType);
        return principal.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Gets all values for a specific claim type.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract claims from.</param>
    /// <param name="claimType">The type of claims to retrieve.</param>
    /// <returns>An enumerable of claim values; empty if no claims are found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="claimType"/> is <see langword="null"/>.</exception>
    public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal principal, string claimType)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(claimType);
        return principal.FindAll(claimType).Select(c => c.Value);
    }

    /// <summary>
    /// Checks if a claim exists.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to check.</param>
    /// <param name="claimType">The type of claim to check for.</param>
    /// <returns><see langword="true"/> if the claim exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="claimType"/> is <see langword="null"/>.</exception>
    public static bool HasClaim(this ClaimsPrincipal principal, string claimType)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(claimType);
        return principal.FindFirst(claimType) is not null;
    }

    /// <summary>
    /// Gets user's organization from claims.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the organization from.</param>
    /// <returns>The organization name if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static string? GetOrganization(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(OrganizationClaimType)?.Value;
    }

    /// <summary>
    /// Gets user's department from claims.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the department from.</param>
    /// <returns>The department name if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static string? GetDepartment(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(DepartmentClaimType)?.Value;
    }

    /// <summary>
    /// Checks if user is authenticated.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to check.</param>
    /// <returns><see langword="true"/> if the user is authenticated; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="principal"/> is <see langword="null"/>.</exception>
    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.Identity?.IsAuthenticated ?? false;
    }
}