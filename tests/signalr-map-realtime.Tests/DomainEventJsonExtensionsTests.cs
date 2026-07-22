#nullable enable

using FluentAssertions;
using SignalRMapRealtime.Events;
using System.Text.Json;
using Xunit;

namespace signalr_map_realtime.Tests;

public class DomainEventJsonExtensionsTests
{
    private readonly LocationUpdatedEvent _sampleLocationEvent = new()
    {
        VehicleId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
        Latitude = 40.7128,
        Longitude = -74.0060,
        Accuracy = 5.5,
        PreviousLatitude = 40.7127,
        PreviousLongitude = -74.0059,
        Speed = 45.2,
        Heading = 90.0,
        TriggeredBy = "test-user",
        CorrelationId = "test-correlation-123"
    };

    private readonly VehicleStatusChangedEvent _sampleStatusEvent = new()
    {
        VehicleId = Guid.Parse("87654321-4321-4321-4321-210987654321"),
        VehiclePlate = "ABC123",
        PreviousStatus = "Inactive",
        NewStatus = "Active",
        Reason = "Manual Update",
        TriggeredBy = "dispatcher",
        CorrelationId = "status-change-456"
    };

    private readonly TrackingSessionCompletedEvent _sampleSessionEvent = new()
    {
        SessionId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
        VehicleId = Guid.Parse("22222222-3333-4444-5555-666666666666"),
        StartedAt = DateTime.UtcNow.AddHours(-1),
        EndedAt = DateTime.UtcNow,
        TotalDistanceKm = 125.75,
        LocationCount = 42,
        AverageSpeedKmh = 65.5,
        TriggeredBy = "system"
    };

