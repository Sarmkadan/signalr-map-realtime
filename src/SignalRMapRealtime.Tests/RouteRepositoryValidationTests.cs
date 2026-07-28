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

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        RouteRepository? repo = null;
        Assert.Throws<ArgumentNullException>(() => repo.Validate());
    }

    [Fact]
    public void Validate_ValidInstance_ReturnsEmpty()
    {
        var repo = CreateRepository();
        IReadOnlyList<string> result = repo.Validate();
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ValidInstance_ReturnsTrue()
    {
        var repo = CreateRepository();
        bool isValid = repo.IsValid();
        Assert.True(isValid);
    }

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

    [Fact]
    public void ValidateParametersForGetRoutesByDateRangeAsync_InvalidRange_ReturnsError()
    {
        var start = new DateTime(2023, 01, 02);
        var end = new DateTime(2023, 01, 01);
        IReadOnlyList<string> result = start.ValidateParametersForGetRoutesByDateRangeAsync(end);
        Assert.Single(result);
        Assert.Contains("Start date must be before or equal to end date", result[0]);
    }

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

    [Fact]
    public void ValidateParametersForGetPendingRoutesAsync_AlwaysEmpty()
    {
        IReadOnlyList<string> result = RouteRepositoryValidation.ValidateParametersForGetPendingRoutesAsync();
        Assert.Empty(result);
    }
}
