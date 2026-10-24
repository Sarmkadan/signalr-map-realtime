#nullable enable
using System;
using Xunit;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Tests;

public class VehicleTests
{
    [Fact]
    public void DefaultValues_ShouldBeInitialized()
    {
        var vehicle = new Vehicle();

        Assert.Equal(0, vehicle.Id);
        Assert.Equal(string.Empty, vehicle.Name);
        Assert.Equal(string.Empty, vehicle.RegistrationNumber);
        Assert.Equal(VehicleStatus.Idle, vehicle.Status);
        Assert.Equal(AssetType.DeliveryVan, vehicle.AssetType);
        Assert.Null(vehicle.DriverId);
        Assert.Null(vehicle.Driver);
        Assert.Null(vehicle.Manufacturer);
        Assert.Null(vehicle.ModelYear);
        Assert.Null(vehicle.VIN);
        Assert.Null(vehicle.LastLocation);
        Assert.False(vehicle.IsOnline);
        Assert.Empty(vehicle.Locations);
        Assert.Empty(vehicle.TrackingSessions);
        Assert.Empty(vehicle.Routes);
        Assert.Empty(vehicle.RowVersion);
    }

    [Fact]
    public void UpdateStatus_ShouldSetStatusAndRefreshUpdatedAt()
    {
        var vehicle = new Vehicle();

        var before = DateTime.UtcNow;
        vehicle.UpdateStatus(VehicleStatus.InTransit);
        var after = DateTime.UtcNow;

        Assert.Equal(VehicleStatus.InTransit, vehicle.Status);
        Assert.InRange(vehicle.UpdatedAt, before, after);
    }

    [Fact]
    public void RecordLocation_ValidLocation_ShouldBecomeLastLocationAndLinkBackToVehicle()
    {
        var vehicle = new Vehicle();
        var first = new Location();
        var second = new Location();

        vehicle.RecordLocation(first);
        vehicle.RecordLocation(second);

        Assert.Same(second, vehicle.LastLocation);
        Assert.Equal(2, vehicle.Locations.Count);
        Assert.Contains(first, vehicle.Locations);
        Assert.Contains(second, vehicle.Locations);
        Assert.Same(vehicle, first.Vehicle);
        Assert.Same(vehicle, second.Vehicle);
    }

    [Fact]
    public void RecordLocation_NullLocation_ShouldThrowAndLeaveStateUnchanged()
    {
        var vehicle = new Vehicle();

        Assert.Throws<ArgumentNullException>(() => vehicle.RecordLocation(null!));

        Assert.Null(vehicle.LastLocation);
        Assert.Empty(vehicle.Locations);
        Assert.Equal(default, vehicle.UpdatedAt);
    }

    [Theory]
    [InlineData(false, 120.0, 100.0, false)] // no last location
    [InlineData(true, null, 100.0, false)]   // last location has no speed
    [InlineData(true, 120.0, null, false)]   // no speed limit configured
    [InlineData(true, 100.0, 100.0, false)]  // exactly at the limit is not exceeded
    [InlineData(true, 100.1, 100.0, true)]   // just above the limit
    [InlineData(true, 80.0, 100.0, false)]   // below the limit
    public void HasExceededSpeedLimit_VariousInputs_ReturnsExpected(
        bool hasLocation, double? speed, double? maxSpeed, bool expected)
    {
        var vehicle = new Vehicle { MaxSpeed = maxSpeed };
        if (hasLocation)
            vehicle.LastLocation = new Location { Speed = speed };

        Assert.Equal(expected, vehicle.HasExceededSpeedLimit());
    }

    [Theory]
    [InlineData(VehicleStatus.Idle, true)]
    [InlineData(VehicleStatus.AtDepot, true)]
    [InlineData(VehicleStatus.InTransit, false)]
    [InlineData(VehicleStatus.Stopped, false)]
    [InlineData(VehicleStatus.Maintenance, false)]
    [InlineData(VehicleStatus.Deactivated, false)]
    [InlineData(VehicleStatus.Available, false)]
    [InlineData(VehicleStatus.InUse, false)]
    public void IsIdle_VariousStatuses_OnlyIdleAndAtDepotAreIdle(VehicleStatus status, bool expected)
    {
        var vehicle = new Vehicle { Status = status };

        Assert.Equal(expected, vehicle.IsIdle());
    }

    [Fact]
    public void SetOnlineStatus_ShouldToggleIsOnlineAndRefreshUpdatedAt()
    {
        var vehicle = new Vehicle();

        var before = DateTime.UtcNow;
        vehicle.SetOnlineStatus(true);
        Assert.True(vehicle.IsOnline);

        vehicle.SetOnlineStatus(false);
        var after = DateTime.UtcNow;

        Assert.False(vehicle.IsOnline);
        Assert.InRange(vehicle.UpdatedAt, before, after);
    }

    [Fact]
    public void Driver_AndDriverId_AreStoredIndependently()
    {
        var vehicle = new Vehicle { DriverId = 5 };
        var driver = new User { Id = 7 };

        vehicle.Driver = driver;

        Assert.Same(driver, vehicle.Driver);
        Assert.Equal(5, vehicle.DriverId);
    }
}
