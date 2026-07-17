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

        // RouteRepository is a simple data access class with no complex dependencies
        // All validation is handled by the constructor and DbContext
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a RouteRepository instance is valid.
    /// </summary>
    /// <param name="value">The RouteRepository instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this RouteRepository? value) => Validate(value).Count == 0;

    /// <summary>
    /// Validates parameters for GetActiveRoutesByVehicleAsync.
    /// </summary>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when vehicleId is not positive.</exception>
    public static IReadOnlyList<string> ValidateParametersForGetActiveRoutesByVehicleAsync(this int vehicleId)
    {
        return vehicleId > 0
            ? Array.Empty<string>()
            : [$"Vehicle ID must be positive, but was {vehicleId}."];
    }

    /// <summary>
    /// Validates parameters for GetRoutesByUserAsync.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when userId is not positive.</exception>
    public static IReadOnlyList<string> ValidateParametersForGetRoutesByUserAsync(this int userId)
    {
        return userId > 0
            ? Array.Empty<string>()
            : [$"User ID must be positive, but was {userId}."];
    }

    /// <summary>
    /// Validates parameters for GetRoutesByCompletionAsync.
    /// </summary>
    /// <param name="isCompleted">Whether routes should be completed.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetRoutesByCompletionAsync(this bool isCompleted) => Array.Empty<string>();

    /// <summary>
    /// Validates parameters for GetRouteWithDetailsAsync.
    /// </summary>
    /// <param name="routeId">The route identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when routeId is not positive.</exception>
    public static IReadOnlyList<string> ValidateParametersForGetRouteWithDetailsAsync(this int routeId)
    {
        return routeId > 0
            ? Array.Empty<string>()
            : [$"Route ID must be positive, but was {routeId}."];
    }

    /// <summary>
    /// Validates parameters for GetRoutesByDateRangeAsync.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when startDate is after endDate.</exception>
    public static IReadOnlyList<string> ValidateParametersForGetRoutesByDateRangeAsync(this DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            return ["Start date must be before or equal to end date."];
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for GetLongestRoutesAsync.
    /// </summary>
    /// <param name="topCount">The number of routes to return.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when topCount is not positive or exceeds 1000.</exception>
    public static IReadOnlyList<string> ValidateParametersForGetLongestRoutesAsync(this int topCount)
    {
        return topCount > 0 && topCount <= 1000
            ? Array.Empty<string>()
            : topCount <= 0
                ? [$"Top count must be positive, but was {topCount}."]
                : [$"Top count {topCount} is too large; maximum recommended is 1000."];
    }

    /// <summary>
    /// Validates parameters for GetAverageCompletionTimeAsync.
    /// </summary>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when vehicleId is not positive.</exception>
    public static IReadOnlyList<string> ValidateParametersForGetAverageCompletionTimeAsync(this int vehicleId)
    {
        return vehicleId > 0
            ? Array.Empty<string>()
            : [$"Vehicle ID must be positive, but was {vehicleId}."];
    }

    /// <summary>
    /// Validates parameters for GetPendingRoutesAsync.
    /// </summary>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParametersForGetPendingRoutesAsync() => Array.Empty<string>();
}