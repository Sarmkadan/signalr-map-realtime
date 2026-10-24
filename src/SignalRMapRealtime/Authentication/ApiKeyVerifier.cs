#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Hashes API keys and matches presented keys against stored <see cref="ApiKeyCredential"/> hashes in constant time.
/// </summary>
/// <remarks>
/// Both sides are reduced to fixed-length SHA-256 digests before comparison, so the comparison time does not
/// depend on the length of the presented key. SHA-256 without a salt is appropriate here because API keys are
/// high-entropy random secrets, not low-entropy passwords.
/// </remarks>
public static class ApiKeyVerifier
{
    private const int DigestLength = 32; // SHA-256 output size in bytes

    /// <summary>
    /// Computes the SHA-256 hash of an API key as a 64-character lowercase hex string.
    /// </summary>
    /// <param name="apiKey">The plaintext API key.</param>
    /// <returns>The lowercase hex SHA-256 hash of <paramref name="apiKey"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="apiKey"/> is <see langword="null"/> or empty.</exception>
    public static string ComputeHash(string apiKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey))).ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether a value is a well-formed hex-encoded SHA-256 hash.
    /// </summary>
    /// <param name="hash">The value to check.</param>
    /// <returns><see langword="true"/> if the value is 64 hex characters; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidHash(string? hash) => DecodeDigest(hash) is not null;

    /// <summary>
    /// Finds the credential whose stored hash matches the presented API key.
    /// Every entry is compared, and the scan does not stop at the first match, so the time taken
    /// does not reveal which entry matched or whether any did.
    /// </summary>
    /// <param name="presentedKey">The plaintext API key supplied by the client.</param>
    /// <param name="credentials">The configured credentials to check against.</param>
    /// <returns>The first matching credential, or <see langword="null"/> if none match.</returns>
    /// <exception cref="ArgumentException"><paramref name="presentedKey"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="credentials"/> is <see langword="null"/>.</exception>
    public static ApiKeyCredential? Find(string presentedKey, IEnumerable<ApiKeyCredential> credentials)
    {
        ArgumentException.ThrowIfNullOrEmpty(presentedKey);
        ArgumentNullException.ThrowIfNull(credentials);

        Span<byte> presentedDigest = stackalloc byte[DigestLength];
        SHA256.HashData(Encoding.UTF8.GetBytes(presentedKey), presentedDigest);

        ApiKeyCredential? match = null;

        foreach (var credential in credentials)
        {
            var storedDigest = DecodeDigest(credential.Sha256);
            var isMatch = storedDigest is not null
                && CryptographicOperations.FixedTimeEquals(presentedDigest, storedDigest);

            if (isMatch)
            {
                match ??= credential;
            }
        }

        return match;
    }

    /// <summary>
    /// Decodes a hex-encoded SHA-256 hash into its raw bytes.
    /// </summary>
    /// <param name="hash">The hex string to decode.</param>
    /// <returns>The 32-byte digest, or <see langword="null"/> if the value is not a 64-character hex string.</returns>
    private static byte[]? DecodeDigest(string? hash) =>
        hash is { Length: DigestLength * 2 } && hash.All(Uri.IsHexDigit)
            ? Convert.FromHexString(hash)
            : null;
}
