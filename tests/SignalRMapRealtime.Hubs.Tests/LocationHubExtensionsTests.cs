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
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToVehicleGroupAsync(LocationHub, int, string)"/>
        /// throws ArgumentNullException when hub is null.
        /// </summary>
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_NullHub_ThrowsArgumentNullException()
        {
            // Arrange
            LocationHub? nullHub = null;
            var vehicleId = 1;
            var methodName = "TestMethod";

            // Act
            var act = () => nullHub!.BroadcastToVehicleGroupAsync(vehicleId, methodName);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToVehicleGroupAsync(LocationHub, int, string)"/>
        /// throws ArgumentException when methodName is null.
        /// </summary>
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_NullMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var vehicleId = 1;
            string? nullMethodName = null;

            // Act
            var act = () => hub.BroadcastToVehicleGroupAsync(vehicleId, nullMethodName!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToVehicleGroupAsync(LocationHub, int, string)"/>
        /// throws ArgumentException when methodName is empty.
        /// </summary>
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_EmptyMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var vehicleId = 1;
            var emptyMethodName = string.Empty;

            // Act
            var act = () => hub.BroadcastToVehicleGroupAsync(vehicleId, emptyMethodName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToVehicleGroupAsync(LocationHub, int, string)"/>
        /// produces the correct group name format for vehicle IDs.
        /// </summary>
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_ValidInput_CreatesCorrectGroupName()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var vehicleId = 42;
            var methodName = "TestMethod";

            // Act
            await hub.BroadcastToVehicleGroupAsync(vehicleId, methodName);

            // Assert
            await hub.Clients.Received(1).Group($"vehicle-{vehicleId}").SendAsync(methodName, Arg.Any<object[]>());
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToVehicleGroupAsync(LocationHub, int, string)"/>
        /// handles zero as a valid vehicle ID.
        /// </summary>
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_ZeroVehicleId_CreatesCorrectGroupName()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var vehicleId = 0;
            var methodName = "TestMethod";

            // Act
            await hub.BroadcastToVehicleGroupAsync(vehicleId, methodName);

            // Assert
            await hub.Clients.Received(1).Group("vehicle-0").SendAsync(methodName, Arg.Any<object[]>());
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToVehicleGroupAsync(LocationHub, int, string, object?[])"/>
        /// passes through arguments correctly.
        /// </summary>
        [Fact]
        public async Task BroadcastToVehicleGroupAsync_WithArguments_PassesArgumentsCorrectly()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var vehicleId = 1;
            var methodName = "UpdatePosition";
            var args = new object?[] { 52.5200, 13.4050, DateTime.UtcNow };

            // Act
            await hub.BroadcastToVehicleGroupAsync(vehicleId, methodName, args);

            // Assert
            await hub.Clients.Received(1).Group($"vehicle-{vehicleId}").SendAsync(methodName, args);
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

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string)"/>
        /// throws ArgumentNullException when hub is null.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_NullHub_ThrowsArgumentNullException()
        {
            // Arrange
            LocationHub? nullHub = null;
            var fleetName = "TestFleet";
            var methodName = "TestMethod";

            // Act
            var act = () => nullHub!.BroadcastToFleetAsync(fleetName, methodName);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string)"/>
        /// throws ArgumentException when fleetName is null.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_NullFleetName_ThrowsArgumentException()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            string? nullFleetName = null;
            var methodName = "TestMethod";

            // Act
            var act = () => hub.BroadcastToFleetAsync(nullFleetName!, methodName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string)"/>
        /// throws ArgumentException when fleetName is empty.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_EmptyFleetName_ThrowsArgumentException()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var emptyFleetName = string.Empty;
            var methodName = "TestMethod";

            // Act
            var act = () => hub.BroadcastToFleetAsync(emptyFleetName, methodName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string)"/>
        /// throws ArgumentException when methodName is null.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_NullMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var fleetName = "TestFleet";
            string? nullMethodName = null;

            // Act
            var act = () => hub.BroadcastToFleetAsync(fleetName, nullMethodName!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string)"/>
        /// throws ArgumentException when methodName is empty.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_EmptyMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var fleetName = "TestFleet";
            var emptyMethodName = string.Empty;

            // Act
            var act = () => hub.BroadcastToFleetAsync(fleetName, emptyMethodName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string)"/>
        /// produces the correct group name format for fleet names.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_ValidInput_CreatesCorrectGroupName()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var fleetName = "TestFleet";
            var methodName = "TestMethod";

            // Act
            await hub.BroadcastToFleetAsync(fleetName, methodName);

            // Assert
            await hub.Clients.Received(1).Group($"fleet-{fleetName}").SendAsync(methodName, Arg.Any<object[]>());
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.BroadcastToFleetAsync(LocationHub, string, string, object?[])"/>
        /// passes through arguments correctly.
        /// </summary>
        [Fact]
        public async Task BroadcastToFleetAsync_WithArguments_PassesArgumentsCorrectly()
        {
            // Arrange
            var hub = Substitute.For<LocationHub>();
            var fleetName = "TestFleet";
            var methodName = "UpdateFleet";
            var args = new object?[] { true, DateTime.UtcNow };

            // Act
            await hub.BroadcastToFleetAsync(fleetName, methodName, args);

            // Assert
            await hub.Clients.Received(1).Group($"fleet-{fleetName}").SendAsync(methodName, args);
        }

        /// <summary>
        /// Verifies that <see cref="LocationHubExtensions.GetConnectionId(LocationHub)"/>
        /// throws ArgumentNullException when hub is null.
        /// </summary>
        [Fact]
        public void GetConnectionId_NullHub_ThrowsArgumentNullException()
        {
            // Arrange
            LocationHub? nullHub = null;

            // Act
            var act = () => nullHub!.GetConnectionId();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
