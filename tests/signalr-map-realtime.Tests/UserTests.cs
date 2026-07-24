#nullable enable
using System;
using Xunit;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Tests;

public class UserTests
{
    [Fact]
    public void DefaultValues_ShouldBeInitialized()
    {
        var user = new User();

        Assert.Equal(0, user.Id);
        Assert.Equal(string.Empty, user.FullName);
        Assert.Equal(string.Empty, user.Email);
        Assert.Null(user.PhoneNumber);
        Assert.Null(user.EmployeeId);
        Assert.Null(user.JobTitle);
        Assert.Null(user.Department);
        Assert.Null(user.ProfileImageUrl);
        Assert.True(user.IsActive);
        Assert.False(user.IsOnline);
        Assert.NotNull(user.AssignedVehicles);
        Assert.NotNull(user.AssignedRoutes);
    }

    [Fact]
    public void SetOnlineStatus_ToTrue_ShouldSetIsOnlineAndLastLogin()
    {
        var user = new User();

        var before = DateTime.UtcNow;
        user.SetOnlineStatus(true);
        var after = DateTime.UtcNow;

        Assert.True(user.IsOnline);
        Assert.NotNull(user.LastLoginAt);
        Assert.InRange(user.LastLoginAt!.Value, before, after);
        Assert.InRange(user.UpdatedAt, before, after);
    }

    [Fact]
    public void SetOnlineStatus_ToFalse_ShouldSetIsOnlineFalse_WithoutChangingLastLogin()
    {
        var user = new User();

        // First set to true to have a LastLoginAt value
        user.SetOnlineStatus(true);
        var lastLogin = user.LastLoginAt;

        var before = DateTime.UtcNow;
        user.SetOnlineStatus(false);
        var after = DateTime.UtcNow;

        Assert.False(user.IsOnline);
        Assert.Equal(lastLogin, user.LastLoginAt);
        Assert.InRange(user.UpdatedAt, before, after);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void IsEmailValid_VariousInputs_ReturnsExpected(string email, bool expected)
    {
        var user = new User { Email = email };
        var result = user.IsEmailValid();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void UpdateLocation_WithValidLocation_ShouldUpdateLastLocationAndUpdatedAt()
    {
        var user = new User();
        var location = new Location(); // assume default ctor exists

        var before = DateTime.UtcNow;
        user.UpdateLocation(location);
        var after = DateTime.UtcNow;

        Assert.Same(location, user.LastLocation);
        Assert.InRange(user.UpdatedAt, before, after);
    }

    [Fact]
    public void UpdateLocation_NullLocation_ShouldThrowArgumentNullException()
    {
        var user = new User();
        Assert.Throws<ArgumentNullException>(() => user.UpdateLocation(null!));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalseAndIsOnlineFalse()
    {
        var user = new User();
        user.SetOnlineStatus(true); // ensure IsOnline is true before deactivating

        var before = DateTime.UtcNow;
        user.Deactivate();
        var after = DateTime.UtcNow;

        Assert.False(user.IsActive);
        Assert.False(user.IsOnline);
        Assert.InRange(user.UpdatedAt, before, after);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = new User();
        user.Deactivate();

        var before = DateTime.UtcNow;
        user.Activate();
        var after = DateTime.UtcNow;

        Assert.True(user.IsActive);
        Assert.InRange(user.UpdatedAt, before, after);
    }
}
