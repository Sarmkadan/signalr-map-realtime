#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="CachingOptions"/> configuration.
/// Validates all numeric ranges, string formats, and business rules for caching behavior.
/// </summary>
public static class CachingOptionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="CachingOptions"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The caching options to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CachingOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate boolean flags
        if (value.Enabled && value.UseDistributedCache && string.IsNullOrWhiteSpace(value.DistributedCacheConnectionString))
        {
            errors.Add("DistributedCacheConnectionString must be provided when UseDistributedCache is true.");
        }

        // Validate duration values (all durations must be positive)
        ValidatePositiveDuration(errors, nameof(CachingOptions.DefaultDurationSeconds), value.DefaultDurationSeconds, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.LocationCacheDurationSeconds), value.LocationCacheDurationSeconds, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.VehicleCacheDurationSeconds), value.VehicleCacheDurationSeconds, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.RouteCacheDurationSeconds), value.RouteCacheDurationSeconds, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.AssetCacheDurationSeconds), value.AssetCacheDurationSeconds, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.SessionCacheDurationSeconds), value.SessionCacheDurationSeconds, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.RefreshTokenAbsoluteExpirationMinutes), value.RefreshTokenAbsoluteExpirationMinutes, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.SessionSlidingExpirationMinutes), value.SessionSlidingExpirationMinutes, 1);
        ValidatePositiveDuration(errors, nameof(CachingOptions.MaxMemoryCacheEntries), value.MaxMemoryCacheEntries, 1);

        // Validate distributed cache connection string format (basic validation)
        if (value.UseDistributedCache && !string.IsNullOrWhiteSpace(value.DistributedCacheConnectionString))
        {
            var connectionString = value.DistributedCacheConnectionString.Trim();
            if (!connectionString.Contains('=') || !connectionString.Contains(';'))
            {
                errors.Add("DistributedCacheConnectionString should be a valid connection string with key=value pairs separated by semicolons.");
            }
        }

        // Validate that location cache duration is reasonable compared to default
        if (value.LocationCacheDurationSeconds > value.DefaultDurationSeconds * 2)
        {
            errors.Add($"LocationCacheDurationSeconds ({value.LocationCacheDurationSeconds}s) should not exceed twice the DefaultDurationSeconds ({value.DefaultDurationSeconds}s) for performance.");
        }

        // Validate that session durations are reasonable
        if (value.SessionSlidingExpirationMinutes <= 0)
        {
            errors.Add("SessionSlidingExpirationMinutes must be greater than 0.");
        }

        if (value.SessionSlidingExpirationMinutes > 1440) // 24 hours
        {
            errors.Add("SessionSlidingExpirationMinutes should not exceed 1440 minutes (24 hours) for security.");
        }

        // Validate that refresh token expiration is reasonable
        if (value.RefreshTokenAbsoluteExpirationMinutes <= 0)
        {
            errors.Add("RefreshTokenAbsoluteExpirationMinutes must be greater than 0.");
        }

        if (value.RefreshTokenAbsoluteExpirationMinutes > 525600) // 1 year
        {
            errors.Add("RefreshTokenAbsoluteExpirationMinutes should not exceed 525600 minutes (1 year).");
        }

        // Validate that max memory cache entries is reasonable
        if (value.MaxMemoryCacheEntries <= 0)
        {
            errors.Add("MaxMemoryCacheEntries must be greater than 0.");
        }

        if (value.MaxMemoryCacheEntries > 1000000) // 1 million entries
        {
            errors.Add("MaxMemoryCacheEntries should not exceed 1000000 for memory efficiency.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CachingOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The caching options to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this CachingOptions value)
    {
        try
        {
            _ = value.Validate();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="CachingOptions"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The caching options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all error messages.</exception>
    public static void EnsureValid(this CachingOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"CachingOptions validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)}");
    }

    private static void ValidatePositiveDuration(List<string> errors, string propertyName, int value, int minimumValue)
    {
        if (value < minimumValue)
        {
            errors.Add($"{propertyName} must be at least {minimumValue} but was {value}.");
        }
    }
}