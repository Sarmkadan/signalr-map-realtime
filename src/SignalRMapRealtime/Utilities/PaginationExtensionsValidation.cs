#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation extensions for PaginationInfo to ensure safe usage
// =====================================================================

using System.Globalization;

namespace SignalRMapRealtime.Utilities;

/// <summary>
/// Provides validation extension methods for <see cref="PaginationInfo"/> to ensure
/// pagination state is valid before use. Includes comprehensive validation for all
/// pagination-related parameters and state.
/// </summary>
public static class PaginationExtensionsValidation
{
    /// <summary>
    /// Validates the pagination state and returns a list of human-readable problems.
    /// Checks for null values, out-of-range numbers, and logical inconsistencies.
    /// </summary>
    /// <param name="value">The pagination info to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this PaginationInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate PageNumber
        if (value.PageNumber < 1)
        {
            problems.Add($"PageNumber must be at least 1, but was {value.PageNumber}.");
        }

        // Validate PageSize
        if (value.PageSize < 1)
        {
            problems.Add($"PageSize must be at least 1, but was {value.PageSize}.");
        }

        // Validate TotalCount
        if (value.TotalCount < 0)
        {
            problems.Add($"TotalCount cannot be negative, but was {value.TotalCount}.");
        }

        // Validate TotalPages
        if (value.TotalPages < 0)
        {
            problems.Add($"TotalPages cannot be negative, but was {value.TotalPages}.");
        }

        // Validate Skip
        if (value.Skip < 0)
        {
            problems.Add($"Skip cannot be negative, but was {value.Skip}.");
        }

        // Validate boolean flags consistency
        if (value.IsFirstPage && value.PageNumber != 1)
        {
            problems.Add("IsFirstPage is true but PageNumber is not 1.");
        }

        if (value.HasPreviousPage && value.PageNumber <= 1)
        {
            problems.Add("HasPreviousPage is true but PageNumber is 1 or less.");
        }

        if (value.HasNextPage && value.PageNumber >= value.TotalPages)
        {
            problems.Add("HasNextPage is true but PageNumber is at or beyond TotalPages.");
        }

        // Validate ItemsOnPage
        if (value.ItemsOnPage < 0)
        {
            problems.Add($"ItemsOnPage cannot be negative, but was {value.ItemsOnPage}.");
        }

        // Validate that ItemsOnPage doesn't exceed PageSize
        if (value.ItemsOnPage > value.PageSize)
        {
            problems.Add($"ItemsOnPage ({value.ItemsOnPage}) cannot exceed PageSize ({value.PageSize}).");
        }

        // Validate that TotalCount >= ItemsOnPage + Skip
        if (value.TotalCount < value.ItemsOnPage + value.Skip)
        {
            problems.Add(
                $"TotalCount ({value.TotalCount}) must be at least ItemsOnPage ({value.ItemsOnPage}) + Skip ({value.Skip}) = {value.ItemsOnPage + value.Skip}."
            );
        }

        // Validate that TotalPages calculation is correct
        var calculatedTotalPages = PaginationExtensions.CalculateTotalPages(value.TotalCount, value.PageSize);
        if (value.TotalPages != calculatedTotalPages)
        {
            problems.Add($"TotalPages ({value.TotalPages}) does not match calculated value ({calculatedTotalPages}) based on TotalCount and PageSize.");
        }

        // Validate that Skip calculation is correct
        var calculatedSkip = PaginationExtensions.CalculateSkip(value.PageNumber, value.PageSize);
        if (value.Skip != calculatedSkip)
        {
            problems.Add($"Skip ({value.Skip}) does not match calculated value ({calculatedSkip}) based on PageNumber and PageSize.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the pagination state is valid.
    /// </summary>
    /// <param name="value">The pagination info to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this PaginationInfo value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the pagination state is valid, throwing an exception if not.
    /// The exception includes a detailed list of all validation problems.
    /// </summary>
    /// <param name="value">The pagination info to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all problems.</exception>
    public static void EnsureValid(this PaginationInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PaginationInfo validation failed with {problems.Count} problem(s):{Environment.NewLine}- " +
            string.Join($"{Environment.NewLine}- ", problems),
            nameof(value)
        );
    }
}