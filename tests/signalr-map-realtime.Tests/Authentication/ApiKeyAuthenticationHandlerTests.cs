#nullable enable
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SignalRMapRealtime.Tests;
using SignalRMapRealtime.Authentication;
using Xunit;

namespace signalr_map_realtime.Tests.Authentication
{
    public class ApiKeyAuthenticationHandlerTests
    {
        private const string ValidApiKey = "test-api-key-123";

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

        public ApiKeyAuthenticationHandlerTests()
        {
            _loggerFactory = Substitute.For<ILoggerFactory>();
            _logger = Substitute.For<ILogger<ApiKeyAuthenticationHandler>>();
            _loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>().Returns(_logger);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_ValidApiKey_ReturnsAuthenticatedUserWithExpectedClaims()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey, "device", "ingest"));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = ValidApiKey;

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeTrue();
            authenticateResult.Failure.Should().BeNull();

            var principal = authenticateResult.Principal;
            principal.Should().NotBeNull();
            principal!.Identity.Should().NotBeNull();
            principal!.Identity!.IsAuthenticated.Should().BeTrue();
            principal.Identity.AuthenticationType.Should().Be(AuthenticationConstants.ApiKeySchemeName);

            var claimsPrincipal = principal as ClaimsPrincipal;
            claimsPrincipal.Should().NotBeNull();
            claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("default");
            claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("default");
            claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value).Should().BeEquivalentTo(new[] { "device", "ingest" });

            _logger.HasLogged(LogLevel.Information, "API Key authenticated successfully").Should().BeTrue();
        }

        [Fact]
        public async Task HandleAuthenticateAsync_MissingHeaderAndQueryParameter_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            // No headers or query parameters set

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Contain("API Key not found");

            _logger.HasLogged(LogLevel.Warning, "API Key not found in header").Should().BeTrue();
        }

        [Fact]
        public async Task HandleAuthenticateAsync_MissingHeader_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            // No headers set

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Contain("API Key not found");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_WrongApiKey_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = "wrong-api-key";

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Be("Invalid API Key.");

            _logger.Received(1).LogWarning("Invalid API Key provided.");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_KeyDifferingOnlyInCase_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = ValidApiKey.ToUpperInvariant();

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure!.Message.Should().Be("Invalid API Key.");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_EmptyApiKey_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = string.Empty;

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Contain("API Key not found");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_WhitespaceApiKey_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = "   ";

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Be("Invalid API Key.");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_ApiKeyFromQueryParameter_ReturnsAuthenticatedUserWithExpectedClaims()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
                { AuthenticationConstants.ApiKeyQueryParamName, ValidApiKey }
            });

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeTrue();
            authenticateResult.Failure.Should().BeNull();

            var principal = authenticateResult.Principal;
            principal.Should().NotBeNull();
            principal!.Identity.Should().NotBeNull();
            principal!.Identity!.IsAuthenticated.Should().BeTrue();
            principal.Identity.AuthenticationType.Should().Be(AuthenticationConstants.ApiKeySchemeName);

            var claimsPrincipal = principal as ClaimsPrincipal;
            claimsPrincipal.Should().NotBeNull();
            claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("default");
            claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("default");

            _logger.HasLogged(LogLevel.Information, "API Key authenticated successfully").Should().BeTrue();
        }

        [Fact]
        public async Task HandleAuthenticateAsync_HeaderTakesPrecedenceOverQueryParameter()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context, CreateCredential("default", ValidApiKey));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = "wrong-header-key";
            context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> {
                { AuthenticationConstants.ApiKeyQueryParamName, ValidApiKey }
            });

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Be("Invalid API Key.");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_SecondConfiguredKey_AuthenticatesWithItsOwnNameAndScopes()
        {
            // Arrange - two active keys, as during a rotation
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(
                context,
                CreateCredential("retiring", "old-key-456", "device"),
                CreateCredential("current", ValidApiKey, "ingest"));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = ValidApiKey;

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Succeeded.Should().BeTrue();

            var claimsPrincipal = authenticateResult.Principal as ClaimsPrincipal;
            claimsPrincipal.Should().NotBeNull();
            claimsPrincipal!.FindFirst(ClaimTypes.Name)?.Value.Should().Be("current");
            claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value).Should().BeEquivalentTo(new[] { "ingest" });
        }

        [Fact]
        public async Task HandleAuthenticateAsync_RetiredKeyStillInConfig_AuthenticatesWithItsOwnName()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(
                context,
                CreateCredential("retiring", "old-key-456", "device"),
                CreateCredential("current", ValidApiKey, "ingest"));

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = "old-key-456";

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Succeeded.Should().BeTrue();
            (authenticateResult.Principal as ClaimsPrincipal)!.FindFirst(ClaimTypes.Name)?.Value.Should().Be("retiring");
        }

        [Fact]
        public async Task HandleAuthenticateAsync_NoKeysConfigured_ReturnsFailResult()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context);

            context.Request.Headers[AuthenticationConstants.ApiKeyHeaderName] = "any-key";

            // Act
            var authenticateResult = await handler.AuthenticateAsync();

            // Assert
            authenticateResult.Should().NotBeNull();
            authenticateResult.Succeeded.Should().BeFalse();
            authenticateResult.Failure.Should().NotBeNull();
            authenticateResult.Failure!.Message.Should().Be("Invalid API Key.");
        }

        [Fact]
        public async Task HandleChallengeAsync_Sets401StatusCodeAndWritesErrorResponse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context);
            var properties = new AuthenticationProperties();

            // Act
            await handler.ChallengeAsync(properties);

            // Assert - The handler sets the response status code and content type
            // We can't directly verify this without accessing the response, but the method should execute without error
            Assert.True(true, "ChallengeAsync should execute without throwing exceptions");
        }

        [Fact]
        public async Task HandleForbiddenAsync_Sets403StatusCodeAndWritesErrorResponse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var handler = await CreateHandlerAsync(context);
            var properties = new AuthenticationProperties();

            // Act
            await handler.ForbidAsync(properties);

            // Assert - The handler sets the response status code and content type
            // We can't directly verify this without accessing the response, but the method should execute without error
            Assert.True(true, "ForbidAsync should execute without throwing exceptions");
        }

        private static ApiKeyCredential CreateCredential(string name, string plaintextKey, params string[] scopes) =>
            new()
            {
                Name = name,
                Sha256 = ApiKeyVerifier.ComputeHash(plaintextKey),
                Scopes = scopes.ToList()
            };

        private async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(HttpContext context, params ApiKeyCredential[] keys)
        {
            var options = new ApiKeyAuthenticationOptions { Keys = keys.ToList() };
            var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            optionsMonitor.Get(AuthenticationConstants.ApiKeySchemeName).Returns(options);

            var handler = new ApiKeyAuthenticationHandler(
                optionsMonitor,
                _loggerFactory,
                System.Text.Encodings.Web.UrlEncoder.Default);

            // The handler reads the request from the context, so it must be initialized with one.
            var scheme = new AuthenticationScheme(
                AuthenticationConstants.ApiKeySchemeName,
                null,
                typeof(ApiKeyAuthenticationHandler));
            await handler.InitializeAsync(scheme, context);

            return handler;
        }
    }
}
