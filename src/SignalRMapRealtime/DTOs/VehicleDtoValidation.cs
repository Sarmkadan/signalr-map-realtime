#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Provides validation methods for <see cref="VehicleDto"/> instances.
/// </summary>
public static class VehicleDtoValidation
{
    private const int MinReasonableYear = 1900;
    private const int MaxReasonableYear = 2100;
    private const double MinFuelLevel = 0.0;
    private const double MaxFuelLevel = 100.0;
    private const double MinMaxSpeed = 0.0;

    /// <summary>
    /// Validates a <see cref="VehicleDto"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The vehicle DTO to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the DTO is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VehicleDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required properties using pattern matching for clarity
        if (value.Name is null or { Length: 0 })
        {
            errors.Add("Vehicle name is required and cannot be empty.");
        }

        if (value.RegistrationNumber is null or { Length: 0 })
        {
            errors.Add("Registration number is required and cannot be empty.");
        }

        if (value.Id <= 0)
        {
            errors.Add("Vehicle ID must be a positive integer.");
        }

        // Validate enum values
        if (!Enum.IsDefined(typeof(VehicleStatus), value.Status))
        {
            errors.Add("Vehicle status is invalid.");
        }

        if (!Enum.IsDefined(typeof(AssetType), value.AssetType))
        {
            errors.Add("Asset type is invalid.");
        }

        // Validate optional numeric properties using expression-bodied methods for one-liners
        if (value.ModelYear.HasValue && (value.ModelYear < MinReasonableYear || value.ModelYear > MaxReasonableYear))
        {
            errors.Add($"Model year must be between {MinReasonableYear} and {MaxReasonableYear}.");
        }

        if (value.MaxSpeed.HasValue && value.MaxSpeed < MinMaxSpeed)
        {
            errors.Add("Maximum speed cannot be negative.");
        }

        if (value.FuelLevel.HasValue && (value.FuelLevel < MinFuelLevel || value.FuelLevel > MaxFuelLevel))
        {
            errors.Add($"Fuel level must be between {MinFuelLevel} and {MaxFuelLevel}.");
        }

        if (value.DriverId.HasValue && value.DriverId <= 0)
        {
            errors.Add("Driver ID must be a positive integer when specified.");
        }

        // Validate Year property (alternate naming) - ensure both ModelYear and Year don't conflict
        if (value.Year.HasValue && (value.Year < MinReasonableYear || value.Year > MaxReasonableYear))
        {
            errors.Add($"Year must be between {MinReasonableYear} and {MaxReasonableYear}.");
        }

        // Validate date properties - use DateTime.MinValue for explicit comparison
        if (value.CreatedAt == DateTime.MinValue)
        {
            errors.Add("CreatedAt timestamp cannot be the default value.");
        }

        if (value.UpdatedAt == DateTime.MinValue)
        {
            errors.Add("UpdatedAt timestamp cannot be the default value.");
        }

        // Validate LastLocation if present
        if (value.LastLocation is not null)
        {
            errors.AddRange(value.LastLocation.Validate());
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="VehicleDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The vehicle DTO to check.</param>
    /// <returns>True if the DTO is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this VehicleDto? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="VehicleDto"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The vehicle DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this VehicleDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"VehicleDto validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="LocationDto"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The location DTO to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the DTO is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this LocationDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.Latitude < -90.0 || value.Latitude > 90.0)
        {
            errors.Add("Latitude must be between -90.0 and 90.0 degrees.");
        }

        if (value.Longitude < -180.0 || value.Longitude > 180.0)
        {
            errors.Add("Longitude must be between -180.0 and 180.0 degrees.");
        }

        if (value.Altitude.HasValue)
        {
            // Altitude can be negative (below sea level) or positive (above sea level)
            // No specific bounds enforced, but should be reasonable
            if (value.Altitude < -10000.0 || value.Altitude > 10000.0)
            {
                errors.Add("Altitude appears to be unrealistic (outside -10000m to 10000m range).");
            }
        }

        if (value.Accuracy.HasValue && value.Accuracy < 0.0)
        {
            errors.Add("GPS accuracy cannot be negative.");
        }

        if (value.Speed.HasValue && value.Speed < 0.0)
        {
            errors.Add("Speed cannot be negative.");
        }

        if (value.Bearing.HasValue)
        {
            if (value.Bearing < 0.0 || value.Bearing > 360.0)
            {
                errors.Add("Bearing must be between 0.0 and 360.0 degrees.");
            }
        }

        if (!Enum.IsDefined(typeof(LocationType), value.LocationType))
        {
            errors.Add("Location type is invalid.");
        }

        if (value.RecordedAt == DateTime.MinValue)
        {
            errors.Add("RecordedAt timestamp cannot be the default value.");
        }

        if (value.CreatedAt == DateTime.MinValue)
        {
            errors.Add("CreatedAt timestamp cannot be the default value.");
        }

        return errors.AsReadOnly();
    }
}