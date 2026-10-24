#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using FluentAssertions;
using NSubstitute;
using SignalRMapRealtime.Hubs;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRMapRealtime.Tests
{
    public class RoutePlaybackHubTests
    {
        [Fact]
        public void Constructor_AllParametersProvided_InitializesSuccessfully()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();

            // Act
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Assert
            hub.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_NullPlaybackService_ThrowsArgumentNullException()
        {
            // Arrange
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new RoutePlaybackHub(null!, logger));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new RoutePlaybackHub(playbackService, null!));
        }

        [Fact]
        public async Task OnConnectedAsync_LogsConnectionAndCallsBase()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context.ConnectionId
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Act
            await hub.OnConnectedAsync();

            // Assert
            logger.HasLogged(LogLevel.Information, "Playback client test-connection-id connected").Should().BeTrue();
        }

        [Fact]
        public async Task OnDisconnectedAsync_LogsDisconnectionAndCallsBase()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context.ConnectionId
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Act
            await hub.OnDisconnectedAsync(null);

            // Assert
            logger.HasLogged(LogLevel.Information, "Playback client test-connection-id disconnected. Reason: clean disconnect").Should().BeTrue();
        }

        [Fact]
        public async Task StartPlayback_ValidRequest_StartsPlaybackAndNotifiesCaller()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var callerMock = Substitute.For<ISingleClientProxy>();
            clientsMock.Caller.Returns(callerMock);
            hub.Clients = clientsMock;

            var request = new StartPlaybackRequest
            {
                SessionId = 123,
                SpeedMultiplier = 1.5,
                Loop = true
            };

            var playbackId = Guid.NewGuid();
            playbackService.StartPlaybackAsync(request).Returns(Task.FromResult(playbackId));

            // Act
            await hub.StartPlayback(request);

            // Assert
            await playbackService.Received(1).StartPlaybackAsync(request);
            await groupsMock.Received(1).AddToGroupAsync(
                Arg.Is("test-connection-id"),
                Arg.Is($"playback-{playbackId}"));
            Sent(callerMock, "PlaybackStarted", p =>
                Prop<Guid>(p, "PlaybackId") == playbackId &&
                Prop<int>(p, "SessionId") == 123 &&
                Prop<double>(p, "AppliedSpeedMultiplier") == 1.5 &&
                Prop<bool>(p, "Loop")).Should().BeTrue();
        }

        [Fact]
        public async Task StartPlayback_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => hub.StartPlayback(null!));
        }

        [Fact]
        public async Task PausePlayback_ValidId_PausesPlaybackAndNotifiesGroup()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            var state = new PlaybackSessionDto(
                playbackId, 456, PlaybackStatus.Paused,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                1.0, DateTime.UtcNow, 0, 100, 50, false);
            playbackService.GetPlaybackStateAsync(playbackId).Returns(Task.FromResult<PlaybackSessionDto?>(state));

            // Act
            await hub.PausePlayback(playbackId);

            // Assert
            await playbackService.Received(1).PausePlaybackAsync(playbackId);
            await playbackService.Received(1).GetPlaybackStateAsync(playbackId);
            Sent(groupMock, "PlaybackPaused", p =>
                p is PlaybackSessionDto s && s.PlaybackId == playbackId && s.Status == PlaybackStatus.Paused).Should().BeTrue();
        }

        [Fact]
        public async Task PausePlayback_Exception_LogsErrorAndNotifiesCaller()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var callerMock = Substitute.For<ISingleClientProxy>();
            clientsMock.Caller.Returns(callerMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            playbackService.When(x => x.PausePlaybackAsync(playbackId)).Do(x => throw new InvalidOperationException("Test error"));

            // Act
            await hub.PausePlayback(playbackId);

            // Assert
            logger.HasLogged(LogLevel.Error, "Error pausing playback").Should().BeTrue();
            Sent(callerMock, "PlaybackError").Should().BeTrue();
        }

        [Fact]
        public async Task ResumePlayback_ValidId_ResumesPlaybackAndNotifiesGroup()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            var state = new PlaybackSessionDto(
                playbackId, 789, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                1.0, DateTime.UtcNow, 50, 100, 50, false);
            playbackService.GetPlaybackStateAsync(playbackId).Returns(Task.FromResult<PlaybackSessionDto?>(state));

            // Act
            await hub.ResumePlayback(playbackId);

            // Assert
            await playbackService.Received(1).ResumePlaybackAsync(playbackId);
            await playbackService.Received(1).GetPlaybackStateAsync(playbackId);
            Sent(groupMock, "PlaybackResumed", p =>
                p is PlaybackSessionDto s && s.PlaybackId == playbackId && s.Status == PlaybackStatus.Playing).Should().BeTrue();
        }

        [Fact]
        public async Task StopPlayback_ValidId_StopsPlaybackAndNotifiesGroup()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();

            // Act
            await hub.StopPlayback(playbackId);

            // Assert
            Sent(groupMock, "PlaybackStopped").Should().BeTrue();
            await playbackService.Received(1).StopPlaybackAsync(playbackId);
            await groupsMock.Received(1).RemoveFromGroupAsync(
                Arg.Is("test-connection-id"),
                Arg.Is($"playback-{playbackId}"));
        }

        [Fact]
        public async Task SeekTo_ValidParameters_SeeksAndNotifiesGroup()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            var timestamp = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var state = new PlaybackSessionDto(
                playbackId, 999, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                1.0, timestamp, 25, 100, 25, false);
            playbackService.GetPlaybackStateAsync(playbackId).Returns(Task.FromResult<PlaybackSessionDto?>(state));

            // Act
            await hub.SeekTo(playbackId, timestamp);

            // Assert
            await playbackService.Received(1).SeekToTimestampAsync(playbackId, timestamp);
            await playbackService.Received(1).GetPlaybackStateAsync(playbackId);
            Sent(groupMock, "PlaybackSeeked").Should().BeTrue();
        }

        [Fact]
        public async Task SetSpeed_ValidParameters_SetsSpeedAndNotifiesGroup()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            var speedMultiplier = 2.0;
            var state = new PlaybackSessionDto(
                playbackId, 111, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                speedMultiplier, DateTime.UtcNow, 0, 100, 0, false);
            playbackService.GetPlaybackStateAsync(playbackId).Returns(Task.FromResult<PlaybackSessionDto?>(state));

            // Act
            await hub.SetSpeed(playbackId, speedMultiplier);

            // Assert
            await playbackService.Received(1).SetPlaybackSpeedAsync(playbackId, speedMultiplier);
            await playbackService.Received(1).GetPlaybackStateAsync(playbackId);
            Sent(groupMock, "PlaybackSpeedChanged", p =>
                Prop<Guid>(p, "PlaybackId") == playbackId && Prop<double>(p, "SpeedMultiplier") == speedMultiplier).Should().BeTrue();
        }

        [Fact]
        public async Task SetPlaybackSpeed_ValidSessionId_FindsAndUpdatesSpeed()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            var sessionId = 42;
            var multiplier = 1.5;
            var clampedMultiplier = 1.5; // Within 0.25-8.0 range

            // Mock GetActivePlaybacksAsync to return a matching playback
            var playbackState = new PlaybackSessionDto(
                playbackId, sessionId, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                1.0, DateTime.UtcNow, 0, 100, 0, false);
            var activePlaybacks = new[] { playbackState };
            playbackService.GetActivePlaybacksAsync().Returns(Task.FromResult((IReadOnlyList<PlaybackSessionDto>)activePlaybacks));

            var updatedState = new PlaybackSessionDto(
                playbackId, sessionId, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                clampedMultiplier, DateTime.UtcNow, 0, 100, 0, false);
            playbackService.GetPlaybackStateAsync(playbackId).Returns(Task.FromResult<PlaybackSessionDto?>(updatedState));

            // Act
            await hub.SetPlaybackSpeed(sessionId, multiplier);

            // Assert
            await playbackService.Received(1).GetActivePlaybacksAsync();
            await playbackService.Received(1).SetPlaybackSpeedAsync(playbackId, clampedMultiplier);
            await playbackService.Received(1).GetPlaybackStateAsync(playbackId);
            Sent(groupMock, "PlaybackSpeedChanged", p =>
                Prop<Guid>(p, "PlaybackId") == playbackId &&
                Prop<int>(p, "SessionId") == sessionId &&
                Prop<double>(p, "SpeedMultiplier") == clampedMultiplier).Should().BeTrue();
        }

        [Fact]
        public async Task SetPlaybackSpeed_InvalidSessionId_SendsErrorToCaller()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var callerMock = Substitute.For<ISingleClientProxy>();
            clientsMock.Caller.Returns(callerMock);
            hub.Clients = clientsMock;

            // Mock GetActivePlaybacksAsync to return empty (no matching session)
            playbackService.GetActivePlaybacksAsync().Returns(Task.FromResult((IReadOnlyList<PlaybackSessionDto>)Array.Empty<PlaybackSessionDto>()));

            // Act
            await hub.SetPlaybackSpeed(999, 2.0);

            // Assert
            await playbackService.Received(1).GetActivePlaybacksAsync();
            Sent(callerMock, "PlaybackError").Should().BeTrue();
        }

        [Fact]
        public async Task SetPlaybackSpeed_OutOfRangeMultiplier_ClampsToValidRange()
        {
            // Arrange
            var playbackService = Substitute.For<IRoutePlaybackService>();
            var logger = Substitute.For<ILogger<RoutePlaybackHub>>();
            var hub = new RoutePlaybackHub(playbackService, logger);

            // Mock the Context
            var contextMock = Substitute.For<HubCallerContext>();
            contextMock.ConnectionId.Returns("test-connection-id");
            hub.Context = contextMock;

            // Mock Groups
            var groupsMock = Substitute.For<IGroupManager>();
            hub.Groups = groupsMock;

            // Mock Clients
            var clientsMock = Substitute.For<IHubCallerClients>();
            var groupMock = Substitute.For<IClientProxy>();
            clientsMock.Group(Arg.Any<string>()).Returns(groupMock);
            hub.Clients = clientsMock;

            var playbackId = Guid.NewGuid();
            var sessionId = 42;
            var multiplier = 15.0; // Way above 8.0 max
            var clampedMultiplier = 8.0; // Should be clamped to max

            // Mock GetActivePlaybacksAsync to return a matching playback
            var playbackState = new PlaybackSessionDto(
                playbackId, sessionId, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                1.0, DateTime.UtcNow, 0, 100, 0, false);
            var activePlaybacks = new[] { playbackState };
            playbackService.GetActivePlaybacksAsync().Returns(Task.FromResult((IReadOnlyList<PlaybackSessionDto>)activePlaybacks));

            var updatedState = new PlaybackSessionDto(
                playbackId, sessionId, PlaybackStatus.Playing,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
                clampedMultiplier, DateTime.UtcNow, 0, 100, 0, false);
            playbackService.GetPlaybackStateAsync(playbackId).Returns(Task.FromResult<PlaybackSessionDto?>(updatedState));

            // Act
            await hub.SetPlaybackSpeed(sessionId, multiplier);

            // Assert
            await playbackService.Received(1).SetPlaybackSpeedAsync(playbackId, clampedMultiplier);
            Sent(groupMock, "PlaybackSpeedChanged").Should().BeTrue();
        }
    
        /// <summary>
        /// Determines whether the proxy received a <c>SendAsync(method, ...)</c> call whose payload matches the predicate.
        /// </summary>
        /// <remarks>
        /// The SendAsync extension forwards to <c>SendCoreAsync(method, object?[], ...)</c>, which NSubstitute can match.
        /// </remarks>
        private static bool Sent(IClientProxy proxy, string method, Func<object?, bool>? payloadPredicate = null)
        {
            return proxy.ReceivedCalls().Any(call =>
                call.GetMethodInfo().Name == nameof(IClientProxy.SendCoreAsync)
                && (string)call.GetArguments()[0]! == method
                && (payloadPredicate is null || payloadPredicate(((object?[])call.GetArguments()[1]!).FirstOrDefault())));
        }

        /// <summary>
        /// Reads a property from an anonymous payload sent by the hub.
        /// </summary>
        private static T? Prop<T>(object? payload, string name)
            => payload?.GetType().GetProperty(name)?.GetValue(payload) is T value ? value : default;
}
}