#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SignalRMapRealtime.Events;
using SignalRMapRealtime.Services;
using System.Globalization;

/// <summary>
/// Provides validation methods for <see cref="GeofenceServiceTests"/> instances.
/// </summary>
public static class GeofenceServiceTestsValidation
{
    /// <summary>
    /// Creates a new instance of the GeofenceService class for testing purposes.
    /// </summary>
    /// <param name="testsInstance">The test instance to create the service from.</param>
    /// <returns>A new instance of the GeofenceService class.</returns>
    private static GeofenceService CreateService(GeofenceServiceTests? testsInstance = null)
    {
        var eventBus = Substitute.For<IEventBus>();
        return new GeofenceService(eventBus, NullLogger<GeofenceService>.Instance);
    }

    /// <summary>
    /// Validates the specified GeofenceServiceTests instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GeofenceServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the service can be created without throwing
        try
        {
            var service = CreateService(value);
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to create GeofenceService: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified GeofenceServiceTests instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this GeofenceServiceTests? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified GeofenceServiceTests instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this GeofenceServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"GeofenceServiceTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}
