using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Hubs;
using SignalRMapRealtime.Services;
using Xunit;

namespace SignalRMapRealtime.Tests
{
    public class RoutePlaybackServiceTests : IAsyncDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<RoutePlaybackHub> _hubContext;
        private readonly IOptions<PlaybackOptions> _options;
        private readonly ILogger<RoutePlaybackService> _logger;
        private readonly RoutePlaybackService _sut;

        public RoutePlaybackServiceTests()
        {
            _scopeFactory = Substitute.For<IServiceScopeFactory>();
            _hubContext = Substitute.For<IHubContext<RoutePlaybackHub>>();
            _options = Options.Create(new PlaybackOptions());
            _logger = Substitute.For<ILogger<RoutePlaybackService>>();
            _sut = new RoutePlaybackService(_scopeFactory, _hubContext, _options, _logger);
        }

        public async ValueTask DisposeAsync()
        {
            await _sut.DisposeAsync();
        }

        [Fact]
        public void Constructor_NullScopeFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("scopeFactory",
                () => new RoutePlaybackService(null!, _hubContext, _options, _logger));
        }

        [Fact]
        public void Constructor_NullHubContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("hubContext",
                () => new RoutePlaybackService(_scopeFactory, null!, _options, _logger));
        }

        [Fact]
        public void Constructor_NullOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("options",
                () => new RoutePlaybackService(_scopeFactory, _hubContext, null!, _logger));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("logger",
                () => new RoutePlaybackService(_scopeFactory, _hubContext, _options, null!));
        }

        [Fact]
        public async Task StartPlaybackAsync_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>("request",
                () => _sut.StartPlaybackAsync(null!));
        }

        [Fact]
        public async Task GetPlaybackStateAsync_UnknownId_ReturnsNull()
        {
            var result = await _sut.GetPlaybackStateAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActivePlaybacksAsync_NoSessions_ReturnsEmptyList()
        {
            var result = await _sut.GetActivePlaybacksAsync();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task PauseResumeStopSeekSpeed_UnknownId_CompletesWithoutError()
        {
            var unknownId = Guid.NewGuid();

            await _sut.PausePlaybackAsync(unknownId);
            await _sut.ResumePlaybackAsync(unknownId);
            await _sut.StopPlaybackAsync(unknownId);
            await _sut.SeekToTimestampAsync(unknownId, DateTime.UtcNow);
            await _sut.SetPlaybackSpeedAsync(unknownId, 2.0);
        }
    }
}
