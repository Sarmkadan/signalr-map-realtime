// SPDX-License-Identifier: MIT
// ---------------------------------------------------------------
// Tests for RouteRepositoryValidation extension methods.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
using Xunit;

namespace SignalRMapRealtime.Tests;

/// <summary>
/// Unit tests for the parameter and instance validation helpers of <c>RouteRepository</c>
/// and <c>RouteRepositoryValidation</c>.
/// </summary>
public class RouteRepositoryValidationTests
{
    private static RouteRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        // The real RouteRepository likely expects an ApplicationDbContext.
        // If its constructor differs, adjust accordingly.
        return new RouteRepository(dbContext);
    }

    /// <summary>
    /// Verifies that calling <c>Validate</c> on a null repository reference throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        RouteRepository? repo = null;
        Assert.Throws<ArgumentNullException>(() => repo.Validate());
    }

    /// <summary>
    /// Verifies that <c>Validate</c> on a repository built over an in-memory context returns no error messages.
    /// </summary>
    [Fact]
    public void Validate_ValidInstance_ReturnsEmpty()
    {
        var repo = CreateRepository();
        IReadOnlyList<string> result = repo.Validate();
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <see langword="true"/> for a repository built over an in-memory context.
    /// </summary>
    [Fact]
    public void IsValid_ValidInstance_ReturnsTrue()
    {
        var repo = CreateRepository();
        bool isValid = repo.IsValid();
        Assert.True(isValid);
    }

    /// <summary>
    /// Verifies that a positive vehicle ID passes <c>ValidateParametersForGetActiveRoutesByVehicleAsync</c>,
    /// while zero or a negative ID yields a single "Vehicle ID must be positive" error.
    /// </summary>
    [Theory]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(-3, false)]
    public void ValidateParametersForGetActiveRoutesByVehicleAsync_ReturnsExpected(int vehicleId, bool expectValid)
    {
        IReadOnlyList<string> result = vehicleId.ValidateParametersForGetActiveRoutesByVehicleAsync();
        if (expectValid)
        {
            Assert.Empty(result);
        }
        else
        {
            Assert.Single(result);
            Assert.Contains("Vehicle ID must be positive", result[0]);
        }
    }

    /// <summary>
    /// Verifies that a start date later than the end date makes <c>ValidateParametersForGetRoutesByDateRangeAsync</c>
    /// return a single "Start date must be before or equal to end date" error.
    /// </summary>
    [Fact]
    public void ValidateParametersForGetRoutesByDateRangeAsync_InvalidRange_ReturnsError()
    {
        var start = new DateTime(2023, 01, 02);
        var end = new DateTime(2023, 01, 01);
        IReadOnlyList<string> result = start.ValidateParametersForGetRoutesByDateRangeAsync(end);
        Assert.Single(result);
        Assert.Contains("Start date must be before or equal to end date", result[0]);
    }

    /// <summary>
    /// Verifies the top-count bounds of <c>ValidateParametersForGetLongestRoutesAsync</c>: 10 is accepted,
    /// 0 fails with a "must be positive" error, and 1500 fails with a "too large" error.
    /// </summary>
    [Theory]
    [InlineData(10, true)]
    [InlineData(0, false)]
    [InlineData(1500, false)]
    public void ValidateParametersForGetLongestRoutesAsync_BoundaryValues(int topCount, bool expectValid)
    {
        IReadOnlyList<string> result = topCount.ValidateParametersForGetLongestRoutesAsync();
        if (expectValid)
        {
            Assert.Empty(result);
        }
        else
        {
            Assert.Single(result);
            if (topCount <= 0)
                Assert.Contains("must be positive", result[0]);
            else
                Assert.Contains("too large", result[0]);
        }
    }

    /// <summary>
    /// Verifies that a positive vehicle ID passes <c>ValidateParametersForGetAverageCompletionTimeAsync</c>,
    /// while zero yields a single "Vehicle ID must be positive" error.
    /// </summary>
    [Theory]
    [InlineData(7, true)]
    [InlineData(0, false)]
    public void ValidateParametersForGetAverageCompletionTimeAsync_VehicleId(int vehicleId, bool expectValid)
    {
        IReadOnlyList<string> result = vehicleId.ValidateParametersForGetAverageCompletionTimeAsync();
        if (expectValid)
        {
            Assert.Empty(result);
        }
        else
        {
            Assert.Single(result);
            Assert.Contains("Vehicle ID must be positive", result[0]);
        }
    }

    /// <summary>
    /// Verifies that <c>RouteRepositoryValidation.ValidateParametersForGetPendingRoutesAsync</c> takes no parameters
    /// and always returns an empty error list.
    /// </summary>
    [Fact]
    public void ValidateParametersForGetPendingRoutesAsync_AlwaysEmpty()
    {
        IReadOnlyList<string> result = RouteRepositoryValidation.ValidateParametersForGetPendingRoutesAsync();
        Assert.Empty(result);
    }
}
