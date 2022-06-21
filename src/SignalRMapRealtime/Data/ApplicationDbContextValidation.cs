#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="ApplicationDbContext"/> instances.
/// </summary>
public static class ApplicationDbContextValidation
{
    /// <summary>
    /// Validates the specified <see cref="ApplicationDbContext"/> instance.
    /// </summary>
    /// <param name="value">The database context to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the context is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ApplicationDbContext value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate DbContextOptions
        if (value.Database is null)
        {
            problems.Add("Database provider is not initialized.");
        }

        // Validate DbSet properties
        ValidateDbSet(value.Users, nameof(value.Users), problems);
        ValidateDbSet(value.Vehicles, nameof(value.Vehicles), problems);
        ValidateDbSet(value.Locations, nameof(value.Locations), problems);
        ValidateDbSet(value.TrackingSessions, nameof(value.TrackingSessions), problems);
        ValidateDbSet(value.Routes, nameof(value.Routes), problems);
        ValidateDbSet(value.Waypoints, nameof(value.Waypoints), problems);
        ValidateDbSet(value.Assets, nameof(value.Assets), problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApplicationDbContext"/> is valid.
    /// </summary>
    /// <param name="value">The database context to check.</param>
    /// <returns><see langword="true"/> if the context is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ApplicationDbContext value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApplicationDbContext"/> is valid.
    /// </summary>
    /// <param name="value">The database context to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the context is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ApplicationDbContext value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApplicationDbContext is invalid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }

    private static void ValidateDbSet<TEntity>(
        Microsoft.EntityFrameworkCore.DbSet<TEntity> dbSet,
        string propertyName,
        ICollection<string> problems)
        where TEntity : class
    {
        if (dbSet is null)
        {
            problems.Add($"DbSet<{typeof(TEntity).Name}> {propertyName} is null.");
        }
    }
}