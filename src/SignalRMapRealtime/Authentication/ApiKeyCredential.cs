#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

/// <summary>
/// A named API key accepted by <see cref="ApiKeyAuthenticationOptions"/>.
/// Only the SHA-256 hash of the key is stored in configuration; the plaintext key is never persisted.
/// </summary>
/// <example>
/// <code>
/// "Keys": [
///   { "Name": "ingest-device-2026-10", "Sha256": "&lt;hash&gt;", "Scopes": [ "device", "ingest" ] }
/// ]
/// </code>
/// The hash can be produced with <c>echo -n 'your-key' | sha256sum</c> or <see cref="ApiKeyVerifier.ComputeHash"/>.
/// </example>
public class ApiKeyCredential
{
    /// <summary>
    /// Unique, human-readable name of the key, used in logs and when rotating keys.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hex-encoded SHA-256 hash (64 characters) of the API key.
    /// </summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Scopes granted to requests authenticated with this key. Each scope is issued as a role claim,
    /// so existing <c>[Authorize(Roles = ...)]</c> policies apply to scoped keys.
    /// </summary>
    public IList<string> Scopes { get; set; } = new List<string>();
}
