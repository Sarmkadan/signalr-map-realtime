#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

/// <summary>
/// Defines constants related to API Key authentication.
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// The name of the API Key authentication scheme.
    /// </summary>
    public const string ApiKeySchemeName = "ApiKey";

    /// <summary>
    /// The name of the HTTP header where the API key is expected.
    /// </summary>
    public const string ApiKeyHeaderName = "X-API-Key";

    /// <summary>
    /// The name of the query parameter where the API key is expected.
    /// </summary>
    public const string ApiKeyQueryParamName = "api_key";
}
