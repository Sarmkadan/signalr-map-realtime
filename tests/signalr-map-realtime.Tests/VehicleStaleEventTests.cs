using System;
using SignalRMapRealtime.Events;
using Xunit;

namespace signalr_map_realtime.Tests
{
    public class VehicleStaleEventTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var evt = new VehicleStaleEvent();

            // Assert
            Assert.Equal(0, evt.VehicleId);
            Assert.Equal(string.Empty, evt.VehicleRegistration);
            Assert.Equal(string.Empty, evt.VehicleName);
            Assert.Equal(default(DateTime), evt.LastUpdateTime);
            Assert.Equal(default(DateTime), evt.StaleSince);
            Assert.Equal(0, evt.StaleWindowMinutes);
            Assert.Equal(0.0, evt.TimeSinceLastUpdateMinutes);
            Assert.False(evt.IsRecovery);
            Assert.False(evt.WasPreviouslyStale);
        }

        [Fact]
        public void PropertySetAndGet_ShouldReturnSetValues()
        {
            // Arrange
            var evt = new VehicleStaleEvent
            {
                VehicleId = 42,
                VehicleRegistration = "ABC-123",
                VehicleName = "Test Vehicle",
                LastUpdateTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                StaleSince = new DateTime(2023, 1, 1, 12, 5, 0, DateTimeKind.Utc),
                StaleWindowMinutes = 10,
                TimeSinceLastUpdateMinutes = 5.5,
                IsRecovery = true,
                WasPreviouslyStale = false
            };

            // Act & Assert
            Assert.Equal(42, evt.VehicleId);
            Assert.Equal("ABC-123", evt.VehicleRegistration);
            Assert.Equal("Test Vehicle", evt.VehicleName);
            Assert.Equal(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), evt.LastUpdateTime);
            Assert.Equal(new DateTime(2023, 1, 1, 12, 5, 0, DateTimeKind.Utc), evt.StaleSince);
            Assert.Equal(10, evt.StaleWindowMinutes);
            Assert.Equal(5.5, evt.TimeSinceLastUpdateMinutes);
            Assert.True(evt.IsRecovery);
            Assert.False(evt.WasPreviouslyStale);
        }

        [Fact]
        public void StringProperties_ShouldAcceptNull()
        {
            // Arrange
            var evt = new VehicleStaleEvent
            {
                VehicleRegistration = null,
                VehicleName = null
            };

            // Act & Assert
            Assert.Null(evt.VehicleRegistration);
            Assert.Null(evt.VehicleName);
        }

        [Fact]
        public void BoundaryValues_ShouldBeAccepted()
        {
            // Arrange
            var evt = new VehicleStaleEvent
            {
                VehicleId = int.MinValue,
                StaleWindowMinutes = int.MaxValue,
                TimeSinceLastUpdateMinutes = double.MaxValue,
                LastUpdateTime = DateTime.MinValue,
                StaleSince = DateTime.MaxValue,
                IsRecovery = true,
                WasPreviouslyStale = true
            };

            // Act & Assert
            Assert.Equal(int.MinValue, evt.VehicleId);
            Assert.Equal(int.MaxValue, evt.StaleWindowMinutes);
            Assert.Equal(double.MaxValue, evt.TimeSinceLastUpdateMinutes);
            Assert.Equal(DateTime.MinValue, evt.LastUpdateTime);
            Assert.Equal(DateTime.MaxValue, evt.StaleSince);
            Assert.True(evt.IsRecovery);
            Assert.True(evt.WasPreviouslyStale);
        }

        [Fact]
        public void Inheritance_ShouldBeDomainEvent()
        {
            // Arrange
            var evt = new VehicleStaleEvent();

            // Act & Assert
            Assert.IsAssignableFrom<DomainEvent>(evt);
            Assert.IsType<VehicleStaleEvent>(evt);
        }
    }
}
