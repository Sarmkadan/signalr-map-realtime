#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SignalRMapRealtime.Examples;

/// <summary>
/// Provides validation helpers for <see cref="RouteOptimizationClient.Waypoint"/> instances.
/// Validates coordinates, names, and ordering constraints.
/// </summary>
public static class RouteOptimizationClientValidation
{
    /// <summary>
    /// Validates a waypoint instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The waypoint to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RouteOptimizationClient.Waypoint? value)
    {
        if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

        var errors = new List<string>();

        // Validate Order (must be positive)
        if (value.Order <= 0)
        {
            errors.Add($"Order must be a positive integer, but was {value.Order}.");
        }

        // Validate Latitude (must be between -90 and 90)
        if (double.IsNaN(value.Latitude) || double.IsInfinity(value.Latitude))
        {
            errors.Add("Latitude must be a valid number.");
        }
        else if (value.Latitude < -90.0 || value.Latitude > 90.0)
        {
            errors.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"Latitude must be between -90 and 90 degrees, but was {value.Latitude:F6}."));
        }

        // Validate Longitude (must be between -180 and 180)
        if (double.IsNaN(value.Longitude) || double.IsInfinity(value.Longitude))
        {
            errors.Add("Longitude must be a valid number.");
        }
        else if (value.Longitude < -180.0 || value.Longitude > 180.0)
        {
            errors.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"Longitude must be between -180 and 180 degrees, but was {value.Longitude:F6}."));
        }

        // Validate Name (must not be null or whitespace)
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name cannot be null or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified waypoint is valid.
    /// </summary>
    /// <param name="value">The waypoint to check.</param>
    /// <returns><see langword="true"/> if the waypoint is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this RouteOptimizationClient.Waypoint? value)
    {
        return value is not null
                && value.Order > 0
                && !double.IsNaN(value.Latitude)
                && !double.IsInfinity(value.Latitude)
                && value.Latitude >= -90.0 && value.Latitude <= 90.0
                && !double.IsNaN(value.Longitude)
                && !double.IsInfinity(value.Longitude)
                && value.Longitude >= -180.0 && value.Longitude <= 180.0
                && !string.IsNullOrWhiteSpace(value.Name);
    }

    /// <summary>
    /// Ensures that the specified waypoint is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The waypoint to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the waypoint is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this RouteOptimizationClient.Waypoint? value)
    {
        if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Waypoint is invalid. Validation errors:\n  - {string.Join("\n  - ", errors)}");
        }
    }
}
