#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="ThrottleOptions"/> configuration.
/// Validates that all throttle intervals are within reasonable bounds and that
/// the configuration is semantically valid for production use.
/// </summary>
public static class ThrottleOptionsValidation
{
    private const int MinimumAllowedIntervalSeconds = 1;
    private const int MaximumAllowedIntervalSeconds = 86400; // 24 hours
    private const int DefaultDeliveryVanIntervalSeconds = 1;

    /// <summary>
    /// Validates the throttle configuration and returns a list of human-readable problems.
    /// Returns an empty list if the configuration is valid.
    /// </summary>
    /// <param name="value">The throttle options to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ThrottleOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (!value.Enabled)
        {
            return errors; // Validation disabled, no errors
        }

        ValidateInterval(value.DeliveryVanIntervalSeconds, nameof(value.DeliveryVanIntervalSeconds), errors);
        ValidateInterval(value.CourierIntervalSeconds, nameof(value.CourierIntervalSeconds), errors);
        ValidateInterval(value.BicycleIntervalSeconds, nameof(value.BicycleIntervalSeconds), errors);
        ValidateInterval(value.MotorcycleIntervalSeconds, nameof(value.MotorcycleIntervalSeconds), errors);
        ValidateInterval(value.PortableIntervalSeconds, nameof(value.PortableIntervalSeconds), errors);
        ValidateInterval(value.FixedAssetIntervalSeconds, nameof(value.FixedAssetIntervalSeconds), errors);
        ValidateInterval(value.DroneIntervalSeconds, nameof(value.DroneIntervalSeconds), errors);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the throttle configuration is valid.
    /// </summary>
    /// <param name="value">The throttle options to check.</param>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ThrottleOptions? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the throttle configuration is valid.
    /// Throws an <see cref="ArgumentException"/> with detailed error messages if validation fails.
    /// </summary>
    /// <param name="value">The throttle options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the configuration is invalid.</exception>
    public static void EnsureValid(this ThrottleOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!value.Enabled)
        {
            return; // Validation disabled, no errors
        }

        var errors = Validate(value);

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ThrottleOptions configuration is invalid. Problems:\n{string.Join("\n", errors)}",
            nameof(value));
    }

    private static void ValidateInterval(int intervalSeconds, string propertyName, List<string> errors)
    {
        if (intervalSeconds < MinimumAllowedIntervalSeconds)
        {
            errors.Add($"{propertyName}: {intervalSeconds} seconds is less than the minimum allowed value of {MinimumAllowedIntervalSeconds} seconds.");
        }
        else if (intervalSeconds > MaximumAllowedIntervalSeconds)
        {
            errors.Add($"{propertyName}: {intervalSeconds} seconds exceeds the maximum allowed value of {MaximumAllowedIntervalSeconds} seconds (24 hours).");
        }
    }
}