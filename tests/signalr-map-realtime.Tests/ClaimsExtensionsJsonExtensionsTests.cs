#nullable enable
using System;
using System.Security.Claims;
using Xunit;
using SignalRMapRealtime.Utilities;

namespace SignalRMapRealtime.Tests;

public class ClaimsExtensionsJsonExtensionsTests
{
    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims) =>
        new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

    [Fact]
    public void ToJson_NullPrincipal_ThrowsArgumentNullException()
    {
        ClaimsPrincipal? principal = null;
        Assert.Throws<ArgumentNullException>(() => principal!.ToJson());
    }

    [Fact]
    public void ToJson_ValidPrincipal_ReturnsJson()
    {
        var principal = CreatePrincipal(new Claim("TestType", "TestValue"));
        var json = principal.ToJson();
        Assert.NotEmpty(json);
        Assert.Contains("TestType", json);
        Assert.Contains("TestValue", json);
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ReturnsNull()
    {
        Assert.Null(ClaimsExtensionsJsonExtensions.FromJson(null));
        Assert.Null(ClaimsExtensionsJsonExtensions.FromJson(""));
        Assert.Null(ClaimsExtensionsJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsPrincipal()
    {
        var principal = CreatePrincipal(new Claim("TestType", "TestValue"));
        var json = principal.ToJson();
        var result = ClaimsExtensionsJsonExtensions.FromJson(json);
        
        Assert.NotNull(result);
        Assert.Equal("TestValue", result!.FindFirst("TestType")?.Value);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ClaimsExtensionsJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrue()
    {
        var principal = CreatePrincipal(new Claim("TestType", "TestValue"));
        var json = principal.ToJson();
        
        bool success = ClaimsExtensionsJsonExtensions.TryFromJson(json, out var result);
        
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("TestValue", result!.FindFirst("TestType")?.Value);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        bool success = ClaimsExtensionsJsonExtensions.TryFromJson("invalid-json", out var result);
        
        Assert.False(success);
        Assert.Null(result);
    }
}
