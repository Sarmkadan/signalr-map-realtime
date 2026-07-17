using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using SignalRMapRealtime.Hubs;
using System;
using System.Threading.Tasks;

namespace SignalRMapRealtime.Hubs.Tests
{
    /// <summary>
    /// Tests for the extension methods defined on <see cref="LocationHub"/>.
    /// </summary>
    public class LocationHubExtensionsTests
    {
        /// <summary>
        /// Verifies that <see cref="LocationHub.BroadcastToVehicleGroupAsync(int,string)"/> sends a message to the correct vehicle group.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier used to construct the group name.</param>
        /// <param name="methodName">The name of the hub method to invoke.</param>
        /// <returns>A task that completes when the broadcast operation has been verified.</returns>
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

        /// <summary>
        /// Verifies that <see cref="LocationHub.BroadcastToFleetAsync(string,string)"/> sends a message to the correct fleet group.
        /// </summary>
        /// <param name="fleetName">The fleet name used to construct the group name.</param>
        /// <param name="methodName">The name of the hub method to invoke.</param>
        /// <returns>A task that completes when the broadcast operation has been verified.</returns>
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

        /// <summary>
        /// Verifies that <see cref="LocationHub.GetConnectionId()"/> returns the current connection identifier.
        /// </summary>
        /// <returns>The connection identifier string.</returns>
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
