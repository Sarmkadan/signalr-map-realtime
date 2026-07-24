#nullable enable
using System;
using Xunit;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

namespace SignalRMapRealtime.Tests;

public class TrackingSessionJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        var session = new TrackingSession
        {
            Id = 1,
            SessionName = "Test Session",
            VehicleId = 100,
            Status = SessionStatus.Active,
            StartTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var json = session.ToJson();

        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("Test Session", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        var session = new TrackingSession
        {
            Id = 2,
            SessionName = "Indented Session",
            VehicleId = 200,
            Status = SessionStatus.Completed
        };

        var json = session.ToJson(indented: true);

        Assert.NotNull(json);
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void ToJson_NullSession_ThrowsArgumentNullException()
    {
        TrackingSession? session = null;

        Assert.Throws<ArgumentNullException>(() => session!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedSession()
    {
        var session = new TrackingSession
        {
            Id = 3,
            SessionName = "Deserialize Test",
            VehicleId = 300,
            Status = SessionStatus.Paused,
            StartTime = new DateTime(2024, 2, 1, 12, 0, 0, DateTimeKind.Utc)
        };
        var json = session.ToJson();

        var result = TrackingSessionJsonExtensions.FromJson(json);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Deserialize Test", result.SessionName);
        Assert.Equal(300, result.VehicleId);
        Assert.Equal(SessionStatus.Paused, result.Status);
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        var invalidJson = "{ invalid json";

        var result = TrackingSessionJsonExtensions.FromJson(invalidJson);

        Assert.Null(result);
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        var emptyJson = "";

        Assert.Throws<ArgumentException>(() => TrackingSessionJsonExtensions.FromJson(emptyJson));
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentException()
    {
        string? nullJson = null;

        Assert.Throws<ArgumentException>(() => TrackingSessionJsonExtensions.FromJson(nullJson!));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedSession()
    {
        var session = new TrackingSession
        {
            Id = 4,
            SessionName = "TryParse Test",
            VehicleId = 400,
            Status = SessionStatus.Completed,
            StartTime = new DateTime(2024, 3, 1, 14, 0, 0, DateTimeKind.Utc)
        };
        var json = session.ToJson();

        var result = TrackingSessionJsonExtensions.TryFromJson(json, out var deserializedSession);

        Assert.True(result);
        Assert.NotNull(deserializedSession);
        Assert.Equal(4, deserializedSession.Id);
        Assert.Equal("TryParse Test", deserializedSession.SessionName);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var invalidJson = "{ invalid";

        var result = TrackingSessionJsonExtensions.TryFromJson(invalidJson, out var deserializedSession);

        Assert.False(result);
        Assert.Null(deserializedSession);
    }

    [Fact]
    public void RoundTripSerialization_PreservesAllProperties()
    {
        var originalSession = new TrackingSession
        {
            Id = 99,
            SessionName = "RoundTrip Test",
            VehicleId = 999,
            RouteId = 50,
            Status = SessionStatus.Active,
            StartTime = new DateTime(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2024, 6, 15, 18, 45, 0, DateTimeKind.Utc),
            TotalDistance = 555.55,
            AverageSpeed = 55.55,
            MaxSpeed = 180.0,
            TotalIdleSeconds = 1800,
            Notes = "Round trip test"
        };

        var json = originalSession.ToJson();
        var deserialized = TrackingSessionJsonExtensions.FromJson(json);

        Assert.NotNull(deserialized);
        Assert.Equal(originalSession.Id, deserialized.Id);
        Assert.Equal(originalSession.SessionName, deserialized.SessionName);
        Assert.Equal(originalSession.VehicleId, deserialized.VehicleId);
        Assert.Equal(originalSession.RouteId, deserialized.RouteId);
        Assert.Equal(originalSession.Status, deserialized.Status);
        Assert.Equal(originalSession.StartTime, deserialized.StartTime);
        Assert.Equal(originalSession.EndTime, deserialized.EndTime);
        Assert.Equal(originalSession.TotalDistance, deserialized.TotalDistance);
        Assert.Equal(originalSession.AverageSpeed, deserialized.AverageSpeed);
        Assert.Equal(originalSession.MaxSpeed, deserialized.MaxSpeed);
        Assert.Equal(originalSession.TotalIdleSeconds, deserialized.TotalIdleSeconds);
        Assert.Equal(originalSession.Notes, deserialized.Notes);
    }
}