using Xunit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
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
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            // Act
            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);
            ConfigureHub(hub);

            // Assert
            hub.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_NullLocationService_ThrowsArgumentNullException()
        {
            // Arrange
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new LocationHub(null!, vehicleService, trackingService, routeRepository, throttler, logger));
        }

        [Fact]
        public void Constructor_NullVehicleService_ThrowsArgumentNullException()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new LocationHub(locationService, null!, trackingService, routeRepository, throttler, logger));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, null!));
        }

        [Fact]
        public async Task SendLocationUpdate_WithValidLocationDto_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            ConfigureHub(hub);
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
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            ConfigureHub(hub);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => hub.SendLocationUpdate(null!));
        }

        [Fact]
        public async Task NotifyAssetRemoved_WithValidVehicleId_DoesNotThrow()
        {
            // Arrange
            var locationService = Substitute.For<ILocationService>();
            var vehicleService = Substitute.For<IVehicleService>();
            var trackingService = Substitute.For<ITrackingService>();
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            ConfigureHub(hub);

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
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            ConfigureHub(hub);

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
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            ConfigureHub(hub);

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
            var routeRepository = CreateRouteRepository();
            var throttler = CreateThrottler();
            var logger = Substitute.For<ILogger<LocationHub>>();

            var hub = new LocationHub(locationService, vehicleService, trackingService, routeRepository, throttler, logger);

            ConfigureHub(hub);

            // Act
            Func<Task> act = async () => await hub.SubscribeToVehicle(42);

            // Assert
            await act.Should().NotThrowAsync();
        }
    
        /// <summary>
        /// Creates a repository substitute. <see cref="RouteRepository"/> has no parameterless constructor, so it is
        /// proxied with a substitute context; the hub only stores the reference and never uses it in these tests.
        /// </summary>
        private static RouteRepository CreateRouteRepository()
        {
            var context = Substitute.For<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            return Substitute.For<RouteRepository>(context);
        }

        /// <summary>
        /// Creates a throttler with default options. Its constructor requires arguments, so it cannot be proxied without them.
        /// </summary>
        private static LocationUpdateThrottler CreateThrottler()
            => new LocationUpdateThrottler(
                Options.Create(new ThrottleOptions()),
                Substitute.For<ILogger<LocationUpdateThrottler>>(),
                Substitute.For<IHubContext<LocationHub>>());

        /// <summary>
        /// Supplies the SignalR runtime members (Clients, Groups, Context) that a hub uses but a unit test does not get.
        /// </summary>
        private static void ConfigureHub(LocationHub hub)
        {
            var clients = Substitute.For<IHubCallerClients>();
            clients.Caller.Returns(Substitute.For<ISingleClientProxy>());
            clients.Group(Arg.Any<string>()).Returns(Substitute.For<IClientProxy>());
            hub.Clients = clients;

            hub.Groups = Substitute.For<IGroupManager>();

            var context = Substitute.For<HubCallerContext>();
            context.ConnectionId.Returns("test-connection");
            hub.Context = context;
        }
}
}
