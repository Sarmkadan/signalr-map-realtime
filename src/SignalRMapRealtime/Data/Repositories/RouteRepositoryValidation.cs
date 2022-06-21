#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using System.Globalization;

/// <summary>
/// Provides validation helpers for RouteRepository to ensure data integrity.
/// </summary>
public static class RouteRepositoryValidation
{
    /// <summary>
    /// Validates a RouteRepository instance for common data issues.
    /// </summary>
    /// <param name="value">The RouteRepository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this RouteRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository instances are typically validated by their constructor
        // No additional validation needed beyond null check for the repository itself

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a RouteRepository instance is valid.
    /// </summary>
    /// <param name="value">The RouteRepository instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this RouteRepository? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a RouteRepository instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The RouteRepository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing the list of problems.</exception>
    public static void EnsureValid(this RouteRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RouteRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for GetActiveRoutesByVehicleAsync.
    /// </summary>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetActiveRoutesByVehicleAsync(this int vehicleId)
    {
        var problems = new List<string>();

        if (vehicleId <= 0)
        {
            problems.Add($"Vehicle ID must be positive, but was {vehicleId}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GetRoutesByUserAsync.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetRoutesByUserAsync(this int userId)
    {
        var problems = new List<string>();

        if (userId <= 0)
        {
            problems.Add($"User ID must be positive, but was {userId}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GetRoutesByCompletionAsync.
    /// </summary>
    /// <param name="isCompleted">Whether routes should be completed.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetRoutesByCompletionAsync(this bool isCompleted)
    {
        // No validation needed for boolean parameter
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for GetRouteWithDetailsAsync.
    /// </summary>
    /// <param name="routeId">The route identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetRouteWithDetailsAsync(this int routeId)
    {
        var problems = new List<string>();

        if (routeId <= 0)
        {
            problems.Add($"Route ID must be positive, but was {routeId}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GetRoutesByDateRangeAsync.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetRoutesByDateRangeAsync(this DateTime startDate, DateTime endDate)
    {
        var problems = new List<string>();

        if (startDate == default)
        {
            problems.Add("Start date cannot be the default DateTime value.");
        }

        if (endDate == default)
        {
            problems.Add("End date cannot be the default DateTime value.");
        }

        if (startDate > endDate)
        {
            problems.Add("Start date must be before or equal to end date.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GetLongestRoutesAsync.
    /// </summary>
    /// <param name="topCount">The number of routes to return.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetLongestRoutesAsync(this int topCount)
    {
        var problems = new List<string>();

        if (topCount <= 0)
        {
            problems.Add($"Top count must be positive, but was {topCount}.");
        }

        if (topCount > 1000)
        {
            problems.Add($"Top count {topCount} is too large; maximum recommended is 1000.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GetAverageCompletionTimeAsync.
    /// </summary>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetAverageCompletionTimeAsync(this int vehicleId)
    {
        var problems = new List<string>();

        if (vehicleId <= 0)
        {
            problems.Add($"Vehicle ID must be positive, but was {vehicleId}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GetPendingRoutesAsync.
    /// </summary>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetPendingRoutesAsync()
    {
        // No parameters to validate
        return Array.Empty<string>();
    }
}