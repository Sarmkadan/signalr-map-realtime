// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using Xunit;

public class DomainModelBehaviorTests
{
    [Fact]
    public void Vehicle_HasExceededSpeedLimit_WhenCurrentSpeedExceedsMax_ReturnsTrue()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            MaxSpeed = 100.0,
            LastLocation = new Location { Speed = 120.0 }
        };

        // Act
        var exceeded = vehicle.HasExceededSpeedLimit();

        // Assert
        exceeded.Should().BeTrue();
    }

    [Fact]
    public void Vehicle_HasExceededSpeedLimit_WhenMaxSpeedNotConfigured_ReturnsFalse()
    {
        // Arrange — MaxSpeed is null, so no limit is enforced
        var vehicle = new Vehicle
        {
            MaxSpeed = null,
            LastLocation = new Location { Speed = 200.0 }
        };

        // Act
        var exceeded = vehicle.HasExceededSpeedLimit();

        // Assert
        exceeded.Should().BeFalse();
    }

    [Fact]
    public void TrackingSession_StartSession_SetsActiveStatusAndRecordsStartTime()
    {
        // Arrange
        var session = new TrackingSession();

        // Act
        session.StartSession();

        // Assert
        session.Status.Should().Be(SessionStatus.Active);
        session.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TrackingSession_RecordLocation_WhenSessionIsPending_ThrowsInvalidOperationException()
    {
        // Arrange — newly created session is Pending, not Active
        var session = new TrackingSession();
        var location = new Location { Latitude = 51.5074, Longitude = -0.1278 };

        // Act
        var act = () => session.RecordLocation(location);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*inactive*");
    }

    [Fact]
    public void Asset_EnableSpecialHandling_WithEmptyInstructions_ThrowsArgumentException()
    {
        // Arrange
        var asset = new Asset { Name = "Fragile Electronics" };

        // Act
        var act = () => asset.EnableSpecialHandling(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*instructions*");
    }
}
