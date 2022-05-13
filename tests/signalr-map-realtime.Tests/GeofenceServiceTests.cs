#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Events;
using SignalRMapRealtime.Services;
using Xunit;

/// <summary>
/// Tests for the GeofenceService class.
/// </summary>
public class GeofenceServiceTests
{
    /// <summary>
    /// Creates a new instance of the GeofenceService class.
    /// </summary>
    /// <param name="eventBus">The event bus to use. Defaults to a mock event bus.</param>
    /// <returns>A new instance of the GeofenceService class.</returns>
    private static GeofenceService CreateService(IEventBus? eventBus = null)
    {
        eventBus ??= Substitute.For<IEventBus>();
        return new GeofenceService(eventBus, NullLogger<GeofenceService>.Instance);
    }

    /// <summary>
    /// Tests that registering a zone with a valid DTO returns a zone DTO with matching properties.
    /// </summary>
    [Fact]
    public async Task RegisterZone_WithValidDto_ReturnsZoneDtoWithMatchingProperties()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateGeofenceDto
        {
            Name = "Warehouse Alpha",
            Description = "Inbound goods area",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 51.5074,
            CenterLongitude = -0.1278,
            RadiusKm = 0.5,
            CreatedBy = "test-user",
        };

        // Act
        var result = await service.RegisterZoneAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Warehouse Alpha");
        result.IsActive.Should().BeTrue();
        result.CenterLatitude.Should().Be(51.5074);
        result.RadiusKm.Should().Be(0.5);
    }

    /// <summary>
    /// Tests that getting active zones after registering a zone contains the registered zone.
    /// </summary>
    [Fact]
    public async Task GetActiveZones_AfterRegisteringZone_ContainsRegisteredZone()
    {
        // Arrange
        var service = CreateService();
        await service.RegisterZoneAsync(new CreateGeofenceDto
        {
            Name = "Zone One",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 48.8566,
            CenterLongitude = 2.3522,
            RadiusKm = 1.0,
        });

        // Act
        var zones = await service.GetActiveZonesAsync();

        // Assert
        zones.Should().HaveCount(1);
        zones[0].Name.Should().Be("Zone One");
    }

    /// <summary>
    /// Tests that removing a zone with an existing zone returns true and the zone is gone.
    /// </summary>
    [Fact]
    public async Task RemoveZone_ExistingZone_ReturnsTrueAndZoneIsGone()
    {
        // Arrange
        var service = CreateService();
        var zone = await service.RegisterZoneAsync(new CreateGeofenceDto
        {
            Name = "Temp Zone",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 0,
            CenterLongitude = 0,
            RadiusKm = 1.0,
        });

        // Act
        var removed = await service.RemoveZoneAsync(zone.Id);
        var remaining = await service.GetActiveZonesAsync();

        // Assert
        removed.Should().BeTrue();
        remaining.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that removing a zone with a non-existent ID returns false.
    /// </summary>
    [Fact]
    public async Task RemoveZone_NonExistentId_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RemoveZoneAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that checking a location when a vehicle enters a circle zone emits an entered alert.
    /// </summary>
    [Fact]
    public async Task CheckLocation_WhenVehicleEntersCircleZone_EmitsEnteredAlert()
    {
        // Arrange
        var eventBus = Substitute.For<IEventBus>();
        var service = CreateService(eventBus);

        // Register a 1 km circle centred on (51.5074, -0.1278)
        await service.RegisterZoneAsync(new CreateGeofenceDto
        {
            Name = "City Centre",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 51.5074,
            CenterLongitude = -0.1278,
            RadiusKm = 1.0,
        });

        var vehicleId = Guid.NewGuid();

        // Act — vehicle arrives inside the circle
        var alerts = await service.CheckLocationAsync(vehicleId, 51.5074, -0.1278);

        // Assert
        alerts.Should().HaveCount(1);
        alerts[0].ViolationType.Should().Be("Entered");
        alerts[0].VehicleId.Should().Be(vehicleId);
        await eventBus.Received(1).PublishAsync(Arg.Any<GeofenceViolationEvent>());
    }

    /// <summary>
    /// Tests that checking a location when a vehicle exits a zone emits an exited alert.
    /// </summary>
    [Fact]
    public async Task CheckLocation_VehicleExitsZone_EmitsExitedAlert()
    {
        // Arrange
        var service = CreateService();
        await service.RegisterZoneAsync(new CreateGeofenceDto
        {
            Name = "Depot",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 51.5074,
            CenterLongitude = -0.1278,
            RadiusKm = 0.5,
        });

        var vehicleId = Guid.NewGuid();

        // First call — enter
        await service.CheckLocationAsync(vehicleId, 51.5074, -0.1278);

        // Act — vehicle moves far away, outside the radius
        var alerts = await service.CheckLocationAsync(vehicleId, 52.0, 1.0);

        // Assert
        alerts.Should().HaveCount(1);
        alerts[0].ViolationType.Should().Be("Exited");
    }

    /// <summary>
    /// Tests that checking a location when a vehicle remains inside a zone produces no duplicate alerts.
    /// </summary>
    [Fact]
    public async Task CheckLocation_VehicleRemainsInsideZone_ProducesNoDuplicateAlerts()
    {
        // Arrange
        var service = CreateService();
        await service.RegisterZoneAsync(new CreateGeofenceDto
        {
            Name = "Parking Lot",
            Type = GeofenceType.Circle,
            IsActive = true,
            CenterLatitude = 51.5074,
            CenterLongitude = -0.1278,
            RadiusKm = 1.0,
        });

        var vehicleId = Guid.NewGuid();

        // Act — vehicle enters once then stays inside
        await service.CheckLocationAsync(vehicleId, 51.5074, -0.1278);
        var secondAlerts = await service.CheckLocationAsync(vehicleId, 51.5075, -0.1279);

        // Assert — no new alert on second check while already inside
        secondAlerts.Should().BeEmpty();
    }
}
