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
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
        _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    /// <summary>
    /// Authenticates the request by validating the provided API key.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Hotfix: Ensure the server properly authenticates API keys.
        // This resolves the client-side reconnection loop by allowing the server
        // to correctly reject unauthorized requests with a 401, preventing
        // clients from endlessly attempting to re-establish connections with invalid credentials.

        string? apiKey = ExtractApiKey();

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API Key not found in header '{ApiKeyHeaderName}', query parameter '{ApiKeyQueryParamName}', or access_token parameter.",
                AuthenticationConstants.ApiKeyHeaderName, AuthenticationConstants.ApiKeyQueryParamName);
            return AuthenticateResult.Fail($"API Key not found in header '{AuthenticationConstants.ApiKeyHeaderName}', query parameter '{AuthenticationConstants.ApiKeyQueryParamName}', or access_token parameter.");
        }

        if (!IsValidApiKey(apiKey))
        {
            _logger.LogWarning("Invalid API Key provided.");
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "API_User"),
            new Claim(ClaimTypes.Name, "API_User")
        };

        // Add role claims based on API key
        var roles = GetRolesForApiKey(apiKey);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        _logger.LogInformation("API Key authenticated successfully with roles: {Roles}", string.Join(", ", roles));
        return AuthenticateResult.Success(ticket);
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
    /// Validates if the API key is valid.
    /// In production, this would check against a database, cache, or configuration.
    /// Currently uses simple validation against known keys defined in configuration.
    /// </summary>
    private bool IsValidApiKey(string apiKey)
    {
        // TODO: Implement actual API key validation against a secure store.
        // For now, validate against a configured API key.
        var validApiKey = _configuration["Authentication:ApiKey"];

        // Placeholder validation - reject empty or obviously invalid keys
        if (string.IsNullOrWhiteSpace(apiKey)) return false;
        if (string.IsNullOrWhiteSpace(validApiKey)) return false; // Configuration error

        return apiKey == validApiKey;
    }

    /// <summary>
    /// Determines the roles for the authenticated user based on the API key.
    /// </summary>
    private IEnumerable<string> GetRolesForApiKey(string apiKey)
    {
        // Check if this is the main API key
        var validApiKey = _configuration["Authentication:ApiKey"];
        if (string.IsNullOrWhiteSpace(validApiKey) || apiKey != validApiKey)
        {
            yield break;
        }

        // Get configured roles for this API key from configuration
        var rolesConfig = _configuration.GetSection("ApiKeyAuthentication:Roles");
        if (rolesConfig.Exists())
        {
            foreach (var role in rolesConfig.Get<string[]>() ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    yield return role.Trim();
                }
            }
        }

        // Default roles for the main API key
        yield return "device";
        yield return "ingest";
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