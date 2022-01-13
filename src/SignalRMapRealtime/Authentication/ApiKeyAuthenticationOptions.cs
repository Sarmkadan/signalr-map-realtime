#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Configuration options for API Key authentication scheme.
/// Extends <see cref="AuthenticationSchemeOptions"/> to integrate with ASP.NET Core's
/// authentication pipeline. API keys are expected in the X-Api-Key header.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The default scheme name used to register this authentication handler.
    /// </summary>
    public const string DefaultScheme = "ApiKey";

    /// <summary>Gets the authentication scheme name.</summary>
    public string Scheme => DefaultScheme;

    /// <summary>Authentication type identifier used in <see cref="System.Security.Claims.ClaimsIdentity"/>.</summary>
    public string AuthenticationType = DefaultScheme;
}
