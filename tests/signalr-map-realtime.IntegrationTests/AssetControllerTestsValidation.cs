#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SignalRMapRealtime.IntegrationTests
{
    /// <summary>
    /// Provides validation helpers for <see cref="AssetControllerTests"/> instances.
    /// </summary>
    public static class AssetControllerTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="AssetControllerTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AssetControllerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="AssetControllerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this AssetControllerTests value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="AssetControllerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this AssetControllerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"AssetControllerTests instance is not valid. Problems: {string.Join(", ", problems)}",
                    nameof(value));
            }
        }
    }
}
