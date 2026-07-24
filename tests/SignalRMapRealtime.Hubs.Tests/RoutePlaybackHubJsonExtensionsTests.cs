using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SignalRMapRealtime.Hubs;
using SignalRMapRealtime.DTOs;
using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace SignalRMapRealtime.Hubs.Tests
{
    /// <summary>
    /// Tests for <see cref="RoutePlaybackHubJsonExtensions"/> JSON serialization and deserialization.
    /// </summary>
    public class RoutePlaybackHubJsonExtensionsTests
    {
        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.ToJson(RoutePlaybackHub, bool)"/>
        /// serializes a RoutePlaybackHub with correct camelCase property naming and ordered data.
        /// </summary>
        [Fact]
        public void ToJson_WithValidRoutePlaybackHub_ProducesCorrectJsonShape()
        {
            // Arrange
            var hub = new RoutePlaybackHub(
                Substitute.For<IRoutePlaybackService>(),
                Substitute.For<ILogger<RoutePlaybackHub>>());

            var playbackFrame = new PlaybackFrameDto(
                Guid.NewGuid(),
                0,
                100,
                DateTime.UtcNow,
                52.5200,
                13.4050,
                45.5,
                270.0,
                35.0,
                10.5,
                50.25,
                49.75,
                50,
                TimeSpan.FromHours(1)
            );

            var routePlayback = new RoutePlaybackHub(
                playbackFrame,
                new[] { playbackFrame },
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow
            );

            // Act
            var json = routePlayback.ToJson();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();

            // Verify JSON can be parsed
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);
            parsed.ValueKind.Should().Be(JsonValueKind.Object);

            // Verify top-level properties exist with correct naming (camelCase)
            parsed.TryGetProperty("playbackFrame", out _).Should().BeTrue();
            parsed.TryGetProperty("waypoints", out _).Should().BeTrue();
            parsed.TryGetProperty("startedAt", out _).Should().BeTrue();
            parsed.TryGetProperty("completedAt", out _).Should().BeTrue();

            // Verify camelCase property names are used
            var jsonString = json;
            jsonString.Should().Contain("playbackFrame");
            jsonString.Should().Contain("waypoints");
            jsonString.Should().Contain("startedAt");
            jsonString.Should().Contain("completedAt");

            // Should NOT contain PascalCase
            jsonString.Should().NotContain("PlaybackFrame");
            jsonString.Should().NotContain("Waypoints");
            jsonString.Should().NotContain("StartedAt");
            jsonString.Should().NotContain("CompletedAt");
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.ToJson(RoutePlaybackHub, bool)"/>
        /// handles an empty route (no waypoints) without throwing.
        /// </summary>
        [Fact]
        public void ToJson_WithEmptyRoute_HandlesGracefully()
        {
            // Arrange
            var hub = new RoutePlaybackHub(
                Substitute.For<IRoutePlaybackService>(),
                Substitute.For<ILogger<RoutePlaybackHub>>());

            var playbackFrame = new PlaybackFrameDto(
                Guid.NewGuid(),
                0,
                0,
                DateTime.UtcNow,
                0.0,
                0.0,
                null,
                null,
                null,
                null,
                0.0,
                null,
                0,
                TimeSpan.Zero
            );

            var routePlayback = new RoutePlaybackHub(
                playbackFrame,
                Array.Empty<PlaybackFrameDto>(), // Empty waypoints
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow
            );

            // Act
            var act = () => routePlayback.ToJson();

            // Assert
            act.Should().NotThrow<Exception>();
            var json = routePlayback.ToJson();
            json.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.ToJson(RoutePlaybackHub, bool)"/>
        /// produces ordered waypoints/timestamps in the JSON output.
        /// </summary>
        [Fact]
        public void ToJson_WithMultipleWaypoints_ProducesOrderedTimestamps()
        {
            // Arrange
            var hub = new RoutePlaybackHub(
                Substitute.For<IRoutePlaybackService>(),
                Substitute.For<ILogger<RoutePlaybackHub>>());

            var waypoints = new[]
            {
                new PlaybackFrameDto(
                    Guid.NewGuid(),
                    0,
                    100,
                    DateTime.UtcNow,
                    52.5200,
                    13.4050,
                    45.5,
                    270.0,
                    35.0,
                    10.5,
                    0.0,
                    100.0,
                    0,
                    TimeSpan.Zero
                ),
                new PlaybackFrameDto(
                    Guid.NewGuid(),
                    1,
                    100,
                    DateTime.UtcNow.AddSeconds(10),
                    52.5210,
                    13.4060,
                    50.0,
                    275.0,
                    36.0,
                    11.0,
                    0.1,
                    99.9,
                    10,
                    TimeSpan.FromSeconds(10)
                ),
                new PlaybackFrameDto(
                    Guid.NewGuid(),
                    2,
                    100,
                    DateTime.UtcNow.AddSeconds(20),
                    52.5220,
                    13.4070,
                    55.0,
                    280.0,
                    37.0,
                    11.5,
                    0.2,
                    99.8,
                    20,
                    TimeSpan.FromSeconds(20)
                )
            };

            var routePlayback = new RoutePlaybackHub(
                waypoints[0],
                waypoints,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow
            );

            // Act
            var json = routePlayback.ToJson();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();

            // Parse and verify waypoints array exists and has correct count
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);
            parsed.TryGetProperty("waypoints", out var waypointsElement).Should().BeTrue();
            waypointsElement.ValueKind.Should().Be(JsonValueKind.Array);
            waypointsElement.GetArrayLength().Should().Be(3);

            // Verify timestamps are in order (ascending)
            var timestamps = waypointsElement.EnumerateArray()
                .Select(w => w.GetProperty("timestamp").GetDateTime())
                .ToList();

            timestamps.Should().BeInAscendingOrder("Waypoints should be ordered by timestamp");
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.ToJson(RoutePlaybackHub, bool)"/>
        /// throws ArgumentNullException when passed null.
        /// </summary>
        [Fact]
        public void ToJson_WithNullValue_ThrowsArgumentNullException()
        {
            // Arrange
            RoutePlaybackHub? nullHub = null;

            // Act
            var act = () => nullHub!.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.FromJson(string)"/>
        /// deserializes a valid JSON string correctly.
        /// </summary>
        [Fact]
        public void FromJson_WithValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var json = @"{
                ""playbackFrame"": {
                    ""playbackId"": ""d3f4e5a6b7c8d9e0f1a2b3c4d5e6f7a"",
                    ""frameIndex"": 0,
                    ""totalFrames"": 100,
                    ""timestamp"": ""2024-01-01T12:00:00Z"",
                    ""latitude"": 52.5200,
                    ""longitude"": 13.4050,
                    ""speed"": 45.5,
                    ""bearing"": 270.0,
                    ""altitude"": 35.0,
                    ""distanceCoveredKm"": 10.5,
                    ""remainingDistanceKm"": 49.75,
                    ""completionPercentage"": 50,
                    ""elapsedTime"": ""PT1H"",
                    ""address"": ""Test Location""
                },
                ""waypoints"": [],
                ""startedAt"": ""2024-01-01T11:00:00Z"",
                ""completedAt"": ""2024-01-01T13:00:00Z""
            }";

            // Act
            var result = RoutePlaybackHubJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.PlaybackFrame.Should().NotBeNull();
            result.PlaybackFrame.PlaybackId.Should().NotBe(Guid.Empty);
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.FromJson(string)"/>
        /// throws ArgumentNullException when passed null JSON string.
        /// </summary>
        [Fact]
        public void FromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? nullJson = null;

            // Act
            var act = () => RoutePlaybackHubJsonExtensions.FromJson(nullJson!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.FromJson(string)"/>
        /// throws ArgumentException when passed empty or whitespace JSON string.
        /// </summary>
        [Fact]
        public void FromJson_WithEmptyOrWhitespaceJson_ThrowsArgumentException()
        {
            // Arrange
            var emptyJson = string.Empty;
            var whitespaceJson = "   ";

            // Act & Assert for empty
            var act1 = () => RoutePlaybackHubJsonExtensions.FromJson(emptyJson);
            act1.Should().Throw<ArgumentException>();

            // Act & Assert for whitespace
            var act2 = () => RoutePlaybackHubJsonExtensions.FromJson(whitespaceJson);
            act2.Should().Throw<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.FromJson(string)"/>
        /// throws JsonException when passed invalid JSON.
        /// </summary>
        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var act = () => RoutePlaybackHubJsonExtensions.FromJson(invalidJson);

            // Assert
            act.Should().Throw<JsonException>();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.TryFromJson(string, out RoutePlaybackHub)"/>
        /// returns false and null for invalid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            RoutePlaybackHub? result = new RoutePlaybackHub(
                Substitute.For<IRoutePlaybackService>(),
                Substitute.For<ILogger<RoutePlaybackHub>>());

            // Act
            var success = RoutePlaybackHubJsonExtensions.TryFromJson(invalidJson, out result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.TryFromJson(string, out RoutePlaybackHub)"/>
        /// returns false and null for empty or whitespace JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_WithEmptyOrWhitespaceJson_ReturnsFalseAndNull()
        {
            // Arrange
            var emptyJson = string.Empty;
            RoutePlaybackHub? result = null;

            // Act
            var success = RoutePlaybackHubJsonExtensions.TryFromJson(emptyJson, out result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.TryFromJson(string, out RoutePlaybackHub)"/>
        /// returns true and deserialized object for valid JSON.
        /// </summary>
        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedObject()
        {
            // Arrange
            var json = @"{
                ""playbackFrame"": {
                    ""playbackId"": ""d3f4e5a6b7c8d9e0f1a2b3c4d5e6f7a"",
                    ""frameIndex"": 0,
                    ""totalFrames"": 100,
                    ""timestamp"": ""2024-01-01T12:00:00Z"",
                    ""latitude"": 52.5200,
                    ""longitude"": 13.4050,
                    ""speed"": 45.5,
                    ""bearing"": 270.0,
                    ""altitude"": 35.0,
                    ""distanceCoveredKm"": 10.5,
                    ""remainingDistanceKm"": 49.75,
                    ""completionPercentage"": 50,
                    ""elapsedTime"": ""PT1H"",
                    ""address"": null
                },
                ""waypoints"": [],
                ""startedAt"": ""2024-01-01T11:00:00Z"",
                ""completedAt"": null
            }";
            RoutePlaybackHub? result = null;

            // Act
            var success = RoutePlaybackHubJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.TryFromJson(string, out RoutePlaybackHub)"/>
        /// throws ArgumentNullException when passed null JSON string.
        /// </summary>
        [Fact]
        public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? nullJson = null;
            RoutePlaybackHub? result = null;

            // Act
            var act = () => RoutePlaybackHubJsonExtensions.TryFromJson(nullJson!, out result);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that <see cref="RoutePlaybackHubJsonExtensions.ToJson(RoutePlaybackHub, bool)"/>
        /// with indented=true produces formatted JSON.
        /// </summary>
        [Fact]
        public void ToJson_WithIndentedTrue_ProducesFormattedJson()
        {
            // Arrange
            var hub = new RoutePlaybackHub(
                Substitute.For<IRoutePlaybackService>(),
                Substitute.For<ILogger<RoutePlaybackHub>>());

            var routePlayback = new RoutePlaybackHub(
                new PlaybackFrameDto(
                    Guid.NewGuid(),
                    0,
                    1,
                    DateTime.UtcNow,
                    0.0,
                    0.0,
                    null,
                    null,
                    null,
                    null,
                    0.0,
                    null,
                    0,
                    TimeSpan.Zero
                ),
                Array.Empty<PlaybackFrameDto>(),
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow
            );

            // Act
            var indentedJson = routePlayback.ToJson(indented: true);
            var compactJson = routePlayback.ToJson(indented: false);

            // Assert
            indentedJson.Should().NotBeNullOrWhiteSpace();
            compactJson.Should().NotBeNullOrWhiteSpace();

            // Indented JSON should contain newlines and be more readable
            indentedJson.Should().Contain("\n");

            // Both should parse to the same structure
            var indentedParsed = JsonSerializer.Deserialize<JsonElement>(indentedJson);
            var compactParsed = JsonSerializer.Deserialize<JsonElement>(compactJson);

            indentedParsed.Should().BeEquivalentTo(compactParsed);
        }
    }
}
