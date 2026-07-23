using Xunit;
using FluentAssertions;
using NSubstitute;
using SignalRMapRealtime.Hubs;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Tests
{
    public class LocationHubTests
    {
        [Fact]
        public void Constructor_AllParametersProvided_InitializesSuccessfully()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            // Act
            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            // Assert
            hub.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_NullLocationService_ThrowsArgumentNullException()
        {
            // Arrange
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new LocationHub(null, vehicleService, trackingService, routeRepository, throttler, logger));
        }

        [Fact]
        public void Constructor_NullVehicleService_ThrowsArgumentNullException()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new LocationHub(locationService, null, trackingService, routeRepository, throttler, logger));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, null));
        }

        [Fact]
        public async Task SendLocationUpdate_WithValidLocationDto_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);
            var locationDto = new CreateLocationDto
            {
                Latitude = 51.5074,
                Longitude = -0.1278,
                VehicleId = 42
            };

            // Act
            Func<Task> act = async () => await hub.SendLocationUpdate(locationDto);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendLocationUpdate_WithNullLocationDto_ThrowsArgumentNullException()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => hub.SendLocationUpdate(null));
        }

        [Fact]
        public async Task NotifyAssetRemoved_WithValidVehicleId_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            // Act
            Func<Task> act = async () => await hub.NotifyAssetRemoved(42);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RequestAllVehicleLocations_WithValidSetup_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            // Act
            Func<Task> act = async () => await hub.RequestAllVehicleLocations();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BroadcastVehicleStatusChange_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            // Act
            Func<Task> act = async () => await hub.BroadcastVehicleStatusChange(42, "Online");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SubscribeToVehicle_WithValidVehicleId_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = Substitute.For<global::SignalRMapRealtime.Data.Repositories.RouteRepository>();
            var throttler = Substitute.For<LocationUpdateThrottler>();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            // Act
            Func<Task> act = async () => await hub.SubscribeToVehicle(42);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}
