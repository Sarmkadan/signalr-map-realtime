#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for DomainEvent classes, particularly LocationUpdatedEvent
// =====================================================================

namespace signalr_map_realtime.Tests;

using FluentAssertions;
using SignalRMapRealtime.Events;
using Xunit;

/// <summary>
/// Contains unit tests for DomainEvent classes.
/// </summary>
public class DomainEventTests
{
    /// <summary>
    /// Tests that LocationUpdatedEvent initializes with default values correctly.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_DefaultConstructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent();

        // Assert
        locationEvent.EventId.Should().NotBe(Guid.Empty);
        locationEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        locationEvent.TriggeredBy.Should().BeNull();
        locationEvent.CorrelationId.Should().BeNull();
        locationEvent.VehicleId.Should().Be(Guid.Empty);
        locationEvent.Latitude.Should().Be(0);
        locationEvent.Longitude.Should().Be(0);
        locationEvent.Accuracy.Should().Be(0);
        locationEvent.PreviousLatitude.Should().BeNull();
        locationEvent.PreviousLongitude.Should().BeNull();
        locationEvent.Speed.Should().BeNull();
        locationEvent.Heading.Should().BeNull();
    }

    /// <summary>
    /// Tests that LocationUpdatedEvent initializes EventId with a new Guid.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_EventId_GeneratesNewGuid()
    {
        // Arrange & Act
        var event1 = new LocationUpdatedEvent();
        var event2 = new LocationUpdatedEvent();

        // Assert
        event1.EventId.Should().NotBe(Guid.Empty);
        event2.EventId.Should().NotBe(Guid.Empty);
        event1.EventId.Should().NotBe(event2.EventId);
    }

    /// <summary>
    /// Tests that LocationUpdatedEvent initializes OccurredAt with current UTC time.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_OccurredAt_DefaultsToCurrentUtcTime()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent();

        // Assert
        locationEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Tests setting all properties on LocationUpdatedEvent.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_AllProperties_SetCorrectly()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow.AddMinutes(-5);
        var eventId = Guid.NewGuid();

        // Act
        var locationEvent = new LocationUpdatedEvent
        {
            EventId = eventId,
            OccurredAt = occurredAt,
            TriggeredBy = "test-user",
            CorrelationId = "corr-123",
            VehicleId = vehicleId,
            Latitude = 40.7128,
            Longitude = -74.0060,
            Accuracy = 5.5,
            PreviousLatitude = 40.7127,
            PreviousLongitude = -74.0059,
            Speed = 65.2,
            Heading = 90.0
        };

        // Assert
        locationEvent.EventId.Should().Be(eventId);
        locationEvent.OccurredAt.Should().Be(occurredAt);
        locationEvent.TriggeredBy.Should().Be("test-user");
        locationEvent.CorrelationId.Should().Be("corr-123");
        locationEvent.VehicleId.Should().Be(vehicleId);
        locationEvent.Latitude.Should().Be(40.7128);
        locationEvent.Longitude.Should().Be(-74.0060);
        locationEvent.Accuracy.Should().Be(5.5);
        locationEvent.PreviousLatitude.Should().Be(40.7127);
        locationEvent.PreviousLongitude.Should().Be(-74.0059);
        locationEvent.Speed.Should().Be(65.2);
        locationEvent.Heading.Should().Be(90.0);
    }

    /// <summary>
    /// Tests LocationUpdatedEvent with null TriggeredBy.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_NullTriggeredBy_StoresNull()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent { TriggeredBy = null };

        // Assert
        locationEvent.TriggeredBy.Should().BeNull();
    }

    /// <summary>
    /// Tests LocationUpdatedEvent with null CorrelationId.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_NullCorrelationId_StoresNull()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent { CorrelationId = null };

        // Assert
        locationEvent.CorrelationId.Should().BeNull();
    }

    /// <summary>
    /// Tests LocationUpdatedEvent with null PreviousLatitude and PreviousLongitude.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_NullPreviousCoordinates_StoresNull()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent
        {
            PreviousLatitude = null,
            PreviousLongitude = null
        };

        // Assert
        locationEvent.PreviousLatitude.Should().BeNull();
        locationEvent.PreviousLongitude.Should().BeNull();
    }

    /// <summary>
    /// Tests LocationUpdatedEvent with null Speed and Heading.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_NullOptionalFields_StoresNull()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent
        {
            Speed = null,
            Heading = null
        };

        // Assert
        locationEvent.Speed.Should().BeNull();
        locationEvent.Heading.Should().BeNull();
    }

    /// <summary>
    /// Tests LocationUpdatedEvent EventName property returns correct type name.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_EventName_ReturnsCorrectTypeName()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent();

        // Assert
        locationEvent.EventName.Should().Be("LocationUpdatedEvent");
    }

    /// <summary>
    /// Tests LocationUpdatedEvent with boundary values for coordinates.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_BoundaryCoordinates_StoresCorrectly()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent
        {
            Latitude = -90.0,  // Minimum valid latitude
            Longitude = -180.0, // Minimum valid longitude
            Accuracy = 0.0
        };

        // Assert
        locationEvent.Latitude.Should().Be(-90.0);
        locationEvent.Longitude.Should().Be(-180.0);
        locationEvent.Accuracy.Should().Be(0.0);
    }

    /// <summary>
    /// Tests LocationUpdatedEvent with maximum valid coordinates.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_MaximumCoordinates_StoresCorrectly()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent
        {
            Latitude = 90.0,   // Maximum valid latitude
            Longitude = 180.0,  // Maximum valid longitude
            Accuracy = 1000.0
        };

        // Assert
        locationEvent.Latitude.Should().Be(90.0);
        locationEvent.Longitude.Should().Be(180.0);
        locationEvent.Accuracy.Should().Be(1000.0);
    }

    /// <summary>
    /// Tests that LocationUpdatedEvent inherits from DomainEvent.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_InheritsFromDomainEvent()
    {
        // Arrange & Act
        var locationEvent = new LocationUpdatedEvent();

        // Assert
        locationEvent.Should().BeAssignableTo<DomainEvent>();
    }

    /// <summary>
    /// Tests that DomainEvent base properties are accessible on LocationUpdatedEvent.
    /// </summary>
    [Fact]
    public void LocationUpdatedEvent_BaseDomainEventProperties_AreAccessible()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var triggeredBy = "system";
        var correlationId = "test-correlation";

        // Act
        var locationEvent = new LocationUpdatedEvent
        {
            EventId = eventId,
            OccurredAt = occurredAt,
            TriggeredBy = triggeredBy,
            CorrelationId = correlationId,
            VehicleId = Guid.NewGuid(),
            Latitude = 0,
            Longitude = 0,
            Accuracy = 0
        };

        // Assert - base class properties
        locationEvent.EventId.Should().Be(eventId);
        locationEvent.OccurredAt.Should().Be(occurredAt);
        locationEvent.TriggeredBy.Should().Be(triggeredBy);
        locationEvent.CorrelationId.Should().Be(correlationId);
        locationEvent.EventName.Should().Be("LocationUpdatedEvent");
    }
}
