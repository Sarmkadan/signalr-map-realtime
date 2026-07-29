#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;
using MyClaimsExtensions = SignalRMapRealtime.Utilities.ClaimsExtensions;

namespace SignalRMapRealtime.Tests;

public class ClaimsExtensionsTests
{
    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims) =>
        new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

    [Fact]
    public void GetUserId_ReturnsId_WhenClaimExists()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "user-123"));
        var result = MyClaimsExtensions.GetUserId(principal);
        Assert.Equal("user-123", result);
    }

    [Fact]
    public void GetUserId_ReturnsNull_WhenClaimMissing()
    {
        var principal = CreatePrincipal();
        var result = MyClaimsExtensions.GetUserId(principal);
        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_NullPrincipal_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.GetUserId(null!));
    }

    [Fact]
    public void GetUserName_ReturnsName_WhenClaimExists()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "John Doe"));
        var result = MyClaimsExtensions.GetUserName(principal);
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void GetUserEmail_ReturnsEmail_WhenClaimExists()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Email, "john@example.com"));
        var result = MyClaimsExtensions.GetUserEmail(principal);
        Assert.Equal("john@example.com", result);
    }

    [Fact]
    public void GetUserRoles_ReturnsAllRoles()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"));
        var roles = MyClaimsExtensions.GetUserRoles(principal).ToList();
        Assert.Equal(2, roles.Count);
        Assert.Contains("Admin", roles);
        Assert.Contains("User", roles);
    }

    [Fact]
    public void GetUserRoles_ReturnsEmpty_WhenNoRoles()
    {
        var principal = CreatePrincipal();
        var roles = MyClaimsExtensions.GetUserRoles(principal);
        Assert.Empty(roles);
    }

    [Fact]
    public void HasRole_ReturnsTrue_WhenRoleExists()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Role, "Manager"));
        var result = MyClaimsExtensions.HasRole(principal, "Manager");
        Assert.True(result);
    }

    [Fact]
    public void HasRole_ReturnsFalse_WhenRoleMissing()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Role, "Employee"));
        var result = MyClaimsExtensions.HasRole(principal, "Admin");
        Assert.False(result);
    }

    [Fact]
    public void HasRole_NullPrincipal_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.HasRole(null!, "Any"));
    }

    [Fact]
    public void HasRole_NullRole_Throws()
    {
        var principal = CreatePrincipal();
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.HasRole(principal, null!));
    }

    [Fact]
    public void HasAnyRole_ReturnsTrue_WhenAnyMatch()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.Role, "Editor"),
            new Claim(ClaimTypes.Role, "Contributor"));
        var result = MyClaimsExtensions.HasAnyRole(principal, "Admin", "Editor");
        Assert.True(result);
    }

    [Fact]
    public void HasAnyRole_ReturnsFalse_WhenNoMatch()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Role, "Viewer"));
        var result = MyClaimsExtensions.HasAnyRole(principal, "Admin", "Editor");
        Assert.False(result);
    }

    [Fact]
    public void HasAnyRole_NullPrincipal_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.HasAnyRole(null!, "Any"));
    }

    [Fact]
    public void HasAnyRole_NullRolesArray_Throws()
    {
        var principal = CreatePrincipal();
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.HasAnyRole(principal, null!));
    }

    [Fact]
    public void GetClaimValue_ReturnsValue_WhenExists()
    {
        var principal = CreatePrincipal(new Claim("custom-claim", "custom-value"));
        var result = MyClaimsExtensions.GetClaimValue(principal, "custom-claim");
        Assert.Equal("custom-value", result);
    }

    [Fact]
    public void GetClaimValue_ReturnsNull_WhenMissing()
    {
        var principal = CreatePrincipal();
        var result = MyClaimsExtensions.GetClaimValue(principal, "nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public void GetClaimValue_NullPrincipal_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.GetClaimValue(null!, "any"));
    }

    [Fact]
    public void GetClaimValue_NullClaimType_Throws()
    {
        var principal = CreatePrincipal();
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.GetClaimValue(principal, null!));
    }

    [Fact]
    public void GetClaimValues_ReturnsAllValues_ForSameType()
    {
        var principal = CreatePrincipal(
            new Claim("multi", "first"),
            new Claim("multi", "second"));
        var values = MyClaimsExtensions.GetClaimValues(principal, "multi").ToList();
        Assert.Equal(2, values.Count);
        Assert.Contains("first", values);
        Assert.Contains("second", values);
    }

    [Fact]
    public void GetClaimValues_ReturnsEmpty_WhenMissing()
    {
        var principal = CreatePrincipal();
        var values = MyClaimsExtensions.GetClaimValues(principal, "absent");
        Assert.Empty(values);
    }

    [Fact]
    public void HasClaim_ReturnsTrue_WhenClaimExists()
    {
        var principal = CreatePrincipal(new Claim("feature", "enabled"));
        var result = MyClaimsExtensions.HasClaim(principal, "feature");
        Assert.True(result);
    }

    [Fact]
    public void HasClaim_ReturnsFalse_WhenClaimMissing()
    {
        var principal = CreatePrincipal();
        var result = MyClaimsExtensions.HasClaim(principal, "missing");
        Assert.False(result);
    }

    [Fact]
    public void HasClaim_NullPrincipal_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.HasClaim(null!, "any"));
    }

    [Fact]
    public void HasClaim_NullClaimType_Throws()
    {
        var principal = CreatePrincipal();
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.HasClaim(principal, null!));
    }

    [Fact]
    public void GetOrganization_ReturnsValue_WhenPresent()
    {
        var principal = CreatePrincipal(new Claim("organization", "Acme Corp"));
        var result = MyClaimsExtensions.GetOrganization(principal);
        Assert.Equal("Acme Corp", result);
    }

    [Fact]
    public void GetOrganization_ReturnsNull_WhenMissing()
    {
        var principal = CreatePrincipal();
        var result = MyClaimsExtensions.GetOrganization(principal);
        Assert.Null(result);
    }

    [Fact]
    public void GetDepartment_ReturnsValue_WhenPresent()
    {
        var principal = CreatePrincipal(new Claim("department", "Engineering"));
        var result = MyClaimsExtensions.GetDepartment(principal);
        Assert.Equal("Engineering", result);
    }

    [Fact]
    public void GetDepartment_ReturnsNull_WhenMissing()
    {
        var principal = CreatePrincipal();
        var result = MyClaimsExtensions.GetDepartment(principal);
        Assert.Null(result);
    }

    [Fact]
    public void IsAuthenticated_ReturnsTrue_WhenIdentityAuthenticated()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var result = MyClaimsExtensions.IsAuthenticated(principal);
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenIdentityNotAuthenticated()
    {
        var identity = new ClaimsIdentity(); // not authenticated
        var principal = new ClaimsPrincipal(identity);
        var result = MyClaimsExtensions.IsAuthenticated(principal);
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_NullPrincipal_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MyClaimsExtensions.IsAuthenticated(null!));
    }
}
