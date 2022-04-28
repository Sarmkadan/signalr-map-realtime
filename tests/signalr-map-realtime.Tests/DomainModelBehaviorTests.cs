#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using Xunit;

/// <summary>
/// Contains unit tests for domain model behavior.
/// </summary>
public class DomainModelBehaviorTests
{
    /// <summary>
    /// Tests the <see cref="Vehicle.HasExceededSpeedLimit"/> method when the current speed exceeds the maximum speed.
    /// </summary>
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

    /// <summary>
    /// Tests the <see cref="Vehicle.HasExceededSpeedLimit"/> method when the maximum speed is not configured.
    /// </summary>
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

    /// <summary>
    /// Tests the <see cref="TrackingSession.StartSession"/> method by verifying that the session status is set to Active and the start time is recorded.
    /// </summary>
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

    /// <summary>
    /// Tests that recording a location throws an <see cref="InvalidOperationException"/> when the session is Pending.
    /// </summary>
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

    /// <summary>
    /// Tests that enabling special handling throws an <see cref="ArgumentException"/> when the instructions are empty.
    /// </summary>
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
