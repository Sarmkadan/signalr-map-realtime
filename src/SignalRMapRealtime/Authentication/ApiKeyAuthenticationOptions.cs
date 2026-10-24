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

    /// <summary>
    /// The API keys accepted by the scheme. A request authenticates with any entry whose hash matches
    /// the presented key, which allows several keys to be active during rotation.
    /// </summary>
    public IList<ApiKeyCredential> Keys { get; set; } = new List<ApiKeyCredential>();

    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}
