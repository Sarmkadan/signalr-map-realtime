#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Options for API Key authentication.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The default scheme name for API Key authentication.
    /// </summary>
    public const string DefaultScheme = "ApiKey";
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}
