#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

using Microsoft.Extensions.Options;

/// <summary>
/// Validates the configured <see cref="ApiKeyCredential"/> entries at startup.
/// Each entry needs a unique name, a hex SHA-256 hash and non-blank scopes.
/// </summary>
public sealed class ApiKeyAuthenticationOptionsValidation : IValidateOptions<ApiKeyAuthenticationOptions>
{
    /// <summary>
    /// Validates the API key entries of the supplied options.
    /// </summary>
    /// <param name="name">The options instance name. Not used; every instance is validated the same way.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>A success result when all entries are valid; otherwise a failure listing every problem found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public ValidateOptionsResult Validate(string? name, ApiKeyAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var problems = new List<string>();
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var credential in options.Keys)
        {
            if (string.IsNullOrWhiteSpace(credential.Name))
            {
                problems.Add("ApiKeyAuthentication.Keys contains an entry with an empty Name.");
            }
            else if (!names.Add(credential.Name))
            {
                problems.Add($"ApiKeyAuthentication.Keys contains duplicate Name '{credential.Name}'.");
            }

            if (!ApiKeyVerifier.IsValidHash(credential.Sha256))
            {
                problems.Add($"ApiKeyAuthentication.Keys['{credential.Name}'].Sha256 must be a 64-character hexadecimal SHA-256 hash.");
            }

            if (credential.Scopes.Any(string.IsNullOrWhiteSpace))
            {
                problems.Add($"ApiKeyAuthentication.Keys['{credential.Name}'].Scopes contains an empty scope.");
            }
        }

        return problems.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(problems);
    }
}
