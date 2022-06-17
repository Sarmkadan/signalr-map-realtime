#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Provides validation methods for <see cref="RouteDto"/> instances.
/// </summary>
public static class RouteDtoValidation
{
    /// <summary>
    /// Validates a <see cref="RouteDto"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The route DTO to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RouteDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name is required and cannot be empty or whitespace.");
        }

        // Validate VehicleId (required positive integer)
        if (value.VehicleId <= 0)
        {
            errors.Add("VehicleId must be a positive integer.");
        }

        // Validate PlannedDepartureTime (must be in the past or present)
        if (value.PlannedDepartureTime > DateTime.UtcNow)
        {
            errors.Add("PlannedDepartureTime must be in the past or present.");
        }

        // Validate EstimatedArrivalTime (must be after PlannedDepartureTime)
        if (value.EstimatedArrivalTime <= value.PlannedDepartureTime)
        {
            errors.Add("EstimatedArrivalTime must be after PlannedDepartureTime.");
        }

        // Validate TotalDistance (if provided, must be positive)
        if (value.TotalDistance.HasValue && value.TotalDistance <= 0)
        {
            errors.Add("TotalDistance must be positive if specified.");
        }

        // Validate ActualDistance (if provided, must be positive)
        if (value.ActualDistance.HasValue && value.ActualDistance <= 0)
        {
            errors.Add("ActualDistance must be positive if specified.");
        }

        // Validate CreatedAt (must be in the past)
        if (value.CreatedAt > DateTime.UtcNow)
        {
            errors.Add("CreatedAt must be in the past.");
        }

        // Validate UpdatedAt (must be in the past)
        if (value.UpdatedAt > DateTime.UtcNow)
        {
            errors.Add("UpdatedAt must be in the past.");
        }

        // Validate Waypoints collection (must not be null, can be empty)
        if (value.Waypoints is null)
        {
            errors.Add("Waypoints collection cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RouteDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The route DTO to check.</param>
    /// <returns>True if the route is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RouteDto? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RouteDto"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The route DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the route is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this RouteDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RouteDto validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}