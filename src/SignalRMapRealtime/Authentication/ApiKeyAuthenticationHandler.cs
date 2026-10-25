#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Authentication;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

/// <summary>
/// Implements API Key authentication for ASP.NET Core applications.
/// This handler validates API keys provided in the "X-API-Key" header or "api_key" query parameter.
/// Keys are matched against the SHA-256 hashes configured in <see cref="ApiKeyAuthenticationOptions.Keys"/>.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">The monitor providing the <see cref="ApiKeyAuthenticationOptions"/> for this scheme.</param>
    /// <param name="logger">The factory used to create the handler's logger.</param>
    /// <param name="encoder">The URL encoder used by the base authentication handler.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/>, <paramref name="logger"/>, or <paramref name="encoder"/> is <see langword="null"/>.</exception>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(encoder);

        _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    /// <summary>
    /// Authenticates the request by validating the provided API key.
    /// On success the principal is named after the matched key and carries its scopes as role claims.
    /// </summary>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Hotfix: Ensure the server properly authenticates API keys.
        // This resolves the client-side reconnection loop by allowing the server
        // to correctly reject unauthorized requests with a 401, preventing
        // clients from endlessly attempting to re-establish connections with invalid credentials.

        var apiKey = ExtractApiKey();

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API Key not found in header '{ApiKeyHeaderName}', query parameter '{ApiKeyQueryParamName}', or access_token parameter.",
                AuthenticationConstants.ApiKeyHeaderName, AuthenticationConstants.ApiKeyQueryParamName);
            return Task.FromResult(AuthenticateResult.Fail($"API Key not found in header '{AuthenticationConstants.ApiKeyHeaderName}', query parameter '{AuthenticationConstants.ApiKeyQueryParamName}', or access_token parameter."));
        }

        var credential = FindCredential(apiKey);
        if (credential is null)
        {
            _logger.LogWarning("Invalid API Key provided.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, credential.Name),
            new(ClaimTypes.Name, credential.Name)
        };
        claims.AddRange(credential.Scopes.Select(scope => new Claim(ClaimTypes.Role, scope.Trim())));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        _logger.LogInformation("API Key authenticated successfully for '{ApiKeyName}' with roles: {Roles}",
            credential.Name, string.Join(", ", credential.Scopes));
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// Extracts API key from request headers, query parameters, or access_token parameter.
    /// Checks X-API-Key header first, then api_key query parameter, then access_token query parameter.
    /// The access_token parameter is used by SignalR clients for WebSocket connections where headers are not available.
    /// </summary>
    private string? ExtractApiKey()
    {
        // Check header first
        if (Request.Headers.TryGetValue(AuthenticationConstants.ApiKeyHeaderName, out var headerValue))
        {
            return headerValue.ToString();
        }

        // Check api_key query parameter
        if (Request.Query.TryGetValue(AuthenticationConstants.ApiKeyQueryParamName, out var queryValue))
        {
            return queryValue.ToString();
        }

        // Check access_token query parameter (standard SignalR WebSocket authentication)
        // This allows clients to authenticate via query string when using WebSocket transport
        if (Request.Query.TryGetValue("access_token", out var accessTokenValue))
        {
            return accessTokenValue.ToString();
        }

        return null;
    }

    /// <summary>
    /// Finds the configured key that matches the presented API key.
    /// The comparison is constant-time and runs over every configured key.
    /// </summary>
    /// <param name="apiKey">The plaintext API key presented by the client.</param>
    /// <returns>The matching credential, or <see langword="null"/> if the key is blank, unknown, or no keys are configured.</returns>
    private ApiKeyCredential? FindCredential(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        if (Options.Keys.Count == 0)
        {
            _logger.LogError("No API keys are configured. Add entries under SignalRMapRealtime:ApiKeyAuthentication:Keys.");
            return null;
        }

        return ApiKeyVerifier.Find(apiKey, Options.Keys);
    }

    /// <summary>
    /// Handles requests that lack authentication credentials but are configured to require them.
    /// Returns 401 Unauthorized.
    /// </summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";
        // Optionally, write a JSON error response body
        return Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"Authentication required or failed.\"}", CancellationToken.None);
    }

    /// <summary>
    /// Handles requests for resources that are protected by authentication, but the authenticated user does not have the necessary permissions.
    /// Returns 403 Forbidden.
    /// </summary>
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "application/json";
        // Optionally, write a JSON error response body
        return Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"You do not have permission to access this resource.\"}", CancellationToken.None);
    }
}
