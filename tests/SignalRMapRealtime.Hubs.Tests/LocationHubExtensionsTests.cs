using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using SignalRMapRealtime.Hubs;
using System;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Hubs.Tests
{
    public class LocationHubExtensionsTests
    {
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_ValidInput_BroadcastsToGroup()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var vehicleId = 1;
            var methodName = "TestMethod";

            // Act
            await hub.BroadcastToVehicleGroupAsync(vehicleId, methodName);

            // Assert
            await hub.Clients.Received(1).Group($"vehicle-{vehicleId}").SendAsync(methodName, Array.Empty<object>());
        }

        [Fact]
        public async Task BroadcastToFleetAsync_ValidInput_BroadcastsToFleet()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var fleetName = "TestFleet";
            var methodName = "TestMethod";

            // Act
            await hub.BroadcastToFleetAsync(fleetName, methodName);

            // Assert
            await hub.Clients.Received(1).Group($"fleet-{fleetName}").SendAsync(methodName, Array.Empty<object>());
        }

        [Fact]
        public void GetConnectionId_ValidHub_ReturnsConnectionId()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            hub.Context.ConnectionId.Returns("TestConnectionId");

            // Act
            var connectionId = hub.GetConnectionId();

            // Assert
            connectionId.Should().Be("TestConnectionId");
        }
    }
}