    [Fact]
    public void ToJson_WithValidDomainEvent_ReturnsNonEmptyJsonString()
    {
        // Act
        var json = DomainEventJsonExtensions.ToJson(_sampleLocationEvent, false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("locationUpdatedEvent");
        json.Should().Contain("vehicleId");
        json.Should().Contain("latitude");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Act
        var json = DomainEventJsonExtensions.ToJson(_sampleLocationEvent, indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\n"); // Should have newlines for formatting
        json.Should().Contain("  "); // Should have indentation
    }

    [Fact]
    public void ToJson_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        DomainEvent? nullEvent = null;

        // Act
        Action act = () => DomainEventJsonExtensions.ToJson(nullEvent!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedDomainEvent()
    {
        // Arrange
        var json = DomainEventJsonExtensions.ToJson(_sampleLocationEvent);

        // Act
        var result = DomainEventJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<LocationUpdatedEvent>();
        var locationEvent = result.Should().BeAssignableTo<LocationUpdatedEvent>().Subject;
        locationEvent.VehicleId.Should().Be(_sampleLocationEvent.VehicleId);
        locationEvent.Latitude.Should().Be(_sampleLocationEvent.Latitude);
        locationEvent.Longitude.Should().Be(_sampleLocationEvent.Longitude);
        locationEvent.Accuracy.Should().Be(_sampleLocationEvent.Accuracy);
        locationEvent.PreviousLatitude.Should().Be(_sampleLocationEvent.PreviousLatitude);
        locationEvent.PreviousLongitude.Should().Be(_sampleLocationEvent.PreviousLongitude);
        locationEvent.Speed.Should().Be(_sampleLocationEvent.Speed);
        locationEvent.Heading.Should().Be(_sampleLocationEvent.Heading);
        locationEvent.TriggeredBy.Should().Be(_sampleLocationEvent.TriggeredBy);
        locationEvent.CorrelationId.Should().Be(_sampleLocationEvent.CorrelationId);
    }

    [Fact]
    public void FromJson_WithDifferentEventTypes_ReturnsCorrectConcreteType()
    {
        // Arrange
        var statusJson = DomainEventJsonExtensions.ToJson(_sampleStatusEvent);
        var sessionJson = DomainEventJsonExtensions.ToJson(_sampleSessionEvent);

        // Act
        var statusResult = DomainEventJsonExtensions.FromJson(statusJson);
        var sessionResult = DomainEventJsonExtensions.FromJson(sessionJson);

        // Assert
        statusResult.Should().NotBeNull();
        statusResult.Should().BeOfType<VehicleStatusChangedEvent>();

        sessionResult.Should().NotBeNull();
        sessionResult.Should().BeOfType<TrackingSessionCompletedEvent>();
    }

    [Fact]
    public void FromJson_WithEmptyOrWhitespaceJson_ReturnsNull()
    {
        // Act
        var emptyResult = DomainEventJsonExtensions.FromJson(string.Empty);
        var whitespaceResult = DomainEventJsonExtensions.FromJson("   ");

        // Assert
        emptyResult.Should().BeNull();
        whitespaceResult.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => DomainEventJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        Action act = () => DomainEventJsonExtensions.FromJson(invalidJson);

        // Assert
        act.Should().Throw<global::System.Text.Json.JsonException>();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = DomainEventJsonExtensions.ToJson(_sampleLocationEvent);

        // Act
        var result = DomainEventJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeOfType<LocationUpdatedEvent>();
        var locationEvent = value.Should().BeAssignableTo<LocationUpdatedEvent>().Subject;
        locationEvent.VehicleId.Should().Be(_sampleLocationEvent.VehicleId);
    }

    [Fact]
    public void TryFromJson_WithEmptyOrWhitespaceJson_ReturnsFalseAndNull()
    {
        // Arrange
        var emptyJson = string.Empty;
        var whitespaceJson = "   ";

        // Act
        var emptyResult = DomainEventJsonExtensions.TryFromJson(emptyJson, out var emptyValue);
        var whitespaceResult = DomainEventJsonExtensions.TryFromJson(whitespaceJson, out var whitespaceValue);

        // Assert
        emptyResult.Should().BeFalse();
        emptyValue.Should().BeNull();

        whitespaceResult.Should().BeFalse();
        whitespaceValue.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => DomainEventJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json";

        // Act
        var result = DomainEventJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Roundtrip_WithLocationUpdatedEvent_PreservesAllProperties()
    {
        // Arrange & Act
        var json = DomainEventJsonExtensions.ToJson(_sampleLocationEvent);
        var deserialized = DomainEventJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        var locationEvent = deserialized.Should().BeAssignableTo<LocationUpdatedEvent>().Subject;

        locationEvent.EventId.Should().Be(_sampleLocationEvent.EventId);
        locationEvent.OccurredAt.Should().BeCloseTo(_sampleLocationEvent.OccurredAt, TimeSpan.FromSeconds(1));
        locationEvent.TriggeredBy.Should().Be(_sampleLocationEvent.TriggeredBy);
        locationEvent.CorrelationId.Should().Be(_sampleLocationEvent.CorrelationId);
        locationEvent.VehicleId.Should().Be(_sampleLocationEvent.VehicleId);
        locationEvent.Latitude.Should().Be(_sampleLocationEvent.Latitude);
        locationEvent.Longitude.Should().Be(_sampleLocationEvent.Longitude);
        locationEvent.Accuracy.Should().Be(_sampleLocationEvent.Accuracy);
        locationEvent.PreviousLatitude.Should().Be(_sampleLocationEvent.PreviousLatitude);
        locationEvent.PreviousLongitude.Should().Be(_sampleLocationEvent.PreviousLongitude);
        locationEvent.Speed.Should().Be(_sampleLocationEvent.Speed);
        locationEvent.Heading.Should().Be(_sampleLocationEvent.Heading);
    }

    [Fact]
    public void Roundtrip_WithVehicleStatusChangedEvent_PreservesAllProperties()
    {
        // Arrange & Act
        var json = DomainEventJsonExtensions.ToJson(_sampleStatusEvent);
        var deserialized = DomainEventJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        var statusEvent = deserialized.Should().BeAssignableTo<VehicleStatusChangedEvent>().Subject;

        statusEvent.EventId.Should().Be(_sampleStatusEvent.EventId);
        statusEvent.OccurredAt.Should().BeCloseTo(_sampleStatusEvent.OccurredAt, TimeSpan.FromSeconds(1));
        statusEvent.TriggeredBy.Should().Be(_sampleStatusEvent.TriggeredBy);
        statusEvent.CorrelationId.Should().Be(_sampleStatusEvent.CorrelationId);
        statusEvent.VehicleId.Should().Be(_sampleStatusEvent.VehicleId);
        statusEvent.VehiclePlate.Should().Be(_sampleStatusEvent.VehiclePlate);
        statusEvent.PreviousStatus.Should().Be(_sampleStatusEvent.PreviousStatus);
        statusEvent.NewStatus.Should().Be(_sampleStatusEvent.NewStatus);
        statusEvent.Reason.Should().Be(_sampleStatusEvent.Reason);
    }

    [Fact]
    public void Roundtrip_WithTrackingSessionCompletedEvent_PreservesAllProperties()
    {
        // Arrange & Act
        var json = DomainEventJsonExtensions.ToJson(_sampleSessionEvent);
        var deserialized = DomainEventJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        var sessionEvent = deserialized.Should().BeAssignableTo<TrackingSessionCompletedEvent>().Subject;

        sessionEvent.EventId.Should().Be(_sampleSessionEvent.EventId);
        sessionEvent.OccurredAt.Should().BeCloseTo(_sampleSessionEvent.OccurredAt, TimeSpan.FromSeconds(1));
        sessionEvent.TriggeredBy.Should().Be(_sampleSessionEvent.TriggeredBy);
        sessionEvent.SessionId.Should().Be(_sampleSessionEvent.SessionId);
        sessionEvent.VehicleId.Should().Be(_sampleSessionEvent.VehicleId);
        sessionEvent.StartedAt.Should().Be(_sampleSessionEvent.StartedAt);
        sessionEvent.EndedAt.Should().Be(_sampleSessionEvent.EndedAt);
        sessionEvent.TotalDistanceKm.Should().Be(_sampleSessionEvent.TotalDistanceKm);
        sessionEvent.LocationCount.Should().Be(_sampleSessionEvent.LocationCount);
        sessionEvent.AverageSpeedKmh.Should().Be(_sampleSessionEvent.AverageSpeedKmh);
    }
}
