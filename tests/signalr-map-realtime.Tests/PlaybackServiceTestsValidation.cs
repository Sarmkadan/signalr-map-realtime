#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="PlaybackServiceTests"/> test cases.
/// </summary>
public static class PlaybackServiceTestsValidation
{
    /// <summary>
    /// Validates that a <see cref="PlaybackServiceTests"/> instance contains valid values for its test scenarios.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PlaybackServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // No public members to validate on the test class itself
        // All validation is handled by the service method contracts

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="PlaybackServiceTests"/> instance is valid for its test scenarios.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this PlaybackServiceTests? value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="PlaybackServiceTests"/> instance is valid for its test scenarios.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance contains validation problems.</exception>
    public static void EnsureValid(this PlaybackServiceTests? value)
    {
        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            message: $"PlaybackServiceTests instance is invalid.{Environment.NewLine}Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}",
            paramName: nameof(value));
    }
}
