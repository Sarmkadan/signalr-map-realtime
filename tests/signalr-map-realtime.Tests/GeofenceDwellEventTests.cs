#nullable enable
using System;
using SignalRMapRealtime.Events;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class GeofenceDwellEventTests
{
    [Fact]
    public void Should_Create_Event_With_Default_Values()
    {
        // Arrange & Act
        var ev = new GeofenceDwellEvent();

        // Assert
        Assert.Equal(Guid.Empty, ev.GeofenceId);
        Assert.Equal(string.Empty, ev.GeofenceName);
        Assert.Equal(Guid.Empty, ev.VehicleId);
        Assert.Equal(string.Empty, ev.VehicleRegistration);
        Assert.Equal(string.Empty, ev.VehicleName);
        Assert.Equal(default(DateTime), ev.EnteredAt);
        Assert.Equal(0d, ev.DwellDurationMinutes);
        Assert.Equal(0, ev.MaxDwellMinutes);
        Assert.Equal(0d, ev.Latitude);
        Assert.Equal(0d, ev.Longitude);
    }

    [Fact]
    public void Should_Set_And_Get_All_Properties_Correctly()
    {
        // Arrange
        var guidGeofence = Guid.NewGuid();
        var guidVehicle = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var ev = new GeofenceDwellEvent
        {
            GeofenceId = guidGeofence,
            GeofenceName = "Test Geofence",
            VehicleId = guidVehicle,
            VehicleRegistration = "ABC-123",
            VehicleName = "Test Vehicle",
            EnteredAt = now,
            DwellDurationMinutes = 12.5,
            MaxDwellMinutes = 30,
            Latitude = 45.12345,
            Longitude = -93.54321
        };

        // Assert
        Assert.Equal(guidGeofence, ev.GeofenceId);
        Assert.Equal("Test Geofence", ev.GeofenceName);
        Assert.Equal(guidVehicle, ev.VehicleId);
        Assert.Equal("ABC-123", ev.VehicleRegistration);
        Assert.Equal("Test Vehicle", ev.VehicleName);
        Assert.Equal(now, ev.EnteredAt);
        Assert.Equal(12.5, ev.DwellDurationMinutes);
        Assert.Equal(30, ev.MaxDwellMinutes);
        Assert.Equal(45.12345, ev.Latitude);
        Assert.Equal(-93.54321, ev.Longitude);
    }

    [Fact]
    public void Should_Handle_Edge_Values_For_Numeric_Properties()
    {
        // Arrange
        var ev = new GeofenceDwellEvent
        {
            DwellDurationMinutes = 0,
            MaxDwellMinutes = int.MaxValue,
            Latitude = -90.0,   // minimum valid latitude
            Longitude = 180.0   // maximum valid longitude
        };

        // Assert
        Assert.Equal(0, ev.DwellDurationMinutes);
        Assert.Equal(int.MaxValue, ev.MaxDwellMinutes);
        Assert.Equal(-90.0, ev.Latitude);
        Assert.Equal(180.0, ev.Longitude);
    }

    [Fact]
    public void Should_Allow_Null_Assignment_To_String_Properties_But_Not_Throw()
    {
        // Arrange
        var ev = new GeofenceDwellEvent();

        // Act & Assert
        // Assigning null to non‑nullable strings is allowed at runtime (warnings only);
        // the test ensures no exception is thrown.
        ev.GeofenceName = null!;
        ev.VehicleRegistration = null!;
        ev.VehicleName = null!;

        Assert.Null(ev.GeofenceName);
        Assert.Null(ev.VehicleRegistration);
        Assert.Null(ev.VehicleName);
    }
}
