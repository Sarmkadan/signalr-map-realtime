#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Validation helpers for <see cref="LocationControllerTests"/>.
    /// </summary>
    public static class LocationControllerTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="LocationControllerTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this LocationControllerTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="LocationControllerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this LocationControllerTests? value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="LocationControllerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this LocationControllerTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    "The LocationControllerTests instance is not valid. " +
                    string.Join(" ", problems),
                    nameof(value));
            }
        }
    }
}
