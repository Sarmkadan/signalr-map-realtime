#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="LocationTrackingException"/> and its derived types.
/// </summary>
public static class LocationTrackingExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="LocationTrackingException"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this LocationTrackingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        switch (value)
        {
            case VehicleNotFoundException vehicleEx:
                ValidateVehicleNotFoundException(vehicleEx, problems);
                break;

            case InvalidLocationException invalidLocEx:
                ValidateInvalidLocationException(invalidLocEx, problems);
                break;

            case AssetNotFoundException assetEx:
                ValidateAssetNotFoundException(assetEx, problems);
                break;

            case TrackingSessionNotFoundException sessionEx:
                ValidateTrackingSessionNotFoundException(sessionEx, problems);
                break;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="LocationTrackingException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this LocationTrackingException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="LocationTrackingException"/> is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this LocationTrackingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"LocationTrackingException is not valid. Problems: {string.Join(", ", problems)}");
        }
    }

    private static void ValidateVehicleNotFoundException(
        VehicleNotFoundException value,
        List<string> problems)
    {
        if (value.VehicleId <= 0)
        {
            problems.Add(
                $"VehicleNotFoundException.VehicleId must be positive, but was {value.VehicleId}.");
        }
    }

    private static void ValidateInvalidLocationException(
        InvalidLocationException value,
        List<string> problems)
    {
        if (value.Latitude is not null)
        {
            if (value.Latitude is < -90 or > 90)
            {
                problems.Add(
                    $"InvalidLocationException.Latitude must be between -90 and 90, but was {value.Latitude.Value.ToString(CultureInfo.InvariantCulture)}.");
            }
        }

        if (value.Longitude is not null)
        {
            if (value.Longitude is < -180 or > 180)
            {
                problems.Add(
                    $"InvalidLocationException.Longitude must be between -180 and 180, but was {value.Longitude.Value.ToString(CultureInfo.InvariantCulture)}.");
            }
        }
    }

    private static void ValidateAssetNotFoundException(
        AssetNotFoundException value,
        List<string> problems)
    {
        if (value.AssetId <= 0)
        {
            problems.Add(
                $"AssetNotFoundException.AssetId must be positive, but was {value.AssetId}.");
        }
    }

    private static void ValidateTrackingSessionNotFoundException(
        TrackingSessionNotFoundException value,
        List<string> problems)
    {
        if (value.SessionId <= 0)
        {
            problems.Add(
                $"TrackingSessionNotFoundException.SessionId must be positive, but was {value.SessionId}.");
        }
    }
}