#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Models;
using Xunit;

/// <summary>
/// Tests for the AssetRepository class.
/// This project has no EF query provider, so only the constructor and the argument guards that run before any data access are covered.
/// Query behaviour needs a provider such as EF InMemory, which the IntegrationTests project references.
/// </summary>
public class AssetRepositoryTests
{
    /// <summary>
    /// Creates a context over an unused SQL Server configuration. The context is never opened, so no server is contacted.
    /// </summary>
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Constructor_ValidContext_CreatesBaseRepository()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var repository = new AssetRepository(context);

        // Assert
        repository.Should().BeAssignableTo<BaseRepository<Asset>>();
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new AssetRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetBySerialNumberAsync_NullSerialNumber_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AssetRepository(context);

        // Act
        Func<Task> act = () => repository.GetBySerialNumberAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetBySerialNumberAsync_BlankSerialNumber_ThrowsArgumentException(string serialNumber)
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AssetRepository(context);

        // Act
        Func<Task> act = () => repository.GetBySerialNumberAsync(serialNumber);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CountByConditionAsync_NullCondition_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AssetRepository(context);

        // Act
        Func<Task> act = () => repository.CountByConditionAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("\t")]
    public async Task CountByConditionAsync_BlankCondition_ThrowsArgumentException(string condition)
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AssetRepository(context);

        // Act
        Func<Task> act = () => repository.CountByConditionAsync(condition);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
