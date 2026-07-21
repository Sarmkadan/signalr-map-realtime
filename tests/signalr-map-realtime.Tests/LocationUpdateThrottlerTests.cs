using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Services;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class LocationUpdateThrottlerTests
{
    private readonly IOptions<ThrottleOptions> _options;
    private readonly ILogger<LocationUpdateThrottler> _logger;
    private readonly LocationUpdateThrottler _throttler;

    public LocationUpdateThrottlerTests()
    {
        _options = Substitute.For<IOptions<ThrottleOptions>>();
        _logger = Substitute.For<ILogger<LocationUpdateThrottler>>();

        var throttleOptions = new ThrottleOptions
        {
            Enabled = true,
            DeliveryVanIntervalSeconds = 1,
            CourierIntervalSeconds = 5,
            BicycleIntervalSeconds = 10,
            MotorcycleIntervalSeconds = 3,
            PortableIntervalSeconds = 15,
            FixedAssetIntervalSeconds = 60,
            DroneIntervalSeconds = 2
        };

        _options.Value.Returns(throttleOptions);
        _throttler = new LocationUpdateThrottler(_options, _logger);
    }

    [Fact]
    public void ShouldThrottle_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var disabledOptions = new ThrottleOptions { Enabled = false };
        var disabledOptionsMock = Substitute.For<IOptions<ThrottleOptions>>();
        disabledOptionsMock.Value.Returns(disabledOptions);
        var disabledThrottler = new LocationUpdateThrottler(disabledOptionsMock, _logger);

        // Act
        var result = disabledThrottler.ShouldThrottle(1, AssetType.DeliveryVan);

        // Assert
        result.Should().BeFalse();
        _logger.Received(1).LogDebug("Throttling disabled, accepting location update for vehicle {VehicleId}", 1);
    }

    [Fact]
    public void ShouldThrottle_FirstUpdate_ReturnsFalse()
    {
        // Act
        var result = _throttler.ShouldThrottle(1, AssetType.DeliveryVan);

        // Assert
        result.Should().BeFalse("First update should always be accepted");
    }

    [Fact]
    public void ShouldThrottle_RapidSubsequentUpdateWithinWindow_ReturnsTrue()
    {
        // Arrange - first update
        var firstResult = _throttler.ShouldThrottle(1, AssetType.DeliveryVan);
        firstResult.Should().BeFalse();

        // Act - second update within 1 second window
        var secondResult = _throttler.ShouldThrottle(1, AssetType.DeliveryVan);

        // Assert
        secondResult.Should().BeTrue("Update within throttle window should be throttled");
        _logger.Received(1).LogDebug(
            "Location update throttled for vehicle {VehicleId} (asset type: {AssetType}). Last update was {LastUpdateSeconds} seconds ago, minimum interval is {IntervalSeconds} seconds",
            1, AssetType.DeliveryVan, Arg.Any<double>(), 1.0);
    }

    [Fact]
    public void ShouldThrottle_UpdateAfterWindowPasses_ReturnsFalse()
    {
        // Arrange - first update
        var firstResult = _throttler.ShouldThrottle(1, AssetType.DeliveryVan);
        firstResult.Should().BeFalse();

        // Act - wait for window to pass (1 second for DeliveryVan)
        Thread.Sleep(1100);

        // Assert - second update after window should be accepted
        var secondResult = _throttler.ShouldThrottle(1, AssetType.DeliveryVan);
        secondResult.Should().BeFalse("Update after throttle window should be accepted");
    }

    [Fact]
    public void ShouldThrottle_PerAssetIsolation_AssetADoesNotAffectAssetB()
    {
        // Arrange - first asset update
        var assetAFirst = _throttler.ShouldThrottle(100, AssetType.DeliveryVan);
        assetAFirst.Should().BeFalse();

        // Act - second asset update (should not be affected by first asset's throttle state)
        var assetBFirst = _throttler.ShouldThrottle(200, AssetType.DeliveryVan);
        assetBFirst.Should().BeFalse();

        // Assert - both should be accepted independently
        assetAFirst.Should().BeFalse();
        assetBFirst.Should().BeFalse();

        // Act - rapid update for asset A should throttle only asset A
        var assetARapid = _throttler.ShouldThrottle(100, AssetType.DeliveryVan);
        assetARapid.Should().BeTrue("Asset A should be throttled");

        // Act - asset B should still be accepted (different vehicle ID)
        var assetBSecond = _throttler.ShouldThrottle(200, AssetType.DeliveryVan);
        assetBSecond.Should().BeFalse("Asset B should not be throttled by Asset A's state");
    }

    [Fact]
    public void ShouldThrottle_DifferentAssetTypes_HaveDifferentThrottleWindows()
    {
        // Arrange & Act & Assert for each asset type
        var deliveryVanResult = _throttler.ShouldThrottle(1, AssetType.DeliveryVan);
        deliveryVanResult.Should().BeFalse();

        var courierResult = _throttler.ShouldThrottle(2, AssetType.Courier);
        courierResult.Should().BeFalse();

        var bicycleResult = _throttler.ShouldThrottle(3, AssetType.Bicycle);
        bicycleResult.Should().BeFalse();

        var motorcycleResult = _throttler.ShouldThrottle(4, AssetType.Motorcycle);
        motorcycleResult.Should().BeFalse();

        var portableResult = _throttler.ShouldThrottle(5, AssetType.Portable);
        portableResult.Should().BeFalse();

        var fixedAssetResult = _throttler.ShouldThrottle(6, AssetType.FixedAsset);
        fixedAssetResult.Should().BeFalse();

        var droneResult = _throttler.ShouldThrottle(7, AssetType.Drone);
        droneResult.Should().BeFalse();
    }

    [Fact]
    public void ShouldThrottle_RespectsEachAssetTypesInterval()
    {
        // DeliveryVan - 1 second interval
        var deliveryVanFirst = _throttler.ShouldThrottle(10, AssetType.DeliveryVan);
        deliveryVanFirst.Should().BeFalse();

        var deliveryVanSecond = _throttler.ShouldThrottle(10, AssetType.DeliveryVan);
        deliveryVanSecond.Should().BeTrue("DeliveryVan should throttle after 1 second");

        // Courier - 5 second interval
        var courierFirst = _throttler.ShouldThrottle(20, AssetType.Courier);
        courierFirst.Should().BeFalse();

        var courierSecond = _throttler.ShouldThrottle(20, AssetType.Courier);
        courierSecond.Should().BeFalse("Courier rapid update should be accepted (within 5 second window)");

        // Wait for courier window to pass
        Thread.Sleep(5100);

        var courierThird = _throttler.ShouldThrottle(20, AssetType.Courier);
        courierThird.Should().BeFalse("Courier update after 5 seconds should be accepted");

        // Portable - 15 second interval
        var portableFirst = _throttler.ShouldThrottle(30, AssetType.Portable);
        portableFirst.Should().BeFalse();

        var portableSecond = _throttler.ShouldThrottle(30, AssetType.Portable);
        portableSecond.Should().BeFalse("Portable rapid update should be accepted (within 15 second window)");
    }

    [Fact]
    public void Remove_RemovesVehicleFromThrottleDictionary()
    {
        // Arrange - add vehicle to throttle dictionary
        var firstResult = _throttler.ShouldThrottle(50, AssetType.DeliveryVan);
        firstResult.Should().BeFalse();

        // Act - remove vehicle
        _throttler.Remove(50);

        // Assert - subsequent update should be accepted (no state to throttle)
        var secondResult = _throttler.ShouldThrottle(50, AssetType.DeliveryVan);
        secondResult.Should().BeFalse("After removal, vehicle should be accepted again");

        _logger.Received(1).LogDebug("Removing throttle entry for vehicle {VehicleId}", 50);
    }

    [Fact]
    public void Remove_NonExistentVehicle_DoesNotThrow()
    {
        // Act - should not throw when removing non-existent vehicle
        Action act = () => _throttler.Remove(999);

        act.Should().NotThrow<KeyNotFoundException>();
    }

    [Fact]
    public void ShouldThrottle_AllAssetTypes_UseCorrectIntervals()
    {
        // Test each asset type with its configured interval
        var intervals = new Dictionary<AssetType, int>
        {
            { AssetType.DeliveryVan, 1 },
            { AssetType.Courier, 5 },
            { AssetType.Bicycle, 10 },
            { AssetType.Motorcycle, 3 },
            { AssetType.Portable, 15 },
            { AssetType.FixedAsset, 60 },
            { AssetType.Drone, 2 }
        };

        foreach (var (assetType, expectedSeconds) in intervals)
        {
            // First update should always pass
            var firstResult = _throttler.ShouldThrottle(1000 + (int)assetType, assetType);
            firstResult.Should().BeFalse($"First update for {assetType} should be accepted");

            // Second update within window should be throttled
            var secondResult = _throttler.ShouldThrottle(1000 + (int)assetType, assetType);
            secondResult.Should().BeTrue($"Second update within {expectedSeconds} seconds should be throttled for {assetType}");

            // Wait for window to pass
            Thread.Sleep((expectedSeconds + 1) * 1000);

            // Third update after window should pass
            var thirdResult = _throttler.ShouldThrottle(1000 + (int)assetType, assetType);
            thirdResult.Should().BeFalse($"Update after {expectedSeconds} seconds should be accepted for {assetType}");
        }
    }

    [Fact]
    public void ShouldThrottle_MultipleVehicles_EachMaintainsOwnState()
    {
        // Arrange - multiple vehicles of same type
        var vehicleIds = new[] { 100, 200, 300, 400, 500 };

        // Act - all first updates should pass
        foreach (var vehicleId in vehicleIds)
        {
            var result = _throttler.ShouldThrottle(vehicleId, AssetType.DeliveryVan);
            result.Should().BeFalse($"First update for vehicle {vehicleId} should be accepted");
        }

        // Act - rapid updates should throttle each vehicle independently
        foreach (var vehicleId in vehicleIds)
        {
            var result = _throttler.ShouldThrottle(vehicleId, AssetType.DeliveryVan);
            result.Should().BeTrue($"Rapid update for vehicle {vehicleId} should be throttled");
        }

        // Act - after window passes, all should be accepted again
        Thread.Sleep(1100);

        foreach (var vehicleId in vehicleIds)
        {
            var result = _throttler.ShouldThrottle(vehicleId, AssetType.DeliveryVan);
            result.Should().BeFalse($"Update after window for vehicle {vehicleId} should be accepted");
        }
    }
    }
